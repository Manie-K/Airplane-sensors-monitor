using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace AirplaneSensorsMonitor.Data
{
    public class SensorDBEntity
    {
        [BsonId]
        [BsonElement("_id"), BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("sensor_id"), BsonRepresentation(BsonType.Int32)]
        public int SensorId { get; set; }

        [BsonElement("sensor_type"), BsonRepresentation(BsonType.String)]
        public string? SensorType { get; set; }

        [BsonElement("value"), BsonRepresentation(BsonType.Double)]
        public double Value { get; set; }

        [BsonElement("timestamp"), BsonRepresentation(BsonType.DateTime)]
        public DateTime Timestamp { get; set; }
    }
}