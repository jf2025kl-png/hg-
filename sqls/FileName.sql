CREATE TABLE GameHistory
(
    Id INT IDENTITY(1,1) PRIMARY KEY,              -- 主键，自增
    Status INT NOT NULL,                           -- 枚举类型，存储为 int
    Time DATETIME2 NOT NULL,                       -- 游戏时间
    ClosingTime DATETIME2 NULL,                    -- 封盘时间
    EndTime DATETIME2 NULL,                        -- 结束时间
    GroupId BIGINT NOT NULL,                       -- 群组Id
    MessageThreadId INT NULL,                      -- 主题Id
    MessageId INT NOT NULL,                        -- 消息Id
    CommissionRate DECIMAL(18,2) NOT NULL,         -- 手续费率
    Profit DECIMAL(18,2) NOT NULL,                 -- 盈利
    BetAmount DECIMAL(18,2) NOT NULL,              -- 总赌注额度
    RewardInviterAmount DECIMAL(18,2) NOT NULL,    -- 奖励邀请者金额
    LotteryDrawId NVARCHAR(100) NULL,              -- 开奖期数Id
    DefineDataJson NVARCHAR(MAX) NULL,             -- 定义数据JSON
    LotteryDrawJson NVARCHAR(MAX) NULL,            -- 开奖数据JSON
    GameId INT NOT NULL,                           -- 游戏Id
    CreatorId BIGINT NOT NULL,                     -- 平台创建人Id
    PlayerId INT NULL                              -- 玩家Id
);
CREATE TABLE Game
(
    Id INT IDENTITY(1,1) PRIMARY KEY,              -- 主键，自增
    GameType INT NOT NULL,                         -- 枚举类型，存储为 int
    CreatorId BIGINT NOT NULL,                     -- 外键（创建人Id）
    GroupId BIGINT NOT NULL,                       -- 群组Id
    ThreadId INT NULL,                             -- 主题Id
    GameStatus INT NOT NULL,                       -- 枚举类型，存储为 int
    FreezeTip NVARCHAR(500) NULL,                  -- 冻结提示
    StartDateTime DATETIME2 NULL,                  -- 激活开始时间
    EndDateTime DATETIME2 NULL,                    -- 有效期截止时间
    Profit DECIMAL(18,2) NOT NULL,                 -- 共盈利
    PrizePool DECIMAL(18,2) NOT NULL DEFAULT 0,    -- 奖池，默认 0
    Url NVARCHAR(200) NULL,                        -- WebApp 链接
    BetStartLimit DECIMAL(18,2) NOT NULL DEFAULT 5,-- 下注起始限额，默认 5
    BetEndLimit DECIMAL(18,2) NOT NULL DEFAULT 1000,-- 下注截止限额，默认 1000
    BotApiToken NVARCHAR(200) NULL                 -- 机器人ApiToken
);
