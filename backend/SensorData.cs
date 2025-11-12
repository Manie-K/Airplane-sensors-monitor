namespace AirplaneSensorsMonitor
{
    public class SensorData
    {
        public required string SensorType { get; set; }
        public required string SensorId { get; set; }
        public double Value { get; set; }
        public DateTime Timestamp { get; set; }
    }

}
