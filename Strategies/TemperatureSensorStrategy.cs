using System;
using System.Globalization;

namespace PiBackend.Strategies
{
    public class TemperatureSensorStrategy : ISensorDataStrategy
    {
        public double ConvertData(string data)
        {
            Console.WriteLine($"Received raw data: {data}"); // Log de ruwe data voor analyse

            // Verwijder onnodige spaties of nieuwe regeltekens
            data = data.Trim();

            // Controleer of de string binaire data bevat
            if (IsBinary(data))
            {
                Console.WriteLine("Interpreting as binary data.");
                // Verwerk de binaire data naar een decimale temperatuurwaarde
                return ConvertBinaryDataToTemperature(data);
            }

            // Controleer of de string gestructureerde data bevat
            if (data.Contains("temp:"))
            {
                Console.WriteLine("Interpreting as structured temperature data.");
                // Verwerk de gestructureerde data
                string temperatureValue = ParseTemperatureValue(data);

                // Controleer of het temperatuurveld niet leeg is
                if (string.IsNullOrEmpty(temperatureValue))
                {
                    Console.WriteLine($"Invalid temperature data: {temperatureValue} in received string: {data}");
                    return double.NaN; // Ongeldige data, retourneer NaN
                }

                // Gebruik InvariantCulture om string naar double om te zetten
                if (double.TryParse(temperatureValue, NumberStyles.Any, CultureInfo.InvariantCulture, out double result))
                {
                    Console.WriteLine($"Parsed temperature value: {result}");
                    // Bepaal de juiste schaalfactor afhankelijk van de lengte van de data
                    double temperature = ApplyScalingFactor(result);

                    // Controleer op een realistisch temperatuurbereik
                    if (temperature < -10 || temperature > 50)
                    {
                        Console.WriteLine($"Temperature value out of realistic range: {temperature} in received string: {data}");
                        return double.NaN;
                    }

                    return Math.Round(temperature, 2); // Houd 2 decimalen aan
                }
                else
                {
                    Console.WriteLine($"Invalid temperature data format: {temperatureValue} in received string: {data}");
                    return double.NaN;
                }
            }

            // Als de data niet binaire data is of geen "temp:" veld bevat, check voor andere bekende velden
            if (data.Contains("hum:"))
            {
                Console.WriteLine($"Identified humidity data, ignoring for now: {data}");
            }
            else
            {
                Console.WriteLine($"Unknown data format: {data}");
            }

            return double.NaN;
        }

        // Helper methode om binaire data om te zetten naar een temperatuurwaarde
        private double ConvertBinaryDataToTemperature(string binaryData)
        {
            try
            {
                Console.WriteLine($"Processing binary data: {binaryData}");

                // Controleer of de binaire data geldig is (alleen nullen en enen)
                if (!IsBinary(binaryData))
                {
                    Console.WriteLine($"Invalid binary data: {binaryData}");
                    return double.NaN;
                }

                // Veronderstel dat de binaire data een 16-bit integer voorstelt
                int value = Convert.ToInt32(binaryData, 2);
                double temperature = ApplyScalingFactor(value); // Pas de juiste schaalfactor toe

                // Controleer of de temperatuur binnen een realistisch bereik valt
                if (temperature >= -10 && temperature <= 50) // Pas aan indien nodig
                {
                    Console.WriteLine($"Converted binary temperature: {temperature}");
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

        // Methode om gestructureerde temperatuurdata te extraheren
        private string ParseTemperatureValue(string data)
        {
            // Zoek naar "temp:" gevolgd door de temperatuurwaarde en haal deze eruit
            var parts = data.Split(new[] { "temp:" }, StringSplitOptions.None);
            if (parts.Length > 1)
            {
                var tempPart = parts[1];
                var tempValue = tempPart.Split(new[] { " ", "bat", "state", "manufac", "type" }, StringSplitOptions.None)[0];

                if (!string.IsNullOrWhiteSpace(tempValue) && IsNumeric(tempValue))
                {
                    return tempValue.Replace(",", "."); // Vervang komma door punt om parsing problemen te voorkomen
                }
            }
            return string.Empty; // Geen geldige temperatuurwaarde gevonden, retourneer lege string
        }

        // Toepassen van de juiste schaalfactor op basis van de lengte van de data
        private double ApplyScalingFactor(double value)
        {
            // Voeg logging toe om te controleren welke schaalfactor wordt toegepast
            Console.WriteLine($"Applying scaling factor to value: {value}");
            // Pas een schaalfactor toe op basis van de waarde
            if (value > 1000)
            {
                return value / 100.0; // Verdeling door 100 voor grotere waarden
            }
            else
            {
                return value / 10.0; // Verdeling door 10 voor kleinere waarden
            }
        }

        // Controleer of een string numeriek is
        private bool IsNumeric(string value)
        {
            return double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out _);
        }

        // Controleer of de string alleen binaire waarden bevat
        private bool IsBinary(string data)
        {
            return !string.IsNullOrWhiteSpace(data) && data.Trim().All(c => c == '0' || c == '1');
        }
    }
}
