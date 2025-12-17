using Org.BouncyCastle.Utilities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Threading.Tasks;

//https://en.wikipedia.org/wiki/IETF_language_tag 语言
namespace 皇冠娱乐
{
    ///<summary>
    ///博彩平台
    ///</summary>
    public class Platform
    {
        ///<summary>
        ///创建者Id为主键 (要求创建者=群主,且群主不可以是匿名管理)
        ///</summary>
        [Key]
        public long CreatorId { get; set; }

        ///<summary>
        ///群Id (首次拉入群后才确认,禁止拉入第二个群,要求有公开群链接,标题和描述,头像,要求和机器人创建者Id一致,且是最高权限的管理员才提供博彩服务)
        ///</summary>        
        public long? GroupId { get; set; }
        public long? ChannelId { get; set; }
        ///<summary>
        ///机器人Id
        ///</summary>
        public long? BotId { get; set; }
        /// <summary>
        /// 平台语言
        /// </summary>
        public Language Language { get; set; } = Language.Chinese;

        #region 机器人私聊设置
        ///<summary>
        ///平台状态
        ///</summary>
        public PlatformStatus PlatformStatus { get; set; } = PlatformStatus.Open;

        ///<summary>
        ///冻结提示
        ///</summary>
        public string? FreezeTip { get; set; }

        ///<summary>
        ///机器人ApiToken
        ///</summary>
        public string BotApiToken { get; set; } = null!;

        ///<summary>
        ///波场钱包地址 (钱包必须其中一个不为空,且要存在这地址)
        ///</summary>
        public string? TronWalletAddress { get; set; }
        ///<summary>
        ///波场钱包私钥可空(如果两个钱包地址都是空的,就是人工结款出金)
        ///</summary>
        public string? TronWalletPrivateKey { get; set; }

        ///<summary>
        ///以太坊钱包地址 (钱包必须其中一个不为空,且要存在这地址)
        ///</summary>
        public string? EthereumWalletAddress { get; set; }
        ///<summary>
        ///以太坊钱包私钥可空(如果两个钱包地址都是空的,就是人工结款出金)
        ///</summary>
        public string? EthereumWalletPrivateKey { get; set; }
        ///<summary>
        ///财务用户Id
        ///</summary>
        public long? FinancerId { get; set; }
        ///<summary>
        ///客服Id集合
        ///</summary>
        public string? ServerIds { get; set; }
        ///<summary>
        ///私钥:可以找回平台所有权:转让平台后,私钥也会变更\CreatorId也会变更\群Id也要求变更
        ///</summary>
        [StringLength(64)]
        public string PrivateKey { get; set; } = null!;
        ///<summary>
        ///是否隐藏私钥
        ///</summary>
        public bool IsHidePrivateKey { get; set; } = false;

        ///<summary>
        ///24小时达到1000USDT流水就返多少百分比  一般2000返3‰
        ///</summary>
        [Column(TypeName = "decimal(18, 2)")]
        public decimal RebateFeeRate { get; set; } = 0.03M;
        /// <summary>
        /// 担保金
        /// </summary>
        [Column(TypeName = "decimal(18, 2)")]
        public decimal SecurityDeposit { get; set; }

        /// <summary>
        /// 手续费率
        /// </summary>
        [Column(TypeName = "decimal(18, 2)")]
        public decimal HandlingFee { get; set; }

        /// <summary>
        /// 新人首充送额度
        /// </summary>
        [Column(TypeName = "decimal(18, 2)")]
        public decimal NewPlayerFirstTimeGiveBonus { get; set; }

        /// <summary>
        /// 充1000U起送多少百分比
        /// </summary>
        [Column(TypeName = "decimal(18, 2)")]
        public decimal RechargeBonusPercentage { get; set; }

        /// <summary>
        /// 签到送多少彩金
        /// </summary>
        [Column(TypeName = "decimal(18, 2)")]
        public decimal SignReceiveBonus { get; set; }

        /// <summary>
        /// 每日完成某些任务送多少彩金
        /// </summary>
        [Column(TypeName = "decimal(18, 2)")]
        public decimal DailyTasksGiveBonus { get; set; }

        /// <summary>
        /// 推广首充送彩金
        /// </summary>
        [Column(TypeName = "decimal(18, 2)")]
        public decimal PromoteFirstTimeRechargeBonus { get; set; }

