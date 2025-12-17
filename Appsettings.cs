namespace 皇冠娱乐
{
    /// <summary>
    /// 应用配置设置
    /// </summary>
    public class Appsettings
    {
        /// <summary>
        /// 皇冠机器人KeyToken
        /// </summary>
        public string ZuoDaoBotKeyToken { get; set; } = null!;

        /// <summary>
        /// 波场钱包地址
        /// </summary>
        public string TronWalletAddress { get; set; } = null!;

        /// <summary>
        /// 以太坊钱包地址
        /// </summary>
        public string EthereumWalletAddress { get; set; } = null!;

        /// <summary>
        /// BTC OMNI链钱包地址
        /// </summary>
        public string OmniWalletAddress { get; set; } = null!;

        /// <summary>
        /// Solana链(SOL)钱包地址
        /// </summary>
        public string SolanaWalletAddress { get; set; } = null!;

        /// <summary>
        /// Cardano链(ADA)钱包地址
        /// </summary>
        public string CardanoWalletAddress { get; set; } = null!;

        /// <summary>
        /// Avalanche链(AVAX)钱包地址
        /// </summary>
        public string AvalancheWalletAddress { get; set; } = null!;

        /// <summary>
        /// Polkadot链(DOT)钱包地址
        /// </summary>
        public string PolkadotWalletAddress { get; set; } = null!;

        /// <summary>
        /// BNB链钱包地址
        /// </summary>
        public string BNBWalletAddress { get; set; } = null!;

        /// <summary>
        /// Polygon链(MATIC)钱包地址
        /// </summary>
        public string PolygonWalletAddress { get; set; } = null!;

        /// <summary>
        /// EOS链钱包地址
        /// </summary>
        public string EOSWalletAddress { get; set; } = null!;

        /// <summary>
        /// 管理员Ids
        /// </summary>
        public HashSet<long> AdminerIds { get; set; } = [];

        /// <summary>
        /// 主题盘开盘费
        /// </summary>
        public decimal CreateBettingThreadFees { get; set; }

        /// <summary>
        /// 首月主题盘营收达标免除开盘费
        /// </summary>
        public decimal FirstMonthWaiverCreateFees { get; set; }

        /// <summary>
        /// 每个盘每个月维护费用
        /// </summary>
        public decimal BettingThreadMonthlyMaintenanceFee { get; set; }

        /// <summary>
        /// 每月主题盘营收达标免除维护费用
        /// </summary>
        public decimal MonthlyBettingThreadWaiverFee { get; set; }

        /// <summary>
        /// 皇冠收益分成占比
        /// </summary>
        public decimal BettingThreadDividend { get; set; }

        /// <summary>
        /// 是否停止提现
        /// </summary>
        public bool IsStopWithdraw { get; set; }
        /// <summary>
        /// 是否提现需要审核
        /// </summary>
        public bool IsApprovalWithdraw { get; set; }

        /// <summary>
        /// 共盈利
        /// </summary>
        public decimal Profit { get; set; }
    }
}
