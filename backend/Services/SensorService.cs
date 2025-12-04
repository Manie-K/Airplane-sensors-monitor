using System.Globalization;
using System.Text;
using System.Text.Json;
using AirplaneSensorsMonitor.Model;

namespace AirplaneSensorsMonitor.Services
{
    public class SensorService : ISensorService
    {
        private readonly IDataService _dataService;

        public SensorService(IDataService dataService)
        {
            _dataService = dataService;
        }

        ///<inheritdoc/>
        public IEnumerable<SensorData> GetSensorData(int? sensorId = null, string? sensorType = null, bool? sortValueDescending = null, bool? sortTimestampDescending = null, DateTime? startDate = null, DateTime? endDate = null)
        {
            return _dataService.GetAllData(sensorId, sensorType, sortValueDescending, sortTimestampDescending, startDate, endDate);
        }

        ///<inheritdoc/>
        public string ExportSensorData(string format, int? sensorId = null, string? sensorType = null, bool? sortValueDescending = null, bool? sortTimestampDescending = null, DateTime? startDate = null, DateTime? endDate = null)
        {
            var data = _dataService.GetAllData(sensorId, sensorType, sortValueDescending, sortTimestampDescending, startDate, endDate);

            return format.ToLower() switch
            {
                "csv" => ExportToCsv(data),
                "json" => ExportToJson(data),
                _ => throw new ArgumentException("Nieobsługiwany format. Dostępne: csv, json")
            };
        }

        ///<inheritdoc/>
        public IEnumerable<SensorSummary> GetSensorSummaries(int? sensorId = null, string? sensorType = null, bool? sortValueDescending = null, bool? sortTimestampDescending = null, DateTime? startDate = null, DateTime? endDate = null, int rowsPerSensorCount = 100)
        {
            var allData = _dataService.GetAllData(sensorId, sensorType, sortValueDescending, sortTimestampDescending, startDate, endDate);

            var summaries = allData
                .GroupBy(m => m.SensorId)
                .Select(g =>
                {
                    var lastMessages = g.OrderByDescending(m => m.Timestamp)
                                        .Take(rowsPerSensorCount)
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

        public IEnumerable<SensorData> GetAvailableSensors()
        {
            var allData = _dataService.GetAllData();

            return allData
                .GroupBy(s => s.SensorId)
                .Select(g => g.OrderByDescending(x => x.Timestamp).First());
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
