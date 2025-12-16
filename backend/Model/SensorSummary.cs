namespace AirplaneSensorsMonitor.Model
{
    public class SensorSummary
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
        /// Last value produced by the sensor
        /// </summary>
        public double LastValue { get; set; }

        /// <summary>
        /// Average value produced by the sensor
        /// </summary>
        public double AverageValue { get; set; }
    }
}
