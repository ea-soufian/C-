using System;
using System.Globalization;

namespace PiBackend.Strategies
{
    public class BinaryTemperatureSensorStrategy : ISensorDataStrategy
    {
        public double ConvertData(string binaryData)
        {
            try
            {
                // Controleer of de binaire data geldig is (alleen nullen en enen)
                if (!IsBinary(binaryData))
                {
                    Console.WriteLine($"Invalid binary data: {binaryData}");
                    return double.NaN;
                }

                // Veronderstel dat de binaire data een 16-bit integer voorstelt
                int value = Convert.ToInt32(binaryData, 2);
                double temperature = value / 100.0; // Pas de juiste schaalfactor toe

                // Controleer of de temperatuur binnen een realistisch bereik valt
                if (temperature >= -10 && temperature <= 50) // Pas aan indien nodig
                {
                    return temperature;
                }
                else
                {
                    Console.WriteLine($"Binary temperature value out of realistic range: {temperature} in received binary data: {binaryData}");
                    return double.NaN;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error converting binary data: {ex.Message}");
                return double.NaN;
            }
        }

        // Controleer of de string alleen binaire waarden bevat
        private bool IsBinary(string data)
        {
            return !string.IsNullOrWhiteSpace(data) && data.Trim().All(c => c == '0' || c == '1');
        }
    }
}
