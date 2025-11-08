using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MQTTnet;
using MQTTnet.Client;
using System.Collections.Concurrent;

namespace AirplaneSensorsMonitor.Services
{
    public class MqttBackgroundService : BackgroundService
    {
        private readonly ILogger<MqttBackgroundService> _logger;
        private readonly ConcurrentQueue<string> _messages = new();
        private IMqttClient? _mqttClient;

        public MqttBackgroundService(ILogger<MqttBackgroundService> logger)
        {
            _logger = logger;
        }

        public ConcurrentQueue<string> Messages => _messages;

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
                var message = Encoding.UTF8.GetString(e.ApplicationMessage.PayloadSegment);
                _logger.LogInformation($"MQTT message received: {message}");
                _messages.Enqueue(message);
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

            await _mqttClient.ConnectAsync(options, stoppingToken);
            //TODO: choose MQTT topics and quality of service level
            await _mqttClient.SubscribeAsync("sensors/#", MQTTnet.Protocol.MqttQualityOfServiceLevel.AtMostOnce);

            _logger.LogInformation("Subscribed to sensors/#");

            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(1000, stoppingToken);
            }
        }

    }
}
