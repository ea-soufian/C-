using Microsoft.AspNetCore.SignalR;

namespace PiBackend.Hubs
{
    public class SensorHub : Hub
    {
        // Deze hub wordt gebruikt om data via SignalR naar de frontend te sturen
        public async Task SendSensorData(string sensorType, double value)
        {
            // Verstuur data naar alle verbonden clients
            await Clients.All.SendAsync("ReceiveSensorData", new
            {
                type = sensorType,
                value = value,
                timestamp = DateTime.UtcNow
            });
        }
    }
}
