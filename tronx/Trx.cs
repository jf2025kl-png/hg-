using Google.Protobuf;
using Microsoft.Extensions.DependencyInjection;
using System;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks;
using TronSharp;
using TronSharp.Accounts;
//using TronSharp.Api;
using TronSharp.Protocol;
/*
 ori is just this tronsharp...not tronnet
 */
namespace 皇冠娱乐.tronx
{
    internal class Trx
    {
   


     /**
     * 

   调用方法返范例
     *                          var transaction = await Transactions.CreateTransactionAsync("TTTTTX8kc1f12HexiiEWabE8u5fjhC62TT", "TTTC6FqoMWFwzk7mVrwwfBcUB1mDhYdTTT", 
     *                          0.000001M, true);
     *                          
     *                          '
     *                          
     */                          
    /// <summary>
    /// 创建交易
    /// </summary>
    public static async Task<Transaction?> CreateTransactionAsync(string from, string to, decimal amountTrx, bool useMainNet)
        {//TronNetwork.Nile → 测试网（以前的 Shasta 已经被 Nile 替代）
            // 初始化 TronSharp 客户端
            var network = useMainNet ? TronNetwork.MainNet : TronNetwork.TestNet;
            //  var client = new TronClient(network);
            // 配置依赖注入
            var services = new ServiceCollection();
            services.AddTronSharp(options =>
            {
                options.Network = useMainNet ? TronNetwork.MainNet : TronNetwork.TestNet; // Shasta 已废弃，用 Nile
               // options.BaseUrl = useMainNet ? "https://api.trongrid.io" : "https://nile.trongrid.io";
              //  options.PrivateKey = "你的私钥"; // 如果需要签名
            });

            var provider = services.BuildServiceProvider();

            // 获取 ITransactionClient
            var transactionClient = provider.GetRequiredService<ITransactionClient>();

            // 转换金额：1 TRX = 1,000,000 SUN
            long amountInSun = (long)(amountTrx * 1_000_000M);

            // 创建交易
            var transactionExt = await transactionClient.CreateTransactionAsync(from, to, amountInSun);

            return transactionExt?.Transaction;
        }

        /// <summary>
        /// 签名交易
        /// </summary>
        public static Transaction SignTransaction(Transaction transaction, string privateKeyHex)
        {
            // 1. 把私钥转成字节
            var privateKeyBytes = HexToBytes(privateKeyHex);

            // 2. 对 RawData 做 SHA256
            using var sha256 = SHA256.Create();
            var rawDataBytes = transaction.RawData.ToByteArray();
            var hash = sha256.ComputeHash(rawDataBytes);

            // 3. 用 ECDSA secp256k1 签名
            var ecKey = new TronSharp.Crypto.ECKey(privateKeyBytes, true);
            var signature = ecKey.Sign(hash);

            // 4. 写入签名
            //  transaction.Signature.Add(ByteString.CopyFrom(signature));
            // 4) 写入签名（使用 DER 编码或紧凑编码）
            // 选择其一：
            // DER 编码（常见做法）
            transaction.Signature.Add(ByteString.CopyFrom(signature.ToDER()));

            // 或：紧凑格式 (r||s)，某些库/节点偏好此格式：
            // transaction.Signature.Add(ByteString.CopyFrom(signature.ToByteArray()));
            return transaction;
        }

        public static byte[] HexToBytes(string hex)
        {
            if (hex.StartsWith("0x")) hex = hex.Substring(2);
            int length = hex.Length;
            byte[] bytes = new byte[length / 2];
            for (int i = 0; i < length; i += 2)
            {
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            }
            return bytes;
        }

        /// <summary>
        /// 广播交易
        /// </summary>
        public static async Task<string> BroadcastTransactionAsync(Transaction signedTransaction)
        {
            bool useMainNet = true;
            // 初始化 TronSharp 客户端
            var network = useMainNet ? TronNetwork.MainNet : TronNetwork.TestNet;
            //  var client = new TronClient(network);
            // 配置依赖注入
            var services = new ServiceCollection();
            services.AddTronSharp(options =>
            {
                options.Network = useMainNet ? TronNetwork.MainNet : TronNetwork.TestNet; // Shasta 已废弃，用 Nile
                                                                                          // options.BaseUrl = useMainNet ? "https://api.trongrid.io" : "https://nile.trongrid.io";
                                                                                          //  options.PrivateKey = "你的私钥"; // 如果需要签名
            });

            var provider = services.BuildServiceProvider();

            // 获取 ITransactionClient
            var transactionClient = provider.GetRequiredService<ITransactionClient>();


            var result = await transactionClient.BroadcastTransactionAsync(signedTransaction);

            if (!result.Result)
            {
                throw new Exception("广播失败: " + result.Message);
            }

            // 计算交易ID (SHA256 RawData)
            using var sha256 = SHA256.Create();
            var txIdBytes = sha256.ComputeHash(signedTransaction.RawData.ToByteArray());
            var txId = BitConverter.ToString(txIdBytes).Replace("-", "").ToLower();

            return txId;
        }

    }
}
