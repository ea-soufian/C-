using Microsoft.AspNetCore.SignalR;
using PiBackend.Hubs;
using PiBackend.Observers;
using PiBackend.Strategies;
using System.IO.Ports;
using PiBackend.Patterns; // Voor SensorTypeManager

namespace PiBackend.Services
{
    public class SerialDataReader
    {
        private readonly SerialPort _serialPort;
        private readonly List<ISerialDataObserver> _observers;
        private SensorDataProcessor _processor;
        private readonly IHubContext<SensorHub> _hubContext; // Voor SignalR

        public SerialDataReader(string portName, int baudRate, SensorDataProcessor processor, IHubContext<SensorHub> hubContext)
        {
            Console.WriteLine($"[INFO] Opening serial port: {portName} with baudrate: {baudRate}");
            _serialPort = new SerialPort(portName, baudRate);
            _serialPort.DtrEnable = true;
            _observers = new List<ISerialDataObserver>();
            _processor = processor;
            _hubContext = hubContext; // Initialiseer de SignalR HubContext
            Console.WriteLine("[INFO] SerialDataReader constructor executed");
        }

        public void Attach(ISerialDataObserver observer)
        {
            _observers.Add(observer);
        }

        public void Detach(ISerialDataObserver observer)
        {
            _observers.Remove(observer);
        }

        private void NotifyObservers(double data)
        {
            foreach (var observer in _observers)
            {
                observer.Update(data);
            }
        }

        private async Task UpdateStrategy(string sensorId, string sensorType)
        {
            // We koppelen elke sensor-ID aan een type (bijv. temperatuur of vochtigheid)
            SensorTypeManager.Instance.AddSensorType(sensorId, sensorType);

            if (sensorType == "Temperature")
            {
                _processor.SetStrategy(new TemperatureSensorStrategy());
                Console.WriteLine("[INFO] Switching to TemperatureSensorStrategy");
            }
            else if (sensorType == "Humidity")
            {
                _processor.SetStrategy(new HumiditySensorStrategy());
                Console.WriteLine("[INFO] Switching to HumiditySensorStrategy");
            }
            else
            {
                Console.WriteLine($"[WARNING] No strategy defined for sensor type: {sensorType}");
            }
        }

        private async Task LogProcessedData(string sensorId, string sensorType, double data)
        {
            if (sensorType == "Temperature")
            {
                Console.WriteLine($"[DATA] Processed temperature from sensor {sensorId}: {data}°C");
            }
            else if (sensorType == "Humidity")
            {
                Console.WriteLine($"[DATA] Processed humidity from sensor {sensorId}: {data}%");
            }

            // Verstuur de data via SignalR naar alle verbonden clients
            await _hubContext.Clients.All.SendAsync("ReceiveSensorData", new
            {
                sensorId = sensorId,
                type = sensorType,
                value = data,
                timestamp = DateTime.UtcNow
            });
        }

        public async Task StartReading()
        {
            try
            {
                Console.WriteLine("[INFO] About to open serial port...");
                _serialPort.Open();
                Console.WriteLine("[INFO] Serial port opened");

                while (true)
                {
                    string rawData = string.Empty; // Zorg ervoor dat rawData buiten de try wordt gedeclareerd
                    try
                    {
                        // Lees ruwe data van de seriële poort
                        rawData = _serialPort.ReadLine();
                        Console.WriteLine($"[DATA] Raw data received: {rawData}");

                        // Extract sensorId en sensortype uit de ruwe data
                        string sensorId = ExtractSensorId(rawData);  // Bijvoorbeeld "serial:123456"
                        string temperatureType = "Temperature"; // Assuming temperature data is always present
                        string humidityType = "Humidity"; // Assuming humidity data is always present

                        // Verwerk zowel temperatuur- als vochtigheidsgegevens
                        await ProcessSensorData(sensorId, temperatureType, rawData);
                        await ProcessSensorData(sensorId, humidityType, rawData);
                    }
                    catch (TimeoutException)
                    {
                        Console.WriteLine("[WARNING] Read timeout: no data received within the timeout period.");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[ERROR] Error reading serial port: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Error opening serial port: {ex.Message}");
            }
        }

        private async Task ProcessSensorData(string sensorId, string sensorType, string rawData)
        {
            // Voeg de sensorId en sensorType toe aan de SensorTypeManager Singleton
            await UpdateStrategy(sensorId, sensorType);

            // Verwerk de ruwe data met de processor, geef nu het sensorType mee
            double standardizedData = _processor.ProcessData(rawData, sensorType);

            if (double.IsNaN(standardizedData))
            {
                Console.WriteLine($"[ERROR] Data processing failed for: {rawData}");
            }
            else
            {
                await LogProcessedData(sensorId, sensorType, standardizedData);
                NotifyObservers(standardizedData);
            }
        }



        // Helpermethode om de sensorId uit de ruwe data te extraheren
        private string ExtractSensorId(string rawData)
        {
            // Dit is een vereenvoudigd voorbeeld van hoe je de sensorId kunt extraheren
            var parts = rawData.Split("serial:");
            return parts.Length > 1 ? parts[1].Split(' ')[0] : "Unknown";
        }

        // Helpermethode om het sensortype uit de ruwe data te extraheren
        private string ExtractSensorType(string rawData)
        {
            if (rawData.Contains("temp:"))
            {
                return "Temperature";
            }
            else if (rawData.Contains("hum:"))
            {
                return "Humidity";
            }
            else
            {
                return "Unknown";
            }
        }
    }
}
