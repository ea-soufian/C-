using Microsoft.AspNetCore.Mvc;
using PiBackend.Services;

namespace PiBackend.Controllers
{
    [ApiController]
    [Route("sensor")]
    public class SensorController : ControllerBase
    {
        private readonly SensorDataProcessor _processor;

        // Injecteer SensorDataProcessor via de constructor (afhankelijk van hoe je je services opzet)
        public SensorController(SensorDataProcessor processor)
        {
            _processor = processor;
        }

        [HttpGet]
        public IActionResult GetSensorData()
        {
            // Controleer of de verwerkte data geldig is voordat je het retourneert
            if (double.IsNaN(_processor.GetLatestTemperature()) || double.IsInfinity(_processor.GetLatestTemperature()))
            {
                return StatusCode(500, "Invalid temperature data received.");
            }
            if (double.IsNaN(_processor.GetLatestHumidity()) || double.IsInfinity(_processor.GetLatestHumidity()))
            {
                return StatusCode(500, "Invalid humidity data received.");
            }

            // Haal live data op van de processor of andere bronnen
            var sensorData = new
            {
                items = new[]
                {
                    new
                    {
                        id = 1,
                        name = "Environment Sensor",
                        serial_number = "SN123456789",
                        last_measurements = new[]
                        {
                            new { type = "temperature", value = _processor.GetLatestTemperature(), unit = "Celsius", timestamp = _processor.GetLastTimestamp().ToString("o") },
                            new { type = "humidity", value = _processor.GetLatestHumidity(), unit = "Percent", timestamp = _processor.GetLastTimestamp().ToString("o") },
                            new { type = "battery", value = 85.0, unit = "Percent", timestamp = _processor.GetLastTimestamp().ToString("o") } // Statische waarde voor batterij
                        },
                        aggregations = new
                        {
                            temperature = new { max_today = _processor.GetMaxTemperatureToday(), min_today = _processor.GetMinTemperatureToday(), unit = "Celsius" },
                            humidity = new { max_today = _processor.GetMaxHumidityToday(), min_today = _processor.GetMinHumidityToday(), unit = "Percent" }
                        },
                        last_measurement_timestamp = _processor.GetLastTimestamp().ToString("o")
                    }
                }
            };

            return Ok(sensorData);
        }
    }
}
