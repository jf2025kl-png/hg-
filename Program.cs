using HtmlAgilityPack;
using HtmlAgilityPack.CssSelectors.NetCore;
using Microsoft.EntityFrameworkCore;
using NBitcoin;
using Newtonsoft.Json;
using System.Globalization;
using System.Text.RegularExpressions;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
// using Tron;
// using Tron.FullNodeHttpApi;
using TronNet;
using TronNet.Crypto;
using Message = Telegram.Bot.Types.Message;
using Microsoft.Extensions.DependencyInjection;


namespace 皇冠娱乐
{
#warning 提现要付TRX手续费等值的USDT
    internal class Program
    {
        /// <summary>
        /// 平台运行机器人集合
        /// </summary>
        public static List<TelegramBotClient> _botClientList = [];

        /// <summary>
        /// 皇冠机器人
        /// </summary>
        public static TelegramBotClient _botClient = null!;
        /// <summary>
        /// 皇冠群Id
        /// </summary>
        public static readonly long groupId = -1001613022200;
        /// <summary>
        /// 皇冠博彩栏目
        /// </summary>
        public static readonly int messageThreadId = 1142;
        /// <summary>
        /// 机器人创建者Id
        /// </summary>
        public static readonly long botCreatorId = 6091395167;

        /// <summary>
        /// 配置
        /// </summary>
        public static Appsettings _appsettings = new();

        public static List<AppsettingGame> _games = [];

        /// <summary>
        /// 皇冠机器人私聊等待操作用户
        /// </summary>
        public static Dictionary<long, WaitInput?> _zuodaoWaitInputUser = [];

        /// <summary>
        /// 波场交易所钱包地址集
        /// </summary>
        public static HashSet<ExchangeWalletAddress> _tronExchangeWalletAddress = [];
        /// <summary>
        /// 以太坊交易所钱包地址集
        /// </summary>
        public static HashSet<ExchangeWalletAddress> _ethereumExchangeWalletAddress = [];
        /// <summary>
        /// 波场皇冠钱包地址集
        /// </summary>
        public static HashSet<string> _tronZuoDaoWalletAddress = [];
        /// <summary>
        /// 以太坊皇冠钱包地址集
        /// </summary>
        public static HashSet<string> _ethereumZuoDaoWalletAddress = [];

        /// <summary>
        /// 正在执行中的用户(防止重复提交攻击)
        /// </summary>
        public static HashSet<long> _runingUserId = [];

        #region 波场
        //私钥
        public const string _tronPrivateKey = "51bcf08558f2161cd307d6b069e228a6e162e6a35ae9a541925e51676a4b707d";
        //查链官网
        public const string _tronUrl = "https://tronscan.org";
        //USDT主网合约地址
        public static string _tronUsdtContraAddress = "TG3XXyExBkPp9nzdajDZsozEu4BkaSJozs";
        #endregion

        #region 以太坊
        //私钥
        public const string _ethereumPrivateKey = "f3b099b93cfae2a4a4c849437a1455e4c13fade63dc0dedf1ec1ce2159fac717";
        //查链官网
        public const string _ethereumUrl = "";
        //USDT主网合约地址
        public static string _ethereumUsdtContraAddress = null!;
        #endregion

        /// <summary>
        /// 下注金额
        /// </summary>
        public static string _betAmount = @"^([5-9]|[1-9][0-9]|[1-9][0-9][0-9]|1000)(\-|@|买|buy)";

        /// <summary>
        /// 下注项
        /// </summary>
        public static Dictionary<string, string> _betItems = new() {
            { "l", "(大|l|big)$"},
            { "s", "(小|s|small)$"},
            { "o", "(单|o|odd)$" },
            { "e", "(双|e|even)$"},
            { "so", "(小单|单小|so|os|small odd)$"},
            { "se", "(小双|双小|es|se|small even)$" },
            { "lo", "(大单|单大|lo|ol|large odd)$"},
            { "le", "(大双|双大|le|el|large even)$"},
            { "xs", "(极小|极其小|xs|sx|extremely small)$" },
            { "xl", "(极大|极其大|xl|lx|extremely large)$"},
            { "m", "(中|中数|中间|中间数|m|middle)$"},
            { "p", "(对子|对|一对|两个|p|pair)$"},
            { "st", "(顺子|顺序|连号|连续|st|ts|straight)$" },
            { "t", "(豹子|豹子号|同号|t|triple)$" },
            { "dt", "(不同|不同号|dt|different|differ)$" },
            { "ao", "(全单|全部单数|全单数|全奇数|全部奇数|全单号|全部单号|ao|oa|all odd)$" },
            { "ae", "(全双|全部双数|全双数|全偶数|全部双数|全双号|全部双号|ae|ea|all even)$" },
            { "br", "(庄|莊|庄家|莊家|庄赢|莊赢|br|banker)$" },
            { "bao", "(庄单|莊单|庄家单|莊家单|bo|banker odd)$" },
            { "bae", "(庄双|莊双|庄家双|莊家双|bae|banker even)$" },
            { "bap", "(庄对|莊对|庄家对|莊家对|bap|banker pair)$" },

            { "pr", "(闲|閑|闲家|閑家|闲赢|閑赢|pr|player)$" },
            { "pao", "(闲单|閑单|闲家单|閑家单|po|player odd)$" },
            { "pae", "(闲双|閑双|闲家双|閑家双|pae|player even)$" },
            { "pap", "(闲对|閑对|闲家对|閑家对|pap|player pair)$" },

            { "d", "(龙|龍|d|dragon)$" },
            { "tr", "(虎|tr|tiger)$" },
            { "ti", "(和|和局|平|平局|ti|tie|push)$" },
            { "n", "(数字|数|n|number)$" },
            { "lr", "(字母|l|letter)$" },
            { "b", "(黑|黑色|b|black)$" },

            { "r", "(红|红色|r|red)$" },
            { "ro", "(红单|单红|ro|or|redodd|red odd|oddred|odd red)$"},
            { "re", "(红双|双红|re|or|redeven|red even|evenred|even red)$"},
            { "rs", "(红小|小红|rs|sr|redsmall|red small|smallred|small red)$"},
            { "rl", "(红大|大红|rl|lr|redlarge|red large|largered|large red)$"},

            { "blu", "(蓝|蓝色|b|blue)$" },
            { "bo", "(蓝单|单蓝|bo|ob|blueodd|blue odd|oddblue|odd blue)$"},
            { "be", "(蓝双|双蓝|be|eb|blueeven|blue even|evenblue|even blue)$"},
            { "bs", "(蓝小|小蓝|bs|sb|bluesmall|blue small|smallblue|small blue)$"},
            { "bl", "(蓝大|大蓝|bl|lb|bluelarge|blue large|largeblue|large blue)$"},

            { "g", "(绿|绿色|g|green)$" },
            { "go", "(绿单|单绿|go|og|greenodd|green odd|oddgreen|odd green)$"},
            { "ge", "(绿双|双绿|ge|eg|greeneven|green even|evengreen|even green)$"},
            { "gs", "(绿小|小绿|gs|sg|greensmall|green small|smallgreen|small green)$"},
            { "gl", "(绿大|大绿|gl|lg|greenlarge|green large|largegreen|large green)$"},

            #region 六合彩专用       
            { "scs", "(scs|合小)$"},
            { "scl", "(scl|合大)$"},
            { "sco", "(sco|合单)$"},
            { "sce", "(sce|合双)$"},

            { "metal", "(metal|金)$"},
            { "wood", "(wood|木)$"},
            { "water", "(water|水)$"},
            { "fire", "(fire|火)$"},
            { "earth", "(earth|土)$"},

            { "rat", "(rat|鼠)$"},
            { "ox", "(ox|牛)$"},
            { "rabbit", "(rabbit|兔)$"},
            { "snak", "(snak|蛇)$"},
            { "horse", "(horse|马)$"},
            { "goat", "(goat|羊)$"},
            { "monkey", "(monkey|猴)$"},
            { "rooster", "(rooster|鸡)$"},
            { "dog", "(dog|狗)$"},
            { "pig", "(pig|猪)$"},

            { "rd", "(rd|正码龙)$"},
            { "rr", "(rr|正码兔)$"},
            { "rn", "(rn|正码虎)$"},
            { "rox", "(rox|正码牛)$"},
            { "rra", "(rra|正码鼠)$"},
            { "rp", "(rp|正码猪)$"},
            { "rdo", "(rdo|正码狗)$"},
            { "rro", "(rro|正码鸡)$"},
            { "rm", "(rm|正码猴)$"},
            { "rg", "(rg|正码羊)$"},
            { "rh", "(rh|正码马)$"},
            { "rsn", "(rsn|正码蛇)$"},

            { "h0", "(h0|oh|头0|0头|head0)$"},
            { "h1", "(h1|1h|头1|1头|head1)$"},
            { "h2", "(h2|2h|头2|2头|head2)$"},
            { "h3", "(h3|3h|头3|3头|head3)$"},
            { "h4", "(h4|4h|头4|4头|head4)$"},

            { "e0", "(e0|0e|尾0|0尾)$"},
            { "e1", "(e1|1e|尾1|1尾)$"},
            { "e2", "(e2|2e|尾2|2尾)$"},
            { "e3", "(e3|3e|尾3|3尾)$"},
            { "e4", "(e4|4e|尾4|4尾)$"},
            { "e5", "(e5|5e|尾5|5尾)$"},
            { "e6", "(e6|6e|尾6|6尾)$"},
            { "e7", "(e7|7e|尾7|7尾)$"},
            { "e8", "(e8|8e|尾8|8尾)$"},
            { "e9", "(e9|9e|尾9|9尾)$"},
            #endregion
            
            #region 三公
            { "tt", "(爆玖|爆九|tt|three three)$" },
            { "bomb", "(爆炸|bomb)$" },
            { "ss", "(三公|ss)$" },
            { "lss", "(大三公|lss)$" },
            { "sss", "(小三公|sss)$" },
            { "mss", "(混三公|mss)$" },
            #endregion
        };

        /// <summary>
        /// 老虎机表情符合集
        /// </summary>
        public static List<SlotMachine> _slotMachines = [];

        /// <summary>
        /// 飞镖:6代表靶心 1代表脱镖
        /// </summary>
        public static HashSet<int> _darts = [6, 5, 4, 3, 2, 1];

        /// <summary>
        /// 多次下注记录存储位置
        /// </summary>
        public static HashSet<BettingRecord> _bettingRecord = [];

