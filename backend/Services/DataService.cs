using AirplaneSensorsMonitor.Data;
using AirplaneSensorsMonitor.Model;
using MongoDB.Driver;

namespace AirplaneSensorsMonitor.Services
{
    public class DataService : IDataService
    {
        private readonly IConfiguration _configuration;
        private readonly IMongoDatabase? _database;
        private readonly IMongoCollection<SensorDBEntity> _sensorsDataCollection;


        public DataService(IConfiguration configuration) 
        {
            _configuration = configuration;

            string? connectionString = _configuration.GetConnectionString("MongoDb");
            MongoUrl mongoUrl = new MongoUrl(connectionString);
            MongoClient mongoClient = new MongoClient(mongoUrl);
            
            _database = mongoClient.GetDatabase(mongoUrl.DatabaseName);
            _sensorsDataCollection = _database.GetCollection<SensorDBEntity>("sensors_data");
        }

        ///<inheritdoc/>
        public IEnumerable<SensorData> GetAllData(int? sensorId = null, string? sensorType = null, bool sortValueDescending = false, bool sortTimestampDescending = true)
        {
            var filterBuilder = Builders<SensorDBEntity>.Filter;
            var filters = new List<FilterDefinition<SensorDBEntity>>();
            SortDefinition<SensorDBEntity> sort;

            if (sensorId.HasValue)
                filters.Add(filterBuilder.Eq(x => x.SensorId, sensorId.Value));

            if (!string.IsNullOrWhiteSpace(sensorType))
                filters.Add(filterBuilder.Eq(x => x.SensorType, sensorType));

            var finalFilter = filters.Count > 0
                ? filterBuilder.And(filters)
                : filterBuilder.Empty;


            if (sortValueDescending)
                sort = Builders<SensorDBEntity>.Sort.Descending(x => x.Value);
            else
                sort = Builders<SensorDBEntity>.Sort.Ascending(x => x.Value);

            if (sortTimestampDescending)
                sort = sort.Descending(x => x.Timestamp);
            else
                sort = sort.Ascending(x => x.Timestamp);

            var results = _sensorsDataCollection
                .Find(finalFilter)
                .Sort(sort)
                .ToEnumerable()
                .Select(x => new SensorData
                {
                    SensorId = x.SensorId,
                    SensorType = x.SensorType ?? string.Empty,
                    Value = x.Value,
                    Timestamp = x.Timestamp
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
                    Timestamp = x.Timestamp
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
