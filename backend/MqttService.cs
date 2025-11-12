using Microsoft.AspNetCore.SignalR;
using MQTTnet;
using MQTTnet.Client;
using System.Collections.Concurrent;
using System.Globalization;
using System.Text;

namespace AirplaneSensorsMonitor.Services
{
    public class MqttService : BackgroundService
    {
        private readonly ILogger<MqttService> _logger;
        private readonly ConcurrentQueue<SensorData> _messages = new();
        private IMqttClient? _mqttClient;
        private readonly IHubContext<SensorDataHub> _hubContext;

        public MqttService(ILogger<MqttService> logger, IHubContext<SensorDataHub> hubContext)
        {
            _logger = logger;
            _hubContext = hubContext;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var factory = new MqttFactory();
            _mqttClient = factory.CreateMqttClient();

            var options = new MqttClientOptionsBuilder()
                //TODO: When dockerizing, change "localhost" to the MQTT broker container name
                .WithClientId("razor-client")
                .WithTcpServer("localhost", 1883)
                .Build();

            _mqttClient.ApplicationMessageReceivedAsync += async e =>
            {
                var topic = e.ApplicationMessage.Topic.Trim();
                var payloadString = Encoding.UTF8.GetString(e.ApplicationMessage.PayloadSegment);

                var parts = topic.Split('/');;

                if (parts.Length == 3 && double.TryParse(payloadString, NumberStyles.Any, CultureInfo.InvariantCulture, out double value))
                {
                    var sensorMessage = new SensorData
                    {
                        SensorType = parts[1],
                        SensorId = parts[2],
                        Value = value,
                        Timestamp = DateTime.UtcNow
                    };

                    _messages.Enqueue(sensorMessage);
                    _logger.LogInformation($"Received {sensorMessage.SensorType} ({sensorMessage.SensorId}) = {sensorMessage.Value}");

                    await _hubContext.Clients.All.SendAsync("ReceiveSensorData", sensorMessage);
                }
                else
                {
                    _logger.LogWarning($"Invalid message or topic: {topic} -> {payloadString}");
                }
                await Task.CompletedTask;
            };

            _mqttClient.DisconnectedAsync += async e =>
            {
                _logger.LogWarning("MQTT disconnected. Reconnecting...");
                await Task.Delay(TimeSpan.FromSeconds(5));
                try 
                { 
                    await _mqttClient.ConnectAsync(options, CancellationToken.None); 
                }
                catch 
                { 
                    _logger.LogError("Reconnect failed"); 
                }
            };

            await _mqttClient.ConnectAsync(options, CancellationToken.None);
            //TODO: choose MQTT topics and quality of service level
            await _mqttClient.SubscribeAsync("sensors/#", MQTTnet.Protocol.MqttQualityOfServiceLevel.AtMostOnce);

            _logger.LogInformation("Subscribed to sensors/#");
        }

        public List<SensorData> GetMessages(string? sensorType = null, string? sensorId = null, bool sortValueDescending = false, bool sortTimestampDescending = true)
        {
            var list = _messages.Reverse().ToList();

            if (!string.IsNullOrEmpty(sensorType))
                list = list.Where(m => m.SensorType == sensorType).ToList();

            if (!string.IsNullOrEmpty(sensorId))
                list = list.Where(m => m.SensorId == sensorId).ToList();

            list = sortValueDescending
                ? list.OrderByDescending(m => m.Value).ToList()
                : list.OrderBy(m => m.Value).ToList();

            list = sortTimestampDescending
                ? list.OrderByDescending(m => m.Value).ToList()
                : list.OrderBy(m => m.Value).ToList();

            return list;
        }

        public List<SensorData> GetLastMessagesBySensor(string sensorId, int count)
        {
            var lastMessages = _messages
                .Reverse()                       
                .Where(m => m.SensorId == sensorId)
                .Take(count)                     
                .ToList();

            return lastMessages;
        }

    }

}
