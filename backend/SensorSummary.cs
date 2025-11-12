namespace AirplaneSensorsMonitor
{
    public class SensorSummary
    {
        public required string SensorId { get; set; }
        public required string SensorType { get; set; }
        public double LastValue { get; set; }
        public double AverageValue { get; set; }
    }
}
