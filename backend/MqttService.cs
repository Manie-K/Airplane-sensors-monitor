using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MQTTnet;
using MQTTnet.Client;
using System.Collections.Concurrent;
using System.Globalization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AirplaneSensorsMonitor.Services
{
    public class MqttService
    {
        private readonly ILogger<MqttService> _logger;
        private readonly ConcurrentQueue<SensorMessage> _messages = new();
        private IMqttClient? _mqttClient;

        public MqttService(ILogger<MqttService> logger)
        {
            _logger = logger;
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
                    var sensorMessage = new SensorMessage
                    {
                        SensorType = parts[1],
                        SensorId = parts[2],
                        Value = value,
                        Timestamp = DateTime.UtcNow
                    };

                    _messages.Enqueue(sensorMessage);
                    _logger.LogInformation($"Received {sensorMessage.SensorType} ({sensorMessage.SensorId}) = {sensorMessage.Value}");
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

            _mqttClient.ConnectAsync(options, CancellationToken.None).Wait();
            //TODO: choose MQTT topics and quality of service level
            _mqttClient.SubscribeAsync("sensors/#", MQTTnet.Protocol.MqttQualityOfServiceLevel.AtMostOnce).Wait();

            _logger.LogInformation("Subscribed to sensors/#");
        }

        public List<SensorMessage> GetMessages(string? sensorType = null, string? sensorId = null, bool sortDescending = false)
        {
            var list = _messages.ToList();

            if (!string.IsNullOrEmpty(sensorType))
                list = list.Where(m => m.SensorType == sensorType).ToList();

            if (!string.IsNullOrEmpty(sensorId))
                list = list.Where(m => m.SensorId == sensorId).ToList();

            list = sortDescending
                ? list.OrderByDescending(m => m.Value).ToList()
                : list.OrderBy(m => m.Value).ToList();

            return list;
        }

    }
}
