CREATE DATABASE gasino;
USE gasino;

-- BotChat
CREATE TABLE BotChat (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    BotId BIGINT NOT NULL,
    ChatId BIGINT NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- GameHistorys
CREATE TABLE GameHistorys (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Status INT NOT NULL,
    Time DATETIME NOT NULL,
    ClosingTime DATETIME NULL,
    EndTime DATETIME NULL,
    GroupId BIGINT NOT NULL,
    MessageThreadId INT NULL,
    MessageId INT NOT NULL,
    CommissionRate DECIMAL(18,2) NOT NULL,
    Profit DECIMAL(18,2) NOT NULL,
    BetAmount DECIMAL(18,2) NOT NULL,
    RewardInviterAmount DECIMAL(18,2) NOT NULL,
    LotteryDrawId VARCHAR(100) NULL,
    DefineDataJson TEXT NULL,
    LotteryDrawJson TEXT NULL,
    GameId INT NOT NULL,
    CreatorId BIGINT NOT NULL,
    PlayerId INT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Games
CREATE TABLE Games (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    GameType INT NOT NULL,
    CreatorId BIGINT NOT NULL,
    GroupId BIGINT NOT NULL,
    ThreadId INT NULL,
    GameStatus INT NOT NULL,
    FreezeTip VARCHAR(500) NULL,
    StartDateTime DATETIME NULL,
    EndDateTime DATETIME NULL,
    Profit DECIMAL(18,2) NOT NULL,
    PrizePool DECIMAL(18,2) NOT NULL DEFAULT 0,
    Url VARCHAR(200) NULL,
    BetStartLimit DECIMAL(18,2) NOT NULL DEFAULT 5,
    BetEndLimit DECIMAL(18,2) NOT NULL DEFAULT 1000,
    BotApiToken VARCHAR(200) NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Platforms
CREATE TABLE Platforms (
    CreatorId BIGINT PRIMARY KEY,
    GroupId BIGINT NULL,
    ChannelId BIGINT NULL,
    BotId BIGINT NULL,
    Language INT NOT NULL DEFAULT 0,
    PlatformStatus INT NOT NULL DEFAULT 0,
    FreezeTip VARCHAR(500) NULL,
    BotApiToken VARCHAR(200) NOT NULL,
    TronWalletAddress VARCHAR(200) NULL,
    TronWalletPrivateKey VARCHAR(200) NULL,
    EthereumWalletAddress VARCHAR(200) NULL,
    EthereumWalletPrivateKey VARCHAR(200) NULL,
    FinancerId BIGINT NULL,
    ServerIds TEXT NULL,
    PrivateKey VARCHAR(64) NOT NULL,
    IsHidePrivateKey BOOLEAN NOT NULL DEFAULT 0,
    RebateFeeRate DECIMAL(18,2) NOT NULL DEFAULT 0.03,
    SecurityDeposit DECIMAL(18,2) NOT NULL,
    HandlingFee DECIMAL(18,2) NOT NULL,
    NewPlayerFirstTimeGiveBonus DECIMAL(18,2) NOT NULL,
    RechargeBonusPercentage DECIMAL(18,2) NOT NULL,
    SignReceiveBonus DECIMAL(18,2) NOT NULL,
    DailyTasksGiveBonus DECIMAL(18,2) NOT NULL,
    PromoteFirstTimeRechargeBonus DECIMAL(18,2) NOT NULL,
    FirstTimeWithdrawGiveBonus DECIMAL(18,2) NOT NULL,
    ResurrectionRechargeGiveBonus DECIMAL(18,2) NOT NULL,
    Dividend DECIMAL(18,2) NOT NULL DEFAULT 0,
    Balance DECIMAL(18,2) NOT NULL DEFAULT 0,
    IsStopWithdraw BOOLEAN NOT NULL DEFAULT 0,
    FinancialOperationAmount DECIMAL(18,2) NOT NULL DEFAULT 0,
    Profit DECIMAL(18,2) NOT NULL DEFAULT 0,
    WalletTotalBalance DECIMAL(18,2) NOT NULL DEFAULT 0,
    Reserves DECIMAL(18,2) NOT NULL DEFAULT 0,
    IsGrouperAutoSend BOOLEAN NOT NULL DEFAULT 0,
    IsNoChatting BOOLEAN NOT NULL DEFAULT 0,
    IsDeleteBetRecord BOOLEAN NOT NULL DEFAULT 0,
    GrouperAutoSendBetStartLimit DECIMAL(18,2) NOT NULL DEFAULT 5,
    GrouperAutoSendBetEndLimit DECIMAL(18,2) NOT NULL DEFAULT 100
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Players
CREATE TABLE Players (
    PlayerId INT PRIMARY KEY,
    CreatorId BIGINT NOT NULL,
    UserId BIGINT NOT NULL,
    IsTryModel BOOLEAN NOT NULL DEFAULT 0,
    PrivateKey VARCHAR(64) NOT NULL,
    IsHidePrivateKey BOOLEAN NOT NULL DEFAULT 0,
    PlayerStatus INT NOT NULL DEFAULT 0,
    FreezeTip VARCHAR(500) NULL,
    Time DATETIME NOT NULL,
    Balance DECIMAL(18,2) NOT NULL DEFAULT 0,
    Integral INT NOT NULL DEFAULT 0,
    RewardBalance DECIMAL(18,2) NOT NULL DEFAULT 0,
    TronWalletAddress VARCHAR(200) NULL,
    EthereumWalletAddress VARCHAR(200) NULL,
    InviterId INT NULL,
    FOREIGN KEY (CreatorId) REFERENCES Platforms(CreatorId)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- 其他表类似转换...