        ///<summary>
        /// 首次提款赠送彩金(增加信誉)
        /// </summary>        
        [Column(TypeName = "decimal(18, 2)")]
        public decimal FirstTimeWithdrawGiveBonus { get; set; }

        /// <summary>
        /// 充值复活金送多少(赌光了再充值赠送彩金)
        /// </summary>
        [Column(TypeName = "decimal(18, 2)")]
        public decimal ResurrectionRechargeGiveBonus { get; set; }

        #region 是否自动管理资金的关键        
        ///<summary>
        ///分红比例 (从邀请的成员亏损后,他的分红比例)
        ///</summary>
        [Range(minimum: 0, maximum: 0.5)]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Dividend { get; set; } = 0;
        ///<summary>
        ///USDT余额(在皇冠的余额)
        ///</summary> 
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Balance { get; set; } = 0;
        ///<summary>
        ///是否停止提现
        ///</summary>
        public bool IsStopWithdraw { get; set; }
        ///<summary>
        ///提现大于多少USDT需财务人员手动操作 : 自动的24小时只能提现一次
        ///</summary>
        [Range(minimum: 0, maximum: 100000)]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal FinancialOperationAmount { get; set; } = 0;
        #endregion
        #endregion

        ///<summary>
        ///共盈利USDT
        ///</summary>
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Profit { get; set; } = 0;
        /// <summary>
        /// 波场钱包和以太坊钱包共计USDT余额
        /// </summary>
        [Column(TypeName = "decimal(18, 2)")]
        public decimal WalletTotalBalance { get; set; } = 0;

        /// <summary>
        /// USDT储备金=钱包总余额(WalletTotalBalance) - 被玩家赢了多少钱(Profit)
        /// </summary>
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Reserves { get; set; } = 0;


#warning 群主派发的,群主自身不可领取

        /// <summary>
        /// 如果长时间无人派发红包/盲盒是否群主自动发包?
        /// </summary>
        public bool IsGrouperAutoSend { get; set; }

        /// <summary>
        /// 是否禁止闲聊
        /// </summary>
        public bool IsNoChatting { get; set; }

        /// <summary>
        /// 是否删除下注记录
        /// </summary>
        public bool IsDeleteBetRecord { get; set; }

        /// <summary>
        /// 群主自动派发下注起始限额
        /// </summary>
        [Column(TypeName = "decimal(18, 2)")]
        public decimal GrouperAutoSendBetStartLimit { get; set; } = 5;

        /// <summary>
        /// 群主自动派发下注截止限额
        /// </summary>
        [Column(TypeName = "decimal(18, 2)")]
        public decimal GrouperAutoSendBetEndLimit { get; set; } = 100;
    }

    /// <summary>
    /// 合伙人
    /// </summary>
    public class Partner
    {
        ///<summary>
        ///财务记录Id
        ///</summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        /// <summary>
        /// 用户Id
        /// </summary>
        public long UserId { get; set; }
        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; } = null!;
        ///<summary>
        ///波场钱包地址 (钱包必须其中一个不为空,且要存在这地址)
        ///</summary>
        public string? TronWalletAddress { get; set; }
        ///<summary>
        ///以太坊钱包地址 (钱包必须其中一个不为空,且要存在这地址)
        ///</summary>
        public string? EthereumWalletAddress { get; set; }
        /// <summary>
        /// 占比
        /// </summary>
        [Range(0.01, 1)]
        public double Proportion { get; set; }

