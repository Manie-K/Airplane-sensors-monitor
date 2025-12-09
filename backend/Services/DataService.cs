using AirplaneSensorsMonitor.Data;
using AirplaneSensorsMonitor.Model;
using MongoDB.Driver;
using SharpCompress.Common;

namespace AirplaneSensorsMonitor.Services
{
    public class DataService : IDataService
    {
        private readonly ILogger<DataService> _logger;
        private readonly IConfiguration _configuration;
        private readonly IMongoDatabase? _database;
        private readonly IMongoCollection<SensorDBEntity> _sensorsDataCollection;


        public DataService(IConfiguration configuration, ILogger<DataService> logger) 
        {
            _configuration = configuration;
            _logger = logger;
            string? connectionString = _configuration.GetConnectionString("MongoDb");
            MongoUrl mongoUrl = new MongoUrl(connectionString);
            MongoClient mongoClient = new MongoClient(mongoUrl);
            
            _database = mongoClient.GetDatabase(mongoUrl.DatabaseName);
            _sensorsDataCollection = _database.GetCollection<SensorDBEntity>("sensors_data");
        }

        ///<inheritdoc/>
        public IEnumerable<SensorData> GetAllData(int? sensorId = null, string? sensorType = null, bool? sortValueDescending = null, bool? sortTimestampDescending = null, DateTime ? startDate = null, DateTime? endDate = null)
        {
            var filterBuilder = Builders<SensorDBEntity>.Filter;
            var filters = new List<FilterDefinition<SensorDBEntity>>();

            var sortDefinitions = new List<SortDefinition<SensorDBEntity>>();

            if (sortValueDescending.HasValue)
            {
                sortDefinitions.Add(sortValueDescending.Value
                    ? Builders<SensorDBEntity>.Sort.Descending(x => x.Value)
                    : Builders<SensorDBEntity>.Sort.Ascending(x => x.Value));
            }

            if (sortTimestampDescending.HasValue)
            {
                sortDefinitions.Add(sortTimestampDescending.Value
                    ? Builders<SensorDBEntity>.Sort.Descending(x => x.Timestamp)
                    : Builders<SensorDBEntity>.Sort.Ascending(x => x.Timestamp));
            }

            if (!sortDefinitions.Any())
            {
                sortDefinitions.Add(Builders<SensorDBEntity>.Sort.Descending(x => x.Timestamp));
            }

            var sort = Builders<SensorDBEntity>.Sort.Combine(sortDefinitions);

            if (sensorId.HasValue)
                filters.Add(filterBuilder.Eq(x => x.SensorId, sensorId.Value));

            if (!string.IsNullOrWhiteSpace(sensorType))
                filters.Add(filterBuilder.Regex(x => x.SensorType, new MongoDB.Bson.BsonRegularExpression(System.Text.RegularExpressions.Regex.Escape(sensorType), "i")));

            if (startDate.HasValue)
                filters.Add(filterBuilder.Gte(x => x.Timestamp, startDate.Value));

            if (endDate.HasValue)
                filters.Add(filterBuilder.Lte(x => x.Timestamp, endDate.Value));

            var finalFilter = filters.Count > 0
                ? filterBuilder.And(filters)
                : filterBuilder.Empty;

            var results = _sensorsDataCollection
                .Find(finalFilter)
                .Sort(sort)
                .ToEnumerable()
                .Select(x => new SensorData
                {
                    SensorId = x.SensorId,
                    SensorType = x.SensorType ?? string.Empty,
                    Value = x.Value,
                    Timestamp = DateTime.SpecifyKind(x.Timestamp, DateTimeKind.Utc)
                });

            return results;
        }

        ///<inheritdoc/>
        public IEnumerable<SensorData> GetSensorData(int sensorId, int count)
        {
            var filter = Builders<SensorDBEntity>.Filter.Eq(x => x.SensorId, sensorId);
            var sort = Builders<SensorDBEntity>.Sort.Descending(x => x.Timestamp);

            var results = _sensorsDataCollection
                .Find(filter)
                .Sort(sort)
                .Limit(count)
                .ToEnumerable()
                .Select(x => new SensorData
                {
                    SensorId = x.SensorId,
                    SensorType = x.SensorType ?? string.Empty,
                    Value = x.Value,
                    Timestamp = DateTime.SpecifyKind(x.Timestamp, DateTimeKind.Utc)
                });

            return results;
        }

        ///<inheritdoc/>
        public async Task SaveDataAsync(SensorData data)
        {
            await _sensorsDataCollection.InsertOneAsync(new SensorDBEntity()
            {
                SensorId = data.SensorId,
                SensorType = data.SensorType,
                Value = data.Value,
                Timestamp = data.Timestamp
            });
        }
    }
}
