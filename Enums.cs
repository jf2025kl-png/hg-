using System.ComponentModel;

namespace 皇冠娱乐
{
    /// <summary>
    /// 博彩平台人员角色
    /// </summary>
    public enum PlatformUserRole
    {
        /// <summary>
        /// 皇冠管理员
        /// </summary>
        Adminer,
        /// <summary>
        /// 创建者
        /// </summary>
        Creator,
        /// <summary>
        /// 财务人员
        /// </summary>
        Financer,
    }

    /// <summary>
    /// 财务类型
    /// </summary>
    public enum FinanceType
    {
        /// <summary>
        /// 充值
        /// </summary>
        [Description("充值")]
        Recharge,

        /// <summary>
        /// 提现
        /// </summary>
        [Description("提现")]
        Withdraw,

        /// <summary>
        /// 领取平台奖金 (用备注区分:新人专享赠送彩金/赠送体验彩金
        /// </summary>
        [Description("平台奖励")]
        ClaimedPlatformBonus,

        /// <summary>
        /// 邀请的人充值了
        /// </summary>
        [Description("受邀请充值")]
        InvitePlayersRecharge,

        /// <summary>
        /// 推广获利
        /// </summary>
        [Description("推广获利")]
        PromotionProfit,

        /// <summary>
        /// 分成
        /// </summary>
        [Description("分成")]
        DividedInto,

        /// <summary>
        /// 主题博彩开盘费
        /// </summary>
        [Description("开盘费")]
        OpeningFee,

        /// <summary>
        /// 主题博彩盘月维护费
        /// </summary>
        [Description("盘口月费")]
        MonthlyMaintenanceFee,

        /// <summary>
        /// 手续费
        /// </summary>
        [Description("手续费")]
        HandlingFee,

        /// <summary>
        /// 奖池金
        /// </summary>
        [Description("奖池金")]
        PoolFee,

        /// <summary>
        /// 返水
        /// </summary>
        [Description("返水")]
        RebateFee,

        /// <summary>
        /// 视讯
        /// </summary>
        [Description("视讯")]
        Video,

        /// <summary>
        /// 电竞
        /// </summary>
        [Description("电竞")]
        Gaming,

        /// <summary>
        /// 电子
        /// </summary>
        [Description("电子")]
        Electronic,

        /// <summary>
        /// 棋牌
        /// </summary>
        [Description("棋牌")]
        ChessCards,

        /// <summary>
        /// 捕鱼
        /// </summary>
        [Description("捕鱼")]
        Fishing,

        /// <summary>
        /// 虚拟
        /// </summary>
        [Description("虚拟")]
        VirtualGame,

        /// <summary>
        /// 体彩
        /// </summary>
        [Description("体彩")]
        SportsContest,

        /// <summary>
        /// 动物
        /// </summary>
        [Description("动物")]
        AnimalContest,

        /// <summary>
        /// 老虎机
        /// </summary>
        [Description("老虎机")]
        SlotMachine,

        /// <summary>
        /// 骰子
        /// </summary>
        [Description("骰子")]
        Dice,

        /// <summary>
        /// 保龄球 1/7  - 投3次
        /// </summary>
        [Description("保龄球")]
        Bowling,

        /// <summary>
        /// 飞镖 1/7  - 投3次
        /// </summary>
        [Description("飞镖")]
        Dart,

        /// <summary>
        /// 足球 1/6  - 投3次
        /// </summary>
        [Description("足球")]
        Soccer,

        /// <summary>
        /// 篮球 1/6  - 投3次
        /// </summary>
        [Description("篮球")]
        Basketball,

        /// <summary>
        /// 红包：不可试玩
        /// </summary>
        [Description("红包")]
        RedEnvelope,

        /// <summary>
        /// 盲盒：不可试玩
        /// </summary>
        [Description("盲盒")]
        BlindBox,

        /// <summary>
        /// 抢庄
        /// </summary>
        [Description("抢庄")]
        GrabBanker,

