using Google.Protobuf;
using Microsoft.Extensions.DependencyInjection;
using NBitcoin;
using System.Security.Cryptography;
using TronNet;
using TronNet;
using TronNet.Protocol;
using Transaction = TronNet.Protocol.Transaction;

public class Transactions
{
    private   static ITransactionClient _transactionClient;

    public Transactions(ITransactionClient transactionClient)
    {
        _transactionClient = transactionClient;
    }
    private static ITronClient _tronClient;
    

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddTronNet(options =>
        {
            options.Network = TronNetwork.MainNet;
          //  options.BaseUrl = "https://api.trongrid.io";
          //  options.PrivateKey = "你的私钥"; // 如果需要签名交易
        });
    }
    // 推荐用依赖注入传入 ITronClient
    public Transactions(ITronClient tronClient)
    {
        _tronClient = tronClient;
    }

    /*
     var transaction = await Transactions.CreateTransactionAsync("TTTTTX8kc1f12HexiiEWabE8u5fjhC62TT", "TTTC6FqoMWFwzk7mVrwwfBcUB1mDhYdTTT", 0.000001M, true);
                                               
                                                var signedTransaction = Transactions.SignTransaction(transaction!, Program._tronPrivateKey);
                                                var broadcast = await Transactions.BroadcastTransactionAsync(signedTransaction);
   
        var broadcast = await Transactions.BroadcastTransactionAsync(signedTransaction);
                                              
     */
    public static async Task<string> BroadcastTransactionAsync(object signedTransaction)
    {
        if (signedTransaction is Transaction transaction)
        {
            var result = await _transactionClient.BroadcastTransactionAsync(transaction);

            if (result.Result)
            {
                // 计算交易 ID (SHA256 哈希)
                using var sha256 = SHA256.Create();
                var txIdBytes = sha256.ComputeHash(transaction.RawData.ToByteArray());
                var txId = BitConverter.ToString(txIdBytes).Replace("-", "").ToLower();

                return txId;
            }
            else
            {
                throw new Exception("Transaction broadcast failed: " + result.Message);
            }
        }
        else
        {
            throw new ArgumentException("Invalid transaction object");
        }
    }

    /**
     * 
     var transaction = await Transactions.CreateTransactionAsync("TTTTTX8kc1f12HexiiEWabE8u5fjhC62TT", "TTTC6FqoMWFwzk7mVrwwfBcUB1mDhYdTTT", 0.000001M, true);
                                               
                                                var signedTransaction = Transactions.SignTransaction(transaction!, Program._tronPrivateKey);
                                                var broadcast = await Transactions.BroadcastTransactionAsync(signedTransaction);
                                              
   调用方法返范例
     *                          var transaction = await Transactions.CreateTransactionAsync("TTTTTX8kc1f12HexiiEWabE8u5fjhC62TT", "TTTC6FqoMWFwzk7mVrwwfBcUB1mDhYdTTT", 
     *                          0.000001M, true);
     *                          
     *                          '
     *                          
     */
    public static async Task<string?> CreateTransactionAsync(string from, string to, decimal amountTrx, bool v4)
    {
        // 初始化 DI
        var services = new ServiceCollection();
        services.AddTronNet(options =>
        {
            options.Network =TronNetwork.TestNet;
         //   options.BaseUrl = "https://api.trongrid.io";
          //  options.PrivateKey = "你的私钥"; // 如果需要签名交易
        });

        var provider = services.BuildServiceProvider();

        // 获取 ITransactionClient，而不是 IWalletClient
        var transactionClient = provider.GetRequiredService<ITransactionClient>();

        // 转换金额：1 TRX = 1,000,000 SUN
        long amountInSun = (long)(amountTrx * 1_000_000M);

        // 创建交易
        TransactionExtention txExt = await transactionClient.CreateTransactionAsync(from, to, amountInSun);

        if (txExt?.Transaction == null)
        {
            throw new Exception("交易创建失败");
        }

        // 返回交易对象的原始信息（比如 RawData）
        Console.WriteLine("Transaction Created:");
        Console.WriteLine(txExt.Transaction);

        return "交易已创建";

    }
    /**
     var transaction = await Transactions.CreateTransactionAsync("TTTTTX8kc1f12HexiiEWabE8u5fjhC62TT", "TTTC6FqoMWFwzk7mVrwwfBcUB1mDhYdTTT", 0.000001M, true);
                                               
                                                var signedTransaction = Transactions.SignTransaction(transaction!, Program._tronPrivateKey);
                                                var broadcast = await Transactions.BroadcastTransactionAsync(signedTransaction);
   
     *   var signedTransaction = Transactions.SignTransaction(transaction!, Program._tronPrivateKey);
                                              
     */
    public static object SignTransaction(string transaction, string tronPrivateKey)
    {
        throw new NotImplementedException();
    }
}