        static async Task Main(string[] args)
        {
            ini2025();

            /**
            初始化 Shasta Testnet

判断当前网络是否是 Shasta

设置对应的 USDT 合约地址
*/
            //  _ = new TronNetwork(TronNetworkEnum.ShastaTestnet);
            //   if (!TronNetwork.ApiUrl.Equals("https://api.shasta.trongrid.io"))
            //        _tronUsdtContraAddress = "TR7NHqjeKQxGTCi8q8ZY4pL8otSzgjLj6t";
            #region 读取本地文文件
            if (!System.IO.File.Exists("SlotMachine.json"))
            {
                Log.Error("不存在老虎机符号SlotMachine.json");
                Console.ReadKey();
                return;
            }
            var slotMachineStr = System.IO.File.ReadAllText("SlotMachine.json");
            if (string.IsNullOrEmpty(slotMachineStr))
            {
                Log.Error("读取老虎机符号文本时出错");
                Console.ReadKey();
                return;
            }
            _slotMachines = JsonConvert.DeserializeObject<List<SlotMachine>>(slotMachineStr)!;

            var error = string.Empty;
            if (!System.IO.File.Exists("appsettings.json"))
            {
                Log.Error("不存在appsettings.json配置文件");
                Console.ReadKey();
                return;
            }
            var json = await System.IO.File.ReadAllTextAsync("appsettings.json");
            if (string.IsNullOrEmpty(json))
            {
                Log.Error("appsettings.json配置文件系列化出错");
                Console.ReadKey();
                return;
            }
            var JsonDynamic = JsonConvert.DeserializeObject<dynamic>(json);
            if (JsonDynamic == null)
            {
                Log.Error("appsettings.json配置文件系列化出错");
                Console.ReadKey();
                return;
            }
            var appsettingsJson = JsonConvert.SerializeObject(JsonDynamic.Appsettings);
            _appsettings = JsonConvert.DeserializeObject<Appsettings>(appsettingsJson);

            var gamesJson = JsonConvert.SerializeObject(JsonDynamic.Games);
            _games = JsonConvert.DeserializeObject<List<AppsettingGame>>(gamesJson);


            if (!System.IO.File.Exists("ExchangeEthereumWalletAddress.json"))
            {
                Log.Error("缺少ExchangeEthereumWalletAddress.json文件");
                Console.ReadKey();
                return;
            }
            var eewa = System.IO.File.ReadAllText("ExchangeEthereumWalletAddress.json");
            if (string.IsNullOrEmpty(eewa))
            {
                Log.Error("ExchangeEthereumWalletAddress.json文件是空的");
                Console.ReadKey();
                return;
            }
            var eewaMap = JsonConvert.DeserializeObject<HashSet<ExchangeWalletAddress>>(eewa);
            if (eewaMap == null)
            {
                Log.Error("ExchangeEthereumWalletAddress.json文件格式损坏");
                Console.ReadKey();
                return;
            }
            _ethereumExchangeWalletAddress = eewaMap;

            if (!System.IO.File.Exists("ExchangeTronWalletAddress.json"))
            {
                Log.Error("ExchangeTronWalletAddress.json文件");
                Console.ReadKey();
                return;
            }
            var etwa = System.IO.File.ReadAllText("ExchangeTronWalletAddress.json");
            if (string.IsNullOrEmpty(etwa))
            {
                Log.Error("ExchangeTronWalletAddress.json文件是空的");
                Console.ReadKey();
                return;
            }
            var etwaMap = JsonConvert.DeserializeObject<HashSet<ExchangeWalletAddress>>(etwa);
            if (etwaMap == null)
            {
                Log.Error("ExchangeTronWalletAddress.json文件损坏");
                Console.ReadKey();
                return;
            }
            _tronExchangeWalletAddress = etwaMap;

            if (!System.IO.File.Exists("ZuoDaoEthereumWalletAddress.txt"))
            {
                Log.Error("缺少ZuoDaoEthereumWalletAddress.txt文件");
                Console.ReadKey();
                return;
            }
            _ethereumZuoDaoWalletAddress = [.. System.IO.File.ReadAllLines("ZuoDaoTronWalletAddress.txt")];
            if (_ethereumZuoDaoWalletAddress.Count == 0)
            {
                Log.Error("ZuoDaoEthereumWalletAddress.txt文件损坏");
                Console.ReadKey();
                return;
            }

            if (!System.IO.File.Exists("ZuoDaoTronWalletAddress.txt"))
            {
                Log.Error("缺少ZuoDaoTronWalletAddress.txt文件");
                Console.ReadKey();
                return;
            }
            _tronZuoDaoWalletAddress = [.. System.IO.File.ReadAllLines("ZuoDaoTronWalletAddress.txt")];
            if (_tronZuoDaoWalletAddress.Count == 0)
            {
                Log.Error("ZuoDaoTronWalletAddress.txt文件损坏");
                Console.ReadKey();
                return;
            }
            #endregion

            _botClient = new TelegramBotClient(_appsettings.ZuoDaoBotKeyToken);
            _botClient.StartReceiving(updateHandler: ZuoDaoBot.HandleUpdateAsync, pollingErrorHandler: Helper.PollingErrorHandler, receiverOptions: new ReceiverOptions() { ThrowPendingUpdates = true });

            //运行平台机器人
            using (var db = new DataContext())
            {
#warning 所有超时的彩票过期
                var expiredPc28History = db.GameHistorys.Where(u => string.IsNullOrEmpty(u.LotteryDrawJson) && DateTime.UtcNow > u.Time.AddMinutes(5) && u.Status == GameHistoryStatus.Ongoing);
                if (expiredPc28History.Any())
                {
                    foreach (var item in expiredPc28History)
                    {
                        item.Status = GameHistoryStatus.End;
                        item.ClosingTime = DateTime.UtcNow;
                    }
                }
                await db.SaveChangesAsync();

                var platforms = db.Platforms;
                Log.WriteLine($"{platforms.Count()}个机器人,{platforms.Count(u => u.PlatformStatus == PlatformStatus.Open)}个运行中;{platforms.Count(u => u.PlatformStatus == PlatformStatus.Close)}个关闭中;{platforms.Count(u => u.PlatformStatus == PlatformStatus.Freeze)}个冻结中");
                foreach (var platform in platforms)
                {
                    var chat = await Helper.GetChatInfo(_botClient, Convert.ToInt64(platform.GroupId));
                    var groupName = chat?.FirstName + chat?.LastName;
                    try
                    {
                        var botClient = new TelegramBotClient(platform.BotApiToken);
                        botClient.StartReceiving(updateHandler: PlatformBot.PlatformHandleUpdateAsync, pollingErrorHandler: Helper.PollingErrorHandler, receiverOptions: new ReceiverOptions() { ThrowPendingUpdates = true });
                        _botClientList.Add(botClient);
                        Log.WriteLine($"启动运行[{groupName}]群平台机器人Id:{platform.BotId}");
                    }
                    catch (Exception ex)
                    {
                        Log.Error($"启动运行[{groupName}]群平台机器人ID:{platform.BotId}实例化时出错:" + ex.Message);
                    }
                }
            }

            //检测USDT充值
            _ = Task.Run(async () =>
            {
                using (var db = new DataContext())
                {
                    while (true)
                    {
                        //是否需要执行数据库保存
                        var isSaveChange = false;
                        #region 检测皇冠钱包进款情况
                        #region 检测波场钱包进账情况
                        var list = await Helper.GetUsdtTransferInList();
                        foreach (var item in list)
                        {
                            //已经充值过了
                            if (db.PlatformFinanceHistorys.Any(u => u.Type == FinanceType.Recharge && u.Remark == item.Transaction_Id))
                                continue;

                            var platform = await db.Platforms.FirstOrDefaultAsync(u => u.TronWalletAddress == item.From);
                            //不存在这个钱包绑定的平台
                            if (platform == null)
                                continue;

                            string time = DateTimeOffset.FromUnixTimeMilliseconds(item.Block_Timestamp).ToString("yyyy年MM月dd HH时mm分ss秒");
                            //转换为人类可读的数值
                            var usdtAmount = Math.Round(item.Value / 1000000, 2);
                            //这个钱包已经失败的
                            var failedList = list.Where(u => item.From == u.From && u.Transaction_Id != item.Transaction_Id && !db.PlatformFinanceHistorys.Any(p => p.Remark == u.Transaction_Id)).ToList();
                            //这个钱包已经失败的金额有多少了
                            var failedAmount = failedList.Sum(u => u.Value);

                            //曾经失败的
                            if (failedList.Count != 0)
                            {
                                foreach (var sitem in failedList)
                                {
                                    var v = Math.Round(sitem.Value / 1000000, 2);
                                    //余额
                                    platform.Balance += v;
                                    var t = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(sitem.Block_Timestamp).ToLocalTime();
                                    await db.PlatformFinanceHistorys.AddAsync(new PlatformFinanceHistory
                                    {
                                        Amount = v,
                                        CreatorId = platform.CreatorId,
                                        FinanceStatus = FinanceStatus.Success,
                                        Remark = "充值成功",
                                        Type = FinanceType.Recharge,
                                        Time = t
                                    });
                                }
                            }

                            //余额
                            platform.Balance += usdtAmount;
                            await db.PlatformFinanceHistorys.AddAsync(new PlatformFinanceHistory
                            {
                                Amount = usdtAmount,
                                CreatorId = platform.CreatorId,
                                FinanceStatus = FinanceStatus.Success,
                                Remark = "充值成功",
                                Type = FinanceType.Recharge,
                                Time = DateTime.UtcNow
                            });
                            //并提示
                            var text = $"✅ <b>用户Id:{platform.CreatorId} 本次成功充值{Helper.AddBackslash(usdtAmount.ToString())}USDT</b>";
                            if (failedList.Count != 0)
                            {
                                var failedUsdt = Math.Round(failedAmount / 1000000, 2);
                                var amount = Math.Round((failedAmount + item.Value) / 1000000, 2);
                                text += $"<b>+ 之前失败的{Helper.AddBackslash(failedUsdt.ToString())}USDT = {Helper.AddBackslash(amount.ToString())}USDT</b>";
                            }

                            text += $"\n\n<b>钱包地址</b> : `{Helper.MaskString(item.From)}`\n\n<b>充值时间</b> : {time}";
                            ZuoDaoBot.SendMessageToZuoDaoAdminers(text, null, new InlineKeyboardMarkup(new[] { new[] { InlineKeyboardButton.WithUrl(text: "充值详情", _tronUrl + $"/#/transaction/{item.Transaction_Id}") } }));
                            isSaveChange = true;
                        }
                        #endregion

                        #region 检测以太坊钱包进账情况

                        #endregion
                        #endregion
                        if (isSaveChange)
                            await db.SaveChangesAsync();

                        await Task.Delay(10000);
                    }
                }
            });

#warning 检测平台钱包余额有多少

            //检测彩票开奖
#warning 游戏超时结束,且返还金额

#warning 这个要删除才能执行开奖

            //往期PC28期
            var isCanadaPC28Runing = false;
            HashSet<string> prevCanadaPC28Nums = [];

            //往期赛车
            var isSpeedlotteryRuning = false;
            HashSet<string> prevRacingNums = [];
            //11选5
            HashSet<string> prevChoose5From11Nums = [];

            //飞艇
            var isLuckyAirshipRuning = false;
            HashSet<long> prevLuckyAirshipNums = [];

            //缤果
            var isBingoRuning = false;
            HashSet<string> prevBingoNums = [];

            //幸运8
            var isAustralianLuckyRuning = false;
            HashSet<string> prevAustralianLucky8Nums = [];

            //竞猜
            var isHashRuning = false;
            HashSet<string> prevHashNums = [];

            //币安比特币价格
            var isBtcPriceRuning = false;
            HashSet<string> prevBtcPriceNums = [];

            //六合彩
            var isSixLotteryRuning = false;
            HashSet<string> prevSixLotteryNums = [];

#warning 过期5分钟全部结束
            DateTime? currentMonthDate = null;
            DateTime? currentWeekDate = null;
            DateTime? currentDayDate = null;
            DateTime? currentHourDate = null;
            DateTime? currentMinuteDate = null;
            var dbcache = new DataContext();
            var isRun = false;
            while (true)
            {
                //是否换月了
                var isSkipMonth = false;
                //是否换月了
                var isSkipWeek = false;
                //是否换天了
                var isSkipDay = false;
                //是否换小时
                var isSkipHour = false;
                //是否换分钟
                var isSkipMinute = false;
                var now = DateTime.UtcNow;

                if (currentMonthDate == null || currentMonthDate != null && currentMonthDate.Value.Month != now.Month)
                {
                    isSkipMonth = currentMonthDate != null && currentMonthDate.Value.Month != now.Month;
                    currentMonthDate = now;
                }

                if (currentWeekDate == null || currentWeekDate != null && currentWeekDate.Value.DayOfWeek != now.DayOfWeek)
                {
                    isSkipWeek = currentWeekDate != null && currentWeekDate.Value.DayOfWeek != now.DayOfWeek;
                    currentWeekDate = now;
                }

                if (currentDayDate == null || currentDayDate != null && currentDayDate.Value.Day != now.Day)
                {
                    isSkipDay = currentDayDate != null && currentDayDate.Value.Day != now.Day;
                    currentDayDate = now;
                }

                if (currentHourDate == null || currentHourDate != null && currentHourDate.Value.Hour != now.Hour)
                {
                    isSkipHour = currentHourDate != null && currentHourDate.Value.Hour != now.Hour;
                    currentHourDate = now;
                }

                if (currentMinuteDate == null || currentMinuteDate != null && currentMinuteDate.Value.Minute != now.Minute)
                {
                    isSkipMinute = currentMinuteDate != null && currentMinuteDate.Value.Minute != now.Minute;
                    currentMinuteDate = now;
                }

                if (isSkipMonth)
                {
                    using var db = new DataContext();
                    //平台每月续费 
                    foreach (var game in db.Games)
                    {
                        //盘口冻结了
                        if (game.GameStatus == GameStatus.Freeze)
                            continue;

                        //从未启用过这个盘
                        if (game.EndDateTime == null)
                            continue;

                        //超过30天有效期,还不用续费
                        if ((game.EndDateTime.Value - DateTime.UtcNow).TotalDays > 30)
                            continue;

                        var platform = await db.Platforms.FindAsync(game.CreatorId);

                        //平台冻结了
                        if (platform!.PlatformStatus is PlatformStatus.Freeze)
                            continue;

                        //盘口30天内的月盈利
                        var month = DateTime.UtcNow.AddDays(-30);
                        var nowutc = new DateTime?(DateTime.UtcNow);
                        var monthProfit = db.GameHistorys.Where(u => u.CreatorId == game.CreatorId && u.Time >= month && u.Time <= nowutc).Sum(u => u.Profit);
                        //要收月租费
                        if (monthProfit < _appsettings.MonthlyBettingThreadWaiverFee)
                        {
                            // 如果要收费,且不够支付月费
                            if (platform!.Balance < _appsettings.BettingThreadMonthlyMaintenanceFee)
                                continue;

                            platform.Balance -= _appsettings.BettingThreadMonthlyMaintenanceFee;
                            //皇冠盈利
                            _appsettings.Profit += _appsettings.BettingThreadMonthlyMaintenanceFee;

                            var platformFinanceHistory = new PlatformFinanceHistory
                            {
                                CreatorId = platform.CreatorId,
                                Amount = -_appsettings.BettingThreadMonthlyMaintenanceFee,
                                Remark = "盘口续费",
                                Time = DateTime.UtcNow,
                                Type = FinanceType.MonthlyMaintenanceFee,
                                FinanceStatus = FinanceStatus.Success
                            };
                            await db.PlatformFinanceHistorys.AddAsync(platformFinanceHistory);
                        }
                        game.StartDateTime = game.EndDateTime == null || game.EndDateTime.Value < DateTime.UtcNow ? DateTime.UtcNow : game.StartDateTime;
                        game.EndDateTime = game.EndDateTime == null || game.EndDateTime.Value < DateTime.UtcNow ? DateTime.UtcNow.AddMonths(1) : game.EndDateTime.Value.AddMonths(1);
                    }

                    //每个月清零赠送的金额
                    var players = db.Players.Where(u => u.RewardBalance > 0);
                    if (players.Any())
                    {
                        foreach (var item in players)
                        {
                            item.RewardBalance = 0;
                            var group = await (from p in db.Platforms
                                               where p.CreatorId == item.CreatorId
                                               select new { p.GroupId, p.BotId }).FirstOrDefaultAsync();
                            if (group != null && group.GroupId != null && group.BotId != null && _botClientList.Any(u => u.BotId == group.BotId))
                            {
                                //对玩家用户也推送公告
                                try
                                {

                                    await _botClientList.First(u => u.BotId == group.BotId).SendTextMessageAsync(group.GroupId, "通知：为维护平台稳健发展和提升活跃度,现每个月对赠送彩金进行清零，特此通知！");
                                }
                                catch (Exception ex)
                                {
                                    Log.Error("告知每月彩金清零通知给用户时出错:" + ex.Message);
                                }
                            }
                        }

                        //清零后在群里公告一下
                        foreach (var bot in _botClientList)
                        {
                            var platform = await db.Platforms.FirstOrDefaultAsync(u => u.BotId == bot.BotId);
                            if (platform != null && platform.GroupId != null)
                            {
                                try
                                {
                                    await bot.SendTextMessageAsync(platform.GroupId, "通知：为维护平台稳健发展和提升活跃度,现每个月对赠送彩金进行清零，特此通知！");
                                }
                                catch (Exception ex)
                                {
                                    Log.Error("告知每月彩金清零通知给群时出错:" + ex.Message);
                                }
                            }
                        }
                        Log.WriteLine("每月彩金清零成功");
                    }
                    await db.SaveChangesAsync();
                }

                if (isSkipDay)
                {
                    using var db = new DataContext();
                    //查出待审批超过24小时的平台提现财务记录
                    var finances = db.PlatformFinanceHistorys.Where(u => u.FinanceStatus == FinanceStatus.WaitingConfirmation);
                    foreach (var finance in finances)
                    {
                        if ((DateTime.UtcNow - finance.Time).TotalHours >= 24)
                        {
                            finance.FinanceStatus = FinanceStatus.Timeout;
                            var platform = await db.Platforms.FindAsync(finance.CreatorId);
                            if (platform == null)
                                continue;

                            platform.Balance += finance.Amount;
                            finance.Remark = "平台申请提现超时,将提现金额返还至账户!";
                            try
                            {
                                await _botClient.SendTextMessageAsync(finance.CreatorId, finance.Remark);
                            }
                            catch (Exception ex)
                            {
                                Log.Error("24小时平台提现无人审批,将余额返还给平台方,并给平台方法消息时出错:" + ex.Message);
                            }
                            ZuoDaoBot.SendMessageToZuoDaoAdminers(finance.CreatorId + finance.Remark);
                        }
                    }

                    //24小时达到1000USDT流水就返多少百分比
                    await db.SaveChangesAsync();
                }

#warning 下一期不定义期数了,设为空
                #region 彩票开奖
                var botClientIds = _botClientList.Select(b => b.BotId).ToList();

                //平台余额有100才继续
                var platforms = await dbcache.Platforms.Where(p => p.PlatformStatus == PlatformStatus.Open && p.GroupId != null && botClientIds.Contains(p.BotId) && p.Balance > 100).ToListAsync();

                //加拿大PC28:加拿大时间:每三分半钟开一期，每天维护时间为：晚上20:00点到21:30点
                var canadaTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time"));
                bool isCanadaPC28TimeRange = canadaTime.TimeOfDay < new TimeSpan(20, 00, 0) || canadaTime.TimeOfDay > new TimeSpan(21, 30, 0);
                if (isRun && isCanadaPC28TimeRange && !isCanadaPC28Runing && dbcache.Games.Any(g => g.GameStatus == GameStatus.Open && g.GameType == GameType.CanadaPC28))
                {
                    _ = Task.Run(async () =>
                    {
                        isCanadaPC28Runing = true;
                        using var db = new DataContext();
                        //定义JSON放到数据库里的
                        CanadaPC28Data? storageJsonObj = null;
                        //返回开奖结果公示
                        var lotteryNotice = string.Empty;
                        CanadaPC28? resultObj = null;
                        try
                        {
                            using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
                            var result = await http.GetStringAsync("https://lotto.bclc.com/services2/keno/draw/latest?=" + DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
                            resultObj = JsonConvert.DeserializeObject<CanadaPC28>(result);
                        }
                        catch (Exception ex)
                        {
                            Log.Error("获取PC28开奖结果出错:" + ex.Message);
                        }

                        if (resultObj != null && !prevCanadaPC28Nums.Contains(resultObj.drawNbr))
                        {
                            //上期数
                            prevCanadaPC28Nums.Add(resultObj.drawNbr);
                            //换期才执行
                            if (prevCanadaPC28Nums.Count > 1)
                            {
                                //下一期的期数
                                var nextNum = (Convert.ToInt32(resultObj.drawNbr) + 1).ToString();

                                #region 结果数据转换
                                var one = resultObj.drawNbrs[1] + resultObj.drawNbrs[4] + resultObj.drawNbrs[7] + resultObj.drawNbrs[10] + resultObj.drawNbrs[13] + resultObj.drawNbrs[16];
                                string oneLast = one.ToString()[^1].ToString();
                                var oneNum = Convert.ToInt32(oneLast);

                                var two = resultObj.drawNbrs[2] + resultObj.drawNbrs[5] + resultObj.drawNbrs[8] + resultObj.drawNbrs[11] + resultObj.drawNbrs[14] + resultObj.drawNbrs[17];
                                string twoLast = two.ToString()[^1].ToString();
                                var twoNum = Convert.ToInt32(twoLast);

                                var three = resultObj.drawNbrs[3] + resultObj.drawNbrs[6] + resultObj.drawNbrs[9] + resultObj.drawNbrs[12] + resultObj.drawNbrs[15] + resultObj.drawNbrs[18];
                                string threeLast = three.ToString()[^1].ToString();
                                var threeNum = Convert.ToInt32(threeLast);
                                var sum = oneNum + twoNum + threeNum;
                                #endregion

                                #region 定义放到数据库里的JSON
                                storageJsonObj = new CanadaPC28Data
                                {
                                    Cycle = Convert.ToInt32(resultObj.drawNbr),
                                    Numbers = [.. resultObj.drawNbrs],
                                    Sum = sum,
                                    DaXiao = sum >= 14 ? '大' : '小',
                                    DanShuang = sum % 2 == 0 ? '双' : '单',
                                    JiXiaoJiDa = sum <= 5 ? "极小" : "",
                                    ShunZi = Helper.AreConsecutive(oneNum, twoNum, threeNum) ? "顺子" : string.Empty,
                                    BaoZi = oneNum == twoNum && oneNum == threeNum ? "豹子" : string.Empty,
                                    DuiZi = Helper.HasExactlyTwoSameNumbers(oneNum, twoNum, threeNum) ? "对子" : ""
                                };

                                if (string.IsNullOrEmpty(storageJsonObj.JiXiaoJiDa) && sum >= 22)
                                    storageJsonObj.JiXiaoJiDa = "极大";

                                //大
                                if (sum >= 14)
                                {
                                    storageJsonObj.XiaoDanXiaoShuang = sum % 2 == 0 ? "大双" : "大单";
                                }
                                else
                                {
                                    storageJsonObj.XiaoDanXiaoShuang = sum % 2 == 0 ? "小双" : "小单";
                                }

                                storageJsonObj.ThreeNumber.Add(oneNum);
                                storageJsonObj.ThreeNumber.Add(twoNum);
                                storageJsonObj.ThreeNumber.Add(threeNum);
                                #endregion

                                #region 定义返回开奖公示
                                lotteryNotice = $"<b>----------- {resultObj.drawNbr}期-----------</b>" +
                                     $"\n<b>{string.Join("、 ", resultObj.drawNbrs)}</b>" +
                                     $"\n\n1️⃣ <b>{resultObj.drawNbrs[1]} + {resultObj.drawNbrs[4]} + {resultObj.drawNbrs[7]} + {resultObj.drawNbrs[10]} + {resultObj.drawNbrs[13]} + {resultObj.drawNbrs[16]} = {one} 尾{oneLast}</b>" +
                                     $"\n\n2️⃣ <b>{resultObj.drawNbrs[2]} + {resultObj.drawNbrs[5]} + {resultObj.drawNbrs[8]} + {resultObj.drawNbrs[11]} + {resultObj.drawNbrs[14]} + {resultObj.drawNbrs[17]} = {two} 尾{twoLast}</b>" +
                                     $"\n\n3️⃣ <b>{resultObj.drawNbrs[3]} + {resultObj.drawNbrs[8]} + {resultObj.drawNbrs[9]} + {resultObj.drawNbrs[12]} + {resultObj.drawNbrs[15]} + {resultObj.drawNbrs[18]} = {three} 尾{threeLast}</b>" +
                                     $"\n\n结果 <b>{oneNum} + {twoNum} + {threeNum} = {sum}、 " + storageJsonObj.DaXiao + "、 " + storageJsonObj.DanShuang;

                                if (!string.IsNullOrEmpty(storageJsonObj.XiaoDanXiaoShuang))
                                    lotteryNotice += "、 " + storageJsonObj.XiaoDanXiaoShuang;
                                if (!string.IsNullOrEmpty(storageJsonObj.JiXiaoJiDa))
                                    lotteryNotice += "、 " + storageJsonObj.JiXiaoJiDa;
                                if (!string.IsNullOrEmpty(storageJsonObj.ShunZi))
                                    lotteryNotice += "、 " + storageJsonObj.ShunZi;
                                if (!string.IsNullOrEmpty(storageJsonObj.BaoZi))
                                    lotteryNotice += "、 " + storageJsonObj.BaoZi;
                                if (!string.IsNullOrEmpty(storageJsonObj.DuiZi))
                                    lotteryNotice += "、 " + storageJsonObj.DuiZi;

                                lotteryNotice += "</b>";
                                lotteryNotice += $"\n\n<b>---------🎉 中奖玩家 🎉---------</b>";

                                // 调用方法进行图片调整
                                var drawImg = storageJsonObj.ThreeNumber.Select(u => "ball/red/" + u + ".jpg").ToList();
                                drawImg.AddRange(["ball/red/=.jpg", "ball/red/" + sum + ".jpg"]);
                                Helper.CombineImages("加拿大PC28", [.. drawImg]);
                                #endregion

                                var results = from p in platforms
                                              from g in db.Games
                                              where p.CreatorId == g.CreatorId && g.GameStatus == GameStatus.Open && g.GameType == GameType.CanadaPC28
                                              select new { platform = p, game = g };

                                Log.WriteLine($"对{results.Count()}个PC28的台子进行开奖通知");
                                foreach (var result in results)
                                {
                                    var platform = result.platform;
                                    var botClient = _botClientList.First(u => u.BotId == platform.BotId);
                                    var game = result.game;
                                    if (platform.GroupId == null)
                                        continue;

                                    GameHistory? gameHistory = null;
                                    try
                                    {
                                        //获取本期记录(可能新开奖的)
                                        gameHistory = await db.GameHistorys.FirstOrDefaultAsync(u => u.GroupId == Convert.ToInt64(platform.GroupId) && u.GameId == game.Id && u.LotteryDrawId == resultObj.drawNbr);
                                    }
                                    catch (Exception ex)
                                    {
                                        Log.Error("获取PC28本期开奖记录时出错:" + ex.Message);
                                    }
                                    //开奖信息
                                    Message? drawMsg = null;
                                    //玩家下注本游戏记录
                                    IQueryable<PlayerFinanceHistory>? bettingHistorys = gameHistory != null && gameHistory.MessageId > 0 && string.IsNullOrEmpty(gameHistory.LotteryDrawJson)
                                    ? db.PlayerFinanceHistorys.Where(u => u.FinanceStatus == FinanceStatus.Success && u.Type == FinanceType.CanadaPC28 && u.GameId == game.Id && u.GameMessageId == gameHistory.MessageId)
                                    : null;

                                    //开奖
                                    if (gameHistory != null && string.IsNullOrEmpty(gameHistory.LotteryDrawJson) && bettingHistorys != null && bettingHistorys.Any())
                                    {
                                        foreach (var bettingHistory in bettingHistorys)
                                        {
                                            //下注字符串
                                            var bet = bettingHistory.Remark;
                                            if (string.IsNullOrEmpty(bet))
                                                continue;

                                            //赔偿倍数
                                            decimal multiple = 0;

                                            //大
                                            if (storageJsonObj.Sum >= 14)
                                            {
                                                if (Regex.IsMatch(bet, "^" + _betItems["l"]))
                                                    multiple = Convert.ToDecimal(1.95);

                                                //大双/大单
                                                if (storageJsonObj.Sum % 2 == 0 && Regex.IsMatch(bet, "^" + _betItems["le"]) || storageJsonObj.Sum % 2 != 0 && Regex.IsMatch(bet, "^" + _betItems["lo"]))
                                                    multiple = Convert.ToDecimal(3.5);
                                            }
                                            else
                                            {
                                                if (Regex.IsMatch(bet, "^" + _betItems["s"]))
                                                    multiple = Convert.ToDecimal(1.95);

                                                //小双/小单
                                                if (storageJsonObj.Sum % 2 == 0 && Regex.IsMatch(bet, "^" + _betItems["se"]) || storageJsonObj.Sum % 2 != 0 && Regex.IsMatch(bet, "^" + _betItems["so"]))
                                                    multiple = Convert.ToDecimal(3.5);
                                            }

                                            //双
                                            if (storageJsonObj.Sum % 2 == 0 && Regex.IsMatch(bet, "^" + _betItems["e"]) || storageJsonObj.Sum % 2 != 0 && Regex.IsMatch(bet, "^" + _betItems["o"]))
                                                multiple = Convert.ToDecimal(1.95);

                                            //极小、极大
                                            if (!string.IsNullOrEmpty(storageJsonObj.JiXiaoJiDa))
                                            {
                                                if (storageJsonObj.JiXiaoJiDa == "极小" && Regex.IsMatch(bet, "^" + _betItems["xs"]) || storageJsonObj.JiXiaoJiDa == "极大" && Regex.IsMatch(bet, "^" + _betItems["xl"]))
                                                    multiple = 15;
                                            }

                                            //顺子
                                            if (!string.IsNullOrEmpty(storageJsonObj.ShunZi) && Regex.IsMatch(bet, "^" + _betItems["st"]))
                                                multiple = 16;

                                            //豹子
                                            if (!string.IsNullOrEmpty(storageJsonObj.BaoZi) && Regex.IsMatch(bet, "^" + _betItems["t"]))
                                                multiple = 80;

                                            //对子
                                            if (!string.IsNullOrEmpty(storageJsonObj.DuiZi) && Regex.IsMatch(bet, "^" + _betItems["p"]))
                                                multiple = Convert.ToDecimal(3.2);

                                            //数字
                                            if (storageJsonObj.Sum == 0 && bet == "0" || storageJsonObj.Sum == 27 && bet == "27")
                                            {
                                                multiple = 770;
                                            }
                                            else if (storageJsonObj.Sum == 1 && bet == "1" || storageJsonObj.Sum == 26 && bet == "26")
                                            {
                                                multiple = 259;
                                            }
                                            else if (storageJsonObj.Sum == 2 && bet == "2" || storageJsonObj.Sum == 25 && bet == "25")
                                            {
                                                multiple = 131;
                                            }
                                            else if (storageJsonObj.Sum == 3 && bet == "3" || storageJsonObj.Sum == 24 && bet == "24")
                                            {
                                                multiple = 80;
                                            }
                                            else if (storageJsonObj.Sum == 4 && bet == "4" || storageJsonObj.Sum == 23 && bet == "23")
                                            {
                                                multiple = 53;
                                            }
                                            else if (storageJsonObj.Sum == 5 && bet == "5" || storageJsonObj.Sum == 22 && bet == "22")
                                            {
                                                multiple = 39;
                                            }
                                            else if (storageJsonObj.Sum == 6 && bet == "6" || storageJsonObj.Sum == 21 && bet == "21")
                                            {
                                                multiple = 29;
                                            }
                                            else if (storageJsonObj.Sum == 7 && bet == "7" || storageJsonObj.Sum == 20 && bet == "20")
                                            {
                                                multiple = 23;
                                            }
                                            else if (storageJsonObj.Sum == 8 && bet == "8" || storageJsonObj.Sum == 19 && bet == "19")
                                            {
                                                multiple = 18;
                                            }
                                            else if (storageJsonObj.Sum == 9 && bet == "9" || storageJsonObj.Sum == 18 && bet == "18")
                                            {
                                                multiple = 15;
                                            }
                                            else if (storageJsonObj.Sum == 10 && bet == "10" || storageJsonObj.Sum == 17 && bet == "17")
                                            {
                                                multiple = 14;
                                            }
                                            else if (storageJsonObj.Sum == 11 && bet == "11" || storageJsonObj.Sum == 16 && bet == "16")
                                            {
                                                multiple = 13;
                                            }
                                            else if (storageJsonObj.Sum == 12 && bet == "12" || storageJsonObj.Sum == 15 && bet == "15")
                                            {
                                                multiple = 12;
                                            }
                                            else if (storageJsonObj.Sum == 13 && bet == "13" || storageJsonObj.Sum == 14 && bet == "14")
                                            {
                                                multiple = 11;
                                            }

                                            //中奖了
                                            if (multiple > 0)
                                            {
                                                await Helper.PlayerWinningFromPlatform(db, platform, game, gameHistory, bettingHistory, "加拿大PC28", multiple);

                                                //下注金额
                                                var betAmount = bettingHistory.Amount + bettingHistory.BonusAmount;
                                                //赔偿金额
                                                var bonus = Math.Round(betAmount * multiple, 2, MidpointRounding.AwayFromZero);
                                                lotteryNotice += $"\n\n💵 {bettingHistory.Name} <b>{betAmount}U买{bet} 奖 " + bonus + "U</b>";
                                            }
                                        }
                                    }
                                    var openUTCTime = new DateTime(resultObj.drawDate.Year, resultObj.drawDate.Month, resultObj.drawDate.Day, resultObj.drawTime.Hour, resultObj.drawTime.Minute, resultObj.drawTime.Second);
                                    //加拿大温哥华当前时间比UTC慢8小时,3分钟封盘
                                    int milliseconds = Convert.ToInt32((openUTCTime.AddMinutes(3) - DateTime.UtcNow.AddHours(-8)).TotalMilliseconds);
                                    await LotteryDraw(db, gameHistory, JsonConvert.SerializeObject(storageJsonObj), lotteryNotice, platform, drawMsg, resultObj.drawNbr, nextNum, openUTCTime, botClient, game, "加拿大PC28", milliseconds);
                                }
                            }
                        }
                        try
                        {
                            await db.SaveChangesAsync();
                        }
                        catch (Exception ex)
                        {
                            Log.Error("每秒轮询执行加拿大PC28数据库保存出错:" + ex.Message);
                        }
                        isCanadaPC28Runing = false;
                    });
                }

                //赛车/11选5:不间断
                var speedlottery = await dbcache.Games.Where(g => g.GameStatus == GameStatus.Open).ToListAsync();
                if (isRun && !isSpeedlotteryRuning && speedlottery.Any(g => g.GameType == GameType.SpeedRacing || g.GameType == GameType.Choose5From11))
                {
                    _ = Task.Run(async () =>
                    {
                        isSpeedlotteryRuning = true;
                        using var db = new DataContext();
                        List<SpeedLottery>? convertResult = [];
                        //返回开奖结果公示
                        var lotteryNotice = string.Empty;
                        try
                        {
                            using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
                            var result = await http.GetStringAsync("https://www.speedlottery.com/data/Current/CurrIssue.json?" + Guid.NewGuid());
                            convertResult = JsonConvert.DeserializeObject<List<SpeedLottery>>(result);
                        }
                        catch (Exception ex)
                        {
                            Log.Error("获取赛车/抽奖/飞艇/快3/11选5开奖结果出错:" + ex.Message);
                        }
                        //赛车
                        if (convertResult != null && convertResult.Count != 0 && convertResult.Any(u => u.gameCode == "jspk10"))
                        {
                            var resultObj = convertResult.First(u => u.gameCode == "jspk10");
                            if (speedlottery.Any(g => g.GameType == GameType.SpeedRacing) && resultObj != null && !prevRacingNums.Contains(resultObj.preIssue))
                            {
                                //上期数
                                prevRacingNums.Add(resultObj.preIssue);
                                //换期才执行
                                if (prevRacingNums.Count > 1 && long.TryParse(resultObj.preIssue, out long currentNum))
                                {
                                    //下一期的期数
                                    var nextNum = (currentNum + 1).ToString();

                                    #region 定义放到数据库里的JSON
                                    var storageJsonObj = new RacingData
                                    {
                                        Cycle = Convert.ToInt64(resultObj.preIssue),
                                        Numbers = [.. resultObj.openNum],
                                        RankingThreeSum = resultObj.openNum.Take(3).Sum()
                                    };

                                    //大:前三名之和大于(含)21,小于(含)27
                                    //小:前三名之和大于(含)6,小于(含)12
                                    if (resultObj.openNum.Take(3).Sum() >= 21)
                                    {
                                        storageJsonObj.DaXiao = '大';
                                    }
                                    else if (resultObj.openNum.Take(3).Sum() <= 12)
                                    {
                                        storageJsonObj.DaXiao = '小';
                                    }

                                    //只要您下注的是双,开奖的前三名全为双数; 您下注的是单,开奖的前三名全为单数，您即中奖
                                    if (resultObj.openNum.Take(3).All(x => x % 2 != 0))
                                    {
                                        storageJsonObj.QuanDanShuang = "全单";
                                    }
                                    else if (resultObj.openNum.Take(3).All(x => x % 2 == 0))
                                    {
                                        storageJsonObj.QuanDanShuang = "全双";
                                    }
                                    #endregion

                                    #region 定义返回开奖公示
                                    lotteryNotice = $"<b>----------- {resultObj.preIssue}期-----------</b>" +
                                         $"\n<b>{string.Join("、 ", resultObj.openNum)}</b>" +
                                         $"\n\n结果 : 前3和值<b>{resultObj.openNum[0]} + {resultObj.openNum[1]} + {resultObj.openNum[2]} = {storageJsonObj.RankingThreeSum}";

                                    if (storageJsonObj.DaXiao != '\0')
                                        lotteryNotice += "、 " + storageJsonObj.DaXiao;

                                    if (!string.IsNullOrEmpty(storageJsonObj.QuanDanShuang))
                                        lotteryNotice += "、 " + storageJsonObj.QuanDanShuang;

                                    lotteryNotice += "</b>";
                                    lotteryNotice += $"\n\n<b>---------🎉 中奖玩家 🎉---------</b>";

                                    // 调用方法进行图片调整
                                    var drawImg = storageJsonObj.Numbers.Select(u => "car/" + u + ".jpg").ToList();
                                    Helper.CombineImages("赛车", [.. drawImg]);
                                    #endregion
                                    var results = from p in platforms
                                                  from g in db.Games
                                                  where p.CreatorId == g.CreatorId && g.GameStatus == GameStatus.Open && g.GameType == GameType.SpeedRacing
                                                  select new { platform = p, game = g };

                                    Log.WriteLine($"对{results.Count()}个赛车的台子进行开奖通知");
                                    foreach (var result in results)
                                    {
                                        var platform = result.platform;
                                        var botClient = _botClientList.First(u => u.BotId == platform.BotId);
                                        var game = result.game;
                                        if (platform.GroupId == null)
                                            continue;
                                        GameHistory? gameHistory = null;
                                        try
                                        {
                                            //获取本期记录(可能新开奖的)
                                            gameHistory = await db.GameHistorys.FirstOrDefaultAsync(u => u.GroupId == Convert.ToInt64(platform.GroupId) && u.GameId == game.Id && u.LotteryDrawId == resultObj.preIssue);
                                        }
                                        catch (Exception ex)
                                        {
                                            Log.Error("获取赛车本期开奖记录时出错:" + ex.Message);
                                        }
                                        //开奖信息
                                        Message? drawMsg = null;
                                        //玩家下注本游戏记录
                                        List<PlayerFinanceHistory>? bettingHistorys = gameHistory != null && gameHistory.MessageId > 0 && string.IsNullOrEmpty(gameHistory.LotteryDrawJson)
                                        ? db.PlayerFinanceHistorys.Where(u => u.FinanceStatus == FinanceStatus.Success && u.Type == FinanceType.SpeedRacing && u.GameId == game.Id && u.GameMessageId == gameHistory.MessageId).ToList()
                                        : null;
                                        //开奖
                                        if (gameHistory != null && string.IsNullOrEmpty(gameHistory.LotteryDrawJson) && bettingHistorys != null && bettingHistorys.Any())
                                        {
                                            foreach (var bettingHistory in bettingHistorys)
                                            {
                                                //下注字符串
                                                var bet = bettingHistory.Remark;
                                                if (string.IsNullOrEmpty(bet))
                                                    continue;

                                                //赔偿倍数
                                                decimal multiple = 0;

                                                //投大小
                                                if (Regex.IsMatch(bet, "^" + _betItems["l"]) && storageJsonObj.DaXiao is '大'
                                                || Regex.IsMatch(bet, "^" + _betItems["s"]) && storageJsonObj.DaXiao is '小')
                                                {
                                                    multiple = Convert.ToDecimal(2.5);
                                                }
                                                //全单全双
                                                else if (storageJsonObj.QuanDanShuang == "全单" && Regex.IsMatch(bet, "^" + _betItems["ao"])
                                                || storageJsonObj.QuanDanShuang == "全双" && Regex.IsMatch(bet, "^" + _betItems["ae"]))
                                                {
                                                    multiple = Convert.ToDecimal(6);
                                                }
                                                //定位胆
                                                else if (bet.Contains('='))
                                                {
                                                    //投注了多少注
                                                    var positions = bet.Split(';');
                                                    //中奖多少注
                                                    var correct = 0;
                                                    foreach (var item in positions)
                                                    {
                                                        //号码
                                                        var num = Convert.ToInt32(item.Split("=")[0]);
                                                        //投注位置
                                                        var position = Convert.ToInt32(item.Split("=")[1]);

                                                        if (storageJsonObj.Numbers.ElementAt(position + 1).Equals(num))
                                                            correct++;
                                                    }

                                                    switch (positions.Length)
                                                    {
                                                        case 1:
                                                            if (correct == 1)
                                                                multiple = Convert.ToDecimal(5);
                                                            break;
                                                        case 2:
                                                            switch (correct)
                                                            {
                                                                case 1:
                                                                    multiple = Convert.ToDecimal(1);
                                                                    break;
                                                                case 2:
                                                                    multiple = Convert.ToDecimal(27.5);
                                                                    break;
                                                                default:
                                                                    break;
                                                            }
                                                            break;
                                                        case 3:
                                                            switch (correct)
                                                            {
                                                                case 1:
                                                                    multiple = Convert.ToDecimal(1);
                                                                    break;
                                                                case 2:
                                                                    multiple = Convert.ToDecimal(5);
                                                                    break;
                                                                case 3:
                                                                    multiple = Convert.ToDecimal(80);
                                                                    break;
                                                                default:
                                                                    break;
                                                            }
                                                            break;
                                                        case 4:
                                                            switch (correct)
                                                            {
                                                                case 1:
                                                                    multiple = Convert.ToDecimal(1);
                                                                    break;
                                                                case 2:
                                                                    multiple = Convert.ToDecimal(2.5);
                                                                    break;
                                                                case 3:
                                                                    multiple = Convert.ToDecimal(10);
                                                                    break;
                                                                case 4:
                                                                    multiple = Convert.ToDecimal(175);
                                                                    break;
                                                                default:
                                                                    break;
                                                            }
                                                            break;
                                                        case 5:
                                                            switch (correct)
                                                            {
                                                                case 1:
                                                                    multiple = Convert.ToDecimal(1);
                                                                    break;
                                                                case 2:
                                                                    multiple = Convert.ToDecimal(1.5);
                                                                    break;
                                                                case 3:
                                                                    multiple = Convert.ToDecimal(4);
                                                                    break;
                                                                case 4:
                                                                    multiple = Convert.ToDecimal(15);
                                                                    break;
                                                                case 5:
                                                                    multiple = Convert.ToDecimal(250);
                                                                    break;
                                                                default:
                                                                    break;
                                                            }
                                                            break;
                                                        case 6:
                                                            switch (correct)
                                                            {
                                                                case 2:
                                                                    multiple = Convert.ToDecimal(1);
                                                                    break;
                                                                case 3:
                                                                    multiple = Convert.ToDecimal(5);
                                                                    break;
                                                                case 4:
                                                                    multiple = Convert.ToDecimal(50);
                                                                    break;
                                                                case 5:
                                                                    multiple = Convert.ToDecimal(1000);
                                                                    break;
                                                                case 6:
                                                                    multiple = Convert.ToDecimal(5000);
                                                                    break;
                                                                default:
                                                                    break;
                                                            }
                                                            break;
                                                        case 7:
                                                            switch (correct)
                                                            {
                                                                case 2:
                                                                    multiple = Convert.ToDecimal(1);
                                                                    break;
                                                                case 3:
                                                                    multiple = Convert.ToDecimal(5);
                                                                    break;
                                                                case 4:
                                                                    multiple = Convert.ToDecimal(12.5);
                                                                    break;
                                                                case 5:
                                                                    multiple = Convert.ToDecimal(200);
                                                                    break;
                                                                case 6:
                                                                    multiple = Convert.ToDecimal(2250);
                                                                    break;
                                                                case 7:
                                                                    multiple = Convert.ToDecimal(10000);
                                                                    break;
                                                                default:
                                                                    break;
                                                            }
                                                            break;
                                                        case 8:
                                                            switch (correct)
                                                            {
                                                                case 2:
                                                                    multiple = Convert.ToDecimal(1);
                                                                    break;
                                                                case 3:
                                                                    multiple = Convert.ToDecimal(5);
                                                                    break;
                                                                case 4:
                                                                    multiple = Convert.ToDecimal(10);
                                                                    break;
                                                                case 5:
                                                                    multiple = Convert.ToDecimal(50);
                                                                    break;
                                                                case 6:
                                                                    multiple = Convert.ToDecimal(250);
                                                                    break;
                                                                case 7:
                                                                    multiple = Convert.ToDecimal(1000);
                                                                    break;
                                                                case 8:
                                                                    multiple = Convert.ToDecimal(20000);
                                                                    break;
                                                                default:
                                                                    break;
                                                            }
                                                            break;
                                                        case 9:
                                                            switch (correct)
                                                            {
                                                                case 2:
                                                                    multiple = Convert.ToDecimal(1);
                                                                    break;
                                                                case 3:
                                                                    multiple = Convert.ToDecimal(2.5);
                                                                    break;
                                                                case 4:
                                                                    multiple = Convert.ToDecimal(5);
                                                                    break;
                                                                case 5:
                                                                    multiple = Convert.ToDecimal(25);
                                                                    break;
                                                                case 6:
                                                                    multiple = Convert.ToDecimal(125);
                                                                    break;
                                                                case 7:
                                                                    multiple = Convert.ToDecimal(2500);
                                                                    break;
                                                                case 8:
                                                                    multiple = Convert.ToDecimal(5000);
                                                                    break;
                                                                case 9:
                                                                    multiple = Convert.ToDecimal(40000);
                                                                    break;
                                                                default:
                                                                    break;
                                                            }
                                                            break;
                                                        case 10:
                                                            switch (correct)
                                                            {
                                                                case 10:
                                                                    multiple = Convert.ToDecimal(444444);
                                                                    break;
                                                                default:
                                                                    break;
                                                            }
                                                            break;
                                                        default:
                                                            break;
                                                    }
                                                }
                                                //排名
                                                else if (bet.Contains('^') && bet.Contains('>'))
                                                {
                                                    var rankingNums = bet.Replace("^", "").Split('>').Select(u => Convert.ToInt32(u)).ToHashSet();
                                                    var isWinning = true;
                                                    for (int a = 0; a < rankingNums.Count; a++)
                                                    {
                                                        if (!storageJsonObj.Numbers.ElementAt(a).Equals(rankingNums.ElementAt(a)))
                                                        {
                                                            isWinning = false;
                                                            break;
                                                        }
                                                    }

                                                    if (isWinning)
                                                    {
                                                        switch (rankingNums.Count)
                                                        {
                                                            case 1:
                                                                multiple = Convert.ToDecimal(5);
                                                                break;
                                                            case 2:
                                                                multiple = Convert.ToDecimal(45);
                                                                break;
                                                            case 3:
                                                                multiple = Convert.ToDecimal(350);
                                                                break;
                                                            case 4:
                                                                multiple = Convert.ToDecimal(2500);
                                                                break;
                                                            default:
                                                                break;
                                                        }
                                                    }
                                                }
                                                //顺子
                                                else if (bet.Contains('^') && bet.Contains('+'))
                                                {
                                                    var nums = bet.Replace("^", "").Split('+').Select(u => Convert.ToInt32(u)).ToHashSet();
                                                    if (Helper.AreConsecutive(nums.ElementAt(0), nums.ElementAt(1), nums.ElementAt(2)))
                                                    {
                                                        multiple = Convert.ToDecimal(15);
                                                    }
                                                }
                                                //前3和值
                                                else if (int.TryParse(bet, out int sum) && storageJsonObj.RankingThreeSum == sum)
                                                {
                                                    if (storageJsonObj.RankingThreeSum is 6 or 7 or 26 or 27)
                                                    {
                                                        multiple = Convert.ToDecimal(59);
                                                    }
                                                    else if (storageJsonObj.RankingThreeSum is 8 or 25)
                                                    {
                                                        multiple = Convert.ToDecimal(29.5);
                                                    }
                                                    else if (storageJsonObj.RankingThreeSum is 9 or 24)
                                                    {
                                                        multiple = Convert.ToDecimal(19.5);
                                                    }
                                                    else if (storageJsonObj.RankingThreeSum is 10 or 23)
                                                    {
                                                        multiple = Convert.ToDecimal(14.5);
                                                    }
                                                    else if (storageJsonObj.RankingThreeSum is 11 or 22)
                                                    {
                                                        multiple = Convert.ToDecimal(11.5);
                                                    }
                                                    else if (storageJsonObj.RankingThreeSum is 12 or 21)
                                                    {
                                                        multiple = Convert.ToDecimal(8.5);
                                                    }
                                                    else if (storageJsonObj.RankingThreeSum is 13 or 20)
                                                    {
                                                        multiple = Convert.ToDecimal(7.5);
                                                    }
                                                    else if (storageJsonObj.RankingThreeSum is 14 or 19)
                                                    {
                                                        multiple = Convert.ToDecimal(6.5);
                                                    }
                                                    else if (storageJsonObj.RankingThreeSum is 15 or 16 or 17 or 18)
                                                    {
                                                        multiple = Convert.ToDecimal(5.5);
                                                    }
                                                }
                                                //中奖了
                                                if (multiple > 0)
                                                {
                                                    await Helper.PlayerWinningFromPlatform(db, platform, game, gameHistory, bettingHistory, "赛车", multiple);

                                                    //下注金额
                                                    var betAmount = bettingHistory.Amount + bettingHistory.BonusAmount;
                                                    //赔偿金额
                                                    var bonus = Math.Round(betAmount * multiple, 2, MidpointRounding.AwayFromZero);
                                                    lotteryNotice += $"\n\n💵 {bettingHistory.Name} <b>{betAmount}U买{bet} 奖 " + bonus + "U</b>";
                                                }
                                            }
                                        }

                                        var openUTCTime = DateTimeOffset.FromUnixTimeMilliseconds(resultObj.currentOpenDateTime).UtcDateTime;
                                        int milliseconds = Convert.ToInt32((openUTCTime.AddMinutes(1) - DateTime.UtcNow).TotalMilliseconds);
                                        await LotteryDraw(db, gameHistory, JsonConvert.SerializeObject(storageJsonObj), lotteryNotice, platform, drawMsg, resultObj.preIssue, nextNum, openUTCTime, botClient, game, "赛车", milliseconds);
                                    }
                                }
                            }
                        }

                        lotteryNotice = string.Empty;
                        //11选5
                        if (convertResult != null && convertResult.Count != 0 && convertResult.Any(u => u.gameCode == "ms11x5"))
                        {
                            var resultObj = convertResult.First(u => u.gameCode == "ms11x5");
                            if (speedlottery.Any(g => g.GameType == GameType.Choose5From11) && resultObj != null && !prevChoose5From11Nums.Contains(resultObj.preIssue))
                            {
                                //上期数
                                prevChoose5From11Nums.Add(resultObj.preIssue);
                                //换期才执行
                                if (prevChoose5From11Nums.Count > 1 && long.TryParse(resultObj.preIssue, out long currentNum))
                                {
                                    //下一期的期数
                                    var nextNum = (currentNum + 1).ToString();
                                    //定义放到数据库里的JSON
                                    var storageJsonObj = new Choose5From11Data
                                    {
                                        Cycle = Convert.ToInt64(resultObj.preIssue),
                                        Numbers = [.. resultObj.openNum]
                                    };

                                    #region 定义返回开奖公示
                                    lotteryNotice = $"<b>----------- {resultObj.preIssue}期-----------</b>" +
                                         $"\n<b>{string.Join("、 ", resultObj.openNum)}</b>";
                                    lotteryNotice += $"\n\n<b>---------🎉 中奖玩家 🎉---------</b>";

                                    // 调用方法进行图片调整
                                    var drawImg = storageJsonObj.Numbers.Select(u => "ball/blue/" + u + ".jpg").ToList();
                                    Helper.CombineImages("11选5", [.. drawImg]);
                                    #endregion

                                    var results = from p in platforms
                                                  from g in db.Games
                                                  where p.CreatorId == g.CreatorId && g.GameStatus == GameStatus.Open && g.GameType == GameType.Choose5From11
                                                  select new { platform = p, game = g };

                                    Log.WriteLine($"对{results.Count()}个11选5的台子进行开奖通知");

                                    foreach (var result in results)
                                    {
                                        var platform = result.platform;
                                        var botClient = _botClientList.First(u => u.BotId == platform.BotId);
                                        var game = result.game;
                                        if (platform.GroupId == null)
                                            continue;

                                        GameHistory? gameHistory = null;
                                        try
                                        {
                                            //获取本期记录(可能新开奖的)
                                            gameHistory = await db.GameHistorys.FirstOrDefaultAsync(u => u.GroupId == Convert.ToInt64(platform.GroupId) && u.GameId == game.Id && u.LotteryDrawId == resultObj.preIssue);
                                        }
                                        catch (Exception ex)
                                        {
                                            Log.Error("获取11选5本期开奖记录时出错:" + ex.Message);
                                        }

                                        //开奖信息
                                        Message? drawMsg = null;
                                        //玩家下注本游戏记录
                                        List<PlayerFinanceHistory>? bettingHistorys = gameHistory != null && gameHistory.MessageId > 0 && string.IsNullOrEmpty(gameHistory.LotteryDrawJson)
                                        ? db.PlayerFinanceHistorys.Where(u => u.FinanceStatus == FinanceStatus.Success && u.Type == FinanceType.Choose5From11 && u.GameId == game.Id && u.GameMessageId == gameHistory.MessageId).ToList()
                                        : null;
                                        //开奖
                                        if (gameHistory != null && string.IsNullOrEmpty(gameHistory.LotteryDrawJson) && bettingHistorys != null && bettingHistorys.Any())
                                        {
                                            foreach (var bettingHistory in bettingHistorys)
                                            {
                                                //下注字符串
                                                var bet = bettingHistory.Remark;
                                                if (string.IsNullOrEmpty(bet))
                                                    continue;

                                                //赔偿倍数
                                                decimal multiple = 0;

                                                //包号
                                                if (bet.Contains('/') || Regex.IsMatch(bet, @"^([1-9]|10)$"))
                                                {
                                                    //包了多少个号
                                                    var nums = bet.Split('/').Select(u => Convert.ToInt32(u)).ToHashSet();
                                                    //中了多少个号
                                                    var winningNums = storageJsonObj.Numbers.Count(u => nums.Contains(u));
                                                    if (winningNums > 0)
                                                    {
                                                        if (nums.Count == 8 && winningNums == 5)
                                                        {
                                                            multiple = Convert.ToDecimal(4.5);
                                                        }
                                                        else if (nums.Count == 7 && winningNums == 5)
                                                        {
                                                            multiple = Convert.ToDecimal(13);
                                                        }
                                                        else if (nums.Count == 6 && winningNums == 5)
                                                        {
                                                            multiple = Convert.ToDecimal(45);
                                                        }
                                                        else if (nums.Count == 5 && winningNums == 5)
                                                        {
                                                            multiple = Convert.ToDecimal(270);
                                                        }
                                                        else if (nums.Count == 4 && winningNums == 4)
                                                        {
                                                            multiple = Convert.ToDecimal(39);
                                                        }
                                                        else if (nums.Count == 3 && winningNums == 3)
                                                        {
                                                            multiple = Convert.ToDecimal(9.5);
                                                        }
                                                        else if (nums.Count == 2 && winningNums == 2)
                                                        {
                                                            multiple = Convert.ToDecimal(3);
                                                        }
                                                        else if (nums.Count == 1 && winningNums == 1 && storageJsonObj.Numbers.First() == nums.First())
                                                        {
                                                            multiple = Convert.ToDecimal(6.5);
                                                        }
                                                    }
                                                }
                                                //前组
                                                else if (bet.Contains('^') && bet.Contains('&'))
                                                {
                                                    var nums = bet.Replace("^", "").Split('&').Select(u => Convert.ToInt32(u)).ToHashSet();
                                                    if (storageJsonObj.Numbers.Count(u => nums.Contains(u)) == nums.Count)
                                                    {
                                                        if (nums.Count == 2)
                                                        {
                                                            multiple = Convert.ToDecimal(32.5);
                                                        }
                                                        else if (nums.Count == 3)
                                                        {
                                                            multiple = Convert.ToDecimal(97.5);
                                                        }
                                                    }
                                                }
                                                //排名
                                                if (bet.Contains('^') && bet.Contains('>'))
                                                {
                                                    var rankingNums = bet.Replace("^", "").Split('>').Select(u => Convert.ToInt32(u)).ToHashSet();
                                                    var isWinning = true;
                                                    for (int a = 0; a < rankingNums.Count; a++)
                                                    {
                                                        if (!storageJsonObj.Numbers.ElementAt(a).Equals(rankingNums.ElementAt(a)))
                                                        {
                                                            isWinning = false;
                                                            break;
                                                        }
                                                    }

                                                    if (isWinning)
                                                    {
                                                        switch (rankingNums.Count)
                                                        {
                                                            case 2:
                                                                multiple = Convert.ToDecimal(62);
                                                                break;
                                                            case 3:
                                                                multiple = Convert.ToDecimal(585);
                                                                break;
                                                            default:
                                                                break;
                                                        }
                                                    }
                                                }

                                                //中奖了
                                                if (multiple > 0)
                                                {
                                                    await Helper.PlayerWinningFromPlatform(db, platform, game, gameHistory, bettingHistory, "11选5", multiple);

                                                    //下注金额
                                                    var betAmount = bettingHistory.Amount + bettingHistory.BonusAmount;
                                                    //赔偿金额
                                                    var bonus = Math.Round(betAmount * multiple, 2, MidpointRounding.AwayFromZero);
                                                    lotteryNotice += $"\n\n💵 {bettingHistory.Name} <b>{betAmount}U买{bet} 奖 " + bonus + "U</b>";
                                                }
                                            }
                                        }
                                        var openUTCTime = DateTimeOffset.FromUnixTimeMilliseconds(resultObj.currentOpenDateTime).UtcDateTime;
                                        int milliseconds = Convert.ToInt32((openUTCTime.AddMinutes(1) - DateTime.UtcNow).TotalMilliseconds);
                                        await LotteryDraw(db, gameHistory, JsonConvert.SerializeObject(storageJsonObj), lotteryNotice, platform, drawMsg, resultObj.preIssue, nextNum, openUTCTime, botClient, game, "11选5", milliseconds);
                                    }
                                }
                            }
                        }
                        try
                        {
                            await db.SaveChangesAsync();
                        }
                        catch (Exception ex)
                        {
                            Log.Error("每秒轮询执行赛车/11选5数据库保存出错:" + ex.Message);
                        }

                        isSpeedlotteryRuning = false;
                    });
                }

                //飞艇 马耳他时间开始时间为每天上午06：04至晚上09：09
                var maltaTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Central European Standard Time"));
                bool isLuckyAirshipTimeRange = maltaTime.TimeOfDay > new TimeSpan(06, 04, 0) && maltaTime.TimeOfDay < new TimeSpan(21, 09, 0);
                if (isRun && isLuckyAirshipTimeRange && !isLuckyAirshipRuning && dbcache.Games.Any(g => g.GameStatus == GameStatus.Open && g.GameType == GameType.LuckyAirship))
                {
                    _ = Task.Run(async () =>
                    {
                        isLuckyAirshipRuning = true;
                        using var db = new DataContext();

                        //定义JSON放到数据库里的
                        LuckyAirshipData? storageJsonObj = null;
                        //返回开奖结果公示
                        var lotteryNotice = string.Empty;
                        LuckyAirship? resultObj = null;
                        try
                        {
                            using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
                            var random = new Random();
                            // 生成随机数字
                            long randomNumber = 0;
                            for (int i = 0; i < 18; i++)
                                randomNumber = randomNumber * 10 + random.Next(0, 10);

                            var result = await http.GetStringAsync($"https://www.luckyairship.com/api/getwiningnumbers.ashx?random=0.{randomNumber}");
                            resultObj = JsonConvert.DeserializeObject<LuckyAirship>(result);
                        }
                        catch (Exception ex)
                        {
                            Log.Error("获取飞艇开奖结果出错:" + ex.Message);
                        }

                        if (resultObj != null && resultObj.numbersArray.Length != 0 && !prevLuckyAirshipNums.Contains(resultObj.openedPeriodNumber))
                        {
                            //上期数
                            prevLuckyAirshipNums.Add(resultObj.openedPeriodNumber);
                            //换期才执行
                            if (prevLuckyAirshipNums.Count > 1)
                            {
                                //下一期
                                long nextNum = resultObj.openedPeriodNumber + 1;

                                #region 定义放到数据库里的JSON
                                var numbers = resultObj.numbersArray.Select(u => Convert.ToInt32(u)).ToList();
                                var sum = numbers.Take(2).Sum();
                                storageJsonObj = new LuckyAirshipData
                                {
                                    Cycle = resultObj.openedPeriodNumber,
                                    Numbers = numbers.ToHashSet(),
                                    Sum = sum,
                                };

                                if (sum >= 3 && sum <= 6)
                                    storageJsonObj.JiDaXiao = "极小";
                                else if (sum >= 7 && sum <= 10)
                                    storageJsonObj.DaXiao = '小';
                                else if (sum == 11)
                                    storageJsonObj.Zhong = '中';
                                else if (sum >= 12 && sum <= 15)
                                    storageJsonObj.DaXiao = '大';
                                else if (sum >= 16 && sum <= 19)
                                    storageJsonObj.JiDaXiao = "极大";
                                #endregion
                                #region 定义返回开奖公示
                                lotteryNotice = $"<b>----------- {resultObj.openedPeriodNumber}期-----------</b>" +
                                     $"\n<b>{string.Join("、 ", resultObj.numbersArray)}</b>" +
                                     $"\n\n结果 <b>{numbers[0]} + {numbers[1]} = {sum}";

                                if (storageJsonObj.DaXiao != '\0')
                                    lotteryNotice += "、 " + storageJsonObj.DaXiao;

                                if (storageJsonObj.Zhong != '\0')
                                    lotteryNotice += "、 " + storageJsonObj.Zhong;

                                if (!string.IsNullOrEmpty(storageJsonObj.JiDaXiao))
                                    lotteryNotice += "、 " + storageJsonObj.JiDaXiao;

                                lotteryNotice += "</b>";
                                lotteryNotice += $"\n\n<b>---------🎉 中奖玩家 🎉---------</b>";

                                // 调用方法进行图片调整
                                try
                                {
                                    var drawImg = numbers.Select(u => "airship/" + u + ".jpg").ToList();
                                    Helper.CombineImages("飞艇", [.. drawImg]);
                                }
                                catch (Exception ex)
                                {
                                    Log.WriteLine("生成横幅图片出错:" + ex.Message);
                                }

                                #endregion
                                var results = from p in platforms
                                              from g in db.Games
                                              where p.CreatorId == g.CreatorId && g.GameStatus == GameStatus.Open && g.GameType == GameType.LuckyAirship
                                              select new { platform = p, game = g };
                                Log.WriteLine($"对{results.Count()}个飞艇的台子进行开奖通知");
                                foreach (var result in results)
                                {
                                    var platform = result.platform;
                                    var botClient = _botClientList.First(u => u.BotId == platform.BotId);
                                    var game = result.game;
                                    if (platform.GroupId == null)
                                        continue;

                                    GameHistory? gameHistory = null;
                                    try
                                    {
                                        //获取本期记录(可能新开奖的)
                                        gameHistory = await db.GameHistorys.FirstOrDefaultAsync(u => u.GroupId == Convert.ToInt64(platform.GroupId) && u.GameId == game.Id && u.LotteryDrawId == resultObj.openedPeriodNumber.ToString());
                                    }
                                    catch (Exception ex)
                                    {
                                        Log.Error("获取飞艇本期开奖记录时出错:" + ex.Message);
                                    }
                                    //开奖信息
                                    Message? drawMsg = null;
                                    //玩家下注本游戏记录
                                    IQueryable<PlayerFinanceHistory>? bettingHistorys = gameHistory != null && gameHistory.MessageId > 0 && string.IsNullOrEmpty(gameHistory.LotteryDrawJson)
                                    ? db.PlayerFinanceHistorys.Where(u => u.FinanceStatus == FinanceStatus.Success && u.Type == FinanceType.LuckyAirship && u.GameId == game.Id && u.GameMessageId == gameHistory.MessageId)
                                    : null;
                                    //开奖
                                    if (gameHistory != null && string.IsNullOrEmpty(gameHistory.LotteryDrawJson) && bettingHistorys != null && bettingHistorys.Any())
                                    {
                                        foreach (var bettingHistory in bettingHistorys)
                                        {
                                            //下注字符串
                                            var bet = bettingHistory.Remark;
                                            if (string.IsNullOrEmpty(bet))
                                                continue;

                                            //赔偿倍数
                                            decimal multiple = 0;

                                            //极小/极大
                                            if (sum >= 3 && sum <= 6 && Regex.IsMatch(bet, "^" + _betItems["xs"]) || sum >= 16 && sum <= 19 && Regex.IsMatch(bet, "^" + _betItems["xl"]))
                                            {
                                                multiple = Convert.ToDecimal(5.5);
                                            }
                                            //小/大
                                            else if (sum >= 7 && sum <= 10 && Regex.IsMatch(bet, "^" + _betItems["s"]) || sum >= 12 && sum <= 15 && Regex.IsMatch(bet, "^" + _betItems["l"]))
                                            {
                                                multiple = Convert.ToDecimal(2.98);
                                            }
                                            //中
                                            else if (sum == 11 && Regex.IsMatch(bet, "^" + _betItems["m"]))
                                            {
                                                multiple = Convert.ToDecimal(8.5);
                                            }
                                            //定位胆
                                            else if (bet.Contains('='))
                                            {
                                                //投注了多少注
                                                var positions = bet.Split(';');
                                                //中奖多少注
                                                var correct = 0;
                                                foreach (var item in positions)
                                                {
                                                    //号码
                                                    var num = Convert.ToInt32(item.Split("=")[0]);
                                                    //投注位置
                                                    var position = Convert.ToInt32(item.Split("=")[1]);

                                                    if (storageJsonObj.Numbers.ElementAt(position + 1).Equals(num))
                                                        correct++;
                                                }

                                                switch (positions.Length)
                                                {
                                                    case 1:
                                                        if (correct == 1)
                                                            multiple = Convert.ToDecimal(5);
                                                        break;
                                                    case 2:
                                                        if (correct == 2)
                                                            multiple = Convert.ToDecimal(25);
                                                        break;
                                                    case 3:
                                                        if (correct == 3)
                                                            multiple = Convert.ToDecimal(125);
                                                        break;
                                                    case 4:
                                                        if (correct == 4)
                                                            multiple = Convert.ToDecimal(625);
                                                        break;
                                                    case 5:
                                                        if (correct == 5)
                                                            multiple = Convert.ToDecimal(3125);
                                                        break;
                                                    case 6:
                                                        if (correct == 6)
                                                            multiple = Convert.ToDecimal(15625);
                                                        break;
                                                    case 7:
                                                        if (correct == 7)
                                                            multiple = Convert.ToDecimal(78125);
                                                        break;
                                                    case 8:
                                                        if (correct == 8)
                                                            multiple = Convert.ToDecimal(390625);
                                                        break;
                                                    case 9:
                                                        if (correct == 9)
                                                            multiple = Convert.ToDecimal(1953125);
                                                        break;
                                                    case 10:
                                                        if (correct == 10)
                                                            multiple = Convert.ToDecimal(10000000);
                                                        break;
                                                    default:
                                                        break;
                                                }
                                            }
                                            //排名
                                            else if (bet.Contains('^') && bet.Contains('>'))
                                            {
                                                var rankingNums = bet.Replace("^", "").Split('>').Select(u => Convert.ToInt32(u)).ToHashSet();
                                                var isWinning = true;
                                                for (int a = 0; a < rankingNums.Count; a++)
                                                {
                                                    if (!storageJsonObj.Numbers.ElementAt(a).Equals(rankingNums.ElementAt(a)))
                                                    {
                                                        isWinning = false;
                                                        break;
                                                    }
                                                }

                                                if (isWinning)
                                                {
                                                    switch (rankingNums.Count)
                                                    {
                                                        case 1:
                                                            multiple = Convert.ToDecimal(9);
                                                            break;
                                                        case 2:
                                                            multiple = Convert.ToDecimal(43);
                                                            break;
                                                        case 3:
                                                            multiple = Convert.ToDecimal(350);
                                                            break;
                                                        default:
                                                            break;
                                                    }
                                                }
                                            }
                                            //前3和值
                                            else if (bet.Contains("^="))
                                            {
                                                var betNum = Convert.ToUInt32(bet.Replace("^=", ""));

                                                if (sum == 3 && betNum == sum || sum == 19 && betNum == sum)
                                                {
                                                    multiple = Convert.ToDecimal(59);
                                                }
                                                else if (sum == 4 && betNum == sum || sum == 18 && betNum == sum)
                                                {
                                                    multiple = Convert.ToDecimal(29.5);
                                                }
                                                else if (sum == 5 && betNum == sum || sum == 17 && betNum == sum)
                                                {
                                                    multiple = Convert.ToDecimal(19.5);
                                                }
                                                else if (sum == 6 && betNum == sum || sum == 16 && betNum == sum)
                                                {
                                                    multiple = Convert.ToDecimal(14.5);
                                                }
                                                else if (sum == 7 && betNum == sum || sum == 15 && betNum == sum)
                                                {
                                                    multiple = Convert.ToDecimal(11.5);
                                                }
                                                else if (sum == 8 && betNum == sum || sum == 14 && betNum == sum)
                                                {
                                                    multiple = Convert.ToDecimal(8.5);
                                                }
                                                else if (sum == 9 && betNum == sum || sum == 13 && betNum == sum)
                                                {
                                                    multiple = Convert.ToDecimal(7.5);
                                                }
                                                else if (sum == 10 && betNum == sum || sum == 11 && betNum == sum)
                                                {
                                                    multiple = Convert.ToDecimal(6.5);
                                                }
                                                else if (sum == 12 && betNum == sum)
                                                {
                                                    multiple = Convert.ToDecimal(5.5);
                                                }
                                            }

                                            //中奖了
                                            if (multiple > 0)
                                            {
                                                await Helper.PlayerWinningFromPlatform(db, platform, game, gameHistory, bettingHistory, "飞艇", multiple);

                                                //下注金额
                                                var betAmount = bettingHistory.Amount + bettingHistory.BonusAmount;
                                                //赔偿金额
                                                var bonus = Math.Round(betAmount * multiple, 2, MidpointRounding.AwayFromZero);
                                                lotteryNotice += $"\n\n💵 {bettingHistory.Name} <b>{betAmount}U买{bet} 奖 " + bonus + "U</b>";
                                            }
                                        }
                                    }
                                    var openUTCTime = resultObj.openedDate;
                                    int milliseconds = Convert.ToInt32((openUTCTime.AddMinutes(4.5) - DateTime.UtcNow.AddHours(2)).TotalMilliseconds);
                                    await LotteryDraw(db, gameHistory, JsonConvert.SerializeObject(storageJsonObj), lotteryNotice, platform, drawMsg, resultObj.openedPeriodNumber.ToString(), nextNum.ToString(), openUTCTime, botClient, game, "飞艇", milliseconds);
                                }
                            }
                        }
                        try
                        {
                            await db.SaveChangesAsync();
                        }
                        catch (Exception ex)
                        {
                            Log.Error("每秒轮询执行飞艇数据库保存出错:" + ex.Message);
                        }
                        isLuckyAirshipRuning = false;
                    });
                }

                //缤果:周一至周日，07:05~23:55，每5分钟开一次
                var taiwanTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Taipei Standard Time"));
                bool isBingoTimeRange = taiwanTime.TimeOfDay > new TimeSpan(07, 05, 0) && taiwanTime.TimeOfDay < new TimeSpan(23, 55, 0);
                if (isRun && isBingoTimeRange && !isBingoRuning && dbcache.Games.Any(g => g.GameStatus == GameStatus.Open && g.GameType == GameType.Bingo))
                {
                    _ = Task.Run(async () =>
                    {
                        isBingoRuning = true;
                        using var db = new DataContext();
                        //定义JSON放到数据库里的
                        RootData? storageJsonObj = null;
                        //返回开奖结果公示
                        var lotteryNotice = string.Empty;
                        Bingo? resultObj = null;
                        try
                        {
                            using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
                            var result = await http.GetStringAsync("https://api.taiwanlottery.com/TLCAPIWeB/Lottery/LastNumber");
                            var obj = JsonConvert.DeserializeObject<Root>(result);
                            resultObj = obj?.content?.bingo;
                        }
                        catch (Exception ex)
                        {
                            Log.Error("缤果:" + ex.Message);
                        }

                        if (resultObj != null && !prevBingoNums.Contains(resultObj.period))
                        {
                            //上期数
                            prevBingoNums.Add(resultObj.period);
                            //换期才执行
                            if (prevBingoNums.Count > 1)
                            {
                                long nextNum = Convert.ToInt64(resultObj.period) + 1;
                                #region 定义放到数据库里的JSON
                                storageJsonObj = new RootData
                                {
                                    Cycle = Convert.ToInt32(resultObj.period),
                                    Numbers = [.. resultObj.lotNumber],
                                    SuperNum = resultObj.lotSpecial
                                };

                                if (!string.IsNullOrEmpty(resultObj.lotBigSmall))
                                    storageJsonObj.DaXiao = resultObj.lotBigSmall == "小" ? '小' : '大';

                                if (!string.IsNullOrEmpty(resultObj.lotOddEven))
                                    storageJsonObj.DaXiao = resultObj.lotOddEven == "雙" ? '双' : '单';
                                #endregion
                                #region 定义返回开奖公示
                                lotteryNotice = $"<b>----------- {resultObj.period}期-----------</b>" +
                                     $"\n<b>{string.Join("、 ", resultObj.lotNumber)}</b>" +
                                     $"\n\n结果 <b>超级号:{resultObj.lotSpecial}";

                                if (storageJsonObj.DaXiao != '\0')
                                    lotteryNotice += "、 " + storageJsonObj.DaXiao;

                                if (storageJsonObj.DanShuang != '\0')
                                    lotteryNotice += "、 " + storageJsonObj.DanShuang;

                                lotteryNotice += "</b>";
                                lotteryNotice += $"\n\n<b>---------🎉 中奖玩家 🎉---------</b>";

                                var superNum = Convert.ToInt32(resultObj.lotSpecial);
                                // 调用方法进行图片调整
                                var drawImg = resultObj.lotNumber.Select(u => u == superNum ? "ball/red/" + u + ".jpg" : "ball/green/" + u + ".jpg").ToList();
                                Helper.CombineImages("缤果", [.. drawImg]);
                                #endregion
                                var results = from p in platforms
                                              from g in db.Games
                                              where p.CreatorId == g.CreatorId && g.GameStatus == GameStatus.Open && g.GameType == GameType.Bingo
                                              select new { platform = p, game = g };
                                Log.WriteLine($"对{results.Count()}个缤果的台子进行开奖通知");
                                foreach (var result in results)
                                {
                                    var platform = result.platform;
                                    var botClient = _botClientList.First(u => u.BotId == platform.BotId);
                                    var game = result.game;
                                    if (platform.GroupId == null)
                                        continue;

                                    GameHistory? gameHistory = null;
                                    try
                                    {
                                        //获取本期记录(可能新开奖的)
                                        gameHistory = await db.GameHistorys.FirstOrDefaultAsync(u => u.GroupId == Convert.ToInt64(platform.GroupId) && u.GameId == game.Id && u.LotteryDrawId == resultObj.period);
                                    }
                                    catch (Exception ex)
                                    {
                                        Log.Error("获取缤果本期开奖记录时出错:" + ex.Message);
                                    }
                                    //开奖信息
                                    Message? drawMsg = null;
                                    //玩家下注本游戏记录
                                    IQueryable<PlayerFinanceHistory>? bettingHistorys = gameHistory != null && gameHistory.MessageId > 0 && string.IsNullOrEmpty(gameHistory.LotteryDrawJson)
                                    ? db.PlayerFinanceHistorys.Where(u => u.FinanceStatus == FinanceStatus.Success && u.Type == FinanceType.Bingo && u.GameId == game.Id && u.GameMessageId == gameHistory.MessageId)
                                    : null;
                                    //开奖
                                    if (gameHistory != null && string.IsNullOrEmpty(gameHistory.LotteryDrawJson) && bettingHistorys != null && bettingHistorys.Any())
                                    {
                                        foreach (var bettingHistory in bettingHistorys)
                                        {
                                            //下注字符串
                                            var bet = bettingHistory.Remark;
                                            if (string.IsNullOrEmpty(bet))
                                                continue;

                                            //赔偿倍数
                                            decimal multiple = 0;

                                            //超级号
                                            if (bet == storageJsonObj.SuperNum)
                                            {
                                                multiple = Convert.ToDecimal(48);
                                            }
                                            //大、小、单、双
                                            else if (Regex.IsMatch(bet, "^" + _betItems["l"]) && storageJsonObj.DaXiao == '大' ||
                                            Regex.IsMatch(bet, "^" + _betItems["s"]) && storageJsonObj.DaXiao == '小' ||
                                            Regex.IsMatch(bet, "^" + _betItems["o"]) && storageJsonObj.DaXiao == '单' ||
                                            Regex.IsMatch(bet, "^" + _betItems["e"]) && storageJsonObj.DaXiao == '双')
                                            {
                                                multiple = Convert.ToDecimal(6);
                                            }
                                            //包号
                                            if (bet.Contains('/') || Regex.IsMatch(bet, @"(0?[1-9]|[1-7][0-9]|80)(/(0?[1-9]|[1-7][0-9]|80)){1,9}$"))
                                            {
                                                //包了多少个号
                                                var nums = bet.Split('/').Select(u => Convert.ToInt32(u)).ToHashSet();
                                                //中了多少个号
                                                var winningNums = storageJsonObj.Numbers.Count(u => nums.Contains(u));
                                                if (winningNums > 0)
                                                {
                                                    switch (nums.Count)
                                                    {
                                                        case 2:
                                                            switch (winningNums)
                                                            {
                                                                case 1:
                                                                    multiple = Convert.ToDecimal(1);
                                                                    break;
                                                                case 2:
                                                                    multiple = Convert.ToDecimal(3);
                                                                    break;
                                                                default:
                                                                    break;
                                                            }
                                                            break;
                                                        case 3:
                                                            switch (winningNums)
                                                            {
                                                                case 2:
                                                                    multiple = Convert.ToDecimal(2);
                                                                    break;
                                                                case 3:
                                                                    multiple = Convert.ToDecimal(20);
                                                                    break;
                                                                default:
                                                                    break;
                                                            }
                                                            break;
                                                        case 4:
                                                            switch (winningNums)
                                                            {
                                                                case 2:
                                                                    multiple = Convert.ToDecimal(1);
                                                                    break;
                                                                case 3:
                                                                    multiple = Convert.ToDecimal(4);
                                                                    break;
                                                                case 4:
                                                                    multiple = Convert.ToDecimal(40);
                                                                    break;
                                                                default:
                                                                    break;
                                                            }
                                                            break;
                                                        case 5:
                                                            switch (winningNums)
                                                            {
                                                                case 3:
                                                                    multiple = Convert.ToDecimal(2);
                                                                    break;
                                                                case 4:
                                                                    multiple = Convert.ToDecimal(20);
                                                                    break;
                                                                case 5:
                                                                    multiple = Convert.ToDecimal(300);
                                                                    break;
                                                                default:
                                                                    break;
                                                            }
                                                            break;
                                                        case 6:
                                                            switch (winningNums)
                                                            {
                                                                case 3:
                                                                    multiple = Convert.ToDecimal(1);
                                                                    break;
                                                                case 4:
                                                                    multiple = Convert.ToDecimal(8);
                                                                    break;
                                                                case 5:
                                                                    multiple = Convert.ToDecimal(40);
                                                                    break;
                                                                case 6:
                                                                    multiple = Convert.ToDecimal(1000);
                                                                    break;
                                                                default:
                                                                    break;
                                                            }
                                                            break;
                                                        case 7:
                                                            switch (winningNums)
                                                            {
                                                                case 3:
                                                                    multiple = Convert.ToDecimal(1);
                                                                    break;
                                                                case 4:
                                                                    multiple = Convert.ToDecimal(2);
                                                                    break;
                                                                case 5:
                                                                    multiple = Convert.ToDecimal(12);
                                                                    break;
                                                                case 6:
                                                                    multiple = Convert.ToDecimal(120);
                                                                    break;
                                                                case 7:
                                                                    multiple = Convert.ToDecimal(3200);
                                                                    break;
                                                                default:
                                                                    break;
                                                            }
                                                            break;
                                                        case 8:
                                                            switch (winningNums)
                                                            {
                                                                case 4:
                                                                    multiple = Convert.ToDecimal(1);
                                                                    break;
                                                                case 5:
                                                                    multiple = Convert.ToDecimal(8);
                                                                    break;
                                                                case 6:
                                                                    multiple = Convert.ToDecimal(40);
                                                                    break;
                                                                case 7:
                                                                    multiple = Convert.ToDecimal(800);
                                                                    break;
                                                                case 8:
                                                                    multiple = Convert.ToDecimal(2000);
                                                                    break;
                                                                default:
                                                                    break;
                                                            }
                                                            break;
                                                        case 9:
                                                            switch (winningNums)
                                                            {
                                                                case 4:
                                                                    multiple = Convert.ToDecimal(1);
                                                                    break;
                                                                case 5:
                                                                    multiple = Convert.ToDecimal(4);
                                                                    break;
                                                                case 6:
                                                                    multiple = Convert.ToDecimal(20);
                                                                    break;
                                                                case 7:
                                                                    multiple = Convert.ToDecimal(120);
                                                                    break;
                                                                case 8:
                                                                    multiple = Convert.ToDecimal(4000);
                                                                    break;
                                                                case 9:
                                                                    multiple = Convert.ToDecimal(40000);
                                                                    break;
                                                                default:
                                                                    break;
                                                            }
                                                            break;
                                                        case 10:
                                                            switch (winningNums)
                                                            {
                                                                case 5:
                                                                    multiple = Convert.ToDecimal(1);
                                                                    break;
                                                                case 6:
                                                                    multiple = Convert.ToDecimal(10);
                                                                    break;
                                                                case 7:
                                                                    multiple = Convert.ToDecimal(100);
                                                                    break;
                                                                case 8:
                                                                    multiple = Convert.ToDecimal(1000);
                                                                    break;
                                                                case 9:
                                                                    multiple = Convert.ToDecimal(10000);
                                                                    break;
                                                                case 10:
                                                                    multiple = Convert.ToDecimal(200000);
                                                                    break;
                                                                default:
                                                                    break;
                                                            }
                                                            break;
                                                        default:
                                                            break;
                                                    }
                                                }
                                            }
                                            //中奖了
                                            if (multiple > 0)
                                            {
                                                await Helper.PlayerWinningFromPlatform(db, platform, game, gameHistory, bettingHistory, "缤果", multiple);

                                                //下注金额
                                                var betAmount = bettingHistory.Amount + bettingHistory.BonusAmount;
                                                //赔偿金额
                                                var bonus = Math.Round(betAmount * multiple, 2, MidpointRounding.AwayFromZero);
                                                lotteryNotice += $"\n\n💵 {bettingHistory.Name} <b>{betAmount}U买{bet} 奖 " + bonus + "U</b>";
                                            }
                                        }
                                    }
                                    var openUTCTime = resultObj.drawDate.ToUniversalTime();
                                    await LotteryDraw(db, gameHistory, JsonConvert.SerializeObject(storageJsonObj), lotteryNotice, platform, drawMsg, resultObj.period, nextNum.ToString(), openUTCTime, botClient, game, "缤果", Convert.ToInt32(1000 * 60 * 4.5));
                                }
                            }
                        }
                        try
                        {
                            await db.SaveChangesAsync();
                        }
                        catch (Exception ex)
                        {
                            Log.Error("每秒轮询执行缤果数据库保存出错:" + ex.Message);
                        }
                        isBingoRuning = false;
                    });
                }

                //幸运8
                if (isRun && !isAustralianLuckyRuning && dbcache.Games.Any(g => g.GameStatus == GameStatus.Open && g.GameType == GameType.AustralianLucky8))
                {
                    _ = Task.Run(async () =>
                    {
                        isAustralianLuckyRuning = true;
                        using var db = new DataContext();

                        //定义JSON放到数据库里的
                        Ball8Data? storageJsonObj = null;
                        //返回开奖结果公示
                        var lotteryNotice = string.Empty;
                        Ball8? resultObj = null;
                        try
                        {
                            using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
                            var html = await http.GetStringAsync("https://www.auluckylottery.com/ajax/ball8.php?token=" + Guid.NewGuid());
                            resultObj = new Ball8();
                            HtmlDocument htmlDoc = new();
                            htmlDoc.LoadHtml(html);
                            resultObj.Time = htmlDoc.DocumentNode.QuerySelector(".brt2f_1").InnerText;

                            resultObj.Draw = htmlDoc.DocumentNode.QuerySelector(".brt2f_2 span").InnerText;
                            var redNumber = htmlDoc.DocumentNode.QuerySelector(".back_red");
                            if (redNumber != null)
                                resultObj.RedNumber = Convert.ToInt32(redNumber.InnerText);
                            var blueNumbers = htmlDoc.DocumentNode.QuerySelectorAll(".back_bule");
                            foreach (var blueNumber in blueNumbers)
                            {
                                resultObj.BlueNumber.Add(Convert.ToInt32(blueNumber.InnerText));
                            }
                            resultObj.NextDraw = htmlDoc.DocumentNode.QuerySelector(".brt3t_number span").InnerText;
                        }
                        catch (Exception ex)
                        {
                            Log.Error("幸运8:" + ex.Message);
                        }

                        if (resultObj != null && resultObj.BlueNumber.Count != 0 && !prevAustralianLucky8Nums.Contains(resultObj.Draw))
                        {
                            //上期数
                            prevAustralianLucky8Nums.Add(resultObj.Draw);
                            //换期才执行
                            if (prevAustralianLucky8Nums.Count > 1)
                            {
                                #region 定义放到数据库里的JSON                           
                                storageJsonObj = new Ball8Data
                                {
                                    Cycle = Convert.ToInt32(resultObj.Draw),
                                    Numbers = [.. resultObj.BlueNumber],
                                    RedNum = resultObj.RedNumber
                                };
                                #endregion
                                #region 定义返回开奖公示
                                lotteryNotice = $"<b>----------- {resultObj.Draw}期-----------</b>" +
                                     $"\n<b>{string.Join("、 ", resultObj.BlueNumber)}</b>";

                                lotteryNotice += $"\n\n<b>---------🎉 中奖玩家 🎉---------</b>";

                                // 调用方法进行图片调整
                                List<string> drawImg = [];
                                foreach (var number in resultObj.BlueNumber)
                                {
                                    if (number is 19 or 20)
                                    {
                                        drawImg.Add("ball/red/" + number + ".jpg");
                                    }
                                    else
                                    {
                                        drawImg.Add("ball/blue/" + number + ".jpg");
                                    }
                                }
                                Helper.CombineImages("幸运8", [.. drawImg]);
                                #endregion
                                var results = from p in platforms
                                              from g in db.Games
                                              where p.CreatorId == g.CreatorId && g.GameStatus == GameStatus.Open && g.GameType == GameType.AustralianLucky8
                                              select new { platform = p, game = g };

                                Log.WriteLine($"对{results.Count()}个幸运8的台子进行开奖通知");
                                foreach (var result in results)
                                {
                                    var platform = result.platform;
                                    var botClient = _botClientList.First(u => u.BotId == platform.BotId);
                                    var game = result.game;
                                    if (platform.GroupId == null)
                                        continue;

                                    GameHistory? gameHistory = null;
                                    try
                                    {
                                        //获取本期记录(可能新开奖的)
                                        gameHistory = await db.GameHistorys.FirstOrDefaultAsync(u => u.GroupId == Convert.ToInt64(platform.GroupId) && u.GameId == game.Id && u.LotteryDrawId == resultObj.Draw);
                                    }
                                    catch (Exception ex)
                                    {
                                        Log.Error("获取幸运8本期开奖记录时出错:" + ex.Message);
                                    }
                                    //开奖信息
                                    Message? drawMsg = null;
                                    //玩家下注本游戏记录
                                    IQueryable<PlayerFinanceHistory>? bettingHistorys = gameHistory != null && gameHistory.MessageId > 0 && string.IsNullOrEmpty(gameHistory.LotteryDrawJson)
                                    ? db.PlayerFinanceHistorys.Where(u => u.FinanceStatus == FinanceStatus.Success && u.Type == FinanceType.AustralianLucky8 && u.GameId == game.Id && u.GameMessageId == gameHistory.MessageId)
                                    : null;
                                    //开奖
                                    if (gameHistory != null && string.IsNullOrEmpty(gameHistory.LotteryDrawJson) && bettingHistorys != null && bettingHistorys.Any())
                                    {
                                        foreach (var bettingHistory in bettingHistorys)
                                        {
                                            //下注字符串
                                            var bet = bettingHistory.Remark;
                                            if (string.IsNullOrEmpty(bet))
                                                continue;

                                            //赔偿倍数
                                            decimal multiple = 0;

                                            //红色
                                            if (Regex.IsMatch(bet, @"^(19|20)$") && int.TryParse(bet, out int num))
                                            {
                                                if (num == 19 && resultObj.RedNumber == 19 || num == 20 && resultObj.RedNumber == 20)
                                                {
                                                    multiple = Convert.ToDecimal(8);
                                                }
                                            }
                                            //任选
                                            else if (bet.Contains('&'))
                                            {
                                                //包了多少个号
                                                var nums = bet.Split('&').Select(u => Convert.ToInt32(u)).ToHashSet();
                                                //中了多少个号
                                                var winningNums = storageJsonObj.Numbers.Count(u => nums.Contains(u));

                                                if (nums.Count == 2 && winningNums == 2)
                                                {
                                                    multiple = Convert.ToDecimal(5.3);
                                                }
                                                else if (nums.Count == 3 && winningNums == 3)
                                                {
                                                    multiple = Convert.ToDecimal(16);
                                                }
                                                else if (nums.Count == 4 && winningNums == 4)
                                                {
                                                    multiple = Convert.ToDecimal(53);
                                                }
                                                else if (nums.Count == 5 && winningNums == 5)
                                                {
                                                    multiple = Convert.ToDecimal(188);
                                                }
                                            }
                                            //排名
                                            else if (bet.Contains('^') && bet.Contains('>'))
                                            {
                                                var rankingNums = bet.Replace("^", "").Split('>').Select(u => Convert.ToInt32(u)).ToHashSet();
                                                var isWinning = true;
                                                for (int a = 0; a < rankingNums.Count; a++)
                                                {
                                                    if (!storageJsonObj.Numbers.ElementAt(a).Equals(rankingNums.ElementAt(a)))
                                                    {
                                                        isWinning = false;
                                                        break;
                                                    }
                                                }

                                                if (isWinning)
                                                {
                                                    switch (rankingNums.Count)
                                                    {
                                                        case 1:
                                                            multiple = Convert.ToDecimal(15);
                                                            break;
                                                        case 2:
                                                            multiple = Convert.ToDecimal(45);
                                                            break;
                                                        case 3:
                                                            multiple = Convert.ToDecimal(2888);
                                                            break;
                                                        default:
                                                            break;
                                                    }
                                                }
                                            }
                                            //靠前
                                            else if (bet.Contains('^') && bet.Contains('/'))
                                            {
                                                var nums = bet.Replace("^", "").Split('/').Select(u => Convert.ToInt32(u)).ToHashSet();

                                                if (storageJsonObj.Numbers.Take(nums.Count).Count(u => nums.Contains(u)) == nums.Count && nums.Count == 2)
                                                {
                                                    multiple = Convert.ToDecimal(23);
                                                }
                                                else if (storageJsonObj.Numbers.Take(nums.Count).Count(u => nums.Contains(u)) == nums.Count && nums.Count == 3)
                                                {
                                                    multiple = Convert.ToDecimal(888);
                                                }
                                            }

                                            //中奖了
                                            if (multiple > 0)
                                            {
                                                await Helper.PlayerWinningFromPlatform(db, platform, game, gameHistory, bettingHistory, "幸运8", multiple);

                                                //下注金额
                                                var betAmount = bettingHistory.Amount + bettingHistory.BonusAmount;
                                                //赔偿金额
                                                var bonus = Math.Round(betAmount * multiple, 2, MidpointRounding.AwayFromZero);
                                                lotteryNotice += $"\n\n💵 {bettingHistory.Name} <b>{betAmount}U买{bet} 奖 " + bonus + "U</b>";
                                            }
                                        }
                                    }

                                    // 将输入的日期时间字符串转换为 DateTimeOffset 对象 Saturday, Mar 02,2024 03:14 am (ACDT)
                                    var localDateTime = DateTimeOffset.ParseExact(resultObj.Time, "dddd, MMM dd,yyyy hh:mm tt '(ACDT)'", CultureInfo.InvariantCulture);
                                    var openUTCTime = localDateTime.DateTime;
                                    int milliseconds = Convert.ToInt32((openUTCTime.AddMinutes(4.5) - DateTime.UtcNow.AddHours(10).AddMinutes(30)).TotalMilliseconds);
                                    await LotteryDraw(db, gameHistory, JsonConvert.SerializeObject(storageJsonObj), lotteryNotice, platform, drawMsg, resultObj.Draw, resultObj.NextDraw, openUTCTime, botClient, game, "幸运8", milliseconds);
                                }
                            }
                        }
                        try
                        {
                            await db.SaveChangesAsync();
                        }
                        catch (Exception ex)
                        {
                            Log.Error("每秒轮询执行幸运8数据库保存出错:" + ex.Message);
                        }
                        isAustralianLuckyRuning = false;
                    });
                }

                //骰子快三
                if (isRun && isSkipMinute && dbcache.Games.Any(g => g.GameStatus == GameStatus.Open && g.GameType == GameType.Dice))
                {
                    _ = Task.Run(async () =>
                    {
                        using var db = new DataContext();

                        var utcNow = DateTime.UtcNow;
                        //下一期
                        var nextNum = utcNow.AddMinutes(1).ToString("yyMMddHHmm");

                        var results = from p in platforms
                                      from g in db.Games
                                      where p.CreatorId == g.CreatorId && g.GameStatus == GameStatus.Open && g.GameType == GameType.Dice
                                      select new { platform = p, game = g };
                        Log.WriteLine($"对{results.Count()}个骰子的台子进行开奖通知");
                        foreach (var result in results)
                        {
                            var platform = result.platform;
                            var botClient = _botClientList.First(u => u.BotId == platform.BotId);
                            var game = result.game;
                            if (platform.GroupId == null)
                                continue;

                            var resultObj = new DiceData
                            {
                                Cycle = utcNow.ToString("yyMMddHHmm")
                            };

                            for (int i = 0; i < 3; i++)
                            {
                                try
                                {
                                    var diceMsg = await botClient.SendDiceAsync(platform.GroupId, game.ThreadId, Emoji.Dice);
                                    resultObj.Numbers.Add(diceMsg.Dice!.Value);
                                }
                                catch (Exception ex)
                                {
                                    Log.Error($"机器人{botClient.BotId}向群Id{platform.GroupId}发送骰子时出错:" + ex.Message);
                                    i--;
                                }
                            }

                            #region 定义存储数据
                            var sum = resultObj.Numbers.Sum();
                            resultObj.Sum = sum;
                            resultObj.DaXiao = sum >= 11 ? '大' : '小';
                            resultObj.DanShuang = sum % 2 == 0 ? '双' : '单';
                            resultObj.XiaoDanXiaoShuangDaDanDaShuang = resultObj.DaXiao.ToString() + resultObj.DanShuang.ToString();

                            if (Helper.AreConsecutive(resultObj.Numbers[0], resultObj.Numbers[1], resultObj.Numbers[2]))
                                resultObj.ShunZi = resultObj.Numbers[0].ToString() + resultObj.Numbers[1].ToString() + resultObj.Numbers[2].ToString();

                            if (resultObj.Numbers[0] == resultObj.Numbers[1] && resultObj.Numbers[0] == resultObj.Numbers[2])
                                resultObj.BaoZi = resultObj.Numbers[0].ToString() + resultObj.Numbers[1].ToString() + resultObj.Numbers[2].ToString();

                            if (Helper.HasExactlyTwoSameNumbers(resultObj.Numbers[0], resultObj.Numbers[1], resultObj.Numbers[2]))
                                resultObj.DuiZi = string.Join("", resultObj.Numbers.Distinct().Select(u => u.ToString()));

                            if (sum >= 15)
                            {
                                resultObj.JiDaJiXiao = "极大";
                            }
                            else if (sum <= 6)
                            {
                                resultObj.JiDaJiXiao = "极小";
                            }
                            resultObj.IsThreeDifferent = resultObj.Numbers.Distinct().Count() == 3;
                            #endregion

                            #region 定义返回开奖公示
                            var lotteryNotice = $"<b>----------- {resultObj.Cycle}期-----------</b>" +
                            $"\n\n<b>开奖结果</b> <b>{resultObj.Numbers[0]} + {resultObj.Numbers[1]} + {resultObj.Numbers[2]} = {sum}";

                            if (resultObj.DaXiao != '\0')
                                lotteryNotice += "、 " + resultObj.DaXiao;

                            if (resultObj.DanShuang != '\0')
                                lotteryNotice += "、 " + resultObj.DanShuang;

                            if (!string.IsNullOrEmpty(resultObj.XiaoDanXiaoShuangDaDanDaShuang))
                                lotteryNotice += "、 " + resultObj.XiaoDanXiaoShuangDaDanDaShuang;

                            if (!string.IsNullOrEmpty(resultObj.ShunZi))
                                lotteryNotice += "、 顺子:" + resultObj.ShunZi;

                            if (!string.IsNullOrEmpty(resultObj.BaoZi))
                                lotteryNotice += "、 豹子:" + resultObj.BaoZi;

                            if (!string.IsNullOrEmpty(resultObj.DuiZi))
                                lotteryNotice += "、 对子:" + resultObj.DuiZi;

                            if (!string.IsNullOrEmpty(resultObj.JiDaJiXiao))
                                lotteryNotice += "、 " + resultObj.JiDaJiXiao;

                            if (resultObj.IsThreeDifferent)
                                lotteryNotice += "、 3不同";

                            lotteryNotice += "</b>";
                            lotteryNotice += $"\n\n<b>---------🎉 中奖玩家 🎉---------</b>";
                            #endregion

                            GameHistory? gameHistory = null;
                            try
                            {
                                //获取本期记录(可能新开奖的)
                                gameHistory = await db.GameHistorys.FirstOrDefaultAsync(u => u.GroupId == Convert.ToInt64(platform.GroupId) && u.GameId == game.Id && u.LotteryDrawId == resultObj.Cycle);
                            }
                            catch (Exception ex)
                            {
                                Log.Error("获取骰子本期开奖记录时出错:" + ex.Message);
                            }
                            //开奖信息
                            Message? drawMsg = null;
                            //玩家下注本游戏记录
                            IQueryable<PlayerFinanceHistory>? bettingHistorys = gameHistory != null && gameHistory.MessageId > 0 && string.IsNullOrEmpty(gameHistory.LotteryDrawJson)
                            ? db.PlayerFinanceHistorys.Where(u => u.FinanceStatus == FinanceStatus.Success && u.Type == FinanceType.Dice && u.GameId == game.Id && u.GameMessageId == gameHistory.MessageId)
                            : null;
                            //开奖
                            if (gameHistory != null && string.IsNullOrEmpty(gameHistory.LotteryDrawJson) && bettingHistorys != null && bettingHistorys.Any())
                            {
                                foreach (var bettingHistory in bettingHistorys)
                                {
                                    //下注字符串
                                    var bet = bettingHistory.Remark;
                                    if (string.IsNullOrEmpty(bet))
                                        continue;

                                    //赔偿倍数
                                    decimal multiple = 0;

                                    //小/大/单/双
                                    if (sum <= 10 && Regex.IsMatch(bet, "^" + _betItems["s"])
                                    || sum >= 11 && Regex.IsMatch(bet, "^" + _betItems["l"])
                                    || sum % 2 != 0 && Regex.IsMatch(bet, "^" + _betItems["o"])
                                    || sum % 2 == 0 && Regex.IsMatch(bet, "^" + _betItems["e"]))
                                    {
                                        multiple = Convert.ToDecimal(1.95);
                                    }
                                    //小单/小双/大单/大双
                                    else if (resultObj.XiaoDanXiaoShuangDaDanDaShuang == "小单" && Regex.IsMatch(bet, "^" + _betItems["so"])
                                    || resultObj.XiaoDanXiaoShuangDaDanDaShuang == "小双" && Regex.IsMatch(bet, "^" + _betItems["se"])
                                    || resultObj.XiaoDanXiaoShuangDaDanDaShuang == "大单" && Regex.IsMatch(bet, "^" + _betItems["lo"])
                                    || resultObj.XiaoDanXiaoShuangDaDanDaShuang == "大双" && Regex.IsMatch(bet, "^" + _betItems["le"]))
                                    {
                                        multiple = Convert.ToDecimal(3.6);
                                    }
                                    //极小/极大
                                    else if (sum <= 6 && Regex.IsMatch(bet, "^" + _betItems["xs"])
                                       || sum >= 15 && Regex.IsMatch(bet, "^" + _betItems["xl"]))
                                    {
                                        multiple = Convert.ToDecimal(7.56);
                                    }
                                    //顺子
                                    else if (!string.IsNullOrEmpty(resultObj.ShunZi) && Regex.IsMatch(bet, "^" + _betItems["st"]))
                                    {
                                        multiple = Convert.ToDecimal(6.3);
                                    }
                                    //指定顺子
                                    else if (!string.IsNullOrEmpty(resultObj.ShunZi))
                                    {
                                        if (resultObj.ShunZi == "123" && bet == "123"
                                        || resultObj.ShunZi == "234" && bet == "234"
                                        || resultObj.ShunZi == "345" && bet == "345"
                                        || resultObj.ShunZi == "456" && bet == "456")
                                            multiple = Convert.ToDecimal(37.8);
                                    }
                                    //豹子
                                    else if (!string.IsNullOrEmpty(resultObj.BaoZi) && Regex.IsMatch(bet, "^" + _betItems["t"]))
                                    {
                                        multiple = Convert.ToDecimal(20.2);
                                    }
                                    //指定豹子
                                    else if (!string.IsNullOrEmpty(resultObj.BaoZi))
                                    {
                                        if (resultObj.BaoZi == "111" && bet == "111"
                                        || resultObj.BaoZi == "222" && bet == "222"
                                        || resultObj.BaoZi == "333" && bet == "333"
                                        || resultObj.BaoZi == "444" && bet == "444"
                                        || resultObj.BaoZi == "555" && bet == "555"
                                        || resultObj.BaoZi == "666" && bet == "666")
                                            multiple = Convert.ToDecimal(129.68);
                                    }
                                    //对子
                                    else if (!string.IsNullOrEmpty(resultObj.DuiZi) && Regex.IsMatch(bet, "^" + _betItems["p"]))
                                    {
                                        multiple = Convert.ToDecimal(1.68);
                                    }
                                    //3不同
                                    else if (resultObj.IsThreeDifferent && Regex.IsMatch(bet, "^" + _betItems["dt"]))
                                    {
                                        multiple = Convert.ToDecimal(1.26);
                                    }
                                    //和值
                                    else if (resultObj.Sum == 3 && bet == "3" || resultObj.Sum == 18 && bet == "18")
                                    {
                                        multiple = Convert.ToDecimal(129.60);
                                    }
                                    else if (resultObj.Sum == 4 && bet == "4" || resultObj.Sum == 17 && bet == "17")
                                    {
                                        multiple = Convert.ToDecimal(43.80);
                                    }
                                    else if (resultObj.Sum == 5 && bet == "5" || resultObj.Sum == 16 && bet == "16")
                                    {
                                        multiple = Convert.ToDecimal(21.60);
                                    }
                                    else if (resultObj.Sum == 6 && bet == "6" || resultObj.Sum == 15 && bet == "15")
                                    {
                                        multiple = Convert.ToDecimal(12.96);
                                    }
                                    else if (resultObj.Sum == 7 && bet == "7" || resultObj.Sum == 14 && bet == "14")
                                    {
                                        multiple = Convert.ToDecimal(8.63);
                                    }
                                    else if (resultObj.Sum == 8 && bet == "8" || resultObj.Sum == 13 && bet == "13")
                                    {
                                        multiple = Convert.ToDecimal(6.16);
                                    }
                                    else if (resultObj.Sum == 9 && bet == "9" || resultObj.Sum == 12 && bet == "12")
                                    {
                                        multiple = Convert.ToDecimal(5.18);
                                    }
                                    else if (resultObj.Sum == 10 && bet == "10" || resultObj.Sum == 11 && bet == "11")
                                    {
                                        multiple = Convert.ToDecimal(4.80);
                                    }

                                    //中奖了
                                    if (multiple > 0)
                                    {
                                        await Helper.PlayerWinningFromPlatform(db, platform, game, gameHistory, bettingHistory, "骰子", multiple);

                                        //下注金额
                                        var betAmount = bettingHistory.Amount + bettingHistory.BonusAmount;
                                        //赔偿金额
                                        var bonus = Math.Round(betAmount * multiple, 2, MidpointRounding.AwayFromZero);
                                        lotteryNotice += $"\n\n💵 {bettingHistory.Name} <b>{betAmount}U买{bet} 奖 " + bonus + "U</b>";
                                    }
                                }
                            }
                            var openUTCTime = utcNow;
                            int milliseconds = 40000;
                            await LotteryDraw(db, gameHistory, JsonConvert.SerializeObject(resultObj), lotteryNotice, platform, drawMsg, resultObj.Cycle, nextNum, openUTCTime, botClient, game, "骰子", milliseconds);
                        }

                        try
                        {
                            await db.SaveChangesAsync();
                        }
                        catch (Exception ex)
                        {
                            Log.Error("每秒轮询执行骰子数据库保存出错:" + ex.Message);
                        }
                    });
                }

#warning 得分上下注的和开奖的得分不匹配,故此赔率也要重新计算
                //保龄球              
                if (isRun && isSkipMinute && dbcache.Games.Any(g => g.GameStatus == GameStatus.Open && g.GameType == GameType.Bowling))
                {
                    _ = Task.Run(async () =>
                    {
                        using var db = new DataContext();

                        var utcNow = DateTime.UtcNow;
                        //下一期
                        var nextNum = utcNow.AddMinutes(1).ToString("yyMMddHHmm");

                        var results = from p in platforms
                                      from g in db.Games
                                      where p.CreatorId == g.CreatorId && g.GameStatus == GameStatus.Open && g.GameType == GameType.Bowling
                                      select new { platform = p, game = g };
                        Log.WriteLine($"对{results.Count()}个保龄球的台子进行开奖通知");
                        foreach (var result in results)
                        {
                            var platform = result.platform;
                            var botClient = _botClientList.First(u => u.BotId == platform.BotId);
                            var game = result.game;
                            if (platform.GroupId == null)
                                continue;

                            var resultObj = new BowlingData
                            {
                                Cycle = utcNow.ToString("yyMMddHHmm")
                            };

                            for (int i = 0; i < 3; i++)
                            {
                                try
                                {
                                    var bowlingMsg = await botClient.SendDiceAsync(platform.GroupId, game.ThreadId, Emoji.Bowling);

                                    switch (bowlingMsg.Dice!.Value)
                                    {
                                        case 6:
                                            resultObj.Numbers.Add(6);
                                            break;
                                        case 5:
                                            resultObj.Numbers.Add(5);
                                            break;
                                        case 4:
                                            resultObj.Numbers.Add(4);
                                            break;
                                        case 3:
                                            resultObj.Numbers.Add(3);
                                            break;
                                        case 2:
                                            resultObj.Numbers.Add(1);
                                            break;
                                        default:
                                            resultObj.Numbers.Add(0);
                                            break;
                                    }

                                }
                                catch (Exception ex)
                                {
                                    Log.Error($"机器人{botClient.BotId}向群Id{platform.GroupId}发送保龄球时出错:" + ex.Message);
                                    i--;
                                }
                            }

                            #region 定义存储数据
                            var sum = resultObj.Numbers.Sum();
                            resultObj.Sum = sum;
                            resultObj.IsContinuous = resultObj.Numbers[0] + 1 == resultObj.Numbers[1] && resultObj.Numbers[1] + 1 == resultObj.Numbers[2];
                            resultObj.IsTriple = resultObj.Numbers[0] == resultObj.Numbers[1] && resultObj.Numbers[0] == resultObj.Numbers[2];
                            resultObj.IsPair = Helper.HasExactlyTwoSameNumbers(resultObj.Numbers[0], resultObj.Numbers[1], resultObj.Numbers[2]);
                            #endregion

                            #region 定义返回开奖公示
                            var lotteryNotice = $"<b>----------- {resultObj.Cycle}期-----------</b>" +
                            $"\n\n<b>开奖结果</b> <b>{resultObj.Numbers[0]} + {resultObj.Numbers[1]} + {resultObj.Numbers[2]} = {sum}";

                            if (resultObj.IsContinuous)
                                lotteryNotice += "、 连顺";

                            if (resultObj.IsTriple)
                                lotteryNotice += "、 豹子";

                            if (resultObj.IsPair)
                                lotteryNotice += "、 对子";

                            lotteryNotice += "</b>";
                            lotteryNotice += $"\n\n<b>---------🎉 中奖玩家 🎉---------</b>";
                            #endregion

                            GameHistory? gameHistory = null;
                            try
                            {
                                //获取本期记录(可能新开奖的)
                                gameHistory = await db.GameHistorys.FirstOrDefaultAsync(u => u.GroupId == Convert.ToInt64(platform.GroupId) && u.GameId == game.Id && u.LotteryDrawId == resultObj.Cycle);
                            }
                            catch (Exception ex)
                            {
                                Log.Error("获取保龄球本期开奖记录时出错:" + ex.Message);
                            }
                            //开奖信息
                            Message? drawMsg = null;
                            //玩家下注本游戏记录
                            IQueryable<PlayerFinanceHistory>? bettingHistorys = gameHistory != null && gameHistory.MessageId > 0 && string.IsNullOrEmpty(gameHistory.LotteryDrawJson)
                            ? db.PlayerFinanceHistorys.Where(u => u.FinanceStatus == FinanceStatus.Success && u.Type == FinanceType.Bowling && u.GameId == game.Id && u.GameMessageId == gameHistory.MessageId)
                            : null;
                            //开奖
                            if (gameHistory != null && string.IsNullOrEmpty(gameHistory.LotteryDrawJson) && bettingHistorys != null && bettingHistorys.Any())
                            {
                                foreach (var bettingHistory in bettingHistorys)
                                {
                                    //下注字符串
                                    var bet = bettingHistory.Remark;
                                    if (string.IsNullOrEmpty(bet))
                                        continue;

                                    //赔偿倍数
                                    decimal multiple = 0;

                                    //连顺
                                    if (resultObj.IsContinuous && Regex.IsMatch(bet, "^" + _betItems["st"]))
                                    {
                                        multiple = Convert.ToDecimal(43.2);
                                    }
                                    //豹子
                                    else if (resultObj.IsTriple && Regex.IsMatch(bet, "^" + _betItems["t"]))
                                    {
                                        multiple = Convert.ToDecimal(21.6);
                                    }
                                    //对子
                                    else if (resultObj.IsPair && Regex.IsMatch(bet, "^" + _betItems["p"]))
                                    {
                                        multiple = Convert.ToDecimal(7.20);
                                    }
                                    //和值
                                    else if (resultObj.Sum == 0 && bet == "0" || resultObj.Sum == 18 && bet == "18")
                                    {
                                        multiple = Convert.ToDecimal(64.8);
                                    }
                                    else if (resultObj.Sum == 1 && bet == "1"
                                    || resultObj.Sum == 2 && bet == "2"
                                    || resultObj.Sum == 4 && bet == "4"
                                    || resultObj.Sum == 17 && bet == "17")
                                    {
                                        multiple = Convert.ToDecimal(7.20);
                                    }
                                    else if (resultObj.Sum == 3 && bet == "3")
                                    {
                                        multiple = Convert.ToDecimal(32.4);
                                    }
                                    else if (resultObj.Sum == 5 && bet == "5"
                                    || resultObj.Sum == 14 && bet == "14"
                                    || resultObj.Sum == 10 && bet == "10")
                                    {
                                        multiple = Convert.ToDecimal(2.7);
                                    }
                                    else if (resultObj.Sum == 6 && bet == "6"
                                    || resultObj.Sum == 13 && bet == "13")
                                    {
                                        multiple = Convert.ToDecimal(4.30);
                                    }
                                    else if (resultObj.Sum == 7 && bet == "7"
                                    || resultObj.Sum == 8 && bet == "8")
                                    {
                                        multiple = Convert.ToDecimal(3.6);
                                    }
                                    else if (resultObj.Sum == 9 && bet == "9")
                                    {
                                        multiple = Convert.ToDecimal(5.8);
                                    }
                                    else if (resultObj.Sum == 11 && bet == "11")
                                    {
                                        multiple = Convert.ToDecimal(6.1);
                                    }
                                    else if (resultObj.Sum == 12 && bet == "12")
                                    {
                                        multiple = Convert.ToDecimal(6.8);
                                    }
                                    else if (resultObj.Sum == 15 && bet == "15")
                                    {
                                        multiple = Convert.ToDecimal(12.9);
                                    }
                                    else if (resultObj.Sum == 16 && bet == "16")
                                    {
                                        multiple = Convert.ToDecimal(21.6);
                                    }

                                    //中奖了
                                    if (multiple > 0)
                                    {
                                        await Helper.PlayerWinningFromPlatform(db, platform, game, gameHistory, bettingHistory, "保龄球", multiple);

                                        //下注金额
                                        var betAmount = bettingHistory.Amount + bettingHistory.BonusAmount;
                                        //赔偿金额
                                        var bonus = Math.Round(betAmount * multiple, 2, MidpointRounding.AwayFromZero);
                                        lotteryNotice += $"\n\n💵 {bettingHistory.Name} <b>{betAmount}U买{bet} 奖 " + bonus + "U</b>";
                                    }
                                }
                            }
                            var openUTCTime = utcNow;
                            int milliseconds = 40000;
                            await LotteryDraw(db, gameHistory, JsonConvert.SerializeObject(resultObj), lotteryNotice, platform, drawMsg, resultObj.Cycle, nextNum, openUTCTime, botClient, game, "保龄球", milliseconds);
                        }

                        try
                        {
                            await db.SaveChangesAsync();
                        }
                        catch (Exception ex)
                        {
                            Log.Error("每秒轮询执行保龄球数据库保存出错:" + ex.Message);
                        }
                    });
                }

                //币安价格(3分钟一期
                if (isRun && !isBtcPriceRuning && dbcache.Games.Any(g => g.GameStatus == GameStatus.Open && g.GameType == GameType.BinanceBTCPrice))
                {
                    _ = Task.Run(async () =>
                    {
                        isBtcPriceRuning = true;
                        using var db = new DataContext();
                        var utcNow = DateTime.UtcNow;
                        //定义JSON放到数据库里的
                        var storageJsonObj = new BtcData();
                        //返回开奖结果公示
                        var lotteryNotice = string.Empty;
                        //下期
                        var nextNum = string.Empty;
                        try
                        {
                            using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(3) };
                            var result = await http.GetStringAsync("https://api.binance.com/api/v3/klines?interval=1m&limit=1&symbol=BTCUSDT");
                            var resObj = JsonConvert.DeserializeObject<List<List<object>>>(result);
                            var utcTime = DateTimeOffset.FromUnixTimeMilliseconds(Convert.ToInt64(resObj![0][0])!);
                            storageJsonObj.Cycle = utcTime.ToString("yyMMddHHmm");
                            nextNum = utcTime.AddMinutes(1).ToString("yyMMddHHmm");
                            storageJsonObj.ClosePrice = Convert.ToString(resObj![0][1])!;
                            storageJsonObj.Number = Regex.Match(storageJsonObj.ClosePrice, @"(?<=\.)[0-9]{2}").Value;
                        }
                        catch (Exception ex)
                        {
                            Log.Error("获取比特币价格时出错:" + ex.Message);
                        }

                        if (!prevBtcPriceNums.Contains(storageJsonObj.Cycle))
                        {
                            //上期数
                            prevBtcPriceNums.Add(storageJsonObj.Cycle);
                            //换期才执行
                            if (prevBtcPriceNums.Count > 1)
                            {
                                #region 定义放到数据库里的JSON
                                var priceInt = Convert.ToInt32(storageJsonObj.Number);
                                storageJsonObj.DaXiao = priceInt >= 50 ? '大' : '小';
                                storageJsonObj.DanShuang = priceInt % 2 == 0 ? '双' : '单';
                                storageJsonObj.XiaoDanXiaoShuangDaDanDaShuang = storageJsonObj.DaXiao.ToString() + storageJsonObj.DanShuang.ToString();

                                if (priceInt >= 70)
                                {
                                    storageJsonObj.JiDaJiXiao = "极大";
                                }
                                else if (priceInt <= 19)
                                {
                                    storageJsonObj.JiDaJiXiao = "极小";
                                }

                                if (storageJsonObj.Number.Distinct().Count() == 1)
                                {
                                    storageJsonObj.LongHuHe = '和';
                                }
                                else
                                {
                                    var first = Convert.ToInt32(storageJsonObj.Number[0]);
                                    var last = Convert.ToInt32(storageJsonObj.Number[1]);
                                    storageJsonObj.LongHuHe = first > last ? '龙' : '虎';
                                }
                                #endregion

                                #region 定义返回开奖公示
                                lotteryNotice = $"<b>----------- {storageJsonObj.Cycle}期-----------</b>" +
                                $"\n\n收盘价 <b>{storageJsonObj.ClosePrice}</b>" +
                                    $"\n\n中奖 <b>{storageJsonObj.Number}、 " + storageJsonObj.LongHuHe + "、 " + storageJsonObj.DaXiao + "、 " + storageJsonObj.DanShuang;

                                if (!string.IsNullOrEmpty(storageJsonObj.XiaoDanXiaoShuangDaDanDaShuang))
                                    lotteryNotice += "、 " + storageJsonObj.XiaoDanXiaoShuangDaDanDaShuang;

                                if (!string.IsNullOrEmpty(storageJsonObj.JiDaJiXiao))
                                    lotteryNotice += "、 " + storageJsonObj.JiDaJiXiao;

                                lotteryNotice += "</b>";
                                lotteryNotice += $"\n\n<b>---------🎉 中奖玩家 🎉---------</b>";
                                #endregion

                                var results = from p in platforms
                                              from g in db.Games
                                              where p.CreatorId == g.CreatorId && g.GameStatus == GameStatus.Open && g.GameType == GameType.BinanceBTCPrice
                                              select new { platform = p, game = g };

                                Log.WriteLine($"对{results.Count()}个比特币的台子进行开奖通知");
                                foreach (var result in results)
                                {
                                    var platform = result.platform;
                                    var botClient = _botClientList.First(u => u.BotId == platform.BotId);
                                    var game = result.game;
                                    if (platform.GroupId == null)
                                        continue;

                                    GameHistory? gameHistory = null;
                                    try
                                    {
                                        //获取本期记录(可能新开奖的)
                                        gameHistory = await db.GameHistorys.FirstOrDefaultAsync(u => u.GroupId == Convert.ToInt64(platform.GroupId) && u.GameId == game.Id && u.LotteryDrawId == storageJsonObj.Cycle);
                                    }
                                    catch (Exception ex)
                                    {
                                        Log.Error("获取比特币本期开奖记录时出错:" + ex.Message);
                                    }
                                    //开奖信息
                                    Message? drawMsg = null;
                                    //玩家下注本游戏记录
                                    IQueryable<PlayerFinanceHistory>? bettingHistorys = gameHistory != null && gameHistory.MessageId > 0 && string.IsNullOrEmpty(gameHistory.LotteryDrawJson)
                                    ? db.PlayerFinanceHistorys.Where(u => u.FinanceStatus == FinanceStatus.Success && u.Type == FinanceType.BinanceBTCPrice && u.GameId == game.Id && u.GameMessageId == gameHistory.MessageId)
                                    : null;

                                    //开奖
                                    if (gameHistory != null && string.IsNullOrEmpty(gameHistory.LotteryDrawJson) && bettingHistorys != null && bettingHistorys.Any())
                                    {
                                        foreach (var bettingHistory in bettingHistorys)
                                        {
                                            //下注字符串
                                            var bet = bettingHistory.Remark;
                                            if (string.IsNullOrEmpty(bet))
                                                continue;

                                            //赔偿倍数
                                            decimal multiple = 0;

                                            //小/大/单/双
                                            if (priceInt <= 49 && Regex.IsMatch(bet, "^" + _betItems["s"])
                                            || priceInt >= 50 && Regex.IsMatch(bet, "^" + _betItems["l"])
                                            || priceInt % 2 != 0 && Regex.IsMatch(bet, "^" + _betItems["o"])
                                            || priceInt % 2 == 0 && Regex.IsMatch(bet, "^" + _betItems["e"]))
                                            {
                                                multiple = Convert.ToDecimal(1.95);
                                            }
                                            //小单/小双/大单/大双
                                            else if (storageJsonObj.XiaoDanXiaoShuangDaDanDaShuang == "小单" && Regex.IsMatch(bet, "^" + _betItems["so"])
                                            || storageJsonObj.XiaoDanXiaoShuangDaDanDaShuang == "小双" && Regex.IsMatch(bet, "^" + _betItems["se"])
                                            || storageJsonObj.XiaoDanXiaoShuangDaDanDaShuang == "大单" && Regex.IsMatch(bet, "^" + _betItems["lo"])
                                            || storageJsonObj.XiaoDanXiaoShuangDaDanDaShuang == "大双" && Regex.IsMatch(bet, "^" + _betItems["le"]))
                                            {
                                                multiple = Convert.ToDecimal(3.2);
                                            }
                                            //极小/极大
                                            else if (priceInt <= 19 && Regex.IsMatch(bet, "^" + _betItems["xs"])
                                               || priceInt >= 70 && Regex.IsMatch(bet, "^" + _betItems["xl"]))
                                            {
                                                multiple = Convert.ToDecimal(3.8);
                                            }
                                            //龙、虎
                                            else if (storageJsonObj.LongHuHe == '龙' && Regex.IsMatch(bet, "^" + _betItems["d"])
                                            || storageJsonObj.LongHuHe == '虎' && Regex.IsMatch(bet, "^" + _betItems["tr"]))
                                            {
                                                multiple = Convert.ToDecimal(1.95);
                                            }
                                            //和
                                            else if (storageJsonObj.LongHuHe == '和' && Regex.IsMatch(bet, "^" + _betItems["ti"]))
                                            {
                                                multiple = Convert.ToDecimal(7);
                                            }
                                            //数字
                                            else if (int.TryParse(bet, out int r) && priceInt == r)
                                            {
                                                multiple = Convert.ToDecimal(68);
                                            }

                                            //中奖了
                                            if (multiple > 0)
                                            {
                                                await Helper.PlayerWinningFromPlatform(db, platform, game, gameHistory, bettingHistory, "比特币", multiple);

                                                //下注金额
                                                var betAmount = bettingHistory.Amount + bettingHistory.BonusAmount;
                                                //赔偿金额
                                                var bonus = Math.Round(betAmount * multiple, 2, MidpointRounding.AwayFromZero);
                                                lotteryNotice += $"\n\n💵 {bettingHistory.Name} <b>{betAmount}U买{bet} 奖 " + bonus + "U</b>";
                                            }
                                        }
                                    }
                                    int milliseconds = 45000;
                                    await LotteryDraw(db, gameHistory, JsonConvert.SerializeObject(storageJsonObj), lotteryNotice, platform, drawMsg, storageJsonObj.Cycle, nextNum, utcNow, botClient, game, "比特币", milliseconds);
                                }
                            }
                        }
                        try
                        {
                            await db.SaveChangesAsync();
                        }
                        catch (Exception ex)
                        {
                            Log.Error("每秒轮询执行比特币数据库保存出错:" + ex.Message);
                        }
                        isBtcPriceRuning = false;
                    });
                }

                //六合彩
                if (isRun && !isSixLotteryRuning && dbcache.Games.Any(g => g.GameStatus == GameStatus.Open && g.GameType == GameType.SixLottery))
                {
                    _ = Task.Run(async () =>
                    {
                        isSixLotteryRuning = true;
                        using var db = new DataContext();
                        //定义JSON放到数据库里的
                        SixLotteryData storageJsonObj = new();
                        //返回开奖结果公示
                        var lotteryNotice = string.Empty;
                        try
                        {
                            using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
                            var json = await http.GetStringAsync("https://api.bjjinet.com/data/opencode/amwfsix");
                            var jsonDynamic = JsonConvert.DeserializeObject<dynamic>(json);
                            if (jsonDynamic != null)
                            {
                                var first = jsonDynamic["data"][0];
                                storageJsonObj.Cycle = first["issue"];
                                storageJsonObj.OpenTime = first["openTime"];
                                string num = first["openCode"];
                                storageJsonObj.Numbers = num.Split(',').Select(u => Convert.ToInt32(u)).ToList();
                            }
                        }
                        catch (Exception ex)
                        {
                            Log.Error("六合彩:" + ex.Message);
                        }

                        if (storageJsonObj.Numbers.Count != 0 && !prevSixLotteryNums.Contains(storageJsonObj.Cycle))
                        {
                            //上期数
                            prevSixLotteryNums.Add(storageJsonObj.Cycle);
                            //换期才执行
                            if (prevSixLotteryNums.Count > 1)
                            {
                                #region 定义放到数据库里的JSON                           
                                var superNum = storageJsonObj.Numbers.Last();
                                var firstNum = superNum < 10 ? superNum : Convert.ToInt32(superNum.ToString()[0].ToString());
                                var lastNum = superNum < 11 ? 0 : Convert.ToInt32(superNum.ToString()[1].ToString());
                                if (superNum < 49)
                                {
                                    if (superNum % 2 == 0)
                                    {
                                        storageJsonObj.XiaoDanShuang = superNum <= 24 ? "小双" : "大双";
                                    }
                                    else
                                    {
                                        storageJsonObj.XiaoDanShuang = superNum <= 24 ? "小单" : "大单";
                                    }

                                    storageJsonObj.HeDaXiao = (firstNum + lastNum) <= 6 ? "合小" : "合大";
                                    storageJsonObj.HeDanShuang = (firstNum + lastNum) % 2 == 0 ? "合双" : "合单";
                                }
                                List<int> red = [1, 2, 7, 8, 12, 13, 18, 19, 23, 24, 29, 30, 34, 35, 40, 45, 46];
                                List<int> blue = [3, 4, 9, 10, 14, 15, 20, 25, 26, 31, 36, 37, 41, 42, 47, 48];
                                List<int> green = [5, 6, 11, 16, 17, 21, 22, 27, 28, 32, 33, 38, 39, 43, 44, 49];

                                List<int> jin = [1, 2, 9, 10, 23, 24, 31, 32, 39, 40];
                                List<int> mu = [5, 06, 13, 14, 21, 22, 35, 36, 43, 44];
                                List<int> shui = [11, 12, 19, 20, 27, 28, 41, 42, 49];
                                List<int> huo = [7, 8, 15, 16, 29, 30, 37, 38, 45, 46];
                                List<int> tu = [3, 4, 17, 18, 25, 26, 33, 34, 47, 48];

                                if (red.Contains(superNum))
                                {
                                    storageJsonObj.HongLanLv = '红';
                                    if (superNum < 49)
                                    {
                                        storageJsonObj.HongLanLvDaXiaoDanShuang.Add(superNum <= 24 ? "红小" : "红大");
                                        storageJsonObj.HongLanLvDaXiaoDanShuang.Add(superNum % 2 == 0 ? "红双" : "红单");
                                    }
                                }
                                else if (blue.Contains(superNum))
                                {
                                    storageJsonObj.HongLanLv = '蓝';
                                    if (superNum < 49)
                                    {
                                        storageJsonObj.HongLanLvDaXiaoDanShuang.Add(superNum <= 24 ? "蓝小" : "蓝大");
                                        storageJsonObj.HongLanLvDaXiaoDanShuang.Add(superNum % 2 == 0 ? "蓝双" : "蓝单");
                                    }
                                }
                                else if (green.Contains(superNum))
                                {
                                    storageJsonObj.HongLanLv = '绿';
                                    if (superNum < 49)
                                    {
                                        storageJsonObj.HongLanLvDaXiaoDanShuang.Add(superNum <= 24 ? "绿小" : "绿大");
                                        storageJsonObj.HongLanLvDaXiaoDanShuang.Add(superNum % 2 == 0 ? "绿双" : "绿单");
                                    }
                                }

                                storageJsonObj.HeadNum = superNum < 10 ? 0 : firstNum;
                                storageJsonObj.EndNum = superNum < 11 ? 0 : lastNum;

                                if (jin.Contains(superNum))
                                {
                                    storageJsonObj.WuXing = '金';
                                }
                                else if (mu.Contains(superNum))
                                {
                                    storageJsonObj.WuXing = '木';
                                }
                                else if (shui.Contains(superNum))
                                {
                                    storageJsonObj.WuXing = '水';
                                }
                                else if (huo.Contains(superNum))
                                {
                                    storageJsonObj.WuXing = '火';
                                }
                                else if (tu.Contains(superNum))
                                {
                                    storageJsonObj.WuXing = '土';
                                }
                                storageJsonObj.ShengXiao = Helper.GetChineseZodiac(superNum);
                                #endregion

                                #region 定义返回开奖公示
                                List<string> zhengma = [];
                                var zm = storageJsonObj.Numbers.Take(6).ToList();
                                for (int i = 0; i < zm.Count(); i++)
                                {
                                    var item = zm.ElementAt(i);
                                    zhengma.Add(item + Helper.GetChineseZodiac(item).ToString());
                                }
                                lotteryNotice = $"<b>------- {storageJsonObj.Cycle}期-------" +
                                $"\n{string.Join("、", storageJsonObj.Numbers)}" +
                                $"\n特:{superNum}{storageJsonObj.ShengXiao}\n正:{string.Join("-", zhengma)}\n特征:";
                                lotteryNotice += "特头(" + storageJsonObj.HeadNum + ")";
                                lotteryNotice += "、特尾(" + storageJsonObj.EndNum + ")";
                                lotteryNotice += "、五行(" + storageJsonObj.WuXing + ")";
                                if (superNum < 49)
                                {
                                    lotteryNotice += "、" + storageJsonObj.XiaoDanShuang;
                                    lotteryNotice += "、" + storageJsonObj.HeDaXiao;
                                    lotteryNotice += "、" + storageJsonObj.HeDanShuang;
                                    lotteryNotice += "、" + storageJsonObj.HongLanLv;
                                    foreach (var item in storageJsonObj.HongLanLvDaXiaoDanShuang)
                                    {
                                        lotteryNotice += "、" + item;
                                    }
                                }
                                lotteryNotice += $"\n\n---------🎉 中奖玩家 🎉---------</b>";

                                // 调用方法进行图片调整
                                List<string> drawImg = [];
                                foreach (var number in zm)
                                {
                                    drawImg.Add("six/" + number + ".jpg");
                                }
                                drawImg.Add("six/特码.jpg");
                                drawImg.Add("six/" + superNum + ".jpg");
                                var fileName = string.Empty;
                                switch (storageJsonObj.ShengXiao)
                                {
                                    case '龙':
                                        fileName = "龙六合彩";
                                        break;
                                    case '兔':
                                        fileName = "兔六合彩";
                                        break;
                                    case '虎':
                                        fileName = "虎六合彩";
                                        break;
                                    case '牛':
                                        fileName = "牛六合彩";
                                        break;
                                    case '鼠':
                                        fileName = "鼠六合彩";
                                        break;
                                    case '猪':
                                        fileName = "猪六合彩";
                                        break;
                                    case '狗':
                                        fileName = "狗六合彩";
                                        break;
                                    case '鸡':
                                        fileName = "鸡六合彩";
                                        break;
                                    case '猴':
                                        fileName = "猴六合彩";
                                        break;
                                    case '羊':
                                        fileName = "羊六合彩";
                                        break;
                                    case '马':
                                        fileName = "马六合彩";
                                        break;
                                    case '蛇':
                                        fileName = "蛇六合彩";
                                        break;
                                    default:
                                        break;
                                }
                                Helper.CombineImages(fileName, [.. drawImg]);
                                #endregion
                                var results = from p in platforms
                                              from g in db.Games
                                              where p.CreatorId == g.CreatorId && g.GameStatus == GameStatus.Open && g.GameType == GameType.SixLottery
                                              select new { platform = p, game = g };

                                Log.WriteLine($"对{results.Count()}个六合彩的台子进行开奖通知");
                                foreach (var result in results)
                                {
                                    var platform = result.platform;
                                    var botClient = _botClientList.First(u => u.BotId == platform.BotId);
                                    var game = result.game;
                                    if (platform.GroupId == null)
                                        continue;

                                    GameHistory? gameHistory = null;
                                    try
                                    {
                                        //获取本期记录(可能新开奖的)
                                        gameHistory = await db.GameHistorys.Where(u => u.GroupId == Convert.ToInt64(platform.GroupId) && u.GameId == game.Id).FirstOrDefaultAsync(u => u.LotteryDrawId == storageJsonObj.Cycle);
                                    }
                                    catch (Exception ex)
                                    {
                                        Log.Error("获取六合彩本期开奖记录时出错:" + ex.Message);
                                    }
                                    //开奖信息
                                    Message? drawMsg = null;
                                    //玩家下注本游戏记录
                                    IQueryable<PlayerFinanceHistory>? bettingHistorys = gameHistory != null && gameHistory.MessageId > 0 && string.IsNullOrEmpty(gameHistory.LotteryDrawJson)
                                    ? db.PlayerFinanceHistorys.Where(u => u.FinanceStatus == FinanceStatus.Success && u.Type == FinanceType.SixLottery && u.GameId == game.Id && u.GameMessageId == gameHistory.MessageId)
                                    : null;
                                    //开奖
                                    if (gameHistory != null && string.IsNullOrEmpty(gameHistory.LotteryDrawJson) && bettingHistorys != null && bettingHistorys.Any())
                                    {
                                        foreach (var bettingHistory in bettingHistorys)
                                        {
                                            //下注字符串
                                            var bet = bettingHistory.Remark;
                                            if (string.IsNullOrEmpty(bet))
                                                continue;

                                            //赔偿倍数
                                            decimal multiple = 0;

                                            //小单、小双、大单、大双
                                            if (storageJsonObj.XiaoDanShuang == "小单" && Regex.IsMatch(bet, "^" + _betItems["so"])
                                            || storageJsonObj.XiaoDanShuang == "小双" && Regex.IsMatch(bet, "^" + _betItems["se"])
                                            || storageJsonObj.XiaoDanShuang == "大单" && Regex.IsMatch(bet, "^" + _betItems["lo"])
                                            || storageJsonObj.XiaoDanShuang == "大双" && Regex.IsMatch(bet, "^" + _betItems["le"]))
                                            {
                                                multiple = Convert.ToDecimal(3.38);
                                            }
                                            //合小、合大
                                            else if (storageJsonObj.XiaoDanShuang == "合小" && Regex.IsMatch(bet, "^" + _betItems["scs"])
                                            || storageJsonObj.XiaoDanShuang == "合大" && Regex.IsMatch(bet, "^" + _betItems["scl"]))
                                            {
                                                multiple = Convert.ToDecimal(1.9);
                                            }
                                            //合单、合双
                                            else if (storageJsonObj.XiaoDanShuang == "合单" && Regex.IsMatch(bet, "^" + _betItems["sco"])
                                            || storageJsonObj.XiaoDanShuang == "合双" && Regex.IsMatch(bet, "^" + _betItems["sce"]))
                                            {
                                                multiple = Convert.ToDecimal(1.9);
                                            }
                                            //红、蓝、绿
                                            else if (storageJsonObj.XiaoDanShuang == "红" && Regex.IsMatch(bet, "^" + _betItems["r"])
                                            || storageJsonObj.XiaoDanShuang == "蓝" && Regex.IsMatch(bet, "^" + _betItems["blu"])
                                             || storageJsonObj.XiaoDanShuang == "绿" && Regex.IsMatch(bet, "^" + _betItems["g"]))
                                            {
                                                multiple = Convert.ToDecimal(2.38);
                                            }
                                            //红单、红双、红大、红小、蓝单、蓝双、蓝大、蓝小、绿单、绿双、绿大、绿小
                                            else if (
                                            storageJsonObj.XiaoDanShuang == "红单" && Regex.IsMatch(bet, "^" + _betItems["ro"])
                                            || storageJsonObj.XiaoDanShuang == "红双" && Regex.IsMatch(bet, "^" + _betItems["re"])
                                            || storageJsonObj.XiaoDanShuang == "红大" && Regex.IsMatch(bet, "^" + _betItems["rl"])
                                            || storageJsonObj.XiaoDanShuang == "红小" && Regex.IsMatch(bet, "^" + _betItems["rs"])
                                            || storageJsonObj.XiaoDanShuang == "蓝单" && Regex.IsMatch(bet, "^" + _betItems["bo"])
                                            || storageJsonObj.XiaoDanShuang == "蓝双" && Regex.IsMatch(bet, "^" + _betItems["be"])
                                            || storageJsonObj.XiaoDanShuang == "蓝大" && Regex.IsMatch(bet, "^" + _betItems["bl"])
                                            || storageJsonObj.XiaoDanShuang == "蓝小" && Regex.IsMatch(bet, "^" + _betItems["bs"])
                                            || storageJsonObj.XiaoDanShuang == "绿单" && Regex.IsMatch(bet, "^" + _betItems["go"])
                                            || storageJsonObj.XiaoDanShuang == "绿双" && Regex.IsMatch(bet, "^" + _betItems["ge"])
                                            || storageJsonObj.XiaoDanShuang == "绿大" && Regex.IsMatch(bet, "^" + _betItems["gl"])
                                            || storageJsonObj.XiaoDanShuang == "绿小" && Regex.IsMatch(bet, "^" + _betItems["gs"]))
                                            {
                                                multiple = Convert.ToDecimal(5);
                                            }
                                            //头0、头1、头2、头3、头4
                                            else if (
                                            firstNum == 0 && Regex.IsMatch(bet, "^" + _betItems["h0"])
                                            || firstNum == 1 && Regex.IsMatch(bet, "^" + _betItems["h1"])
                                            || firstNum == 2 && Regex.IsMatch(bet, "^" + _betItems["h2"])
                                            || firstNum == 3 && Regex.IsMatch(bet, "^" + _betItems["h3"])
                                            || firstNum == 4 && Regex.IsMatch(bet, "^" + _betItems["h4"]))
                                            {
                                                multiple = Convert.ToDecimal(4.18);
                                            }
                                            //尾0、尾1、尾2、尾3、尾4、尾5、尾6、尾7、尾8、尾9
                                            else if (
                                            lastNum == 0 && Regex.IsMatch(bet, "^" + _betItems["e0"])
                                            || lastNum == 1 && Regex.IsMatch(bet, "^" + _betItems["e1"])
                                            || lastNum == 2 && Regex.IsMatch(bet, "^" + _betItems["e2"])
                                            || lastNum == 3 && Regex.IsMatch(bet, "^" + _betItems["e3"])
                                            || lastNum == 4 && Regex.IsMatch(bet, "^" + _betItems["e4"])
                                            || lastNum == 5 && Regex.IsMatch(bet, "^" + _betItems["e5"])
                                            || lastNum == 6 && Regex.IsMatch(bet, "^" + _betItems["e6"])
                                            || lastNum == 7 && Regex.IsMatch(bet, "^" + _betItems["e7"])
                                            || lastNum == 8 && Regex.IsMatch(bet, "^" + _betItems["e8"])
                                            || lastNum == 9 && Regex.IsMatch(bet, "^" + _betItems["e9"]))
                                            {
                                                multiple = Convert.ToDecimal(7.8);
                                            }
                                            //金、木、水、火、土
                                            else if (
                                            storageJsonObj.WuXing == '金' && Regex.IsMatch(bet, "^" + _betItems["metal"])
                                            || storageJsonObj.WuXing == '木' && Regex.IsMatch(bet, "^" + _betItems["wood"])
                                            || storageJsonObj.WuXing == '火' && Regex.IsMatch(bet, "^" + _betItems["fire"])
                                            || storageJsonObj.WuXing == '水' && Regex.IsMatch(bet, "^" + _betItems["water"])
                                            || storageJsonObj.WuXing == '土' && Regex.IsMatch(bet, "^" + _betItems["earth"]))
                                            {
                                                multiple = Convert.ToDecimal(4.18);
                                            }
                                            //生肖                                         
                                            else if (
                                            storageJsonObj.ShengXiao == '龙' && Regex.IsMatch(bet, "^" + _betItems["d"])
                                            || storageJsonObj.ShengXiao == '兔' && Regex.IsMatch(bet, "^" + _betItems["rabbit"])
                                            || storageJsonObj.ShengXiao == '虎' && Regex.IsMatch(bet, "^" + _betItems["tr"])
                                            || storageJsonObj.ShengXiao == '牛' && Regex.IsMatch(bet, "^" + _betItems["ox"])
                                            || storageJsonObj.ShengXiao == '鼠' && Regex.IsMatch(bet, "^" + _betItems["rat"])
                                            || storageJsonObj.ShengXiao == '猪' && Regex.IsMatch(bet, "^" + _betItems["pig"])
                                            || storageJsonObj.ShengXiao == '狗' && Regex.IsMatch(bet, "^" + _betItems["dog"])
                                            || storageJsonObj.ShengXiao == '鸡' && Regex.IsMatch(bet, "^" + _betItems["rooster"])
                                            || storageJsonObj.ShengXiao == '猴' && Regex.IsMatch(bet, "^" + _betItems["monkey"])
                                            || storageJsonObj.ShengXiao == '羊' && Regex.IsMatch(bet, "^" + _betItems["goat"])
                                            || storageJsonObj.ShengXiao == '马' && Regex.IsMatch(bet, "^" + _betItems["horse"])
                                            || storageJsonObj.ShengXiao == '蛇' && Regex.IsMatch(bet, "^" + _betItems["snak"]))
                                            {
                                                multiple = Convert.ToDecimal(9.5);
                                            }//特码
                                            else if (bet == superNum.ToString())
                                            {
                                                multiple = Convert.ToDecimal(41.88);
                                            }
                                            //正码龙、正码兔、正码虎、正码牛、正码鼠、正码猪、正码狗、正码鸡、正码猴、正码羊、正码马、正码蛇
                                            else if (bet.Contains("正码"))
                                            {
                                                foreach (var item in zm)
                                                {
                                                    var zmsx = "正码" + Helper.GetChineseZodiac(item).ToString();
                                                    if (bet == zmsx)
                                                    {
                                                        multiple = Convert.ToDecimal(1.58);
                                                        break;
                                                    }
                                                }
                                            }
                                            //正码
                                            else if (bet.Contains('正') && zm.Select(u => u.ToString()).Contains(bet[1..]))
                                            {
                                                multiple = Convert.ToDecimal(6);
                                            }
                                            //包码
                                            else if (bet.Contains('包'))
                                            {
                                                int numOfwins = 0;
                                                var betNums = bet[1..].Split('/').Select(u => Convert.ToInt32(u));
                                                foreach (var bn in betNums)
                                                {
                                                    if (storageJsonObj.Numbers.Contains(bn))
                                                        numOfwins++;
                                                }

                                                if (numOfwins >= 3)
                                                {
                                                    //中3(不包特码)赔38倍
                                                    if (numOfwins == 3 && !betNums.Contains(superNum))
                                                    {
                                                        multiple = Convert.ToDecimal(38);
                                                    }
                                                    //中3(包特码)赔88倍
                                                    else if (numOfwins == 3 && betNums.Contains(superNum))
                                                    {
                                                        multiple = Convert.ToDecimal(88);
                                                    }
                                                    //中4(不包特码)赔188倍
                                                    else if (numOfwins == 4 && !betNums.Contains(superNum))
                                                    {
                                                        multiple = Convert.ToDecimal(188);
                                                    }
                                                    //中4(包特码)赔1800倍
                                                    else if (numOfwins == 4 && betNums.Contains(superNum))
                                                    {
                                                        multiple = Convert.ToDecimal(1800);
                                                    }
                                                    //中5(不包特码)赔8888倍
                                                    else if (numOfwins == 5 && !betNums.Contains(superNum))
                                                    {
                                                        multiple = Convert.ToDecimal(8888);
                                                    }
                                                    //中5(包特码)赔88888倍
                                                    else if (numOfwins == 5 && betNums.Contains(superNum))
                                                    {
                                                        multiple = Convert.ToDecimal(88888);
                                                    }
                                                    //中6赔888888倍
                                                    else if (numOfwins == 6 && !betNums.Contains(superNum))
                                                    {
                                                        multiple = Convert.ToDecimal(888888);
                                                    }
                                                }
                                            }
                                            //连肖
                                            else if (Regex.IsMatch(bet, @"(龙|兔|虎|牛|鼠|猪|狗|鸡|猴|羊|马|蛇)(\+(龙|兔|虎|牛|鼠|猪|狗|鸡|猴|羊|马|蛇)){1,4}$"))
                                            {
                                                var betShengXiao = bet.Split('+');
                                                var numShengXiaos = storageJsonObj.Numbers.Select(u => Helper.GetChineseZodiac(u).ToString());
                                                int numOfwins = 0;
                                                foreach (var item in betShengXiao)
                                                {
                                                    if (numShengXiaos.Contains(item))
                                                        numOfwins++;
                                                }

                                                switch (numOfwins)
                                                {
                                                    case 2:
                                                        multiple = Convert.ToDecimal(2.38);
                                                        break;
                                                    case 3:
                                                        multiple = Convert.ToDecimal(6.88);
                                                        break;
                                                    case 4:
                                                        multiple = Convert.ToDecimal(23.27);
                                                        break;
                                                    case 5:
                                                    case 6:
                                                    case 7:
                                                        multiple = Convert.ToDecimal(72.24);
                                                        break;
                                                    default:
                                                        break;
                                                }
                                            }
                                            //连尾
                                            else if (bet.Contains("连尾"))
                                            {
                                                int numOfwins = 0;
                                                var betNums = bet[2..].Split('+').Select(u => Convert.ToInt32(u));
                                                var numLasts = storageJsonObj.Numbers.Select(u => u < 11 ? 0 : Convert.ToInt32(superNum.ToString()[1].ToString()));
                                                foreach (var item in betNums)
                                                {
                                                    if (numLasts.Contains(item))
                                                        numOfwins++;
                                                }

                                                if (betNums.Count() == 2 && numOfwins == 2)
                                                {
                                                    multiple = Convert.ToDecimal(1.6);
                                                }
                                                else if (betNums.Count() == 3 && numOfwins == 3)
                                                {
                                                    multiple = Convert.ToDecimal(4.8);
                                                }
                                                else if (betNums.Count() == 4 && numOfwins == 4)
                                                {
                                                    multiple = Convert.ToDecimal(15.25);
                                                }
                                                else if (betNums.Count() == 5 && numOfwins == 5)
                                                {
                                                    multiple = Convert.ToDecimal(37);
                                                }
                                            }
                                            //连码
                                            else if (bet.Contains("连码"))
                                            {
                                                int numOfwins = 0;
                                                var betNums = Regex.Match(bet, @"([1-9]|[1-4][0-9])(\+([1-9]|[1-4][0-9])){1,3}").Value.Split('+').Select(u => Convert.ToInt32(u));
                                                foreach (var item in betNums)
                                                {
                                                    if (storageJsonObj.Numbers.Contains(item))
                                                        numOfwins++;
                                                }

                                                //四全中(6000倍)
                                                if (bet.Contains("四全中") && betNums.Count() == 4 && numOfwins == 4)
                                                {
                                                    multiple = Convert.ToDecimal(6000);
                                                }
                                                //三全中(520倍)
                                                else if (bet.Contains("三全中") && betNums.Count() == 3 && numOfwins == 3)
                                                {
                                                    multiple = Convert.ToDecimal(520);
                                                }
                                                //三中三(65倍)
                                                else if (bet.Contains("三中") && betNums.Count() == 3 && numOfwins == 3)
                                                {
                                                    multiple = Convert.ToDecimal(65);
                                                }
                                                //三中二(20倍)
                                                else if (bet.Contains("三中") && betNums.Count() == 3 && numOfwins == 2)
                                                {
                                                    multiple = Convert.ToDecimal(20);
                                                }
                                                //二全中(57倍)
                                                else if (bet.Contains("二全中") && betNums.Count() == 2 && numOfwins == 2)
                                                {
                                                    multiple = Convert.ToDecimal(57);
                                                }
                                                //二中二(29倍) 2个中奖号码都为正码称之为“中二”
                                                else if (bet.Contains("二中") && betNums.Count() == 2 && numOfwins == 2 && !betNums.Contains(superNum))
                                                {
                                                    multiple = Convert.ToDecimal(29);
                                                }
                                                //二中特(25倍) 1个为正码1个为特码称之为“中特”
                                                else if (bet.Contains("二中") && betNums.Count() == 2 && numOfwins == 2 && betNums.Contains(superNum))
                                                {
                                                    multiple = Convert.ToDecimal(25);
                                                }
                                                //特串(120倍)  1个为正码1个为特码
                                                else if (bet.Contains("特串") && betNums.Count() == 2 && numOfwins == 2 && betNums.Contains(superNum))
                                                {
                                                    multiple = Convert.ToDecimal(120);
                                                }
                                            }//中一
                                            else if (bet.Contains("中一"))
                                            {
                                                int numOfwins = 0;
                                                var betNums = Regex.Match(bet, @"([1-9]|[1-4][0-9])(/([1-9]|[1-4][0-9])){2,5}").Value.Split('/').Select(u => Convert.ToInt32(u));
                                                foreach (var item in betNums)
                                                {
                                                    if (storageJsonObj.Numbers.Contains(item))
                                                        numOfwins++;
                                                }

                                                //三中一(2.2倍)
                                                if (betNums.Count() == 3 && numOfwins >= 1)
                                                {
                                                    multiple = Convert.ToDecimal(2.2);
                                                }
                                                //四中一(1.7倍)
                                                else if (betNums.Count() == 4 && numOfwins >= 1)
                                                {
                                                    multiple = Convert.ToDecimal(1.7);
                                                }
                                                //五中一(1.5倍)
                                                else if (betNums.Count() == 5 && numOfwins >= 1)
                                                {
                                                    multiple = Convert.ToDecimal(1.5);
                                                }
                                                //六中一(1.38倍)
                                                else if (betNums.Count() == 6 && numOfwins >= 1)
                                                {
                                                    multiple = Convert.ToDecimal(1.38);
                                                }
                                            }
                                            //不中
                                            else if (bet.Contains("不中"))
                                            {
                                                int numOfwins = 0;
                                                var betNums = Regex.Match(bet, @"([1-9]|[1-4][0-9])(/([1-9]|[1-4][0-9])){2,5}").Value.Split('&').Select(u => Convert.ToInt32(u));
                                                foreach (var item in betNums)
                                                {
                                                    if (storageJsonObj.Numbers.Contains(item))
                                                        numOfwins++;
                                                }

                                                if (numOfwins == 0)
                                                {
                                                    //五不中(1.8倍)
                                                    if (betNums.Count() == 5)
                                                    {
                                                        multiple = Convert.ToDecimal(1.8);
                                                    }
                                                    //六不中(2.2倍)
                                                    else if (betNums.Count() == 6)
                                                    {
                                                        multiple = Convert.ToDecimal(2.2);
                                                    }
                                                    //七不中(2.6倍)
                                                    else if (betNums.Count() == 7)
                                                    {
                                                        multiple = Convert.ToDecimal(2.6);
                                                    }
                                                    //八不中(3.1倍)
                                                    else if (betNums.Count() == 8)
                                                    {
                                                        multiple = Convert.ToDecimal(3.1);
                                                    }
                                                    //九不中(3.7倍)
                                                    else if (betNums.Count() == 9)
                                                    {
                                                        multiple = Convert.ToDecimal(3.7);
                                                    }
                                                    //十不中(4.5倍)
                                                    else if (betNums.Count() == 10)
                                                    {
                                                        multiple = Convert.ToDecimal(4.5);
                                                    }
                                                    //十一不中(5.6倍)
                                                    else if (betNums.Count() == 11)
                                                    {
                                                        multiple = Convert.ToDecimal(5.6);
                                                    }
                                                    //十二不中(6.7倍)
                                                    else if (betNums.Count() == 12)
                                                    {
                                                        multiple = Convert.ToDecimal(6.7);
                                                    }
                                                }
                                            }

                                            //中奖了
                                            if (multiple > 0)
                                            {
                                                await Helper.PlayerWinningFromPlatform(db, platform, game, gameHistory, bettingHistory, "六合彩", multiple);

                                                //下注金额
                                                var betAmount = bettingHistory.Amount + bettingHistory.BonusAmount;
                                                //赔偿金额
                                                var bonus = Math.Round(betAmount * multiple, 2, MidpointRounding.AwayFromZero);
                                                lotteryNotice += $"\n\n💵 {bettingHistory.Name} <b>{betAmount}U买{bet} 奖 " + bonus + "U</b>";
                                            }
                                        }
                                    }
                                    // 将本地时间转换为北京时间
                                    var beijingZone = TimeZoneInfo.FindSystemTimeZoneById("China Standard Time");
                                    DateTime beijingTime = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.Local, beijingZone);
                                    int milliseconds = Convert.ToInt32((storageJsonObj.OpenTime.AddMinutes(4.5) - beijingTime).TotalMilliseconds);
                                    var openUTCTime = TimeZoneInfo.ConvertTimeToUtc(storageJsonObj.OpenTime, beijingZone);
                                    var currentNum = Convert.ToUInt32(storageJsonObj.Cycle.Substring(9));

                                    var nextNum = string.Empty;
                                    if (currentNum >= 288)
                                    {
                                        nextNum = beijingTime.ToString("yyyyMMdd") + "0" + "001";
                                    }
                                    else
                                    {
                                        if (currentNum < 9)
                                        {
                                            nextNum = beijingTime.ToString("yyyyMMdd") + "0" + "00" + (currentNum + 1);
                                        }
                                        else if (currentNum < 99)
                                        {
                                            nextNum = beijingTime.ToString("yyyyMMdd") + "0" + "0" + (currentNum + 1);
                                        }
                                        else
                                        {
                                            nextNum = beijingTime.ToString("yyyyMMdd") + "0" + (currentNum + 1);
                                        }
                                    }
                                    await LotteryDraw(db, gameHistory, JsonConvert.SerializeObject(storageJsonObj), lotteryNotice, platform, drawMsg, storageJsonObj.Cycle, nextNum, openUTCTime, botClient, game, fileName, milliseconds);
                                }
                            }
                        }
                        try
                        {
                            await db.SaveChangesAsync();
                        }
                        catch (Exception ex)
                        {
                            Log.Error("每秒轮询执行六合彩数据库保存出错:" + ex.Message);
                        }
                        isSixLotteryRuning = false;
                    });
                }

                //竞猜\轮盘赌\牛牛\21点\三公\百家乐\龙虎
                if (isRun && isSkipMinute && !isHashRuning && dbcache.Games.Where(u => u.GameStatus == GameStatus.Open).Any(g =>
                    g.GameType == GameType.Roulette
                    || g.GameType == GameType.Cow
                    || g.GameType == GameType.Blackjack
                    || g.GameType == GameType.Sangong
                    || g.GameType == GameType.Baccarat
                    || g.GameType == GameType.TrxHash
                    || g.GameType == GameType.DragonTiger))
                {
                    string txId = string.Empty;
                getTxId:
                    try
                    {


                        var transaction = await Transactions.CreateTransactionAsync("TTTTTX8kc1f12HexiiEWabE8u5fjhC62TT", "TTTC6FqoMWFwzk7mVrwwfBcUB1mDhYdTTT", 0.000001M, true);


                        // var transaction = await Transactions.CreateTransactionAsync("TTTTTX8kc1f12HexiiEWabE8u5fjhC62TT", "TTTC6FqoMWFwzk7mVrwwfBcUB1mDhYdTTT", 0.000001M, true);



                        if (string.IsNullOrEmpty(transaction) || transaction?.Contains("Error") == true)
                        {
                            Log.Error("创建每分钟的HASH交易地址时出错:" + transaction);
                            goto getTxId;
                        }
                        var signedTransaction = Transactions.SignTransaction(transaction!, _tronPrivateKey);
                        var broadcast = await Transactions.BroadcastTransactionAsync(signedTransaction);
                        var broadcastObj = broadcast == null ? null : JsonConvert.DeserializeObject<dynamic>(broadcast);
                        if (broadcastObj == null || broadcastObj?.result == false)
                        {
                            Log.Error("广播每分钟的HASH地址时出错:" + broadcast);
                            goto getTxId;
                        }
                        txId = broadcastObj!.txid!;
                    }
                    catch (Exception ex)
                    {
                        Log.Error("创建每分钟的HASH交易地址时出错:" + ex.Message);
                        goto getTxId;
                    }

                    //本期
                    var currentNum = DateTime.UtcNow.ToString("yyMMddHHmm");
                    var openUTCTime = DateTime.UtcNow;
                    if (!prevHashNums.Contains(currentNum))
                    {
                        //已开期数
                        prevHashNums.Add(currentNum);
                        //换期才执行
                        if (prevHashNums.Count > 1)
                        {
                            //下一期的期数
                            var nextNum = DateTime.UtcNow.AddMinutes(1).ToString("yyMMddHHmm");

                            isHashRuning = true;
                            using var db = new DataContext();

                            //竞猜:每分钟波场TRX交易获得64位字符串哈希,玩家对后两位进行投注
                            if (isRun && dbcache.Games.Where(u => u.GameStatus == GameStatus.Open).Any(g => g.GameType == GameType.TrxHash))
                            {
                                //返回开奖结果公示
                                var lotteryNotice = string.Empty;
                                var res = txId.Substring(txId.Length - 2);
                                var resNum = Regex.Match(res, @"[0-9]{0,2}");
                                int sum = 0;
                                if (resNum != null && !string.IsNullOrEmpty(resNum.Value))
                                {
                                    if (resNum.Value.Length == 1)
                                    {
                                        sum = Convert.ToInt32(resNum.Value);
                                    }
                                    else
                                    {
                                        sum = Convert.ToInt32(resNum.Value[0].ToString()) + Convert.ToInt32(resNum.Value[1].ToString());
                                    }
                                }

                                #region 定义放到数据库里的JSON
                                var storageJsonObj = new TrxHashData
                                {
                                    Phase = currentNum,
                                    Numbers = txId,
                                    Time = DateTime.UtcNow,
                                    IsDuiZi = res.ToCharArray().Distinct().Count() == 1,
                                    Sum = sum
                                };
                                #endregion

                                #region 定义返回开奖公示
                                lotteryNotice = $"<b>---------- {currentNum}期----------" +
                                     $"\n{txId}" +
                                     $"\n\n结果:{res}、 和值({sum})";

                                //数字+字母
                                if (Regex.IsMatch(res, @"[0-9][a-z]"))
                                {
                                    lotteryNotice += "、 数字+字母";
                                }
                                //字母+数字
                                else if (Regex.IsMatch(res, @"[a-z][0-9]"))
                                {
                                    lotteryNotice += "、 字母+数字";
                                }
                                //全数字
                                else if (Regex.IsMatch(res, @"[0-9][0-9]"))
                                {
                                    lotteryNotice += "、 全数字";
                                }
                                //全字母
                                else if (Regex.IsMatch(res, @"[a-z][a-z]"))
                                {
                                    lotteryNotice += "、 全字母";
                                }

                                if (storageJsonObj.IsDuiZi)
                                    lotteryNotice += "、 对子";

                                lotteryNotice += $"\n\n---------🎉 中奖玩家 🎉---------</b>";
                                #endregion
                                var results = from p in platforms
                                              from g in db.Games
                                              where p.CreatorId == g.CreatorId && g.GameStatus == GameStatus.Open && g.GameType == GameType.TrxHash
                                              select new { platform = p, game = g };

                                Log.WriteLine($"对{results.Count()}个竞猜的台子进行开奖通知");
                                foreach (var result in results)
                                {
                                    var platform = result.platform;
                                    var botClient = _botClientList.First(u => u.BotId == platform.BotId);
                                    var game = result.game;
                                    if (platform.GroupId == null)
                                        continue;
                                    GameHistory? gameHistory = null;
                                    try
                                    {
                                        //获取本期记录(可能新开奖的)
                                        gameHistory = await db.GameHistorys.FirstOrDefaultAsync(u => u.GroupId == Convert.ToInt64(platform.GroupId) && u.GameId == game.Id && u.LotteryDrawId == currentNum);
                                    }
                                    catch (Exception ex)
                                    {
                                        Log.Error("获取竞猜本期开奖记录时出错:" + ex.Message);
                                    }
                                    //开奖信息
                                    Message? drawMsg = null;
                                    //玩家下注本游戏记录
                                    List<PlayerFinanceHistory>? bettingHistorys = gameHistory != null && gameHistory.MessageId > 0 && string.IsNullOrEmpty(gameHistory.LotteryDrawJson)
                                    ? db.PlayerFinanceHistorys.Where(u => u.FinanceStatus == FinanceStatus.Success && u.Type == FinanceType.TrxHash && u.GameId == game.Id && u.GameMessageId == gameHistory.MessageId).ToList()
                                    : null;
                                    //开奖
                                    if (gameHistory != null && string.IsNullOrEmpty(gameHistory.LotteryDrawJson) && bettingHistorys != null && bettingHistorys.Count != 0)
                                    {
                                        foreach (var bettingHistory in bettingHistorys)
                                        {
                                            //下注字符串
                                            var bet = bettingHistory.Remark;
                                            if (string.IsNullOrEmpty(bet))
                                                continue;

                                            //赔偿倍数
                                            decimal multiple = 0;

                                            //数字+字母/字母+数字
                                            if (Regex.IsMatch(res, @"[0-9][a-z]") && Regex.IsMatch(bet, @"^(nl|数字+字母|数字字母)$")
                                                || Regex.IsMatch(res, @"[a-z][0-9]") && Regex.IsMatch(bet, @"^(ln|字母+数字|字母数字)$"))
                                            {
                                                multiple = Convert.ToDecimal(2.98);
                                            }
                                            //全数字
                                            else if (Regex.IsMatch(res, @"[0-9][0-9]") && Regex.IsMatch(bet, Program._betItems["n"]))
                                            {
                                                multiple = Convert.ToDecimal(1.79);
                                            }
                                            //全字母
                                            else if (Regex.IsMatch(res, @"[a-z][a-z]") && Regex.IsMatch(bet, Program._betItems["lr"]))
                                            {
                                                multiple = Convert.ToDecimal(4.97);
                                            }
                                            //对子
                                            else if (storageJsonObj.IsDuiZi && Regex.IsMatch(bet, Program._betItems["p"]))
                                            {
                                                multiple = Convert.ToDecimal(11.2);
                                            }

                                            //中奖了
                                            if (multiple > 0)
                                            {
                                                await Helper.PlayerWinningFromPlatform(db, platform, game, gameHistory, bettingHistory, "竞猜", multiple);

                                                //下注金额
                                                var betAmount = bettingHistory.Amount + bettingHistory.BonusAmount;
                                                //赔偿金额
                                                var bonus = Math.Round(betAmount * multiple, 2, MidpointRounding.AwayFromZero);
                                                lotteryNotice += $"\n\n💵 {bettingHistory.Name} <b>{betAmount}U买{bet} 奖 " + bonus + "U</b>";
                                            }
                                        }
                                    }

                                    int milliseconds = Convert.ToInt32((openUTCTime.AddSeconds(45) - DateTime.UtcNow).TotalMilliseconds);
                                    await LotteryDraw(db, gameHistory, JsonConvert.SerializeObject(storageJsonObj), lotteryNotice, platform, drawMsg, currentNum, nextNum, openUTCTime, botClient, game, "竞猜", milliseconds);
                                }
                            }

                            //龙虎
                            if (isRun && dbcache.Games.Where(u => u.GameStatus == GameStatus.Open).Any(g => g.GameType == GameType.DragonTiger))
                            {
                                //返回开奖结果公示
                                var lotteryNotice = string.Empty;

                                var firstNum = Convert.ToInt32(txId.First());
                                var lastNum = Convert.ToInt32(txId.Last());
                                var fileName = "";
                                #region 定义放到数据库里的JSON
                                var storageJsonObj = new DragonTigerData
                                {
                                    Phase = currentNum,
                                    Number = txId,
                                    Time = DateTime.UtcNow
                                };
                                if (firstNum == lastNum)
                                {
                                    storageJsonObj.Res = '和';
                                    fileName = "龙虎和";
                                }
                                else
                                {
                                    storageJsonObj.Res = firstNum > lastNum ? '龙' : '虎';
                                    fileName = firstNum > lastNum ? "龙赢" : "虎赢";
                                }
                                #endregion

                                #region 定义返回开奖公示
                                lotteryNotice = $"<b>---------- {currentNum}期----------" +
                                     $"\n{txId}" +
                                     $"\n\n结果:{storageJsonObj.Res}";

                                lotteryNotice += $"\n\n---------🎉 中奖玩家 🎉---------</b>";
                                #endregion
                                var results = from p in platforms
                                              from g in db.Games
                                              where p.CreatorId == g.CreatorId && g.GameStatus == GameStatus.Open && g.GameType == GameType.DragonTiger
                                              select new { platform = p, game = g };

                                Log.WriteLine($"对{results.Count()}个龙虎的台子进行开奖通知");
                                foreach (var result in results)
                                {
                                    var platform = result.platform;
                                    var botClient = _botClientList.First(u => u.BotId == platform.BotId);
                                    var game = result.game;
                                    if (platform.GroupId == null)
                                        continue;
                                    GameHistory? gameHistory = null;
                                    try
                                    {
                                        //获取本期记录(可能新开奖的)
                                        gameHistory = await db.GameHistorys.FirstOrDefaultAsync(u => u.GroupId == Convert.ToInt64(platform.GroupId) && u.GameId == game.Id && u.LotteryDrawId == currentNum);
                                    }
                                    catch (Exception ex)
                                    {
                                        Log.Error("获取龙虎本期开奖记录时出错:" + ex.Message);
                                    }
                                    //开奖信息
                                    Message? drawMsg = null;
                                    //玩家下注本游戏记录
                                    List<PlayerFinanceHistory>? bettingHistorys = gameHistory != null && gameHistory.MessageId > 0 && string.IsNullOrEmpty(gameHistory.LotteryDrawJson)
                                    ? db.PlayerFinanceHistorys.Where(u => u.FinanceStatus == FinanceStatus.Success && u.Type == FinanceType.DragonTiger && u.GameId == game.Id && u.GameMessageId == gameHistory.MessageId).ToList()
                                    : null;
                                    //开奖
                                    if (gameHistory != null && string.IsNullOrEmpty(gameHistory.LotteryDrawJson) && bettingHistorys != null && bettingHistorys.Count != 0)
                                    {
                                        foreach (var bettingHistory in bettingHistorys)
                                        {
                                            //下注字符串
                                            var bet = bettingHistory.Remark;
                                            if (string.IsNullOrEmpty(bet))
                                                continue;

                                            //赔偿倍数
                                            decimal multiple = 0;

                                            //投
                                            if (storageJsonObj.Res == '龙' && Regex.IsMatch(bet, Program._betItems["d"])
                                                || storageJsonObj.Res == '虎' && Regex.IsMatch(bet, Program._betItems["tr"]))
                                            {
                                                multiple = Convert.ToDecimal(1.93);
                                            }
                                            else if (storageJsonObj.Res == '和' && Regex.IsMatch(bet, Program._betItems["ti"]))
                                            {
                                                multiple = Convert.ToDecimal(12.8);
                                            }

                                            //中奖了
                                            if (multiple > 0)
                                            {
                                                await Helper.PlayerWinningFromPlatform(db, platform, game, gameHistory, bettingHistory, "龙虎", multiple);

                                                //下注金额
                                                var betAmount = bettingHistory.Amount + bettingHistory.BonusAmount;
                                                //赔偿金额
                                                var bonus = Math.Round(betAmount * multiple, 2, MidpointRounding.AwayFromZero);
                                                lotteryNotice += $"\n\n💵 {bettingHistory.Name} <b>{betAmount}U买{bet} 奖 " + bonus + "U</b>";
                                            }
                                        }
                                    }

                                    int milliseconds = Convert.ToInt32((openUTCTime.AddSeconds(45) - DateTime.UtcNow).TotalMilliseconds);
                                    await LotteryDraw(db, gameHistory, JsonConvert.SerializeObject(storageJsonObj), lotteryNotice, platform, drawMsg, currentNum, nextNum, openUTCTime, botClient, game, fileName, milliseconds);
                                }

                            }

                            //哈希牛牛
                            //https://haxi.xinbi.com/
                            //每分钟波场TRX交易获得64位字符串哈希结果
                            //提取哈希最后5位作为牌面,如：（000 * **68cba）为牌面.
                            //前三位（68c)为庄家号码（平台是庄家）
                            //后三位(cba)为闲家号码（玩家是闲家）
                            //三位数各自相加,字母为10,相加后取个位数。
                            //数字1 - 9依次为牛一至牛九，为0则为牛牛。
                            //按牛牛 > 牛九 > 牛八.... > 牛一以此类推.根据结果决定，牛牛为十倍，牛九为九倍，牛八为八倍，牛七为七倍，牛六为六倍，牛五为五倍，牛四为四倍，牛三为三倍，牛二为二倍，牛一为一倍

                            //如庄闲同点,牛一牛二则庄赢,牛三点以上同点和局，扣转账金额7% 手续费退还本金
                            if (isRun && dbcache.Games.Where(u => u.GameStatus == GameStatus.Open).Any(g => g.GameType == GameType.Cow))
                            {
                                //返回开奖结果公示
                                var lotteryNotice = string.Empty;
                                #region 定义放到数据库里的JSON
                                var storageJsonObj = new CowData
                                {
                                    Phase = currentNum,
                                    Number = txId,
                                    Time = DateTime.UtcNow
                                };
                                string lastSixStr = txId.Substring(txId.Length - 6);
                                var bankerStr = lastSixStr.Substring(0, 3);
                                //庄家个位数
                                var bankerDigits = bankerStr.ToCharArray().Select(u =>
                                {
                                    if (int.TryParse(u.ToString(), out var ui))
                                    {
                                        return Convert.ToInt32(ui);
                                    }
                                    else
                                    {
                                        return 0;
                                    }
                                }).Sum() % 10;

                                var playerStr = lastSixStr.Substring(2, 3);
                                //玩家个位数
                                var playerDigits = playerStr.ToCharArray().Select(u =>
                                {
                                    if (int.TryParse(u.ToString(), out var ui))
                                    {
                                        return Convert.ToInt32(ui);
                                    }
                                    else
                                    {
                                        return 0;
                                    }
                                }).Sum() % 10;
                                #endregion

                                #region 定义返回开奖公示
                                lotteryNotice = $"<b>---------- {currentNum}期----------</b>" +
                                     $"\n{txId.Substring(0, txId.Length - 6)}<b>{lastSixStr}</b>" +
                                     $"\n\n截取 : <b>{lastSixStr}</b>\n\n庄家 : <b>{bankerStr}</b> (相加后个位数 <b>{bankerDigits}</b>)\n玩家 : <b>{playerStr}</b> (相加后个位数 <b>{playerDigits}</b>)";

                                //庄赢
                                if (bankerDigits == playerDigits || bankerDigits <= 2 || bankerDigits > playerDigits)
                                {
                                    lotteryNotice += "\n\n赢家 : <b>庄家</b>";
                                    if (bankerDigits == 0)
                                    {
                                        lotteryNotice += " (牛牛<b>10</b>倍)";
                                    }
                                    else
                                    {
                                        lotteryNotice += $" (牛<b>{bankerDigits} - {bankerDigits}</b>倍)";
                                    }
                                }
                                else
                                {
                                    lotteryNotice += $"\n\n赢家 : <b>玩家<b> (牛<b>{playerDigits} - {playerDigits}</b>倍)";
                                }

                                lotteryNotice += $"\n\n<b>---------🎉 中奖玩家 🎉---------</b>";
                                #endregion
                                var results = from p in platforms
                                              from g in db.Games
                                              where p.CreatorId == g.CreatorId && g.GameStatus == GameStatus.Open && g.GameType == GameType.Cow
                                              select new { platform = p, game = g };

                                Log.WriteLine($"对{results.Count()}个哈希牛牛的台子进行开奖通知");
                                foreach (var result in results)
                                {
                                    var platform = result.platform;
                                    var botClient = _botClientList.First(u => u.BotId == platform.BotId);
                                    var game = result.game;
                                    if (platform.GroupId == null)
                                        continue;
                                    GameHistory? gameHistory = null;
                                    try
                                    {
                                        //获取本期记录(可能新开奖的)
                                        gameHistory = await db.GameHistorys.FirstOrDefaultAsync(u => u.GroupId == Convert.ToInt64(platform.GroupId) && u.GameId == game.Id && u.LotteryDrawId == currentNum);
                                    }
                                    catch (Exception ex)
                                    {
                                        Log.Error("获取哈希牛牛本期开奖记录时出错:" + ex.Message);
                                    }
                                    //开奖信息
                                    Message? drawMsg = null;
                                    //玩家下注本游戏记录
                                    List<PlayerFinanceHistory>? bettingHistorys = gameHistory != null && gameHistory.MessageId > 0 && string.IsNullOrEmpty(gameHistory.LotteryDrawJson)
                                    ? db.PlayerFinanceHistorys.Where(u => u.FinanceStatus == FinanceStatus.Success && u.Type == FinanceType.Cow && u.GameId == game.Id && u.GameMessageId == gameHistory.MessageId).ToList()
                                    : null;
#warning 这里玩家赢了不但要返回9/10的投注钱,还得奖励*倍给他.  玩家输了就按照牛牛倍数返回给他其他未赔偿倍数的钱
                                    //开奖
                                    if (gameHistory != null && string.IsNullOrEmpty(gameHistory.LotteryDrawJson) && bettingHistorys != null && bettingHistorys.Count != 0)
                                    {
                                        foreach (var bettingHistory in bettingHistorys)
                                        {
                                            //下注字符串
                                            var bet = bettingHistory.Remark;
                                            if (string.IsNullOrEmpty(bet))
                                                continue;

                                            //赔偿倍数
                                            decimal multiple = 0;

                                            //庄赢
                                            if (bankerDigits == playerDigits || bankerDigits <= 2 || bankerDigits > playerDigits)
                                            {
                                                if (bankerDigits == 0)
                                                {
                                                    multiple = Convert.ToDecimal(10);
                                                }
                                                else
                                                {
                                                    multiple = Convert.ToDecimal(bankerDigits);
                                                }
                                            }
                                            else
                                            {
                                                multiple = Convert.ToDecimal(bankerDigits);
                                            }

                                            //中奖了
                                            if (multiple > 0)
                                            {
                                                await Helper.PlayerWinningFromPlatform(db, platform, game, gameHistory, bettingHistory, "哈希牛牛", multiple);

                                                //下注金额
                                                var betAmount = bettingHistory.Amount + bettingHistory.BonusAmount;
                                                //赔偿金额
                                                var bonus = Math.Round(betAmount * multiple, 2, MidpointRounding.AwayFromZero);
                                                lotteryNotice += $"\n\n💵 {bettingHistory.Name} <b>{betAmount}U买{bet} 奖 " + bonus + "U</b>";
                                            }
                                        }
                                    }

                                    int milliseconds = Convert.ToInt32((openUTCTime.AddSeconds(45) - DateTime.UtcNow).TotalMilliseconds);
                                    await LotteryDraw(db, gameHistory, JsonConvert.SerializeObject(storageJsonObj), lotteryNotice, platform, drawMsg, currentNum, nextNum, openUTCTime, botClient, game, "哈希牛牛", milliseconds);
                                }

                            }

                            //21点
                            //0为10,a算还是A,bcd分别是JQK,去掉ef,和局算庄赢, 最多18人参与
                            //发牌规则:从64字符串左边至右边开始分发牌面,每次补牌都是假设已有两张牌 + 10点不超过21点才补牌
                            //1、庄家先发2张(4和2),这个要补牌,因为4 + 2 + 10不超过21点.所得牌为4 + 2 + 1
                            //2、后面玩家根据下注顺序,和庄家一样发牌.假设有5个玩家.所得牌面分别是:
                            //3、哈希3bf生成牌面为3 + 10 + 10
                            //4、哈希195生成牌面为1 + 9 + 5
                            //5、哈希eb0生成牌面为10 + 10 + 10                         
                            if (isRun && dbcache.Games.Where(u => u.GameStatus == GameStatus.Open).Any(g => g.GameType == GameType.Blackjack))
                            {
                                //返回开奖结果公示
                                var lotteryNotice = string.Empty;
                                //返回游戏平台
                                var results = from p in platforms
                                              from g in db.Games
                                              where p.CreatorId == g.CreatorId && g.GameStatus == GameStatus.Open && g.GameType == GameType.Blackjack
                                              select new { platform = p, game = g };

                                #region 定义放到数据库里的JSON
                                var storageJsonObj = new BlackjackData
                                {
                                    Phase = currentNum,
                                    Number = txId,
                                    Time = DateTime.UtcNow
                                };

                                //生成牌
                                List<CardKeyValue> cards = txId
                                .Replace("e", "")
                                .Replace("f", "")
                                .ToArray()
                                .Select(u =>
                                {
                                    var card = u.ToString();
                                    if (card is "a")
                                    {
                                        return new CardKeyValue { Num = 1, Card = "A" };
                                    }
                                    else if (card is "0")
                                    {
                                        return new CardKeyValue { Num = 10, Card = "10" };
                                    }
                                    else if (card is "b")
                                    {
                                        return new CardKeyValue { Num = 10, Card = "J" };
                                    }
                                    else if (card is "c")
                                    {
                                        return new CardKeyValue { Num = 10, Card = "Q" };
                                    }
                                    else if (card is "d")
                                    {
                                        return new CardKeyValue { Num = 10, Card = "K" };
                                    }
                                    else
                                    {
                                        return new CardKeyValue { Num = Convert.ToInt32(card), Card = card };
                                    }
                                }).ToList();

                                //发牌用户
                                List<PlayerCard> allCards = [];
                                var currentI = 0;
                                //获取可以发多少个人
                                var cardCount = (int)Math.Floor(Convert.ToDouble(cards.Count / 3));
                                for (int i = 0; i < cardCount; i++)
                                {
                                    List<CardKeyValue> cardItems = [];
                                    for (int a = 0; a < 3; a++)
                                    {
                                        if (cards[currentI] != null && cardItems.Select(u => u.Num).Sum() < 12)
                                        {
                                            cardItems.Add(cards[currentI]);
                                            currentI++;
                                        }
                                    }

                                    if (cardItems.Count >= 2)
                                        allCards.Add(new PlayerCard { Cards = cardItems });
                                }
                                #endregion

                                //庄家牌
                                var bankerCard = allCards[0];
                                var bankerCards = bankerCard.Cards.Select(u => u.Card);
                                var bankerSum = bankerCard.Cards.Select(u => u.Num).Sum();

                                Log.WriteLine($"对{results.Count()}个21点的台子进行开奖通知");
                                foreach (var result in results)
                                {
                                    var platform = result.platform;
                                    var botClient = _botClientList.First(u => u.BotId == platform.BotId);
                                    var game = result.game;
                                    if (platform.GroupId == null)
                                        continue;
                                    GameHistory? gameHistory = null;
                                    try
                                    {
                                        //获取本期记录(可能新开奖的)
                                        gameHistory = await db.GameHistorys.FirstOrDefaultAsync(u => u.GroupId == Convert.ToInt64(platform.GroupId) && u.GameId == game.Id && u.LotteryDrawId == currentNum);
                                    }
                                    catch (Exception ex)
                                    {
                                        Log.Error("获取21点本期开奖记录时出错:" + ex.Message);
                                    }
                                    //开奖信息
                                    Message? drawMsg = null;
                                    //玩家下注本游戏记录
                                    List<PlayerFinanceHistory>? bettingHistorys = gameHistory != null && gameHistory.MessageId > 0 && string.IsNullOrEmpty(gameHistory.LotteryDrawJson)
                                    ? db.PlayerFinanceHistorys.Where(u => u.FinanceStatus == FinanceStatus.Success && u.Type == FinanceType.Blackjack && u.GameId == game.Id && u.GameMessageId == gameHistory.MessageId).ToList()
                                    : null;

                                    lotteryNotice = $"<b>---------- {currentNum}期----------</b>" +
                                           $"\n<b>{txId}</b>" +
                                           $"\n生成牌规则:去掉e和f. 0=10;a=A,b=J;c=Q;d=K.\n补牌规则:假设已有2张牌 + 10点不超过21点才补牌" +
                                           $"\n牌面:\n庄家 :  <b>{bankerSum}点</b> ({string.Join(',', bankerCards)})" +
                                           $"\n\n<b>---------♠️ 玩家牌面 ♠️---------</b>";

                                    if (bettingHistorys?.Count == 0)
                                        lotteryNotice += "\n\n         😅 本期无玩家下注";

                                    //开奖
                                    if (gameHistory != null && string.IsNullOrEmpty(gameHistory.LotteryDrawJson) && bettingHistorys != null && bettingHistorys.Count != 0)
                                    {
                                        //庄家是0,所以要从1开始
                                        var currentCardI = 1;
                                        foreach (var bettingHistory in bettingHistorys)
                                        {
                                            //赔偿倍数
                                            decimal multiple = 0;
                                            //下注字符串
                                            var bet = bettingHistory.Remark;
                                            if (string.IsNullOrEmpty(bet))
                                                continue;

                                            var playerCard = allCards[currentCardI];
                                            playerCard.PlayerId = bettingHistory.PlayerId;

                                            var playerCards = playerCard.Cards.Select(u => u.Card);
                                            var nums = playerCard.Cards.Select(u => u.Num).ToList();
                                            var playerSum = nums.Sum();
                                            lotteryNotice += $"\n\n🙎‍♂️ {bettingHistory.Name} : <b>{playerSum}点</b> ({string.Join(',', playerCards)})";

                                            //买比庄家小、买比庄家大
                                            if (Regex.IsMatch(bet, "^" + _betItems["s"]) && playerSum < bankerSum || Regex.IsMatch(bet, "^" + _betItems["l"]) && playerSum > bankerSum)
                                            {
                                                multiple = Convert.ToDecimal(1.95);
                                            }
                                            //买庄家和闲家一样大
                                            else if (Regex.IsMatch(bet, "^" + _betItems["ti"]) && playerSum == bankerSum)
                                            {
                                                multiple = Convert.ToDecimal(8.00);
                                            }

                                            //对子(排除豹子)
                                            if (playerCards.Count() == 2 && playerCards.Distinct().Count() == 1 || playerCards.Count() == 3 && playerCards.Distinct().Count() == 2)
                                            {
                                                lotteryNotice += "、<b>对子</b>";
                                                if (Regex.IsMatch(bet, "^" + _betItems["p"]))
                                                {
                                                    multiple = Convert.ToDecimal(2.38);
                                                }
                                            }
                                            //豹子
                                            else if (playerCards.Count() == 3 && playerCards.Distinct().Count() == 1 && Regex.IsMatch(bet, "^" + _betItems["t"]))
                                            {
                                                lotteryNotice += "、<b>豹子</b>";
                                                multiple = Convert.ToDecimal(21.88);
                                            }
                                            //顺子
                                            else if (playerCards.Count() == 3)
                                            {
                                                if (Helper.AreConsecutive(nums[0], nums[1], nums[2]) && Regex.IsMatch(bet, "^" + _betItems["st"]))
                                                {
                                                    lotteryNotice += "、<b>顺子</b>";
                                                    multiple = Convert.ToDecimal(26.8);
                                                }
                                            }

                                            currentCardI++;
                                            //中奖了
                                            if (multiple > 0)
                                            {
                                                await Helper.PlayerWinningFromPlatform(db, platform, game, gameHistory, bettingHistory, "21点", multiple);
                                                //下注金额
                                                var betAmount = bettingHistory.Amount + bettingHistory.BonusAmount;
                                                //赔偿金额
                                                var bonus = Math.Round(betAmount * multiple, 2, MidpointRounding.AwayFromZero);
                                                lotteryNotice += $"\n🗳<b>{betAmount}U买{bet} 奖 " + bonus + "U</b>";
                                            }
                                        }
                                    }
                                    int milliseconds = Convert.ToInt32((openUTCTime.AddSeconds(45) - DateTime.UtcNow).TotalMilliseconds);
                                    await LotteryDraw(db, gameHistory, JsonConvert.SerializeObject(storageJsonObj), lotteryNotice, platform, drawMsg, currentNum, nextNum, openUTCTime, botClient, game, "21点", milliseconds);
                                }
                                storageJsonObj.PlayerCards = allCards;
                            }

                            //三公
                            //每分钟波场TRX交易获得64位字符串哈希生成牌面,0为10,a算还是A,bcd分别是JQK,去掉ef,和局算庄赢, 最多18人参与
                            //发牌规则:从64字符串左边至右边开始分发牌面,分别一人三张
                            //牌型10、J、Q、K的点数都算作0，也称为公仔牌

                            //爆玖：由三个3构成的牌型 比如333
                            //炸弹：由点数相同的三张牌组成的牌型 比如QQQ、222
                            //三公：由任意三张不相同的公仔牌构成的牌型 比如KKQ、KQJ
                            //点数牌：任意三张牌点数相加取个数位 比如KQ9 = 9点、553 = 3点、235 = 0点

                            //点数大小：K > Q > J > 10 > 9 > 8 > 7 > 6 > 5 > 4 > 3 > 2 > A
                            //牌型大小：爆玖 > 炸弹 > 三公 > 9点 > 8点 > 7点 > 6点 > 5点 > 4点 > 3点 > 2点 > 1点 > 0点
                            //炸弹大小：KKK > QQQ > JJJ > 101010 > 999 > 888 > 777 > 666 > 555 > 444 > 333 > 222 > AAA
                            //基本牌大小：如果多家牌型都是三公，则先比较玩家最大的那张公牌大小(如KQJ > QQJ)。如果还是相同，则比较玩家最大公牌的花色(如：黑桃K方块Q红桃J > 红桃K方块Q红桃J)。
                            //点数牌大小：如果多个玩家都是点数相同点数牌，那麽先比较玩家的公仔牌数量，谁的数多谁大(如JQ9 > J 10 9)。

                            //如果数量还是一致则比较最大的那张单牌的大小(如KQ9 > JQ9)。
                            //如果大小还是相同则比较最大牌的花色。三公 > 双公九 > 单公九 > 九点 > 双公八 > 单公八 > 八点 > 双公七 > 单公七 > 七点 > 双公六 > 单公六 > 六点 > 双公五 > 单公五 > 五点 > 双公四 > 单公四 > 四点 > 双公三 > 单公三 > 三点 > 双公二 > 单公二 > 二点 > 双公一 > 单公一 > 一点 > 双公零 > 单公零 > 零点
                            if (dbcache.Games.Where(u => u.GameStatus == GameStatus.Open).Any(g => g.GameType == GameType.Sangong))
                            {
                                //返回开奖结果公示
                                var lotteryNotice = string.Empty;
                                var results = from p in platforms
                                              from g in db.Games
                                              where p.CreatorId == g.CreatorId && g.GameStatus == GameStatus.Open && g.GameType == GameType.Sangong
                                              select new { platform = p, game = g };

                                #region 定义放到数据库里的JSON
                                var storageJsonObj = new SangongData
                                {
                                    Phase = currentNum,
                                    Number = txId,
                                    Time = DateTime.UtcNow
                                };

                                //生成牌
                                List<CardKeyValue> cards = txId
                                .Replace("e", "")
                                .Replace("f", "")
                                .ToArray()
                                .Select(u =>
                                {
                                    var card = u.ToString();
                                    if (card is "a")
                                    {
                                        return new CardKeyValue { Num = 1, Card = "A" };
                                    }
                                    else if (card is "0")
                                    {
                                        return new CardKeyValue { Num = 0, Card = "10" };
                                    }
                                    else if (card is "b")
                                    {
                                        return new CardKeyValue { Num = 0, Card = "J" };
                                    }
                                    else if (card is "c")
                                    {
                                        return new CardKeyValue { Num = 0, Card = "Q" };
                                    }
                                    else if (card is "d")
                                    {
                                        return new CardKeyValue { Num = 0, Card = "K" };
                                    }
                                    else
                                    {
                                        return new CardKeyValue { Num = Convert.ToInt32(card), Card = card };
                                    }
                                }).ToList();

                                //发牌给用户
                                List<PlayerCard> allCards = [];
                                var currentI = 0;
                                //获取可以发多少个人
                                var cardCount = (int)Math.Floor(Convert.ToDouble(cards.Count / 3));
                                for (int i = 0; i < cardCount; i++)
                                {
                                    List<CardKeyValue> cardItems = [];
                                    for (int a = 0; a < 3; a++)
                                    {
                                        cardItems.Add(cards[currentI]);
                                        currentI++;
                                    }
                                    allCards.Add(new PlayerCard { Cards = cardItems });
                                }
                                #endregion

                                //庄家牌
                                var bankerCard = allCards[0];
                                var bankerCards = bankerCard.Cards.Select(u => u.Card);
                                var bankerSum = bankerCard.Cards.Select(u => u.Num).Sum() % 10;
                                var bankerCardStr = string.Join(',', bankerCards);
                                Log.WriteLine($"对{results.Count()}个三公的台子进行开奖通知");
                                foreach (var result in results)
                                {
                                    var platform = result.platform;
                                    var botClient = _botClientList.First(u => u.BotId == platform.BotId);
                                    var game = result.game;
                                    if (platform.GroupId == null)
                                        continue;
                                    GameHistory? gameHistory = null;
                                    try
                                    {
                                        //获取本期记录(可能新开奖的)
                                        gameHistory = await db.GameHistorys.FirstOrDefaultAsync(u => u.GroupId == Convert.ToInt64(platform.GroupId) && u.GameId == game.Id && u.LotteryDrawId == currentNum);
                                    }
                                    catch (Exception ex)
                                    {
                                        Log.Error("获取三公本期开奖记录时出错:" + ex.Message);
                                    }
                                    //开奖信息
                                    Message? drawMsg = null;
                                    //玩家下注本游戏记录
                                    List<PlayerFinanceHistory>? bettingHistorys = gameHistory != null && gameHistory.MessageId > 0 && string.IsNullOrEmpty(gameHistory.LotteryDrawJson)
                                    ? db.PlayerFinanceHistorys.Where(u => u.FinanceStatus == FinanceStatus.Success && u.Type == FinanceType.Sangong && u.GameId == game.Id && u.GameMessageId == gameHistory.MessageId).ToList()
                                    : null;

                                    lotteryNotice = $"<b>---------- {currentNum}期----------</b>" +
                                        $"\n<b>{txId}</b>" +
                                        $"\n生成牌规则:去掉e和f. 0=10;a=A,b=J;c=Q;d=K.(10、J、Q、K都算0点)\n" +
                                        $"\n牌面:\n庄家 :  <b>{bankerSum}点</b> ({bankerCardStr})";

                                    if (bankerCardStr == "3,3,3")
                                    {
                                        lotteryNotice += "、<b>爆九</b>";
                                    }
                                    else if (bankerCards.Distinct().Count() == 1)
                                    {
                                        lotteryNotice += "、<b>炸弹</b>";
                                    }
                                    else if (bankerSum == 0 && bankerCards.Count(u => u == "J" || u == "Q" || u == "K") == 3)
                                    {
                                        lotteryNotice += "、<b>三公</b>";
                                    }

                                    lotteryNotice += "\n\n<b>---------♠️ 玩家牌面 ♠️---------</b>";
                                    if (bettingHistorys?.Count == 0)
                                        lotteryNotice += "\n\n         😅 本期无玩家下注";

                                    //开奖
                                    if (gameHistory != null && string.IsNullOrEmpty(gameHistory.LotteryDrawJson) && bettingHistorys != null && bettingHistorys.Count != 0)
                                    {
                                        //庄家是0,所以要从1开始
                                        var currentCardI = 1;
                                        foreach (var bettingHistory in bettingHistorys)
                                        {
                                            //下注字符串
                                            var bet = bettingHistory.Remark;
                                            if (string.IsNullOrEmpty(bet))
                                                continue;
                                            //赔偿倍数
                                            decimal multiple = 0;

                                            var playerCard = allCards[currentCardI];
                                            playerCard.PlayerId = bettingHistory.PlayerId;

                                            var playerCards = playerCard.Cards.Select(u => u.Card);
                                            var nums = playerCard.Cards.Select(u => u.Num).ToList();
                                            var playerSum = nums.Sum() % 10;
                                            var cardStr = string.Join(',', playerCards);
                                            lotteryNotice += $"\n\n🙎‍♂️ {bettingHistory.Name} : <b>{playerSum}点</b> ({cardStr})";

                                            //每分钟波场TRX交易获得64位字符串哈希生成牌面,0为10,a算还是A,bcd分别是JQK,去掉ef,和局算庄赢, 最多18人参与
                                            //发牌规则:从64字符串左边至右边开始分发牌面,分别一人三张
                                            //牌型10、J、Q、K的点数都算作0，也称为公仔牌
                                            //爆九 三个3
                                            if (cardStr == "3,3,3")
                                            {
                                                lotteryNotice += $"、<b>爆九</b>";
                                                if (Regex.IsMatch(bet, "^" + Program._betItems["tt"]))
                                                {
                                                    lotteryNotice += $" 🎉";
                                                    multiple = Convert.ToDecimal(188.88);
                                                }
                                            }
                                            //炸弹 点数相同的三张牌组成(排除333)
                                            else if (playerCards.Distinct().Count() == 1)
                                            {
                                                lotteryNotice += $"、<b>炸弹</b>";
                                                if (Regex.IsMatch(bet, "^" + Program._betItems["bomb"]))
                                                {
                                                    lotteryNotice += $" 🎉";
                                                    multiple = Convert.ToDecimal(128.88);
                                                }
                                            }
                                            //大三公 三张相同的公仔牌,如jjj,QQQ,KKK
                                            else if (cardStr is "J,J,J" or "Q,Q,Q" or "K,K,K")
                                            {
                                                lotteryNotice += $"、<b>大三公</b>";
                                                if (Regex.IsMatch(bet, "^" + Program._betItems["lss"]))
                                                {
                                                    lotteryNotice += $" 🎉";
                                                    multiple = Convert.ToDecimal(288.88);
                                                }
                                            }
                                            //小三公 三张相同的点数牌
                                            else if (cardStr is "A,A,A" or "2,2,2" or "3,3,3" or "4,4,4" or "5,5,5" or "6,6,6" or "7,7,7" or "8,8,8" or "9,9,9")
                                            {
                                                lotteryNotice += $"、<b>小三公</b>";
                                                if (Regex.IsMatch(bet, "^" + Program._betItems["sss"]))
                                                {
                                                    lotteryNotice += $" 🎉";
                                                    multiple = Convert.ToDecimal(168.88);
                                                }
                                            }
                                            //混三公 三张公仔牌(可以2张一样的公仔牌)
                                            else if (playerCards.Count(u => u == "J" || u == "Q" || u == "K") == 3)
                                            {
                                                lotteryNotice += $"、<b>混三公</b>";
                                                if (Regex.IsMatch(bet, "^" + Program._betItems["mss"]))
                                                {
                                                    lotteryNotice += $" 🎉";
                                                    multiple = Convert.ToDecimal(58.88);
                                                }
                                            }
                                            //三公 任意三张公仔牌构成
                                            else if (playerCards.Count(u => u == "J" || u == "Q" || u == "K") == 3)
                                            {
                                                lotteryNotice += $"、<b>三公</b>";
                                                if (Regex.IsMatch(bet, "^" + Program._betItems["ss"]))
                                                {
                                                    lotteryNotice += $" 🎉";
                                                    multiple = Convert.ToDecimal(58.88);
                                                }
                                            }
                                            //点数
                                            else if (playerSum.ToString() == bet)
                                            {
                                                lotteryNotice += $" 🎉";
                                                multiple = Convert.ToDecimal(8.8);
                                            }
                                            //和
                                            else if (bankerSum == playerSum)
                                            {
                                                lotteryNotice += $"、<b>和</b>";
                                                if (Regex.IsMatch(bet, "^" + Program._betItems["ti"]))
                                                {
                                                    lotteryNotice += $" 🎉";
                                                    multiple = Convert.ToDecimal(8.5);
                                                }
                                            }
                                            //对子 两张牌能组成对子(排除3个相同的)
                                            else if (playerCards.Distinct().Count() == 2)
                                            {
                                                lotteryNotice += $"、<b>对子</b>";
                                                if (Regex.IsMatch(bet, "^" + Program._betItems["p"]))
                                                {
                                                    lotteryNotice += $" 🎉";
                                                    multiple = Convert.ToDecimal(3.88);
                                                }
                                            }
                                            //投自己大、小
                                            else if (Regex.IsMatch(bet, "^" + Program._betItems["l"]) && bankerSum < playerSum
                                                || Regex.IsMatch(bet, "^" + Program._betItems["s"]) && bankerSum > playerSum)
                                            {
                                                if (Regex.IsMatch(bet, "^" + Program._betItems["l"]) && bankerSum < playerSum)
                                                {
                                                    lotteryNotice += $"、<b>比庄大</b> 🎉";
                                                }
                                                else if (Regex.IsMatch(bet, "^" + Program._betItems["s"]) && bankerSum > playerSum)
                                                {
                                                    lotteryNotice += $"、<b>比庄小</b> 🎉";
                                                }
                                                multiple = Convert.ToDecimal(1.93);
                                            }

                                            currentCardI++;
                                            //中奖了
                                            if (multiple > 0)
                                            {
                                                await Helper.PlayerWinningFromPlatform(db, platform, game, gameHistory, bettingHistory, "三公", multiple);

                                                //下注金额
                                                var betAmount = bettingHistory.Amount + bettingHistory.BonusAmount;
                                                //赔偿金额
                                                var bonus = Math.Round(betAmount * multiple, 2, MidpointRounding.AwayFromZero);
                                                lotteryNotice += $"\n🗳<b>{betAmount}U买{bet} 奖 " + bonus + "U</b>";
                                            }
                                        }
                                    }

                                    int milliseconds = Convert.ToInt32((openUTCTime.AddSeconds(45) - DateTime.UtcNow).TotalMilliseconds);
                                    await LotteryDraw(db, gameHistory, JsonConvert.SerializeObject(storageJsonObj), lotteryNotice, platform, drawMsg, currentNum, nextNum, openUTCTime, botClient, game, "三公", milliseconds);
                                }
                                storageJsonObj.PlayerCards = allCards;
                            }

                            //百家乐
                            // 百家乐是扑克竞猜游戏
                            // 玩家可投注庄和闲其中一方
                            // 最低0点(10、J、Q、K的牌为0点),A为1点,2-9按照牌面点数计,最高为9点,两张牌相加(超过10以个位尾数计)最高为赢
                            if (isRun && dbcache.Games.Where(u => u.GameStatus == GameStatus.Open).Any(g => g.GameType == GameType.Baccarat))
                            {
                                //返回开奖结果公示
                                var lotteryNotice = string.Empty;
                                #region 定义放到数据库里的JSON
                                var storageJsonObj = new BaccaratData
                                {
                                    Phase = currentNum,
                                    Number = txId,
                                    Time = DateTime.UtcNow
                                };

                                //生成庄和闲的4张牌牌
                                List<CardKeyValue> cards = txId
                                .Replace("e", "")
                                .Replace("f", "")
                                .ToArray()
                                .Select(u =>
                                {
                                    var card = u.ToString();
                                    if (card is "a")
                                    {
                                        return new CardKeyValue { Num = 1, Card = "A" };
                                    }
                                    else if (card is "0")
                                    {
                                        return new CardKeyValue { Num = 0, Card = "10" };
                                    }
                                    else if (card is "b")
                                    {
                                        return new CardKeyValue { Num = 0, Card = "J" };
                                    }
                                    else if (card is "c")
                                    {
                                        return new CardKeyValue { Num = 0, Card = "Q" };
                                    }
                                    else if (card is "d")
                                    {
                                        return new CardKeyValue { Num = 0, Card = "K" };
                                    }
                                    else
                                    {
                                        return new CardKeyValue { Num = Convert.ToInt32(card), Card = card };
                                    }
                                }).ToList().Take(4).ToList();

                                //发牌给用户
                                List<PlayerCard> allCards = [];
                                var currentI = 0;
                                for (int i = 0; i < 2; i++)
                                {
                                    List<CardKeyValue> cardItems = [];
                                    for (int a = 0; a < 2; a++)
                                    {
                                        cardItems.Add(cards[currentI]);
                                        currentI++;
                                    }

                                    allCards.Add(new PlayerCard { Cards = cardItems });
                                }

                                storageJsonObj.PlayerCards = allCards;
                                #endregion

                                #region 定义返回开奖公示
                                lotteryNotice = $"<b>---------- {currentNum}期----------</b>" +
                                     $"\n<b>{txId}</b>" +
                                     $"\n生成牌规则:去掉e和f. 0=10;a=A,b=J;c=Q;d=K. (10、J、Q、K的牌为0点)";
                                var banker = allCards[0].Cards;
                                var player = allCards[1].Cards;
                                var bankerSum = banker.Select(u => u.Num).Sum();
                                var isBankerPair = banker.Select(u => u.Card).Distinct().Count() == 1;
                                var playerSum = player.Select(u => u.Num).Sum();
                                var isPlayerPair = player.Select(u => u.Card).Distinct().Count() == 1;

                                //'庄单': 1:1.94(庄家牌面点数相加为单)                                    
                                //'庄双': 1:1.94(庄家牌面点数相加为双)
                                lotteryNotice += $"\n庄家 : <b>{bankerSum}点</b> ({string.Join(',', banker.Select(u => u.Card))})、<b>" + (bankerSum % 2 == 0 ? '双' : '单') + "</b>";

                                //'庄对': 1:11(只要庄家2张牌是相同的)
                                if (isBankerPair)
                                    lotteryNotice += "、<b>庄对</b>";

                                //'闲单': 1:1.96(闲家牌面点数相加为单)
                                //'闲双': 1:1.9(闲家牌面点数相加为双)
                                lotteryNotice += $"\n闲家 : <b>{playerSum}点</b> ({string.Join(',', player.Select(u => u.Card))})、<b>" + (playerSum % 2 == 0 ? '双' : '单') + "</b>";

                                //'闲对': 1:11(只要闲家2张牌是相同的)
                                if (isPlayerPair)
                                    lotteryNotice += "、<b>闲对</b>";

                                //'庄赢': 1:1.95(庄家点数比闲家大, 抽佣5 %)
                                if (bankerSum > playerSum)
                                    lotteryNotice += "\n\n结果 : <b>庄赢</b>";
                                //'闲赢': 1:1(闲家点数比庄家大)
                                else if (bankerSum < playerSum)
                                    lotteryNotice += "\n\n结果 : <b>闲赢</b>";
                                else
                                    //'和局': 1:8(另: 仅下注闲家或庄家的退回筹码, 下注其他不退)
                                    lotteryNotice += "\n\n结果 : <b>和局</b>";

                                lotteryNotice += $"\n\n<b>---------🎉 中奖玩家 🎉---------</b>";
                                #endregion
                                var results = from p in platforms
                                              from g in db.Games
                                              where p.CreatorId == g.CreatorId && g.GameStatus == GameStatus.Open && g.GameType == GameType.Baccarat
                                              select new { platform = p, game = g };

                                Log.WriteLine($"对{results.Count()}个百家乐的台子进行开奖通知");
                                foreach (var result in results)
                                {
                                    var platform = result.platform;
                                    var botClient = _botClientList.First(u => u.BotId == platform.BotId);
                                    var game = result.game;
                                    if (platform.GroupId == null)
                                        continue;
                                    GameHistory? gameHistory = null;
                                    try
                                    {
                                        //获取本期记录(可能新开奖的)
                                        gameHistory = await db.GameHistorys.FirstOrDefaultAsync(u => u.GroupId == Convert.ToInt64(platform.GroupId) && u.GameId == game.Id && u.LotteryDrawId == currentNum);
                                    }
                                    catch (Exception ex)
                                    {
                                        Log.Error("获取百家乐本期开奖记录时出错:" + ex.Message);
                                    }
                                    //开奖信息
                                    Message? drawMsg = null;
                                    //玩家下注本游戏记录
                                    List<PlayerFinanceHistory>? bettingHistorys = gameHistory != null && gameHistory.MessageId > 0 && string.IsNullOrEmpty(gameHistory.LotteryDrawJson)
                                    ? db.PlayerFinanceHistorys.Where(u => u.FinanceStatus == FinanceStatus.Success && u.Type == FinanceType.Baccarat && u.GameId == game.Id && u.GameMessageId == gameHistory.MessageId).ToList()
                                    : null;
                                    //开奖
                                    if (gameHistory != null && string.IsNullOrEmpty(gameHistory.LotteryDrawJson) && bettingHistorys != null && bettingHistorys.Count != 0)
                                    {
                                        foreach (var bettingHistory in bettingHistorys)
                                        {
                                            //下注字符串
                                            var bet = bettingHistory.Remark;
                                            if (string.IsNullOrEmpty(bet))
                                                continue;

                                            //赔偿倍数
                                            decimal multiple = 0;

                                            //庄、闲
                                            if (bankerSum > playerSum && Regex.IsMatch(bet, @"^" + _betItems["br"])
                                                || bankerSum < playerSum && Regex.IsMatch(bet, @"^" + _betItems["pr"]))
                                            {
                                                multiple = Convert.ToDecimal(1.93);
                                            }
                                            //和
                                            else if (bankerSum == playerSum && Regex.IsMatch(bet, @"^" + _betItems["ti"]))
                                            {
                                                multiple = Convert.ToDecimal(8.5);
                                            }
                                            //庄单、庄双、闲单、闲双
                                            else if (bankerSum % 2 == 0 && Regex.IsMatch(bet, @"^" + _betItems["bae"])
                                                || bankerSum % 2 != 0 && Regex.IsMatch(bet, @"^" + _betItems["bao"])
                                                || playerSum % 2 == 0 && Regex.IsMatch(bet, @"^" + _betItems["pae"])
                                                || playerSum % 2 != 0 && Regex.IsMatch(bet, @"^" + _betItems["pao"]))
                                            {
                                                multiple = Convert.ToDecimal(3.68);
                                            }
                                            //庄对、闲对
                                            else if (isBankerPair && Regex.IsMatch(bet, @"^" + _betItems["bap"])
                                                || isPlayerPair && Regex.IsMatch(bet, @"^" + _betItems["pap"]))
                                            {
                                                multiple = Convert.ToDecimal(4.18);
                                            }

                                            //中奖了
                                            if (multiple > 0)
                                            {
                                                await Helper.PlayerWinningFromPlatform(db, platform, game, gameHistory, bettingHistory, "百家乐", multiple);

                                                //下注金额
                                                var betAmount = bettingHistory.Amount + bettingHistory.BonusAmount;
                                                //赔偿金额
                                                var bonus = Math.Round(betAmount * multiple, 2, MidpointRounding.AwayFromZero);
                                                lotteryNotice += $"\n\n💵 {bettingHistory.Name} <b>{betAmount}U买{bet} 奖 " + bonus + "U</b>";
                                            }
                                        }
                                    }

                                    int milliseconds = Convert.ToInt32((openUTCTime.AddSeconds(45) - DateTime.UtcNow).TotalMilliseconds);
                                    await LotteryDraw(db, gameHistory, JsonConvert.SerializeObject(storageJsonObj), lotteryNotice, platform, drawMsg, currentNum, nextNum, openUTCTime, botClient, game, "百家乐", milliseconds);
                                }

                            }

                            //轮盘赌 轮盘赌最终结果以每分钟的64位交易哈希首个字符作为定格结果,玩家对其进行有奖竞猜!
                            if (isRun && dbcache.Games.Where(u => u.GameStatus == GameStatus.Open).Any(g => g.GameType == GameType.Roulette))
                            {
                                //返回开奖结果公示
                                var lotteryNotice = string.Empty;
                                var res = txId[0].ToString();
                                #region 定义放到数据库里的JSON
                                var storageJsonObj = new RouletteData
                                {
                                    Phase = currentNum,
                                    Number = txId,
                                    Time = DateTime.UtcNow,
                                    Char = txId[0]
                                };
                                #endregion

                                #region 定义返回开奖公示
                                lotteryNotice = $"<b>---------- {currentNum}期----------" +
                                     $"\n{txId}" +
                                     $"\n\n结果:{res}";

                                if (res is "0" or "1" or "2")
                                {
                                    lotteryNotice += "、 极小";
                                    storageJsonObj.JiDaZhongXiao = "极小";
                                }
                                else if (res is "d" or "e" or "f")
                                {
                                    lotteryNotice += "、 极大";
                                    storageJsonObj.JiDaZhongXiao = "极大";
                                }
                                else if (res is "3" or "4" or "5" or "6")
                                {
                                    lotteryNotice += "、 小";
                                    storageJsonObj.JiDaZhongXiao = "小";
                                }
                                else if (res is "9" or "a" or "b" or "c")
                                {
                                    lotteryNotice += "、 大";
                                    storageJsonObj.JiDaZhongXiao = "大";
                                }
                                else if (res is "7" or "8")
                                {
                                    lotteryNotice += "、 中";
                                    storageJsonObj.JiDaZhongXiao = "中";
                                }

                                lotteryNotice += $"\n\n---------🎉 中奖玩家 🎉---------</b>";
                                #endregion
                                var results = from p in platforms
                                              from g in db.Games
                                              where p.CreatorId == g.CreatorId && g.GameStatus == GameStatus.Open && g.GameType == GameType.Roulette
                                              select new { platform = p, game = g };

                                Log.WriteLine($"对{results.Count()}个轮盘赌的台子进行开奖通知");
                                foreach (var result in results)
                                {
                                    var platform = result.platform;
                                    var botClient = _botClientList.First(u => u.BotId == platform.BotId);
                                    var game = result.game;
                                    if (platform.GroupId == null)
                                        continue;
                                    GameHistory? gameHistory = null;
                                    try
                                    {
                                        //获取本期记录(可能新开奖的)
                                        gameHistory = await db.GameHistorys.FirstOrDefaultAsync(u => u.GroupId == Convert.ToInt64(platform.GroupId) && u.GameId == game.Id && u.LotteryDrawId == currentNum);
                                    }
                                    catch (Exception ex)
                                    {
                                        Log.Error("获取轮盘赌本期开奖记录时出错:" + ex.Message);
                                    }
                                    //开奖信息
                                    Message? drawMsg = null;
                                    //玩家下注本游戏记录
                                    List<PlayerFinanceHistory>? bettingHistorys = gameHistory != null && gameHistory.MessageId > 0 && string.IsNullOrEmpty(gameHistory.LotteryDrawJson)
                                    ? db.PlayerFinanceHistorys.Where(u => u.FinanceStatus == FinanceStatus.Success && u.Type == FinanceType.Roulette && u.GameId == game.Id && u.GameMessageId == gameHistory.MessageId).ToList()
                                    : null;
                                    //开奖
                                    if (gameHistory != null && string.IsNullOrEmpty(gameHistory.LotteryDrawJson) && bettingHistorys != null && bettingHistorys.Count != 0)
                                    {
                                        foreach (var bettingHistory in bettingHistorys)
                                        {
                                            //下注字符串
                                            var bet = bettingHistory.Remark;
                                            if (string.IsNullOrEmpty(bet))
                                                continue;

                                            //赔偿倍数
                                            decimal multiple = 0;

                                            //极小极大
                                            if (storageJsonObj.JiDaZhongXiao == "极小" && Regex.IsMatch(bet, "^" + _betItems["xs"]) || storageJsonObj.JiDaZhongXiao == "极大" && Regex.IsMatch(bet, "^" + _betItems["xl"]))
                                            {
                                                multiple = Convert.ToDecimal(4.5);
                                            }
                                            //小大
                                            else if (storageJsonObj.JiDaZhongXiao == "小" && Regex.IsMatch(bet, "^" + _betItems["s"]) || storageJsonObj.JiDaZhongXiao == "大" && Regex.IsMatch(bet, "^" + _betItems["l"]))
                                            {
                                                multiple = Convert.ToDecimal(3.38);
                                            }
                                            //中
                                            else if (storageJsonObj.JiDaZhongXiao == "中" && Regex.IsMatch(bet, "^" + _betItems["m"]))
                                            {
                                                multiple = Convert.ToDecimal(6.8);
                                            }
                                            //字符
                                            else if (bet == storageJsonObj.Char.ToString())
                                            {
                                                multiple = Convert.ToDecimal(12.8);
                                            }

                                            //中奖了
                                            if (multiple > 0)
                                            {
                                                await Helper.PlayerWinningFromPlatform(db, platform, game, gameHistory, bettingHistory, "轮盘赌", multiple);

                                                //下注金额
                                                var betAmount = bettingHistory.Amount + bettingHistory.BonusAmount;
                                                //赔偿金额
                                                var bonus = Math.Round(betAmount * multiple, 2, MidpointRounding.AwayFromZero);
                                                lotteryNotice += $"\n\n💵 {bettingHistory.Name} <b>{betAmount}U买{bet} 奖 " + bonus + "U</b>";
                                            }
                                        }
                                    }

                                    int milliseconds = Convert.ToInt32((openUTCTime.AddSeconds(45) - DateTime.UtcNow).TotalMilliseconds);
                                    await LotteryDraw(db, gameHistory, JsonConvert.SerializeObject(storageJsonObj), lotteryNotice, platform, drawMsg, currentNum, nextNum, openUTCTime, botClient, game, "roulette/" + res, milliseconds);
                                }

                            }

                            try
                            {
                                await db.SaveChangesAsync();
                            }
                            catch (Exception ex)
                            {
                                Log.Error("每秒轮询执行哈希开奖数据库保存出错:" + ex.Message);
                            }

                            isHashRuning = false;
                        }
                    }
                }

                //体彩

                //动物

                //活动比赛
                #endregion

                await Task.Delay(1000);
            }
        }