        /// <summary>
        /// 刮刮乐:类似盲盒
        /// </summary>
        [Description("刮刮乐")]
        ScratchOff,                

        /// <summary>
        /// 轮盘赌:从哈希上提取特征制作
        /// </summary>
        [Description("轮盘赌")]
        Roulette,

        /// <summary>
        /// 牛牛:从哈希上提取特征制作
        /// </summary>
        [Description("牛牛")]
        Cow,

        /// <summary>
        /// 21点:从哈希上提取特征制作
        /// </summary>
        [Description("21点")]
        Blackjack,

        /// <summary>
        /// 三公:从哈希上提取特征制作
        /// </summary>
        [Description("三公")]
        Sangong,

        /// <summary>
        /// 百家乐:从哈希上提取特征制作
        /// </summary>
        [Description("百家乐")]
        Baccarat,

        /// <summary>
        /// 竞猜
        /// </summary>
        [Description("竞猜")]
        TrxHash,

        /// <summary>
        /// 幸运数
        /// </summary>
        [Description("幸运数")]
        LuckyHash,

        /// <summary>
        /// 比特币
        /// </summary>
        [Description("比特币")]
        BinanceBTCPrice,

        /// <summary>
        /// 外汇
        /// </summary>
        [Description("外汇")]
        Forex,

        /// <summary>
        /// 股票
        /// </summary>
        [Description("股票")]
        Stock,

        /// <summary>
        /// 龙虎
        /// </summary>
        [Description("龙虎")]
        DragonTiger,

        /// <summary>
        /// 六合彩
        /// </summary>
        [Description("六合彩")]
        SixLottery,

        /// <summary>
        /// 加拿大PC28
        /// </summary>
        [Description("加拿大PC28")]
        CanadaPC28,

        /// <summary>
        /// 赛车
        /// </summary>
        [Description("赛车")]
        SpeedRacing,

        /// <summary>
        /// 飞艇
        /// </summary>
        [Description("飞艇")]
        LuckyAirship,

        /// <summary>
        /// 11选5
        /// </summary>
        [Description("11选5")]
        Choose5From11,

        /// <summary>
        /// 缤果
        /// </summary>
        [Description("缤果")]
        Bingo,

        /// <summary>
        /// 幸运8
        /// </summary>
        [Description("幸运8")]
        AustralianLucky8,

        /// <summary>
        /// 大乐透
        /// </summary>
        [Description("大乐透")]
        BigLottery,

        /// <summary>
        /// 四星彩
        /// </summary>
        [Description("四星彩")]
        FourStarLottery
    }

    /// <summary>
    /// 游戏盘口状态
    /// </summary>
    public enum GameStatus
    {
        /// <summary>
        /// 开启
        /// </summary>
        Open,
        /// <summary>
        /// 关闭
        /// </summary>
        Close,
        /// <summary>
        /// 到期(需要付费)
        /// </summary>
        Expire,
        /// <summary>
        /// 冻结
        /// </summary>
        Freeze,
    }

    /// <summary>
    /// 游戏记录状态
    /// </summary>
    public enum GameHistoryStatus
    {
        /// <summary>
        /// 进行中
        /// </summary>
        Ongoing,
        /// <summary>
        /// 结束
        /// </summary>
        End,
        /// <summary>
        /// 过期
        /// </summary>
        Expired
    }

    /// <summary>
    /// 博彩平台状态
    /// </summary>
    public enum PlatformStatus
    {
        /// <summary>
        /// 开启
        /// </summary>
        Open,
        /// <summary>
        /// 关闭
        /// </summary>
        Close,
        /// <summary>
        /// 冻结
        /// </summary>
        Freeze,
    }

