namespace AirplaneSensorsMonitor.Services
{
    public interface ISensorTokenService
    {
        /// <summary>
        /// Gets the token balance for the specified sensor.
        /// </summary>
        /// <param name="sensorId">ID of the sensor.</param>
        /// <returns>Balance amount.</returns>
        public Task<decimal> GetBalanceAsync(int sensorId);

        /// <summary>
        /// Rewards the sensor with the specified amount of tokens.
        /// </summary>
        /// <param name="sensorId">ID of sensor to reward.</param>
        /// <param name="amount">Amount to reward.</param>
        /// <returns></returns>
        public Task RewardSensorAsync(int sensorId, decimal amount);

        public string GetSensorAddress(int sensorId);
    }
}