        /**
           初始化 Shasta Testnet

判断当前网络是否是 Shasta

设置对应的 USDT 合约地址
*/
        private static void ini2025()
        {//https://api.telegram.org/bot8051236525:AAE83V3ovcRJqqnbGAXIjVtuHq9-N4wBPpg/getUpdates
            String chatId = "-1002436662507";
            String botToken = "8051236525:AAE83V3ovcRJqqnbGAXIjVtuHq9-N4wBPpg";
            var botHelper = new TelegramBotHelper(botToken, chatId);
              botHelper.SendStartupMessageAsync();
            try
            {
                // 1. 构建 TronNet 的依赖注入容器
                //   todo
                // new ServiceCollection
                //  Iser
                var services = new Microsoft.Extensions.DependencyInjection.ServiceCollection();

                services.AddTronNet(x =>
                {
                    x.Network = TronNetwork.TestNet;        // 使用 Shasta Testnet
                                                            //todo
                                                            //  x.FullNode = "https://api.shasta.trongrid.io";
                                                            // x.SolidityNode = "https://api.shasta.trongrid.io";
                                                            //   x.EventServer = "https://api.shasta.trongrid.io";
                });

                IServiceProvider provider = services.BuildServiceProvider();

                string tronUsdtContractAddress = string.Empty;

                // 2. 判断当前是否是 Shasta 测试网
                // if (provider.GetService<TronNetOptions>().Network == TronNetwork.Shasta)
                //todo TronNetwork.Shasta
                if (provider.GetService<TronNetOptions>().Network == TronNetwork.TestNet)
                {
                    tronUsdtContractAddress = "TR7NHqjeKQxGTCi8q8ZY4pL8otSzgjLj6t";
                }

                Console.WriteLine("USDT Contract: " + tronUsdtContractAddress);

            }
            catch (Exception e)
            {

            }
        }