    /// <summary>
    /// 财务状态
    /// </summary>
    public enum FinanceStatus
    {
        /// <summary>
        /// 等待确认
        /// </summary>
        [Description("未确认")]
        WaitingConfirmation,
        /// <summary>
        /// 拒绝
        /// </summary>
        [Description("拒绝")]
        Reject,
        /// <summary>
        /// 成功
        /// </summary>
        [Description("成功")]
        Success,
        /// <summary>
        /// 超时未操作
        /// </summary>
        [Description("超时")]
        Timeout,
        /// <summary>
        /// 冻结
        /// </summary>
        [Description("冻结")]
        Freeze,
        /// <summary>
        /// 返还
        /// </summary>
        [Description("返还")]
        Return,
    }

    /// <summary>
    /// 皇冠机器人私聊等待输入操作
    /// </summary>
    public enum WaitInput
    {
        #region 设置平台群
        /// <summary>
        /// 设置机器人的ApiToken
        /// </summary>
        BotApiToken,
        /// <summary>
        /// 转让博彩平台所有权
        /// </summary>
        TransferOwnership,
        /// <summary>
        /// 申诉找回
        /// </summary>
        AppealRecovery,
        /// <summary>
        /// 设置波场钱包地址
        /// </summary>
        TronWalletAddress,
        /// <summary>
        /// 设置波场钱包私钥
        /// </summary>
        TronWalletPrivateKey,
        /// <summary>
        /// 设置以太坊钱包地址
        /// </summary>
        EthereumWalletAddress,
        /// <summary>
        /// 设置以太坊钱包私钥
        /// </summary>
        EthereumWalletPrivateKey,
        /// <summary>
        /// 设置博彩群Id
        /// </summary>
        GroupId,
        /// <summary>
        /// 设置财务Id
        /// </summary>
        FinancerId,
        /// <summary>
        /// 设置分红比例 (从邀请的成员亏损后,他的分红比例)
        /// </summary>
        Dividend,
        /// <summary>
        /// 提现(无论是平台提现还是会员提现) 格式要求:私钥=>100.00
        /// </summary>
        Withdraw,
        /// <summary>
        /// 设置提现大于多少资金需财务人工手动操作
        /// </summary>
        FinancialOperationAmount,
        #endregion

        /// <summary>
        /// 激活某个博彩盘口游戏
        /// </summary>
        ActivateGame,
        /// <summary>
        /// 月费续费某个盘口
        /// </summary>
        RenewalGame,
    }

    /// <summary>
    /// 会员状态
    /// </summary>
    public enum PlayerStatus
    {
        /// <summary>
        /// 正常
        /// </summary>
        Normal,
        /// <summary>
        /// 冻结
        /// </summary>
        Freeze,
    }

    /// <summary>
    /// 游戏类型
    /// </summary>
    public enum GameType
    {
        /// <summary>
        /// 视讯
        /// </summary>
        Video,

        /// <summary>
        /// 电竞
        /// </summary>
        Gaming,

        /// <summary>
        /// 电子
        /// </summary>
        Electronic,

        /// <summary>
        /// 棋牌
        /// </summary>
        ChessCards,

        /// <summary>
        /// 捕鱼
        /// </summary>
        Fishing,

        /// <summary>
        /// 虚拟
        /// </summary>
        VirtualGame,

        /// <summary>
        /// 体彩
        /// </summary>
        SportsContest,

        /// <summary>
        /// 动物:蟋蟀\斗鸡\斗牛\斗狗\赛马\赛狗
        /// </summary>
        AnimalContest,

        /// <summary>
        /// 老虎机
        /// </summary>
        SlotMachine,

        /// <summary>
        /// 骰子     - 投3次
        /// </summary>
        Dice,

        /// <summary>
        /// 保龄球 1/6  - 投3次
        /// </summary>
        Bowling,

        /// <summary>
        /// 飞镖 1/  - 投3次
        /// </summary>
        Dart,

        /// <summary>
        /// 足球 1/5  - 投3次
        /// </summary>
        Soccer,

        /// <summary>
        /// 篮球 1/5  - 投3次
        /// </summary>
        Basketball,

        /// <summary>
        /// 红包：不可试玩
        /// </summary>
        RedEnvelope,

