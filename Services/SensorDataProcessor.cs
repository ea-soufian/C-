using PiBackend.Strategies;
using System;
using PiBackend.Singletons;

namespace PiBackend.Services
{
    public class SensorDataProcessor
    {
        private ISensorDataStrategy _strategy;
        private double _latestTemperature;
        private double _latestHumidity;
        private DateTime _lastTimestamp;
        private double _maxTemperatureToday;
        private double _minTemperatureToday;
        private double _maxHumidityToday;
        private double _minHumidityToday;

        // Constructor blijft zoals het was, maar we initialiseren nu ook de nieuwe velden
        public SensorDataProcessor(ISensorDataStrategy initialStrategy)
        {
            _strategy = initialStrategy;
            _latestTemperature = double.NaN;
            _latestHumidity = double.NaN;
            _lastTimestamp = DateTime.UtcNow;
            _maxTemperatureToday = double.MinValue;
            _minTemperatureToday = double.MaxValue;
            _maxHumidityToday = double.MinValue;
            _minHumidityToday = double.MaxValue;
        }

        // Methode om de strategy te veranderen, zoals in het oorspronkelijke ontwerp
        public void SetStrategy(ISensorDataStrategy strategy)
        {
            _strategy = strategy;
        }

        // Methode om de data te verwerken zoals voorheen, maar nu met extra logica om de laatste waarden op te slaan
        public double ProcessData(string data, string sensorType)
        {
            try
            {
                // Probeer de data te converteren met de strategie
                double standardizedData = _strategy.ConvertData(data);
                _lastTimestamp = DateTime.UtcNow;

                // Controleer of de data geldig is (geen NaN of Infinity)
                if (double.IsNaN(standardizedData) || double.IsInfinity(standardizedData))
                {
                    Console.WriteLine($"[ERROR] Invalid data received: {standardizedData}");
                    return double.NaN;
                }

                // Gebruik nu de sensorType-parameter in plaats van SensorTypeManager
                if (sensorType == "Temperature")
                {
                    if (ValidateSensorData(standardizedData)) // Validatie controle
                    {
                        _latestTemperature = standardizedData;
                        _maxTemperatureToday = Math.Max(_maxTemperatureToday, standardizedData);
                        _minTemperatureToday = Math.Min(_minTemperatureToday, standardizedData);
                    }
                    else
                    {
                        Console.WriteLine("[ERROR] Temperature data out of range.");
                        return double.NaN;
                    }
                }
                else if (sensorType == "Humidity")
                {
                    _latestHumidity = standardizedData;
                    _maxHumidityToday = Math.Max(_maxHumidityToday, standardizedData);
                    _minHumidityToday = Math.Min(_minHumidityToday, standardizedData);
                }

                return standardizedData;
            }
            catch (FormatException ex)
            {
                Console.WriteLine($"[ERROR] Data format issue: {ex.Message}");
                return double.NaN;
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine($"[ERROR] Operation issue: {ex.Message}");
                return double.NaN;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] An unexpected error occurred: {ex.Message}");
                return double.NaN;
            }
        }


        // Validatie methode voor temperatuur
        public bool ValidateSensorData(double temperature)
        {
            // Check if temperature is within an acceptable range (0 to 50 degrees Celsius)
            return temperature >= 0 && temperature <= 50;
        }

        // Nieuwe methodes om de laatste waarden en aggregaties op te vragen
        public double GetLatestTemperature()
        {
            return double.IsNaN(_latestTemperature) || double.IsInfinity(_latestTemperature)
                ? 0.0 // Default waarde als de data ongeldig is
                : _latestTemperature;
        }

        public double GetLatestHumidity()
        {
            return double.IsNaN(_latestHumidity) || double.IsInfinity(_latestHumidity)
                ? 0.0 // Default waarde als de data ongeldig is
                : _latestHumidity;
        }

        public DateTime GetLastTimestamp()
        {
            return _lastTimestamp;
        }

        public double GetMaxTemperatureToday()
        {
            return double.IsNaN(_maxTemperatureToday) || double.IsInfinity(_maxTemperatureToday)
                ? 0.0 // Default waarde
                : _maxTemperatureToday;
        }

        public double GetMinTemperatureToday()
        {
            return double.IsNaN(_minTemperatureToday) || double.IsInfinity(_minTemperatureToday)
                ? 0.0 // Default waarde
                : _minTemperatureToday;
        }

        public double GetMaxHumidityToday()
        {
            return double.IsNaN(_maxHumidityToday) || double.IsInfinity(_maxHumidityToday)
                ? 0.0 // Default waarde
                : _maxHumidityToday;
        }

        public double GetMinHumidityToday()
        {
            return double.IsNaN(_minHumidityToday) || double.IsInfinity(_minHumidityToday)
                ? 0.0 // Default waarde
                : _minHumidityToday;
        }
    }
}
