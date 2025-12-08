using Nethereum.Hex.HexTypes;
using Nethereum.Signer;
using Nethereum.Util;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;
using System.Numerics;
using System.Text;

namespace AirplaneSensorsMonitor.Services
{
    public class SensorTokenService : ISensorTokenService
    {
        private const string BALANCE_OF_FUNCTION = "balanceOf";
        private const string MINT_FUNCTION = "mint";

        private readonly string _rpcUrl;
        private readonly string _privateKey;
        private string _contractAddress;
        private readonly string _abi;
        private readonly string _byteCode;
        private readonly Web3 _web3;
        private readonly Dictionary<int, string> _wallets;

        public SensorTokenService(IConfiguration configuration)
        {
            _rpcUrl = configuration.GetValue<string>("Blockchain:RpcUrl")
                ?? throw new ArgumentNullException("RPC URL is not configured.");
            _privateKey = configuration.GetValue<string>("Blockchain:PrivateKey")
                ?? throw new ArgumentNullException("Private key is not configured.");

            _abi = File.Exists(configuration.GetValue<string>("Blockchain:PathToAbi")!)
                ? File.ReadAllText(configuration.GetValue<string>("Blockchain:PathToAbi")!)
                : throw new FileNotFoundException("Contract ABI file not found.");

            _byteCode = File.Exists(configuration.GetValue<string>("Blockchain:PathToBin")!)
                ? File.ReadAllText(configuration.GetValue<string>("Blockchain:PathToBin")!)
                : throw new FileNotFoundException("Contract BIN file not found.");

            _wallets = new Dictionary<int, string>(16);

            var account = new Account(_privateKey, new BigInteger(1337));
            _web3 = new Web3(account, _rpcUrl);

            // Automatyczny deploy kontraktu
            _contractAddress = DeployContractAsync().GetAwaiter().GetResult();
        }

        private async Task<string> DeployContractAsync()
        {
            Console.WriteLine("[INFO] Deploying SensorToken contract...");
            var receipt = await _web3.Eth.DeployContract.SendRequestAndWaitForReceiptAsync(
                _abi,
                _byteCode,
                _web3.TransactionManager.Account.Address,
                new HexBigInteger(6000000)
            );

            Console.WriteLine($"[INFO] Contract deployed at: {receipt.ContractAddress}");
            return receipt.ContractAddress;
        }

        public async Task<decimal> GetBalanceAsync(int sensorId)
        {
            var address = GetSensorAddress(sensorId);
            try
            {
                var contract = _web3.Eth.GetContract(_abi, _contractAddress);
                var balanceFunction = contract.GetFunction(BALANCE_OF_FUNCTION);

                BigInteger balanceWei = await balanceFunction.CallAsync<BigInteger>(address);
                decimal balance = Web3.Convert.FromWei(balanceWei);

                Console.WriteLine($"[INFO] Balance for sensor {sensorId} ({address}) is {balance} tokens.");
                return balance;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Failed to get balance for {sensorId}: {ex.Message}");
                return 0;
            }
        }

        public async Task RewardSensorAsync(int sensorId, decimal amount)
        {
            var address = GetSensorAddress(sensorId);
            var amountWei = Web3.Convert.ToWei(amount);

            try
            {
                var contract = _web3.Eth.GetContract(_abi, _contractAddress);
                var rewardFunction = contract.GetFunction(MINT_FUNCTION);

                var receipt = await rewardFunction.SendTransactionAndWaitForReceiptAsync(
                    from: _web3.TransactionManager.Account.Address,
                    gas: new HexBigInteger(200000),
                    value: new HexBigInteger(0),
                    functionInput: new object[] { address, amountWei }
                );

                Console.WriteLine($"[INFO] Rewarded sensor {sensorId} ({address}) with {amount} tokens. Txn Status: {receipt.Status.Value}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Failed to reward sensor {sensorId}: {ex.Message}");
            }
        }

        public string GetSensorAddress(int sensorId)
        {
            if (_wallets.TryGetValue(sensorId, out string? address))
                return address!;

            var privateKeyHex = new Sha3Keccack().CalculateHash($"Sensor-{sensorId}");
            var privateKeyBytes = Enumerable.Range(0, privateKeyHex.Length)
                .Where(x => x % 2 == 0)
                .Select(x => Convert.ToByte(privateKeyHex.Substring(x, 2), 16))
                .ToArray();
            var ecKey = new EthECKey(privateKeyBytes, true);
            string newAddr = ecKey.GetPublicAddress();
            _wallets[sensorId] = newAddr;

            return newAddr;
        }
    }
}