        /// <summary>
        /// 盲盒：不可试玩
        /// </summary>
        BlindBox,

        /// <summary>
        /// 抢庄
        /// </summary>
        GrabBanker,

        /// <summary>
        /// 刮刮乐
        /// </summary>
        ScratchOff,              

        /// <summary>
        /// 轮盘赌:从哈希上提取特征制作
        /// </summary>
        Roulette,

        /// <summary>
        /// 牛牛:从哈希上提取特征制作
        /// </summary>
        Cow,

        /// <summary>
        /// 21点:从哈希上提取特征制作
        /// </summary>
        Blackjack,

        /// <summary>
        /// 三公:从哈希上提取特征制作
        /// </summary>
        Sangong,

        /// <summary>
        /// 百家乐:从哈希上提取特征制作
        /// </summary>
        Baccarat,

        /// <summary>
        /// 竞猜
        /// </summary>
        TrxHash,

        /// <summary>
        /// 幸运数
        /// </summary>
        LuckyHash,

        /// <summary>
        /// 比特币
        /// </summary>
        BinanceBTCPrice,

        /// <summary>
        /// 外汇
        /// </summary>
        Forex,

        /// <summary>
        /// 股票
        /// </summary>
        Stock,

        /// <summary>
        /// 龙虎
        /// </summary>
        DragonTiger,

        /// <summary>
        /// 六合彩
        /// </summary>
        SixLottery,

        /// <summary>
        /// 加拿大PC28
        /// </summary>
        CanadaPC28,

        /// <summary>
        /// 赛车
        /// </summary>
        SpeedRacing,

        /// <summary>
        /// 飞艇
        /// </summary>
        LuckyAirship,

        /// <summary>
        /// 11选5
        /// </summary>
        Choose5From11,

        /// <summary>
        /// 缤果
        /// </summary>
        Bingo,

        /// <summary>
        /// 幸运8
        /// </summary>
        AustralianLucky8,

        /// <summary>
        /// 大乐透
        /// </summary>
        BigLottery,

        /// <summary>
        /// 四星彩
        /// </summary>
        FourStarLottery

        //抢庄:谁赢了上一局,自动成为下一局的庄家,庄家默认赢面比较大

        //从PC28那里还能提取出:
        //BC50/50(每次有奖)\大乐透MAX\德州扑克Pacific Hold'Em Poker\扑克乐透PokerLotto

        //10U夺宝
        //幸运时时彩
        //SG飞艇
        //极速时时彩
        //福彩双色球
        //福彩3D

        //七乐彩
        //大乐透
        //三星彩
        //四星彩
        //大乐透

        //https://www.thelotter.com/lottery-tickets/
        //https://www.agentlotto.com/en/play-lottery/
        //美国兆彩
        //美国威力球 
        //美国超级百万MegaMilions
        //美国强力球Powerball
        //欧洲百万EuroMillions
        //意大利超级乐透Superenalotto
        //欧洲乐透彩
        //欧洲梦
        //意大利超级巨星
        //西班牙La Primitiva
        //澳洲奥兹乐透Oz Lotto
        //澳洲Superdraw Saturaday Lotto
        //西班牙El Gordo

        //https://jc.zhcw.com/?_ga=2.59193322.63728460.1710475225-1649458770.1710475225
        //https://www.zgzcw.com/
        //https://www.sporttery.cn/

    }

