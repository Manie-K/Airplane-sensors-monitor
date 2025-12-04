using AirplaneSensorsMonitor.Model;
using Microsoft.AspNetCore.SignalR;
using MQTTnet;
using MQTTnet.Client;
using System.Globalization;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace AirplaneSensorsMonitor.Services
{
    public class MqttService : BackgroundService, IMqttService
    {
        private readonly ILogger<MqttService> _logger;
        private readonly IHubContext<SensorDataHub> _hubContext;
        private readonly IDataService dataService;
        private readonly ISensorService sensorService;
        private readonly ISensorTokenService sensorTokenService;
        private readonly string _mqttHost;
        private readonly int _mqttPort;
        private readonly string _topicFilter;
        private readonly string _clientId;
        private readonly TimeSpan _reconnectDelay;
        private readonly decimal _rewardAmount;

        private IMqttClient? _mqttClient;

        public MqttService(
            ILogger<MqttService> logger,
            IHubContext<SensorDataHub> hubContext,
            IDataService dataService,
            IConfiguration configuration,
            ISensorService sensorService,
            ISensorTokenService sensorTokenService)
        {
            _logger = logger;
            _hubContext = hubContext;
            
            this.dataService = dataService;
            this.sensorService = sensorService;
            this.sensorTokenService = sensorTokenService;

            _mqttHost = configuration.GetValue<string>("Mqtt:Host") ?? "localhost";
            _mqttPort = configuration.GetValue<int?>("Mqtt:Port") ?? 1883;
            _topicFilter = configuration.GetValue<string>("Mqtt:TopicFilter") ?? "sensors/#";
            _clientId = configuration.GetValue<string>("Mqtt:ClientId") ?? $"razor-client-{Environment.MachineName}";

            _rewardAmount = configuration.GetValue<decimal?>("Blockchain:TokenRewardAmount") ?? 5m;

            var reconnectSeconds = configuration.GetValue<int?>("Mqtt:ReconnectDelaySeconds") ?? 5;
            _reconnectDelay = TimeSpan.FromSeconds(Math.Max(1, reconnectSeconds));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _mqttClient = new MqttFactory().CreateMqttClient();

            var options = new MqttClientOptionsBuilder()
                .WithClientId(_clientId)
                .WithTcpServer(_mqttHost, _mqttPort)
                .Build();
            var topicFilter = new MqttTopicFilterBuilder()
                .WithTopic(_topicFilter)
                .WithAtMostOnceQoS()
                .Build();

            _mqttClient.ApplicationMessageReceivedAsync += async e =>
            {
                var topic = e.ApplicationMessage.Topic.Trim();
                var payloadString = Encoding.UTF8.GetString(e.ApplicationMessage.PayloadSegment);

                var parts = topic.Split('/');;

                if (parts.Length == 3 && double.TryParse(payloadString, NumberStyles.Any, CultureInfo.InvariantCulture, out double value))
                {
                    SensorData sensorMessage = new SensorData
                    {
                        SensorType = parts[1],
                        SensorId = int.Parse(parts[2]),
                        Value = value,
                        Timestamp = DateTime.UtcNow
                    };

                    await dataService.SaveDataAsync(sensorMessage);
                    await sensorTokenService.RewardSensorAsync(sensorMessage.SensorId, _rewardAmount);

                    _logger.LogInformation($"Received {sensorMessage.SensorType} ({sensorMessage.SensorId}) = {sensorMessage.Value}");

                    await _hubContext.Clients.All.SendAsync("ReceiveSensorData", sensorMessage);
                    await _hubContext.Clients.All.SendAsync("ReceiveSensorSummary", sensorService.GetSensorSummaries().ToList());
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
                await Task.Delay(_reconnectDelay, stoppingToken);
                try 
                { 
                    await _mqttClient.ConnectAsync(options, CancellationToken.None); 
                }
                catch 
                { 
                    _logger.LogError("Reconnect failed"); 
                }
            };

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await _mqttClient.ConnectAsync(options, stoppingToken);
                    await _mqttClient.SubscribeAsync(topicFilter, stoppingToken);
                    _logger.LogInformation("Subscribed to {Topic}", _topicFilter);
                    break;
                }
                catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
                {
                    _logger.LogError(ex, "Unable to connect to MQTT at {Host}:{Port}. Retrying in {Delay}s", _mqttHost, _mqttPort, _reconnectDelay.TotalSeconds);
                    await Task.Delay(_reconnectDelay, stoppingToken);
                }
            }

            if (stoppingToken.IsCancellationRequested)
            {
                return;
            }

            _logger.LogInformation("MQTT client running with host {Host}:{Port}", _mqttHost, _mqttPort);
        }
    }

}
