using AirplaneSensorsMonitor.Model;

namespace AirplaneSensorsMonitor.Services
{
    public interface IDataService
    {
        /// <summary>
        /// Saves the provided sensor data.
        /// </summary>
        /// <param name="data">Data to be saved</param>
        public Task SaveDataAsync(SensorData data);

        /// <summary>
        /// Retrieves all sensors data.
        /// </summary>
        /// <param name="sensorId">Optional sensor ID to filter by</param>
        /// <param name="sensorType">Optional sensor type to filter by</param>
        /// <param name="sortValueDescending">Sort by value in descending order</param>
        /// <param name="sortTimestampDescending">Sort by timestamp in descending order</param>
        /// <returns>All sensor data saved in database taking filers into account</returns>
        public IEnumerable<SensorData> GetAllData(int? sensorId = null, string? sensorType = null, bool sortValueDescending = false, bool sortTimestampDescending = true);

        /// <summary>
        /// Retrieves sensor data for a specific sensor ID limited to a certain count.
        /// </summary>
        /// <param name="sensorId">ID of the sensor</param>
        /// <param name="count">Number of data rows to retrieve</param>
        /// <returns>Number of last sensor data saved in database from given sensor</returns>
        public IEnumerable<SensorData> GetSensorData(int sensorId, int count);
    }
}
