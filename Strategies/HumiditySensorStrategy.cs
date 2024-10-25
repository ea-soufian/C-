using System;
using System.Globalization;

namespace PiBackend.Strategies
{
    public class HumiditySensorStrategy : ISensorDataStrategy
    {
        public double ConvertData(string data)
        {
            // Verwijder onnodige spaties of nieuwe regeltekens
            data = data.Trim();

            // Controleer of de string überhaupt een "hum:" veld bevat voordat we verder gaan
            if (!data.Contains("hum:"))
            {
                Console.WriteLine($"[ERROR] No 'hum:' field found in data string: {data}");
                return double.NaN;
            }

            // Ontleed de gestructureerde data om het vochtigheidsveld te vinden
            string humidityValue = ParseHumidityValue(data);

            // Controleer of het vochtigheidsveld niet leeg is
            if (string.IsNullOrEmpty(humidityValue))
            {
                Console.WriteLine($"Invalid humidity data: {humidityValue} in received string: {data}");
                return double.NaN; // Ongeldige data, retourneer NaN
            }

            // Gebruik InvariantCulture om string naar double om te zetten
            if (double.TryParse(humidityValue, NumberStyles.Any, CultureInfo.InvariantCulture, out double result))
            {
                // Deel door 10 om de werkelijke vochtigheid te krijgen (bijv. 504 wordt 50.4 %)
                double humidity = result / 10.0;

                // Controleer of de vochtigheid binnen een realistisch bereik valt (bijv. 0 tot 100 %)
                if (humidity < 0 || humidity > 100)
                {
                    Console.WriteLine($"Humidity value out of range: {humidity} in received string: {data}");
                    return double.NaN; // Ongeldige vochtigheidswaarde
                }

                return humidity; // Retourneer de vochtigheidswaarde
            }
            else
            {
                Console.WriteLine($"Invalid humidity data format: {humidityValue} in received string: {data}");
                return double.NaN; // Ongeldige data, retourneer NaN
            }
        }

        // Verbeterde methode om de vochtigheidswaarde te extraheren uit de gestructureerde string
        private string ParseHumidityValue(string data)
        {
            // Controleer of de string een "hum:" veld bevat
            if (data.Contains("hum:"))
            {
                // Zoek naar "hum:" gevolgd door de vochtigheidswaarde en haal deze eruit
                var parts = data.Split(new[] { "hum:" }, StringSplitOptions.None);
                if (parts.Length > 1)
                {
                    var humPart = parts[1];

                    // Zoek naar de eerste niet-numerieke karakterpositie om de vochtigheidswaarde correct te extraheren
                    int index = 0;
                    while (index < humPart.Length && (char.IsDigit(humPart[index]) || humPart[index] == ',' || humPart[index] == '.'))
                    {
                        index++;
                    }

                    // Haal alleen de numerieke waarde eruit
                    var humValue = humPart.Substring(0, index);
                    return humValue.Replace(",", "."); // Vervang komma door punt om parsing problemen te voorkomen
                }
            }

            return string.Empty; // Geen "hum:" veld gevonden, retourneer lege string
        }
    }
}
