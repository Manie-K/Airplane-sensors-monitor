namespace AirplaneSensorsMonitor.Model
{
    public class SensorData
    {
        /// <summary>
        /// ID of the sensor
        /// </summary>
        public required int SensorId { get; set; }

        /// <summary>
        /// Type of the sensor
        /// </summary>
        public required string SensorType { get; set; }

        /// <summary>
        /// Value produced by the sensor
        /// </summary>
        public double Value { get; set; }

        /// <summary>
        /// Date and time when the value was recorded
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Sensor balance
        /// </summary>
        public decimal? Balance { get; set; }
    }
}