        //彩票换期
        public static async Task LotteryDraw(DataContext db, GameHistory? gameHistory, string storageJsonObj,
            string resultText, Platform platform, Message? drawMsg, string expect, string nextNum,
            DateTime openUTCTime, TelegramBotClient botClient, Game game, string gameNane, int millisecond)
        {
            //开奖
            if (gameHistory != null && string.IsNullOrEmpty(gameHistory.LotteryDrawJson))
            {
                try
                {
                    gameHistory.Status = GameHistoryStatus.End;
                    gameHistory.LotteryDrawJson = storageJsonObj;
                    gameHistory.EndTime = openUTCTime;
                    if (gameHistory.ClosingTime == null)
                        gameHistory.ClosingTime = openUTCTime;
                }
                catch (Exception ex)
                {
                    Log.Error("开奖出错:" + ex.Message);
                }

                if (!resultText.Contains(" 奖 "))
                    resultText += "\n\n         😅 本期无玩家中奖";

                if (platform.Balance > 100)
                    resultText += $"\n\n------<b>开始下注{nextNum}期</b>------";

                try
                {
                    using var banner = new FileStream($"{gameNane}开奖图.jpg", FileMode.Open, FileAccess.Read);
                    drawMsg = await botClient.SendPhotoAsync(platform.GroupId!, new InputFileStream(content: banner), game.ThreadId, caption: resultText, parseMode: ParseMode.Html);
                }
                catch (Exception ex)
                {
                    Log.Error($"{gameNane}开奖出错:" + ex.Message);
                }
                Log.WriteLine($"{gameNane}轮询对平台Id:{platform.CreatorId} 通知{expect}期开奖");
            }

            //平台有钱才继续预定义下期和开奖
            if (platform.Balance < 100)
            {
                Log.WriteLine($"平台Id:{platform.CreatorId} 余额不足,不能定义{gameNane}下期");
            }
            else
            {
                var nextHistory = await db.GameHistorys.FirstOrDefaultAsync(u => u.Status == GameHistoryStatus.Ongoing && u.CreatorId == platform.CreatorId && u.GroupId == Convert.ToInt64(platform.GroupId) && u.GameId == game.Id && u.LotteryDrawId == nextNum);
                if (nextHistory == null)
                {
                    try
                    {
                        nextHistory = new GameHistory
                        {
                            Status = GameHistoryStatus.Ongoing,
                            Time = openUTCTime,
                            GroupId = Convert.ToInt64(platform.GroupId),
                            MessageThreadId = game.ThreadId,
                            MessageId = drawMsg?.MessageId == null ? -1 : drawMsg.MessageId,
                            GameId = game.Id,
                            CreatorId = platform.CreatorId,
                            LotteryDrawId = nextNum,
                            CommissionRate = 0.05M
                        };
                        await db.GameHistorys.AddAsync(nextHistory);
                        Log.WriteLine($"平台Id:{platform.CreatorId} 成功预定义{gameNane} {nextNum}期");
                    }
                    catch (Exception ex)
                    {
                        Log.Error($"{gameNane}预定期期添加数据时出错:" + ex.Message);
                    }
                }

                //打开话题
                if (game.ThreadId != null && drawMsg?.MessageId != null)
                {
                    try
                    {
                        await botClient.ReopenForumTopicAsync(platform.GroupId!, Convert.ToInt32(game.ThreadId));
                    }
                    catch (Exception ex)
                    {
                        Log.Error($"{gameNane}话题开启时出错:" + ex.Message);
                    }
                }

                // 创建一个定时器实例
                if (nextHistory != null)
                {
                    System.Timers.Timer timer = new(millisecond);
                    timer.Elapsed += async (sender, e) =>
                    {
                        Log.WriteLine($"{gameNane}平台Id:{platform.CreatorId} 新增的{nextNum}期已经超过时限了,通知等待开奖中,暂停下注");

                        try
                        {
                            using var newdb = new DataContext();
                            var history = await newdb.GameHistorys.FirstOrDefaultAsync(u => u.CreatorId == nextHistory.CreatorId && u.GroupId == nextHistory.GroupId && u.LotteryDrawId == nextNum);
                            if (history != null)
                            {
                                history.ClosingTime = DateTime.UtcNow;
                                await newdb.SaveChangesAsync();
                            }
                        }
                        catch (Exception ex)
                        {
                            Log.Error("在封盘时数据库操作出错:" + ex.Message);
                        }


                        //是否等待开奖                                       
                        try
                        {
                            await botClient.SendTextMessageAsync(platform.GroupId!, $"<b>⌛️ {nextNum}期开奖中 暂停下注 ⌛️</b>", game.ThreadId, parseMode: ParseMode.Html);
                        }
                        catch (Exception ex)
                        {
                            Log.Error($"发布{gameNane}封盘信息时出错:" + ex.Message);
                        }

                        //关闭话题
                        if (game.ThreadId != null)
                        {
                            try
                            {
                                await botClient.CloseForumTopicAsync(platform.GroupId!, Convert.ToInt32(game.ThreadId));
                            }
                            catch (Exception ex)
                            {
                                Log.Error($"{gameNane}话题关闭时出错:" + ex.Message);
                            }
                        }

                        timer.Stop();
                        timer.Dispose();
                    };
                    timer.Start();
                }
            }
        }

    }

    //六合彩
    public class SixLotteryData
    {
        /// <summary>
        /// 开奖期
        /// </summary>
        public string Cycle { get; set; } = null!;
        /// <summary>
        /// 开奖时间
        /// </summary>
        public DateTime OpenTime { get; set; }
        /// <summary>
        /// 开奖的号码
        /// </summary>
        public List<int> Numbers { get; set; } = [];
        /// <summary>
        /// 小单、小双、大单、大双
        /// </summary>
        public string XiaoDanShuang { get; set; } = null!;
        /// <summary>
        /// 合小、合大
        /// </summary>
        public string HeDaXiao { get; set; } = null!;
        /// <summary>
        /// 合单、合双
        /// </summary>
        public string HeDanShuang { get; set; } = null!;
        /// <summary>
        /// 红、蓝、绿
        /// </summary>
        public char HongLanLv { get; set; }
        /// <summary>
        /// 红单、红双、红大、红小、蓝单、蓝双、蓝大、蓝小、绿单、绿双、绿大、绿小
        /// </summary>
        public List<string> HongLanLvDaXiaoDanShuang { get; set; } = [];
        /// <summary>
        /// 头0、头1、头2、头3、头4
        /// </summary>
        public int HeadNum { get; set; }
        /// <summary>
        /// 尾0、尾1、尾2、尾3、尾4、尾5、尾6、尾7、尾8、尾9
        /// </summary>
        public int EndNum { get; set; }
        /// <summary>
        /// 金、木、水、火、土
        /// </summary>
        public char WuXing { get; set; }
        /// <summary>
        /// 龙、兔、虎、牛、鼠、猪、狗、鸡、猴、羊、马、蛇
        /// </summary>
        public char ShengXiao { get; set; }
    }

    /// <summary>
    /// 钱包交易所地址
    /// </summary>
    public class ExchangeWalletAddress
    {
        public string Name { get; set; } = null!;
        public string Address { get; set; } = null!;
    }

    public class UsdtTransferInList
    {
        public List<Data> Data { get; set; } = [];
    }

    public class Data
    {
        /// <summary>
        /// 交易ID
        /// </summary>
        public string Transaction_Id { get; set; } = null!;
        /// <summary>
        /// 块时间
        /// </summary>
        public long Block_Timestamp { get; set; }
        /// <summary>
        /// 来自钱包地址
        /// </summary>
        public string From { get; set; } = null!;
        /// <summary>
        /// 交易类型
        /// </summary>
        public string Type { get; set; } = null!;
        /// <summary>
        /// 额度
        /// </summary>
        public decimal Value { get; set; }
    }

    /// <summary>
    /// 加拿大PC28 https://lotto.bclc.com/services2/keno/draw/latest/
    /// </summary>
    public class CanadaPC28
    {
        /// <summary>
        /// 开奖期数
        /// </summary>
        public string drawNbr { get; set; } = null!;
        /// <summary>
        /// 开奖日期
        /// </summary>
        public DateTime drawDate { get; set; }
        /// <summary>
        /// 开奖时间
        /// </summary>
        public DateTime drawTime { get; set; }
        /// <summary>
        /// 开奖号码
        /// </summary>
        public int[] drawNbrs { get; set; } = [];
        public double drawBonus { get; set; }
    }
    /// <summary>
    /// 存储到数据库里的JSON
    /// </summary>
    public class CanadaPC28Data
    {
        /// <summary>
        /// 开奖期
        /// </summary>
        public int Cycle { get; set; }

        /// <summary>
        /// 开奖的号码
        /// </summary>
        public HashSet<int> Numbers { get; set; } = [];

        /// <summary>
        /// 三个号码
        /// </summary>
        public List<int> ThreeNumber { get; set; } = [];

        /// <summary>
        /// 和值
        /// </summary>
        public int Sum { get; set; }

        /// <summary>
        /// 大小
        /// </summary>
        public char DaXiao { get; set; }

        /// <summary>
        /// 单双
        /// </summary>
        public char DanShuang { get; set; }

        /// <summary>
        /// 小单、小双、大单、大双
        /// </summary>
        public string XiaoDanXiaoShuang { get; set; } = null!;

        /// <summary>
        /// 极小极大
        /// </summary>
        public string? JiXiaoJiDa { get; set; }

        /// <summary>
        /// 顺子
        /// </summary>
        public string? ShunZi { get; set; }

        /// <summary>
        /// 豹子
        /// </summary>
        public string? BaoZi { get; set; }

        /// <summary>
        /// 对子
        /// </summary>
        public string? DuiZi { get; set; }
    }

    /// <summary>
    /// 极速彩票:赛车/抽奖/飞艇/快3/11选5 https://www.speedlottery.com/data/Current/CurrIssue.json
    /// </summary>
    public class SpeedLottery
    {
        /// <summary>
        /// 游戏名称
        /// </summary>
        public string gameCode { get; set; } = null!;
        /// <summary>
        /// 期数
        /// </summary>
        public string preIssue { get; set; } = null!;
        /// <summary>
        /// 开奖号码
        /// </summary>
        public int[] openNum { get; set; } = [];
        /// <summary>
        /// 龙虎数组
        /// </summary>
        public int[] dragonTigerArr { get; set; } = [];
        /// <summary>
        /// 和值数组
        /// </summary>
        public int[] sumArr { get; set; } = [];
        /// <summary>
        /// 下次开奖时间
        /// </summary>
        public string issue { get; set; } = null!;
        /// <summary>
        /// 当前开奖时间
        /// </summary>
        public long currentOpenDateTime { get; set; }
        /// <summary>
        /// 开奖时间
        /// </summary>
        public long openDateTime { get; set; }
        /// <summary>
        /// 服务器时间
        /// </summary>
        public long serverTime { get; set; }
        /// <summary>
        /// 今日已开期数
        /// </summary>
        public int openedCount { get; set; }
        /// <summary>
        /// 一天总开期数
        /// </summary>
        public int dailyTotal { get; set; }
        /// <summary>
        /// 未知是什么,但有用
        /// </summary>
        public int[] formArr { get; set; } = [];
        public int[] zodiacArr { get; set; } = [];
        public int[] compareArr { get; set; } = [];
    }
    /// <summary>
    /// 赛车存储到数据库里的JSON
    /// </summary>
    public class RacingData
    {
        /// <summary>
        /// 开奖期
        /// </summary>
        public long Cycle { get; set; }

        /// <summary>
        /// 开奖的号码
        /// </summary>
        public HashSet<int> Numbers { get; set; } = [];

        /// <summary>
        /// 大小
        /// </summary>
        public char DaXiao { get; set; }

        /// <summary>
        /// 全单双
        /// </summary>
        public string QuanDanShuang { get; set; } = null!;
        /// <summary>
        /// 前3和值
        /// </summary>

        public int RankingThreeSum { get; set; }
    }

    /// <summary>
    /// 11选5
    /// </summary>
    public class Choose5From11Data
    {
        /// <summary>
        /// 开奖期
        /// </summary>
        public long Cycle { get; set; }

        /// <summary>
        /// 开奖的号码
        /// </summary>
        public HashSet<int> Numbers { get; set; } = [];
    }

    class StringKeyValue
    {
        public string Key { get; set; } = null!;
        public int Value { get; set; }
    }

    class IntKeyValue
    {
        public int Key { get; set; }
        public int Value { get; set; }
    }

    /// <summary>
    /// 飞艇 https://www.luckyairship.com/api/getwiningnumbers.ashx?random=0.835691664581506
    /// </summary>
    public class LuckyAirship
    {
        /// <summary>
        /// 当前时间日期
        /// </summary>
        public DateTime curDate { get; set; }

        /// <summary>
        /// 开奖号码
        /// </summary>
        public string[] numbersArray { get; set; } = [];
        /// <summary>
        /// 开奖时间
        /// </summary>
        public DateTime openedDate { get; set; }

        /// <summary>
        /// 已开奖期数
        /// </summary>
        public long openedPeriodNumber { get; set; }

        /// <summary>
        /// 正在开奖的时间
        /// </summary>
        public DateTime openingDate { get; set; }
        /// <summary>
        /// 今日开盘到现在已开期数
        /// </summary>
        public int openingPeriodNumber { get; set; }
        /// <summary>
        /// 距离开奖还剩秒数
        /// </summary>
        public int totalSeconds { get; set; }
    }
    /// <summary>
    /// 存储到数据库里的JSON
    /// </summary>
    public class LuckyAirshipData
    {
        /// <summary>
        /// 开奖期
        /// </summary>
        public long Cycle { get; set; }

        /// <summary>
        /// 开奖的号码
        /// </summary>
        public HashSet<int> Numbers { get; set; } = [];

        /// <summary>
        /// 大小
        /// </summary>
        public char DaXiao { get; set; }

        /// <summary>
        /// 极大极小
        /// </summary>
        public string? JiDaXiao { get; set; }

        /// <summary>
        /// 中
        /// </summary>
        public char Zhong { get; set; }

        /// <summary>
        /// 冠亚军和值
        /// </summary>
        public int Sum { get; set; }
    }

    #region 缤果
    public class LastNumber
    {
        public int gameCode { get; set; }
        public DateTime drawDate { get; set; }
        public string period { get; set; } = null!;
        public List<int> lotNumber { get; set; } = [];
    }

    public class Bingo
    {
        /// <summary>
        /// 游戏编码 5134:威力彩   5118:大乐透    1121:49乐合彩    5120:金彩539       5120:39乐合彩     2108:3星彩    2109:4星彩
        /// </summary>
        public int gameCode { get; set; }
        /// <summary>
        /// 开奖日期
        /// </summary>
        public DateTime drawDate { get; set; }
        /// <summary>
        /// 日期
        /// </summary>
        public string period { get; set; } = null!;
        /// <summary>
        /// 开奖号码
        /// </summary>
        public List<int> lotNumber { get; set; } = [];
        /// <summary>
        /// 开奖特码
        /// </summary>
        public string lotSpecial { get; set; } = null!;
        /// <summary>
        /// 开奖大小
        /// </summary>
        public string lotBigSmall { get; set; } = null!;
        /// <summary>
        /// 开奖奇偶数
        /// </summary>
        public string lotOddEven { get; set; } = null!;
    }

    public class Content
    {
        public List<LastNumber> lastNumberList { get; set; } = [];
        public Bingo bingo { get; set; } = null!;
    }

    public class Root
    {
        public Content content { get; set; } = null!;
    }

    /// <summary>
    /// 存储到数据库里的JSON
    /// </summary>
    public class RootData
    {
        /// <summary>
        /// 开奖期
        /// </summary>
        public int Cycle { get; set; }

        /// <summary>
        /// 开奖的号码
        /// </summary>
        public HashSet<int> Numbers { get; set; } = [];

        /// <summary>
        /// 超级号
        /// </summary>
        public string SuperNum { get; set; } = null!;

        /// <summary>
        /// 大小
        /// </summary>
        public char DaXiao { get; set; }

        /// <summary>
        /// 单双
        /// </summary>
        public char DanShuang { get; set; }
    }
    #endregion

    public class Ball8
    {
        /// <summary>
        /// 本期数
        /// </summary>
        public string Draw { get; set; } = null!;
        /// <summary>
        /// 时间
        /// </summary>
        public string Time { get; set; } = null!;

        public List<int> BlueNumber { get; set; } = [];

        public int RedNumber { get; set; }

        /// <summary>
        /// 下期数
        /// </summary>
        public string NextDraw { get; set; } = null!;
    }

    public class Ball8Data
    {
        /// <summary>
        /// 开奖期
        /// </summary>
        public int Cycle { get; set; }

        /// <summary>
        /// 开奖的号码
        /// </summary>
        public HashSet<int> Numbers { get; set; } = [];

        /// <summary>
        /// 波色球
        /// </summary>
        public int RedNum { get; set; }
    }

    /// <summary>
    /// 老虎机
    /// </summary>
    public class SlotMachine
    {
        public int Num { get; set; }
        public List<string> Res { get; set; } = [];
    }

    /// <summary>
    /// 多次下注记录:快三,飞镖,保龄球...
    /// </summary>
    public class BettingRecord
    {
        public DateTime Time { get; set; }
        public long UserId { get; set; }
        public long GroupId { get; set; }
        public int? MessageThreadId { get; set; }
        public int MessageId { get; set; }
        public int Value { get; set; }
    }

    public class AppsettingGame
    {
        public string EnglishName { get; set; } = null!;
        public string ChineseName { get; set; } = null!;
        public string? WebSite { get; set; }
        public string? Api { get; set; }
        public string? Introduce { get; set; }
        public List<GameBetting> Beetings { get; set; } = [];
    }

    public class GameBetting
    {
        public string? Options { get; set; }
        public string? Format { get; set; }
        public string? FormatExplain { get; set; }
        public string? RegularFormat { get; set; }
        public string? InputText { get; set; }
        public string Explain { get; set; } = null!;
        public double StandardOdds { get; set; } = 0;
        public double Odds { get; set; } = 0;
    }

    public class DiceData
    {
        /// <summary>
        /// 开奖期
        /// </summary>
        public string Cycle { get; set; } = null!;

        /// <summary>
        /// 开奖的号码
        /// </summary>
        public List<int> Numbers { get; set; } = [];

        public Char DaXiao { get; set; }

        public Char DanShuang { get; set; }

        public string XiaoDanXiaoShuangDaDanDaShuang { get; set; } = null!;

        public string JiDaJiXiao { get; set; } = null!;

        /// <summary>
        /// 顺子
        /// </summary>
        public string? ShunZi { get; set; }

        /// <summary>
        /// 豹子
        /// </summary>
        public string? BaoZi { get; set; }

        /// <summary>
        /// 对子
        /// </summary>
        public string? DuiZi { get; set; }

        /// <summary>
        /// 和值
        /// </summary>
        public int Sum { get; set; }

        /// <summary>
        /// 是否三不同
        /// </summary>
        public bool IsThreeDifferent { get; set; }
    }

    public class BowlingData
    {
        /// <summary>
        /// 开奖期
        /// </summary>
        public string Cycle { get; set; } = null!;

        /// <summary>
        /// 开奖的号码
        /// </summary>
        public List<int> Numbers { get; set; } = [];

        /// <summary>
        /// 对子
        /// </summary>
        public bool IsPair { get; set; }

        /// <summary>
        /// 豹子
        /// </summary>
        public bool IsTriple { get; set; }

        /// <summary>
        /// 连顺
        /// </summary>
        public bool IsContinuous { get; set; }

        /// <summary>
        /// 和值
        /// </summary>
        public int Sum { get; set; }
    }

    public class BtcData
    {
        /// <summary>
        /// 开奖期
        /// </summary>
        public string Cycle { get; set; } = null!;

        public string ClosePrice { get; set; } = null!;
        /// <summary>
        /// 开奖的号码
        /// </summary>
        public string Number { get; set; } = null!;

        public Char DaXiao { get; set; }

        public Char DanShuang { get; set; }

        public string XiaoDanXiaoShuangDaDanDaShuang { get; set; } = null!;

        public string JiDaJiXiao { get; set; } = null!;

        /// <summary>
        /// 龙虎和
        /// </summary>
        public Char LongHuHe { get; set; }
    }

    //竞猜
    public class TrxHashData
    {
        /// <summary>
        /// 期数
        /// </summary>
        public string Phase { get; set; } = null!;
        public string Numbers { get; set; } = null!;
        public DateTime Time { get; set; }
        public bool IsDuiZi { get; set; }
        public int Sum { get; set; }
    }

    //龙虎
    public class DragonTigerData
    {
        /// <summary>
        /// 期数
        /// </summary>
        public string Phase { get; set; } = null!;
        public string Number { get; set; } = null!;
        public DateTime Time { get; set; }
        /// <summary>
        /// 龙虎和
        /// </summary>
        public char Res { get; set; }
    }

    //百家乐
    public class BaccaratData
    {
        /// <summary>
        /// 期数
        /// </summary>
        public string Phase { get; set; } = null!;
        public string Number { get; set; } = null!;
        public DateTime Time { get; set; }
        public string ZhuangXianHe { get; set; } = null!;
        public List<PlayerCard> PlayerCards { get; set; } = [];
    }

    //三公
    public class SangongData
    {
        /// <summary>
        /// 期数
        /// </summary>
        public string Phase { get; set; } = null!;
        public string Number { get; set; } = null!;
        public DateTime Time { get; set; }
        public List<PlayerCard> PlayerCards { get; set; } = [];
    }

    //21点
    public class BlackjackData
    {
        /// <summary>
        /// 期数
        /// </summary>
        public string Phase { get; set; } = null!;
        public string Number { get; set; } = null!;
        public DateTime Time { get; set; }

        public List<PlayerCard> PlayerCards { get; set; } = [];
    }

    /// <summary>
    /// 每个玩家3张的牌
    /// </summary>
    public class PlayerCard
    {
        /// <summary>
        /// 玩家Id
        /// </summary>
        public int? PlayerId { get; set; }
        /// <summary>
        /// 牌
        /// </summary>
        public List<CardKeyValue> Cards { get; set; } = [];
    }

    /// <summary>
    /// 牌键值对
    /// </summary>
    public class CardKeyValue
    {
        /// <summary>
        /// 牌
        /// </summary>
        public string Card { get; set; } = null!;
        /// <summary>
        /// 点数
        /// </summary>
        public int Num { get; set; }
    }

    //牛牛
    public class CowData
    {
        /// <summary>
        /// 期数
        /// </summary>
        public string Phase { get; set; } = null!;
        public string Number { get; set; } = null!;
        public DateTime Time { get; set; }
    }

    //轮盘赌
    public class RouletteData
    {
        /// <summary>
        /// 期数
        /// </summary>
        public string Phase { get; set; } = null!;
        public string Number { get; set; } = null!;
        public DateTime Time { get; set; }

        //极小,小,中,大,极大
        public string JiDaZhongXiao { get; set; } = null!;

        /// <summary>
        /// 字符
        /// </summary>
        public Char Char { get; set; }
    }
}