    /// <summary>
    /// 语言
    /// </summary>
    public enum Language
    {
        /// <summary>
        /// 中文
        /// </summary>
        Chinese,
        /// <summary>
        /// 英语
        /// </summary>
        English,
        /// <summary>
        /// 西班牙语
        /// </summary>
        Spanish,
        /// <summary>
        /// 阿拉伯语
        /// </summary>
        Arabic,
        /// <summary>
        /// 法语
        /// </summary>
        French,
        /// <summary>
        /// 俄语
        /// </summary>
        Russian,
        /// <summary>
        /// 葡萄牙语
        /// </summary>
        Portuguese,
        /// <summary>
        /// 孟加拉语
        /// </summary>
        Bengali,
        /// <summary>
        /// 德语
        /// </summary>
        German,
        /// <summary>
        /// 日语
        /// </summary>
        Japanese,
        /// <summary>
        /// 印度尼西亚语
        /// </summary>
        Indonesian,
        /// <summary>
        /// 比利时荷兰语
        /// </summary>
        BelgianDutch,
        /// <summary>
        /// 意大利语
        /// </summary>
        Italian,
        /// <summary>
        /// 希伯来语
        /// </summary>
        ModernStandardHebrew,
        /// <summary>
        /// 罗马尼亚语
        /// </summary>
        Romanian,
        /// <summary>
        /// 南非荷兰语
        /// </summary>
        SouthAfricanDutch,
        /// <summary>
        /// 希腊语
        /// </summary>
        Greek,
        /// <summary>
        /// 匈牙利语
        /// </summary>
        Hungarian,
        /// <summary>
        /// 波兰语
        /// </summary>
        Polish,
        /// <summary>
        /// 乌尔都语
        /// </summary>
        Urdu,
        /// <summary>
        /// 土耳其语
        /// </summary>
        Turkish,
        /// <summary>
        /// 韩语
        /// </summary>
        Korean,
        /// <summary>
        /// 瑞典语
        /// </summary>
        Swedish,
        /// <summary>
        /// 塞尔维亚语
        /// </summary>
        Serbian,
        /// <summary>
        /// 乌克兰语
        /// </summary>
        Ukrainian,
        /// <summary>
        /// 挪威语
        /// </summary>
        Norwegian,
        /// <summary>
        /// 丹麦语
        /// </summary>
        Danish,
        /// <summary>
        /// 泰语
        /// </summary>
        Thai,
        /// <summary>
        /// 芬兰语
        /// </summary>
        Finnish,
        /// <summary>
        /// 捷克语
        /// </summary>
        Czech,
        /// <summary>
        /// 斯洛伐克语
        /// </summary>
        Slovak,
        /// <summary>
        /// 克罗地亚语
        /// </summary>
        Croatian,
        /// <summary>
        /// 拉脱维亚语
        /// </summary>
        Latvian,
        /// <summary>
        /// 爱沙尼亚语
        /// </summary>
        Estonian,
        /// <summary>
        /// 斯洛文尼亚语
        /// </summary>
        Slovenian,
        /// <summary>
        /// 立陶宛语
        /// </summary>
        Lithuanian,
        /// <summary>
        /// 格鲁吉亚语
        /// </summary>
        Georgian,
        /// <summary>
        /// 阿尔巴尼亚语
        /// </summary>
        Albanian,
        /// <summary>
        /// 马其顿语
        /// </summary>
        Macedonian,
        /// <summary>
        /// 波斯语
        /// </summary>
        Persian,
        /// <summary>
        /// 阿姆哈拉语
        /// </summary>
        Amharic,
        /// <summary>
        /// 斯瓦希里语
        /// </summary>
        Swahili,
        /// <summary>
        /// 阿塞拜疆语
        /// </summary>
        Azerbaijani,
        /// <summary>
        /// 柬埔寨语
        /// </summary>
        Khmer,
        /// <summary>
        /// 塔吉克语
        /// </summary>
        Tajik,
        /// <summary>
        /// 乌兹别克语
        /// </summary>
        Uzbek,
        /// <summary>
        /// 纳瓦特尔语
        /// </summary>
        Navajo,
        /// <summary>
        /// 土库曼语
        /// </summary>
        Turkmen,
        /// <summary>
        /// 布尔语
        /// </summary>
        Zulu,
        /// <summary>
        /// 阿美哈拉语
        /// </summary>
        Afrikaans,
        /// <summary>
        /// 越南语
        /// </summary>
        Vietnamese
    }
}