        ///<summary>
        ///平台Id(外键)
        ///</summary>
        public long CreatorId { get; set; }
    }

    ///<summary>
    ///存储的对话Id
    ///</summary>
    public class BotChat
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        ///<summary>
        ///机器人Id
        ///</summary>
        public long BotId { get; set; }

        ///<summary>
        ///对话Id
        ///</summary>
        public long ChatId { get; set; }
    }

    ///<summary>
    ///博彩平台操作记录
    ///</summary>
    public class PlatformOperateHistory
    {
        ///<summary>
        ///财务记录Id
        ///</summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        ///<summary>
        ///创建时间
        ///</summary>
        public DateTime Time { get; set; }

        ///<summary>
        ///操作者角色
        ///</summary>
        public PlatformUserRole PlatformUserRole { get; set; }

        ///<summary>
        ///操作者用户Id
        ///</summary>
        public long OperateUserId { get; set; }

        ///<summary>
        ///备注
        ///</summary>
        public string? Remark { get; set; }

        ///<summary>
        ///平台Id(外键)
        ///</summary>
        public long CreatorId { get; set; }
    }

    ///<summary>
    ///平台财务历史记录
    ///</summary>
    public class PlatformFinanceHistory
    {
        ///<summary>
        ///财务记录Id
        ///</summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        ///<summary>
        ///财务操作状态
        ///</summary>
        public FinanceStatus FinanceStatus { get; set; }

        ///<summary>
        ///创建时间
        ///</summary>
        public DateTime Time { get; set; }
        ///<summary>
        ///财务类型
        ///</summary>
        public FinanceType Type { get; set; }
        ///<summary>
        ///金额(通过正负数决定流通方向)
        ///</summary>
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Amount { get; set; } = 0;
        ///<summary>
        ///备注
        ///</summary>
        public string? Remark { get; set; }

        ///<summary>
        ///平台Id(外键)
        ///</summary>
        public long CreatorId { get; set; }
    }

    ///<summary>
    ///博彩平台玩家
    ///</summary>
    public class Player
    {
        ///<summary>
        ///财务记录Id
        ///</summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PlayerId { get; set; }

        ///<summary>
        ///博彩群平台Id(外键)
        ///</summary>
        public long CreatorId { get; set; }

        ///<summary>
        ///用户Id
        ///</summary>
        public long UserId { get; set; }

        /// <summary>
        /// 是否试玩模式
        /// </summary>
        public bool IsTryModel { get; set; }

        ///<summary>
        ///私钥:(群ID+用户ID)
        ///</summary>
        [StringLength(64)]
        public string PrivateKey { get; set; } = null!;
        ///<summary>
        ///是否隐藏私钥
        ///</summary>
        public bool IsHidePrivateKey { get; set; } = false;

        ///<summary>
        ///会员状态
        ///</summary>
        public PlayerStatus PlayerStatus { get; set; } = PlayerStatus.Normal;
        ///<summary>
        ///冻结提示
        ///</summary>
        public string? FreezeTip { get; set; }

        ///<summary>
        ///时间
        ///</summary>
        public DateTime Time { get; set; }
        ///<summary>
        ///USDT余额
        ///</summary>
        [Range(minimum: 0, maximum: 99999999)]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Balance { get; set; } = 0;

        /// <summary>
        /// 积分：充值、邀请可获得 （可转化为现金）
        /// </summary>
        public int Integral { get; set; } = 0;

        ///<summary>
        ///赠送的USDT余额,不可提现(新用户专享\充值多少送多少/活动......)
        ///</summary>
        [Range(minimum: 0, maximum: 99999999)]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal RewardBalance { get; set; } = 0;

        ///<summary>
        ///波场钱包地址
        ///</summary>
        public string? TronWalletAddress { get; set; }

        ///<summary>
        ///以太坊钱包地址
        ///</summary>
        public string? EthereumWalletAddress { get; set; }

        ///<summary>
        ///邀请者Id
        ///</summary>
        public int? InviterId { get; set; }
    }

    ///<summary>
    ///财务记录
    ///</summary>
    public class PlayerFinanceHistory
    {
        ///<summary>
        ///财务记录Id
        ///</summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        ///<summary>
        ///财务操作状态
        ///</summary>
        public FinanceStatus FinanceStatus { get; set; }
        ///<summary>
        ///创建时间
        ///</summary>
        public DateTime Time { get; set; }
        ///<summary>
        ///财务类型
        ///</summary>
        public FinanceType Type { get; set; }

        /// <summary>
        /// 名称
        /// </summary>
        public string? Name { get; set; }

        ///<summary>
        ///余额那里产生的金额(通过正负数决定流通方向)
        ///</summary>
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Amount { get; set; } = 0;

        ///<summary>
        ///彩金那里产生的金额(通过正负数决定流通方向)
        ///</summary>
        [Column(TypeName = "decimal(18, 2)")]
        public decimal BonusAmount { get; set; } = 0;

        ///<summary>
        ///抽成额度
        ///</summary>
        [Column(TypeName = "decimal(18, 2)")]
        public decimal CommissionAmount { get; set; }
        ///<summary>
        ///备注
        ///</summary>
        public string? Remark { get; set; }
        ///<summary>
        ///对方Id
        ///</summary>
        public int OtherId { get; set; }
        ///<summary>
        ///盘口每局游戏的外键Id
        ///</summary>
        public int? GameId { get; set; }

        ///<summary>
        ///盘口每局游戏的信息Id
        ///</summary>
        public int? GameMessageId { get; set; }
        /// <summary>
        /// 玩家Id
        /// </summary>
        public int PlayerId { get; set; }
    }

    ///<summary>
    ///游戏主题设置
    ///</summary>
    public class Game
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        ///<summary>
        ///游戏类型
        ///</summary>
        public GameType GameType { get; set; }

        ///<summary>
        ///Id(外键)
        ///</summary>
        public long CreatorId { get; set; }

        /// <summary>
        /// 群组Id
        /// </summary>
        public long GroupId { get; set; }

        ///<summary>
        ///主题Id
        ///</summary>
        public int? ThreadId { get; set; }
        ///<summary>
        ///主题盘口状态
        ///</summary>
        public GameStatus GameStatus { get; set; }
        ///<summary>
        ///冻结提示
        ///</summary>
        public string? FreezeTip { get; set; }
        ///<summary>
        ///激活开始时间
        ///</summary>
        public DateTime? StartDateTime { get; set; }
        ///<summary>
        ///有效期截止时间
        ///</summary>
        public DateTime? EndDateTime { get; set; }
        ///<summary>
        ///共盈利
        ///</summary>
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Profit { get; set; }

        /// <summary>
        /// 奖池
        /// </summary>
        [Column(TypeName = "decimal(18, 2)")]
        public decimal PrizePool { get; set; } = 0;

        /// <summary>
        /// 用于WEBAPP,比如视讯,电子
        /// </summary>
        public string? Url { get; set; }

        /// <summary>
        /// 下注起始限额
        /// </summary>
        [Column(TypeName = "decimal(18, 2)")]
        public decimal BetStartLimit { get; set; } = 5;

        /// <summary>
        /// 下注截止限额
        /// </summary>
        [Column(TypeName = "decimal(18, 2)")]
        public decimal BetEndLimit { get; set; } = 1000;

        /// <summary>
        /// 机器人ApiToken
        /// </summary>
        public string? BotApiToken { get; set; }
    }

    ///<summary>
    ///每局游戏的历史记录
    ///</summary>
    public class GameHistory
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        /// <summary>
        /// 游戏状态
        /// </summary>
        public GameHistoryStatus Status { get; set; }
        ///<summary>
        ///时间
        ///</summary>
        public DateTime Time { get; set; }
        /// <summary>
        /// 封盘(禁止下注)时间
        /// </summary>
        public DateTime? ClosingTime { get; set; }
        /// <summary>
        /// 结束时间
        /// </summary>
        public DateTime? EndTime { get; set; }
        ///<summary>
        ///群组Id
        ///</summary>
        public long GroupId { get; set; }
        ///<summary>
        ///主题Id
        ///</summary>
        public int? MessageThreadId { get; set; }
        ///<summary>
        ///消息Id
        ///</summary>
        public int MessageId { get; set; }
        ///<summary>
        ///抽手续费率
        ///</summary>
        [Column(TypeName = "decimal(18, 2)")]
        public decimal CommissionRate { get; set; }
        /// <summary>
        /// 共盈利（赢了就是正数，输了就负数）
        /// </summary>
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Profit { get; set; }
        /// <summary>
        /// 总赌注额度
        /// </summary>
        [Column(TypeName = "decimal(18, 2)")]
        public decimal BetAmount { get; set; }
        /// <summary>
        /// 奖励邀请者金额
        /// </summary>
        [Column(TypeName = "decimal(18, 2)")]
        public decimal RewardInviterAmount { get; set; }

        /// <summary>
        /// 开奖期数Id
        /// </summary>
        public string? LotteryDrawId { get; set; }

        /// <summary>
        /// 定义数据JSON(常用在体彩\动物\活动比赛)
        /// </summary>
        public string? DefineDataJson { get; set; }

        /// <summary>
        /// 开奖数据JSON
        /// </summary>
        public string? LotteryDrawJson { get; set; }

        /// <summary>
        /// 游戏Id
        /// </summary>
        public int GameId { get; set; }
        /// <summary>
        /// 平台创建人Id
        /// </summary>
        public long CreatorId { get; set; }
        /// <summary>
        /// 创建此记录的玩家Id
        /// </summary>
        public int? PlayerId { get; set; }
    }
}
