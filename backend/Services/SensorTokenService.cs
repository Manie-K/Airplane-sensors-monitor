using System.Numerics;
using Nethereum.Hex.HexTypes;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;

namespace AirplaneSensorsMonitor.Services
{
    public class SensorTokenService : ISensorTokenService
    {
        private const string BALANCE_OF_FUNCTION = "balanceOf";
        private const string MINT_FUNCTION = "mint";

        private readonly string _rpcUrl;
        private readonly string _privateKey;
        private readonly string _contractAddress;
        private readonly string _abi;
        private readonly Web3 _web3;
        private readonly Dictionary<int, string> _wallets;

        public SensorTokenService(IConfiguration configuration)
        {
            _rpcUrl = configuration.GetValue<string>("Blockchain:RpcUrl") ?? throw new ArgumentNullException("RPC URL is not configured.");
            _privateKey = configuration.GetValue<string>("Blockchain:PrivateKey") ?? throw new ArgumentNullException("Private key is not configured.");
            _contractAddress = configuration.GetValue<string>("Blockchain:ContractAddress") ?? throw new ArgumentNullException("Contract address is not configured.");

            _abi = File.Exists("Blockchain:PathToAbi") ? File.ReadAllText("Blockchain:PathToAbi") : throw new FileNotFoundException("Contract ABI file not found.");
            _wallets = new Dictionary<int, string>(16);

            var account = new Account(_privateKey, new BigInteger(1337));
            _web3 = new Web3(account, _rpcUrl);
        }

        /// <inheritdoc/>
        public async Task<decimal> GetBalanceAsync(int sensorId)
        {
            var address = GetSensorAddress(sensorId);
            try
            {
                var contract = _web3.Eth.GetContract(_abi, _contractAddress);
                var balanceFunction = contract.GetFunction(BALANCE_OF_FUNCTION);
                var balanceWei = await balanceFunction.CallAsync<BigInteger>(address);
                return Web3.Convert.FromWei(balanceWei);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR]: Failed to get balance for {sensorId}: {ex.Message}");
                return 0;
            }
        }

        /// <inheritdoc/>
        public async Task RewardSensorAsync(int sensorId, decimal amount)
        {
            var address = GetSensorAddress(sensorId);
            var amountWei = Web3.Convert.ToWei(amount);

            try
            {
                var contract = _web3.Eth.GetContract(_abi, _contractAddress);
                var rewardFunction = contract.GetFunction(MINT_FUNCTION);

                var transactionInput = new Nethereum.RPC.Eth.DTOs.TransactionInput
                {
                    From = _web3.TransactionManager.Account.Address,
                    Gas = new HexBigInteger(100000),
                    GasPrice = new HexBigInteger(2000000000),
                    To = _contractAddress,
                    Value = new HexBigInteger(0),
                    Data = rewardFunction.GetData(address, amountWei)
                };

                var receipt = await _web3.TransactionManager.SendTransactionAsync(transactionInput);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR]: Failed to reward sensor {sensorId}, {ex.Message}!");
            }
        }

        /// <summary>
        /// Helper method to retrieve address of given sensor.
        /// </summary>
        /// <param name="sensorId">ID of the sensor</param>
        /// <returns>Adress (string)</returns>
        private string GetSensorAddress(int sensorId)
        {
            if (_wallets.TryGetValue(sensorId, out var address))
            {
                return address;
            }

            var ecKey = Nethereum.Signer.EthECKey.GenerateKey(System.Text.Encoding.UTF8.GetBytes($"Sensor-{sensorId}"));
            var newAddr = ecKey.GetPublicAddress();
            _wallets[sensorId] = newAddr;

            return newAddr;
        }
    }
}
