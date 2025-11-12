using AirplaneSensorsMonitor.Services;
using System.Globalization;
using System.Text;
using System.Text.Json;

namespace AirplaneSensorsMonitor
{
    public class SensorDataService
    {
        private readonly MqttService _mqttService;

        public SensorDataService(MqttService mqttService)
        {
            _mqttService = mqttService;
        }

        public List<SensorData> GetSensorData(string? sensorType = null, string? sensorId = null, bool sortValueDescending = false, bool sortTimestampDescending = true)
        {
            return _mqttService.GetMessages(sensorType = null, sensorId = null, sortValueDescending, sortTimestampDescending);
        }

        public string ExportSensorData(string format, string? sensorType = null, string? sensorId = null, bool sortValueDescending = false, bool sortTimestampDescending = true)
        {
            var messages = _mqttService.GetMessages(sensorType, sensorId, sortValueDescending, sortTimestampDescending);

            return format.ToLower() switch
            {
                "csv" => ExportToCsv(messages),
                "json" => ExportToJson(messages),
                _ => throw new ArgumentException("Nieobsługiwany format. Dostępne: csv, json")
            };
        }

        private string ExportToCsv(List<SensorData> messages)
        {
            var sb = new StringBuilder();
            var separator = ";";

            sb.AppendLine($"SensorId{separator}SensorType{separator}Value{separator}Timestamp");

            foreach (var m in messages)
            {
                string sensorId = EscapeCsv(m.SensorId);
                string sensorType = EscapeCsv(m.SensorType);
                string value = m.Value.ToString(CultureInfo.InvariantCulture);
                string timestamp = m.Timestamp.ToString("dd-MM-yyyy HH:mm:ss");

                sb.AppendLine($"{sensorId}{separator}{sensorType}{separator}{value}{separator}{timestamp}");
            }

            return sb.ToString();
        }

        private string ExportToJson(List<SensorData> messages)
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            return JsonSerializer.Serialize(messages, options);
        }

        private static string EscapeCsv(string input)
        {
            if (input.Contains(';') || input.Contains('"') || input.Contains('\n'))
            {
                input = input.Replace("\"", "\"\"");
                return $"\"{input}\"";
            }
            return input;
        }

        public List<SensorSummary> GetSensorSummaries(int lastMessagesPerSensor = 100)
        {
            var allMessages = _mqttService.GetMessages();

            var summaries = allMessages
                .GroupBy(m => m.SensorId)
                .Select(g =>
                {
                    var lastMessages = g.OrderByDescending(m => m.Timestamp)
                                        .Take(lastMessagesPerSensor)
                                        .ToList();

                    return new SensorSummary
                    {
                        SensorId = g.Key,
                        SensorType = lastMessages.First().SensorType, 
                        LastValue = lastMessages.First().Value,
                        AverageValue = lastMessages.Average(m => m.Value)
                    };
                })
                .ToList();

            return summaries;
        }

    }

}
