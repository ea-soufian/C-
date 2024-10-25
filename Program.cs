using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PiBackend.Services;
using PiBackend.Observers;
using PiBackend.Strategies;
using PiBackend.Singletons;
using PiBackend.Hubs; // Voor SignalR
using Microsoft.AspNetCore.SignalR; // Nodig voor SignalR HubContext
using System.Threading.Tasks; // Voor Task ondersteuning

namespace PiBackend
{
    public class Program
    {
        public static async Task Main(string[] args) // Maak Main async
        {
            var builder = WebApplication.CreateBuilder(args);

            // Voeg CORS ondersteuning toe
            builder.Services.AddCors(options =>
            {
                options.AddDefaultPolicy(
                    policy =>
                    {
                        policy.WithOrigins("https://climate.dops.tech") // Sta frontend domein toe
                              .AllowAnyHeader()
                              .AllowAnyMethod()
                              .AllowCredentials();
                    });
            });

            // Voeg controllers en SignalR toe
            builder.Services.AddControllers();
            builder.Services.AddSignalR(); // Voeg SignalR toe aan de services

            // Voeg de ISensorDataStrategy en SensorDataProcessor toe via dependency injection
            builder.Services.AddSingleton<ISensorDataStrategy, TemperatureSensorStrategy>(); // Registreer de strategie
            builder.Services.AddSingleton<SensorDataProcessor>(); // Registreer de SensorDataProcessor

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();
            app.UseCors();

            // Pas de SignalR hub toe
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHub<SensorHub>("/sensorhub"); // Map de SensorHub naar een route
            });

            // Start de server
            var serverTask = Task.Run(() => {
                Console.WriteLine("Starting server on https://localhost:8888");
                app.Run("https://localhost:8888");
            });

            // Start het lezen van de seriële data in een achtergrondtaak
            try
            {
                // Haal de SignalR HubContext op via Dependency Injection
                var hubContext = app.Services.GetRequiredService<IHubContext<SensorHub>>();

                // Haal de SensorDataProcessor op via Dependency Injection
                var processor = app.Services.GetRequiredService<SensorDataProcessor>();

                // Voeg logging toe voor de initialisatie van SerialDataReader
                Console.WriteLine("[INFO] Initializing SerialDataReader with COM3 and baudrate 9600...");
                var serialDataReader = new SerialDataReader("COM3", 9600, processor, hubContext);
                Console.WriteLine("[INFO] SerialDataReader initialized");

                // Voeg een observer toe voor het loggen van data
                serialDataReader.Attach(new ConsoleLoggerObserver());
                Console.WriteLine("Observer attached");

                // Start het lezen van de seriële data in een aparte taak
                var readingTask = Task.Run(async () => await serialDataReader.StartReading());
                Console.WriteLine("Started reading serial data");

                // Wacht op de server en de leeslogica
                await Task.WhenAll(serverTask, readingTask);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Exception while initializing SerialDataReader: {ex.Message}");
            }
        }
    }
}
