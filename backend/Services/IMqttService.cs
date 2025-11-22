using AirplaneSensorsMonitor.Model;

namespace AirplaneSensorsMonitor.Services
{
    public interface IMqttService
    {
        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="sensorId"></param>
        /// <param name="sensorType"></param>
        /// <param name="sortValueDescending"></param>
        /// <param name="sortTimestampDescending"></param>
        /// <returns></returns>
        public IEnumerable<SensorData> GetMessages(int? sensorId = null, string? sensorType = null, bool sortValueDescending = false, bool sortTimestampDescending = true);

        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="sensorId"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public IEnumerable<SensorData> GetLastMessagesBySensor(int sensorId, int count);
    }
}