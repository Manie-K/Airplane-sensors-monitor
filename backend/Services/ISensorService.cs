using AirplaneSensorsMonitor.Model;

namespace AirplaneSensorsMonitor.Services
{
    public interface ISensorService
    {
        /// <summary>
        /// Fetches sensor data with optional filtering and sorting.
        /// </summary>
        /// <param name="sensorId"></param>
        /// <param name="sensorType"></param>
        /// <param name="sortValueDescending"></param>
        /// <param name="sortTimestampDescending"></param>
        /// <returns></returns>
        public IEnumerable<SensorData> GetSensorData(int? sensorId = null, string ? sensorType = null, bool? sortValueDescending = null, bool? sortTimestampDescending = null, DateTime? startDate = null, DateTime? endDate = null);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="format"></param>
        /// <param name="sensorId"></param>
        /// <param name="sensorType"></param>
        /// <param name="sortValueDescending"></param>
        /// <param name="sortTimestampDescending"></param>
        /// <returns></returns>
        public string ExportSensorData(string format, int? sensorId = null, string? sensorType = null, bool? sortValueDescending = null, bool? sortTimestampDescending = null, DateTime? startDate = null, DateTime? endDate = null);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rowsPerSensorCount"></param>
        /// <returns></returns>
        public IEnumerable<SensorSummary> GetSensorSummaries(int? sensorId = null, string? sensorType = null, bool? sortValueDescending = null, bool? sortTimestampDescending = null, DateTime? startDate = null, DateTime? endDate = null, int rowsPerSensorCount = 100);
    }
}