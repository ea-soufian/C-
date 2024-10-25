using System;

namespace PiBackend.Observers
{
    public class ConsoleLoggerObserver : ISerialDataObserver
    {
        public void Update(double data)
        {
            Console.WriteLine($"Received data: {data}");
        }
    }
}
