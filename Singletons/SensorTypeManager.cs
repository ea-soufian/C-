namespace PiBackend.Singletons // Gebruik dezelfde namespace als je project of mapstructuur
{
    public class SensorTypeManager
    {
        private static SensorTypeManager _instance;
        private static readonly object _lock = new object();
        private string _currentSensorType;

        // Private constructor om directe instantie creatie te voorkomen
        private SensorTypeManager()
        {
            _currentSensorType = "Temperature"; // Standaard sensortype
        }

        // Publieke methode om de enkele instantie van de klasse te verkrijgen
        public static SensorTypeManager Instance
        {
            get
            {
                // Dubbel-check locking voor thread-safety
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new SensorTypeManager();
                        }
                    }
                }
                return _instance;
            }
        }

        // Methode om het sensortype in te stellen
        public void SetSensorType(string sensorType)
        {
            _currentSensorType = sensorType;
            Console.WriteLine($"Sensor type set to: {_currentSensorType}");
        }

        // Methode om het huidige sensortype op te halen
        public string GetSensorType()
        {
            return _currentSensorType;
        }
    }
}


