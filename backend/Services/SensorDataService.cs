using System.Globalization;
using System.Text;
using System.Text.Json;
using AirplaneSensorsMonitor.Model;

namespace AirplaneSensorsMonitor.Services
{
    public class SensorDataService : ISensorDataService
    {
        private readonly IMqttService _mqttService;

        public SensorDataService(IMqttService mqttService)
        {
            _mqttService = mqttService;
        }

        ///<inheritdoc/>
        public IEnumerable<SensorData> GetSensorData(int? sensorId = null, string? sensorType = null, bool sortValueDescending = false, bool sortTimestampDescending = true)
        {
            return _mqttService.GetMessages(sensorId, sensorType, sortValueDescending, sortTimestampDescending);
        }

        ///<inheritdoc/>
        public string ExportSensorData(string format, int? sensorId = null, string? sensorType = null, bool sortValueDescending = false, bool sortTimestampDescending = true)
        {
            var messages = _mqttService.GetMessages(sensorId, sensorType, sortValueDescending, sortTimestampDescending);

            return format.ToLower() switch
            {
                "csv" => ExportToCsv(messages),
                "json" => ExportToJson(messages),
                _ => throw new ArgumentException("Nieobsługiwany format. Dostępne: csv, json")
            };
        }

        ///<inheritdoc/>
        public IEnumerable<SensorSummary> GetSensorSummaries(int messagesPerSensorCount = 100)
        {
            var allMessages = _mqttService.GetMessages();

            var summaries = allMessages
                .GroupBy(m => m.SensorId)
                .Select(g =>
                {
                    var lastMessages = g.OrderByDescending(m => m.Timestamp)
                                        .Take(messagesPerSensorCount)
                                        .ToList();

                    return new SensorSummary
                    {
                        SensorId = g.Key,
                        SensorType = lastMessages.First().SensorType,
                        LastValue = lastMessages.First().Value,
                        AverageValue = lastMessages.Average(m => m.Value)
                    };
                });

            return summaries;
        }


        private string ExportToCsv(IEnumerable<SensorData> messages)
        {
            var sb = new StringBuilder();
            var separator = ";";

            sb.AppendLine($"SensorId{separator}SensorType{separator}Value{separator}Timestamp");

            foreach (var m in messages)
            {
                string sensorId = EscapeCsv(m.SensorId.ToString());
                string sensorType = EscapeCsv(m.SensorType);
                string value = m.Value.ToString(CultureInfo.InvariantCulture);
                string timestamp = m.Timestamp.ToString("dd-MM-yyyy HH:mm:ss");

                sb.AppendLine($"{sensorId}{separator}{sensorType}{separator}{value}{separator}{timestamp}");
            }

            return sb.ToString();
        }

        private string ExportToJson(IEnumerable<SensorData> messages)
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            return JsonSerializer.Serialize(messages, options);
        }


        private string EscapeCsv(string input)
        {
            if (input.Contains(';') || input.Contains('"') || input.Contains('\n'))
            {
                input = input.Replace("\"", "\"\"");
                return $"\"{input}\"";
            }
            return input;
        }
    }
}
