namespace PiBackend.Patterns
{
    public class SensorTypeManager
    {
        private static SensorTypeManager? _instance; // Voeg ? toe om het nullable te maken
        private static readonly object _lock = new object();

        // Dictionary to store sensorId and its associated type (e.g., "Temperature", "Humidity")
        private Dictionary<string, string> _sensorTypes;

        private SensorTypeManager()
        {
            _sensorTypes = new Dictionary<string, string>();
        }

        public static SensorTypeManager Instance
        {
            get
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = new SensorTypeManager();
                    }
                    return _instance;
                }
            }
        }

        // Add or update the sensor type associated with a sensorId
        public void AddSensorType(string sensorId, string typeName)
        {
            if (!_sensorTypes.ContainsKey(sensorId))
            {
                _sensorTypes.Add(sensorId, typeName);
            }
            else
            {
                _sensorTypes[sensorId] = typeName; // Update existing sensor type if already added
            }
        }

        // Retrieve the sensor type for a given sensorId
        public string GetSensorType(string sensorId)
        {
            return _sensorTypes.ContainsKey(sensorId) ? _sensorTypes[sensorId] : "Unknown";
        }

        // Optional: A method to get all sensor types, if needed in the future
        public Dictionary<string, string> GetAllSensorTypes()
        {
            return new Dictionary<string, string>(_sensorTypes); // Return a copy to avoid external modifications
        }
    }
}
