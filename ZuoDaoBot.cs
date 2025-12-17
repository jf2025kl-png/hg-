using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
namespace 皇冠娱乐
{
    public static class ZuoDaoBot
    {
        //皇冠机器人收到消息时执行的方法
        public static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            using var db = new DataContext();
            await Task.Run(async () =>
            {
                switch (update.Type)
                {
                    //收到信息
                    case UpdateType.Message:
                        if (!db.BotChats.Any(u => u.BotId == botClient.BotId && u.ChatId == update.Message!.Chat.Id))
                        {
                            await db.BotChats.AddAsync(new BotChat { BotId = Convert.ToInt64(botClient.BotId), ChatId = update.Message!.Chat.Id });
                            await db.SaveChangesAsync();
                        }

                        var msg = update.Message!;
                        var text = msg.Text;
                        var user = msg.From!;
                        var uid = user.Id;
                        var chatId = msg.Chat.Id;
                        //返回给用户的信息
                        var returnText = string.Empty;
                        //返回的出错信息
                        string returnError = string.Empty;
                        //底部键盘按钮
                        List<List<KeyboardButton>> inputBtn = [];
                        //信息按钮
                        List<List<InlineKeyboardButton>> msgBtn = [];

                        if (msg.Type is not MessageType.Text || string.IsNullOrEmpty(text))
                            return;
                        text = text.Trim();

                        //查看客服联系方式
                        if (text is "/kefu")
                        {
                            returnText = "<b>🙎‍♂️皇冠客服中心 : @ZuoDao_KeFuBot</b> \n\n ⚠️ 我们不会主动私聊客户,请谨防诈骗!";
                            try
                            {
                                await Program._botClient.SendTextMessageAsync(chatId: chatId, text: returnText, parseMode: ParseMode.Html);
                            }
                            catch (Exception ex)
                            {
                                Log.Error("/kefu 返回信息时出错:" + ex.Message);
                            }
                            return;
                        }

                        //正在执行中的用户(防止重复提交攻击)
                        if (Program._runingUserId.Contains(uid))
                        {
                            Helper.DeleteMessage(botClient, update, 3, "请勿频繁操作,请稍等", cancellationToken);
                            return;
                        }
                        Program._runingUserId.Add(uid);

                        //等待用户的下一步操作
                        WaitInput? waitInput = null;
                        if (!Program._zuodaoWaitInputUser.TryGetValue(uid, out waitInput))
                        {
                            Program._zuodaoWaitInputUser.Add(uid, waitInput);
                        }

                        //是否皇冠管理员
                        var isZuoDaoAdminer = Program._appsettings.AdminerIds.Contains(uid);
                        //机器人私聊收到信息时
                        if (msg!.Chat.Type == ChatType.Private)
                        {
                            //创建好的平台
                            var platform = !isZuoDaoAdminer ? await db.Platforms.FindAsync(uid) : null;

                            //盘口集合
                            var games = platform == null ? null : await db.Games.Where(u => u.CreatorId == platform.CreatorId).ToListAsync();
                            //老虎机
                            var slotMachine = games?.FirstOrDefault(u => u.GameType == GameType.SlotMachine);
                            //骰子
                            var dice = games?.FirstOrDefault(u => u.GameType == GameType.Dice);
                            //保龄球
                            var bowling = games?.FirstOrDefault(u => u.GameType == GameType.Bowling);
                            //飞镖
                            var dart = games?.FirstOrDefault(u => u.GameType == GameType.Dart);
                            //足球
                            var soccer = games?.FirstOrDefault(u => u.GameType == GameType.Soccer);
                            //篮球
                            var basketball = games?.FirstOrDefault(u => u.GameType == GameType.Basketball);
                            //百家乐
                            var baccarat = games?.FirstOrDefault(u => u.GameType == GameType.Baccarat);
                            //刮刮乐
                            var scratchOff = games?.FirstOrDefault(u => u.GameType == GameType.ScratchOff);
                            //体彩
                            var sportsContest = games?.FirstOrDefault(u => u.GameType == GameType.SportsContest);
                            //动物
                            var animalContest = games?.FirstOrDefault(u => u.GameType == GameType.AnimalContest);
                            //视讯
                            var video = games?.FirstOrDefault(u => u.GameType == GameType.Video);
                            //电竞
                            var gaming = games?.FirstOrDefault(u => u.GameType == GameType.Gaming);
                            //电子
                            var electronic = games?.FirstOrDefault(u => u.GameType == GameType.Electronic);
                            //棋牌
                            var chessCards = games?.FirstOrDefault(u => u.GameType == GameType.ChessCards);
                            //捕鱼
                            var fishing = games?.FirstOrDefault(u => u.GameType == GameType.Fishing);
                            //虚拟
                            var virtualGame = games?.FirstOrDefault(u => u.GameType == GameType.VirtualGame);
                            //竞猜
                            var trxHash = games?.FirstOrDefault(u => u.GameType == GameType.TrxHash);
                            //幸运数
                            var luckyHash = games?.FirstOrDefault(u => u.GameType == GameType.LuckyHash);
                            //比特币
                            var binanceBTCPrice = games?.FirstOrDefault(u => u.GameType == GameType.BinanceBTCPrice);
                            //外汇
                            var forex = games?.FirstOrDefault(u => u.GameType == GameType.Forex);
                            //股票
                            var stock = games?.FirstOrDefault(u => u.GameType == GameType.Stock);
                            //轮盘赌
                            var roulette = games?.FirstOrDefault(u => u.GameType == GameType.Roulette);
                            //牛牛
                            var cow = games?.FirstOrDefault(u => u.GameType == GameType.Cow);
                            //21点
                            var blackjack = games?.FirstOrDefault(u => u.GameType == GameType.Blackjack);
                            //三公
                            var sangong = games?.FirstOrDefault(u => u.GameType == GameType.Sangong);
                            //龙虎
                            var dragonTiger = games?.FirstOrDefault(u => u.GameType == GameType.DragonTiger);
                            //六合彩
                            var sixLottery = games?.FirstOrDefault(u => u.GameType == GameType.SixLottery);
                            //红包
                            var redEnvelope = games?.FirstOrDefault(u => u.GameType == GameType.RedEnvelope);
                            //加拿大PC28
                            var canadaPC28 = games?.FirstOrDefault(u => u.GameType == GameType.CanadaPC28);
                            //盲盒
                            var blindBox = games?.FirstOrDefault(u => u.GameType == GameType.BlindBox);
                            //抢庄
                            var grabBanker = games?.FirstOrDefault(u => u.GameType == GameType.GrabBanker);
                            //赛车
                            var speedRacing = games?.FirstOrDefault(u => u.GameType == GameType.SpeedRacing);
                            //飞艇
                            var luckyAirship = games?.FirstOrDefault(u => u.GameType == GameType.LuckyAirship);
                            //11选5
                            var choose5From11 = games?.FirstOrDefault(u => u.GameType == GameType.Choose5From11);
                            //缤果
                            var bingo = games?.FirstOrDefault(u => u.GameType == GameType.Bingo);
                            //幸运8
                            var australianLucky8 = games?.FirstOrDefault(u => u.GameType == GameType.AustralianLucky8);
                            //大乐透
                            var bigLottery = games?.FirstOrDefault(u => u.GameType == GameType.BigLottery);
                            //四星彩
                            var fourStarLottery = games?.FirstOrDefault(u => u.GameType == GameType.FourStarLottery);
                            //秘钥:博彩平台的私钥
                            string? privateKey = string.Empty;
                            //传过来的机器人ApiToken
                            string botApiToken = string.Empty;
                            //平台财务历史记录
                            PlatformFinanceHistory? platformFinanceHistory = null;
                            //博彩平台操作记录
                            PlatformOperateHistory? platformOperateHistory = null;
                            //向皇冠管理员们发的通知信息
                            string? returnTipForZuoDaoAdminer = string.Empty;

                            //如果是皇冠管理员
                            if (isZuoDaoAdminer)
                            {
                                //返回平台列表
                                if (text.Contains("全部平台列表")
                                || Regex.IsMatch(text, @"^◀️\s[0-9]{1,4}$")
                                || Regex.IsMatch(text, @"^[0-9]{1,4}\s▶️$"))
                                {
                                    var platforms = await db.Platforms.ToListAsync();
                                    if (platforms.Count == 0)
                                    {
                                        returnError = "还未有平台,再接再厉哦!";
                                    }
                                    else
                                    {
                                        int page = text.Contains("全部平台列表") ? 0 : Convert.ToInt32(Regex.Match(text, @"[0-9]{1,4}").Value);

                                        if (text.Contains("全部平台列表"))
                                        {
                                            returnText = $"皇冠共<b>{platforms.Count}</b>个博彩平台" +
                                            $"\n\n<b>{platforms.Count(u => u.PlatformStatus == PlatformStatus.Open)}</b>个开启中," +
                                            $"\n\n<b>{platforms.Count(u => u.PlatformStatus == PlatformStatus.Close)}</b>个关闭中," +
                                            $"\n\n<b>{platforms.Count(u => u.PlatformStatus == PlatformStatus.Freeze)}</b>个已冻结," +
                                            $"\n\n皇冠的平台余额:<b>{platforms.Sum(u => (double)u.Balance)}</b>USDT," +
                                            $"皇冠官方共盈利:<b>{Program._appsettings.Profit}</b>USDT," +
                                            $"全部平台共盈利:<b>{platforms.Sum(u => (double)u.Profit)}</b>USDT";
                                        }
                                        else
                                        {
                                            returnText = $"您现在正在查看第{page}页平台列表";
                                        }
                                        var result = platforms.Skip(page * 10).Take(10);
                                        foreach (var item in result)
                                        {
                                            string platformStatus = string.Empty;
                                            switch (item.PlatformStatus)
                                            {
                                                case PlatformStatus.Open:
                                                    platformStatus = "✅ 开启中";
                                                    break;
                                                case PlatformStatus.Close:
                                                    platformStatus = "❎ 关闭中";
                                                    break;
                                                case PlatformStatus.Freeze:
                                                    platformStatus = "🚫 冻结中";
                                                    break;
                                                default:
                                                    break;
                                            }
                                            inputBtn.Add([new KeyboardButton($"{platformStatus} 平台({item.CreatorId})")]);
                                        }

                                        List<KeyboardButton> pageBtn = [];
                                        if (page > 0)
                                            pageBtn.Add(new KeyboardButton($"◀️ {page}"));

                                        if (platforms.Count > ((page + 1) * 10))
                                            pageBtn.Add(new KeyboardButton($"({page + 2}) ▶️"));

                                        if (pageBtn.Count != 0)
                                            inputBtn.Add(pageBtn);
                                    }
                                }
                                //审批平台提现
                                else if (text is "💸 平台提现审批"
                                || Regex.IsMatch(text, @"^平台\([0-9]{10}\)-\([0-9]{1,10}\)请求提现.*U$")
                                || text.Contains("/WithdrawalApproval")
                                || text.Contains("/DeniedWithdrawal"))
                                {
                                    if (text is "💸 平台提现审批")
                                    {
                                        var awitConfirmation = db.PlatformFinanceHistorys.Where(u => u.FinanceStatus == FinanceStatus.WaitingConfirmation);
                                        if (!awitConfirmation.Any())
                                        {
                                            returnError = "无需审批的提现!";
                                        }
                                        else
                                        {
                                            returnText = "以下平台提现请求等待审批";
                                            foreach (var item in awitConfirmation)
                                            {
                                                inputBtn.Add([new KeyboardButton($"平台({item.CreatorId})-({item.Id})请求提现{item.Amount}U")]);
                                            }
                                        }
                                    }
                                    else if (Regex.IsMatch(text, @"^平台\([0-9]{10}\)-\([0-9]{1,10}\)请求提现.*U$"))
                                    {
                                        var id = Regex.Match(text, @"(?<=\)-\()[0-9]{1,10}(?=\))");
                                        if (id == null || string.IsNullOrEmpty(id.Value))
                                        {
                                            returnError = "格式有误,审批失败!";
                                        }
                                        else if (!db.PlatformFinanceHistorys.Any(u => u.Id == Convert.ToInt32(id.Value) && u.FinanceStatus == FinanceStatus.WaitingConfirmation))
                                        {
                                            returnError = "财务记录不存在";
                                        }
                                        else
                                        {
                                            platformFinanceHistory = await db.PlatformFinanceHistorys.FirstOrDefaultAsync(u => u.Id == Convert.ToInt32(id.Value) && u.FinanceStatus == FinanceStatus.WaitingConfirmation);
                                            if (platformFinanceHistory == null)
                                            {
                                                returnError = "提现记录不存在";
                                            }
                                            else
                                            {
                                                returnText = $"提现审批通过请点击👉  /WithdrawalApproval{platformFinanceHistory.Id}\n\n\n提现审批不通过请点击👉  /DeniedWithdrawal{platformFinanceHistory.Id}";
                                            }
                                        }
                                    }
                                    else if (text.Contains("/WithdrawalApproval") || text.Contains("/DeniedWithdrawal"))
                                    {
                                        var id = Regex.Match(text, @"[0-9]{1,10}");
                                        platformFinanceHistory = await db.PlatformFinanceHistorys.FirstOrDefaultAsync(u => u.Id == Convert.ToInt32(id.Value) && u.FinanceStatus == FinanceStatus.WaitingConfirmation);
                                        if (id == null || string.IsNullOrEmpty(id.Value))
                                        {
                                            returnError = "格式有误,审批失败!";
                                        }
                                        else if (platformFinanceHistory == null)
                                        {
                                            returnError = "财务记录不存在";
                                        }
                                        else
                                        {
                                            platform = await db.Platforms.FindAsync(platformFinanceHistory.CreatorId);
                                            //审核通过
                                            if (text.Contains("/WithdrawalApproval"))
                                            {
                                                returnText = $"审批通过,{platformFinanceHistory.CreatorId}成功提现{platformFinanceHistory.Amount}USDT";
                                                returnTipForZuoDaoAdminer = returnText;
                                                platformFinanceHistory.FinanceStatus = FinanceStatus.Success;

#warning 给对方提现
                                            }
                                            else
                                            {
                                                returnText = $"审批不通过,{platformFinanceHistory.CreatorId}的提现未成功";
                                                platformFinanceHistory.FinanceStatus = FinanceStatus.Reject;
                                            }
                                            platformFinanceHistory.Remark = returnText;
                                            //告知审核结果
                                            try
                                            {
                                                await Program._botClient.SendTextMessageAsync(platformFinanceHistory.CreatorId, returnText);
                                            }
                                            catch (Exception ex)
                                            {
                                                Log.Error("告知提现审批结果时出错:" + ex.Message);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        returnError = "输入有误";
                                    }
                                }
                                #region 系统配置                                
                                else if (text is "⚙️ 全局系统配置")
                                {
                                    //点击后展示系统配置信息,提示修改方法:ZuoDaoBotKeyToken=
                                    returnText = $"<b>波场钱包地址</b>:{Program._appsettings.TronWalletAddress}" +
                                    $"\n\n<b>以太坊钱包地址</b>:{Program._appsettings.EthereumWalletAddress}" +
                                    $"\n\n<b>盘口开盘费用</b>:{Program._appsettings.CreateBettingThreadFees} USDT" +
                                    $"\n\n<b>开盘首月免开盘费盈利额度</b>:{Program._appsettings.FirstMonthWaiverCreateFees} USDT" +
                                    $"\n\n<b>盘口月维护费</b>:{Program._appsettings.BettingThreadMonthlyMaintenanceFee} USDT" +
                                    $"\n\n<b>盘口每月免维护费盈利额度</b>:{Program._appsettings.MonthlyBettingThreadWaiverFee} USDT" +
                                    $"\n\n<b>皇冠收益分成占比</b>:{Program._appsettings.BettingThreadDividend}" +
                                    $"\n\n<b>皇冠是否暂停平台提现</b>:" + (Program._appsettings.IsStopWithdraw ? '是' : '否') + " (非玩家提现)" +
                                    $"\n\n<b>平台提现是否需要皇冠管理员审核</b>:" + (Program._appsettings.IsApprovalWithdraw ? '是' : '否') + " (非审核玩家提现)" +
                                    $"\n\n<b>皇冠截止今日共盈利</b>:{Program._appsettings.Profit} USDT" +
                                    $"\n\n\n✏️ <b>编辑系统配置,按照格式编辑发送:</b>" +
                                    $"\n\n盘口开盘费用 : <b>CreateBettingThreadFees=数字</b> 数字可以是0.00-10000.00之间的任意二位小数点数值" +
                                    $"\n\n开盘首月免开盘费盈利额度 : <b>FirstMonthWaiverCreateFees=数字</b> 数字可以是0.00-100000000.00之间的任意二位小数点数值" +
                                    $"\n\n盘口月维护费 : <b>BettingThreadMonthlyMaintenanceFee=数字</b> 数字可以是0.00-100000.00之间的任意二位小数点数值" +
                                    $"\n\n盘口每月免维护费盈利额度 : <b>MonthlyBettingThreadWaiverFee=数字</b> 数字可以是0.00-100000000.00之间的任意二位小数点数值" +
                                    $"\n\n皇冠收益分成占比 : <b>BettingThreadDividend=数字</b> 数字可以是0.00-0.90之间的任意两位小数点数值" +
                                    $"\n\n皇冠是否暂停平台提现 : <b>IsStopWithdraw=值</b> 值可以是'是'和'否'" +
                                    $"\n\n平台提现是否需要皇冠管理员审核 : <b>IsApprovalWithdraw=值</b> 值可以是'是'和'否'";
                                }
                                else if (text.Contains("CreateBettingThreadFees=")
                                || text.Contains("FirstMonthWaiverCreateFees=")
                                || text.Contains("BettingThreadMonthlyMaintenanceFee=")
                                || text.Contains("MonthlyBettingThreadWaiverFee=")
                                || text.Contains("BettingThreadDividend=")
                                || text.Contains("IsStopWithdraw=")
                                || text.Contains("IsApprovalWithdraw="))
                                {
                                    var value = text[(text.IndexOf('=') + 1)..].Trim();
                                    if (string.IsNullOrEmpty(value))
                                    {
                                        returnError = "编辑失败,格式不正确!";
                                    }
                                    else
                                    {
                                        if (text.Contains("CreateBettingThreadFees="))
                                        {
                                            if (!Regex.IsMatch(value, @"^(?!0(\.0{1,2})?$|10000(\.0{1,2})?$)\d{1,4}(\.\d{1,2})?$") || !decimal.TryParse(value, out decimal result20) || result20 < 0 || result20 > 10000)
                                            {
                                                returnError = "编辑失败,格式不正确!";
                                            }
                                            else
                                            {
                                                Program._appsettings.CreateBettingThreadFees = Convert.ToDecimal(value);
                                            }
                                        }
                                        else if (text.Contains("FirstMonthWaiverCreateFees="))
                                        {
                                            if (!Regex.IsMatch(value, @"^(?!0(\.0{1,2})?$|100000000(\.0{1,2})?$)\d{1,8}(\.\d{1,2})?$") || !decimal.TryParse(value, out decimal result21) || result21 < 0 || result21 > 100000000)
                                            {
                                                returnError = "编辑失败,格式不正确!";
                                            }
                                            else
                                            {
                                                Program._appsettings.FirstMonthWaiverCreateFees = Convert.ToDecimal(value);
                                            }
                                        }
                                        else if (text.Contains("BettingThreadMonthlyMaintenanceFee="))
                                        {
                                            if (!Regex.IsMatch(value, @"^(?!0(\.0{1,2})?$|100000(\.0{1,2})?$)\d{1,5}(\.\d{1,2})?$") || !decimal.TryParse(value, out decimal result22) || result22 < 0 || result22 > 100000)
                                            {
                                                returnError = "编辑失败,格式不正确!";
                                            }
                                            else
                                            {
                                                Program._appsettings.BettingThreadMonthlyMaintenanceFee = Convert.ToDecimal(value);
                                            }
                                        }
                                        else if (text.Contains("MonthlyBettingThreadWaiverFee="))
                                        {
                                            if (!Regex.IsMatch(value, @"^(?!0(\.0{1,2})?$|100000000(\.0{1,2})?$)\d{1,8}(\.\d{1,2})?$") || !decimal.TryParse(value, out decimal result23) || result23 < 0 || result23 > 100000000)
                                            {
                                                returnError = "编辑失败,格式不正确!";
                                            }
                                            else
                                            {
                                                Program._appsettings.MonthlyBettingThreadWaiverFee = Convert.ToDecimal(value);
                                            }
                                        }
                                        else if (text.Contains("BettingThreadDividend="))
                                        {
                                            if (!Regex.IsMatch(value, @"^(0|0\.(0[0-9]|[0-8][0-9]?|90))$") || !double.TryParse(value, out double result24) || result24 < 0 || result24 > 0.9)
                                            {
                                                returnError = "编辑失败,格式不正确!";
                                            }
                                            else
                                            {
                                                Program._appsettings.BettingThreadDividend = Convert.ToDecimal(value);
                                            }
                                        }
                                        else if (text.Contains("IsStopWithdraw="))
                                        {
                                            if (value is not "是" and not "否")
                                            {
                                                returnError = "编辑失败,格式不正确!";
                                            }
                                            else
                                            {
                                                Program._appsettings.IsStopWithdraw = value is "是";
                                            }
                                        }
                                        else if (text.Contains("IsApprovalWithdraw="))
                                        {
                                            if (value is not "是" and not "否")
                                            {
                                                returnError = "编辑失败,格式不正确!";
                                            }
                                            else
                                            {
                                                Program._appsettings.IsApprovalWithdraw = value is "是";
                                            }
                                        }

                                        if (string.IsNullOrEmpty(returnError))
                                        {
                                            returnText = "系统配置修改成功,请点击底部按钮[⚙️ 全局系统配置]刷新查看修改结果";
                                            await Helper.SaveAppsettings();
                                        }
                                    }
                                }
                                #endregion
                                #region 平台
                                //点击了查看列表中的平台项
                                else if (Regex.IsMatch(text, @"\s平台\([0-9]{10}\)\s"))
                                {
                                    var id = Convert.ToInt64(Regex.Match(text, @"[0-9]{10}").Value);
                                    platform = await db.Platforms.FindAsync(id);
                                    if (platform == null)
                                    {
                                        returnError = "此平台不存在";
                                    }
                                    else
                                    {
                                        string platformStatus = string.Empty;
                                        switch (platform.PlatformStatus)
                                        {
                                            case PlatformStatus.Open:
                                                platformStatus = "✅ 开启中";
                                                break;
                                            case PlatformStatus.Close:
                                                platformStatus = "❎ 关闭中";
                                                break;
                                            case PlatformStatus.Freeze:
                                                platformStatus = "🚫 冻结中";

                                                break;
                                            default:
                                                break;
                                        }

                                        var chat = await Helper.GetChatInfo(Program._botClient, Convert.ToInt64(platform.GroupId));
                                        var groupName = chat?.FirstName + chat?.LastName;
                                        returnText = "<b>" + groupName + "的信息</b>" +
                                        $"\n\n创建者Id:{platform.CreatorId}" +
                                        $"\n\n群组Id:{platform.GroupId}" +
                                        $"\n\n群组名称:{groupName}" +
                                        $"\n\n是否停止提现:" + (platform.IsStopWithdraw ? '是' : '否') +
                                        $"\n\n平台状态:{platformStatus}" +
                                        $"\n\n状态提示:{platform.FreezeTip}" +
                                        $"\n\n波场钱包:{platform.TronWalletAddress}" +
                                        $"\n\n以太坊钱包:{platform.EthereumWalletAddress}" +
                                        $"\n\n财务Id:{platform.FinancerId}" +
                                        $"\n\n邀请者分红比例:{platform.Dividend}" +
                                        $"\n\n平台在皇冠的余额:{platform.Balance}" +
                                        $"\n\n提现需财务干预额度:{platform.FinancialOperationAmount}" +
                                        $"\n\n平台共盈利:{platform.Profit}";

                                        inputBtn.Add([new KeyboardButton(platformStatus.Contains("冻结") ? $"解冻({platform.CreatorId})平台" : $"冻结({platform.CreatorId})平台")]);
                                        inputBtn.Add([new KeyboardButton($"💰 查看({platform.CreatorId})财务记录 {db.PlatformFinanceHistorys.Count(u => u.CreatorId == platform.CreatorId)}")]);
                                        inputBtn.Add([new KeyboardButton($"👥 查看({platform.CreatorId})平台玩家 {db.Players.Count(u => u.CreatorId == platform.CreatorId)}")]);
                                        var setGameInputBtn = (string gameName, Game? game) =>
                                        {
                                            if (game != null)
                                            {
                                                var reStatus = string.Empty;
                                                if (game.GameStatus == GameStatus.Freeze)
                                                {
                                                    reStatus = "已冻结";
                                                }
                                                else if (game.GameStatus == GameStatus.Open)
                                                {
                                                    reStatus = "开启中";
                                                }
                                                else if (game.GameStatus == GameStatus.Close)
                                                {
                                                    reStatus = "关闭中";
                                                }
                                                else
                                                {
                                                    reStatus = "已过期";
                                                }
                                                inputBtn.Add([new KeyboardButton($"盘口:({platform.CreatorId})的{gameName}({reStatus})")]);
                                            }
                                        };
                                        setGameInputBtn("老虎机", slotMachine);
                                        setGameInputBtn("骰子", dice);
                                        setGameInputBtn("保龄球", bowling);
                                        setGameInputBtn("飞镖", dart);
                                        setGameInputBtn("足球", soccer);
                                        setGameInputBtn("篮球", basketball);
                                        setGameInputBtn("红包", redEnvelope);
                                        setGameInputBtn("盲盒", blindBox);
                                        setGameInputBtn("抢庄", grabBanker);
                                        setGameInputBtn("刮刮乐", scratchOff);
                                        setGameInputBtn("体彩", sportsContest);
                                        setGameInputBtn("动物", animalContest);
                                        setGameInputBtn("视讯", video);
                                        setGameInputBtn("电竞", gaming);
                                        setGameInputBtn("电子", electronic);
                                        setGameInputBtn("棋牌", chessCards);
                                        setGameInputBtn("捕鱼", fishing);
                                        setGameInputBtn("虚拟", virtualGame);
                                        setGameInputBtn("轮盘赌", roulette);
                                        setGameInputBtn("牛牛", cow);
                                        setGameInputBtn("21点", blackjack);
                                        setGameInputBtn("三公", sangong);
                                        setGameInputBtn("百家乐", baccarat);
                                        setGameInputBtn("竞猜", trxHash);
                                        setGameInputBtn("幸运数", luckyHash);
                                        setGameInputBtn("比特币", binanceBTCPrice);
                                        setGameInputBtn("外汇", forex);
                                        setGameInputBtn("股票", stock);
                                        setGameInputBtn("龙虎", dragonTiger);
                                        setGameInputBtn("六合彩", sixLottery);
                                        setGameInputBtn("PC28", canadaPC28);
                                        setGameInputBtn("赛车", speedRacing);
                                        setGameInputBtn("飞艇", luckyAirship);
                                        setGameInputBtn("11选5", choose5From11);
                                        setGameInputBtn("缤果", bingo);
                                        setGameInputBtn("幸运8", australianLucky8);
                                        setGameInputBtn("大乐透", bigLottery);
                                        setGameInputBtn("四星彩", fourStarLottery);
                                        inputBtn.Add([new KeyboardButton($"↩️ 返回全部平台列表")]);
                                    }
                                }
                                //冻结/解冻平台命令
                                else if (Regex.IsMatch(text, @"^解冻\([0-9]{10}\)平台$") || Regex.IsMatch(text, @"^冻结\([0-9]{10}\)平台$") || text.Contains("/UnFreeze") || text.Contains("/Freeze"))
                                {
                                    //解冻命令
                                    if (Regex.IsMatch(text, @"^解冻\([0-9]{10}\)平台$"))
                                    {
                                        returnText = $"确定解冻请点击👉 /UnFreeze{Regex.Match(text, @"[0-9]{10}")}";
                                    }
                                    //冻结命令
                                    else if (Regex.IsMatch(text, @"^冻结\([0-9]{10}\)平台$"))
                                    {
                                        returnText = $"如果要冻结{Regex.Match(text, @"[0-9]{10}")}平台,请发送:<code>/Freeze{Regex.Match(text, @"[0-9]{10}")}=冻结理由</code>";
                                    }
                                    //冻结
                                    else if (Regex.IsMatch(text, @"^/Freeze\s[0-9]{10}=.+$"))
                                    {
                                        // 冻结
                                        var id = Convert.ToInt64(Regex.Match(text, @"[0-9]{10}").Value);
                                        //冻结理由
                                        var freezeTip = text[(text.IndexOf('=') + 1)..];
                                        platform = await db.Platforms.FirstOrDefaultAsync(u => u.CreatorId == id);
                                        if (platform == null)
                                        {
                                            returnError = "操作失败,此平台不存在";
                                        }
                                        else if (platform.PlatformStatus == PlatformStatus.Freeze)
                                        {
                                            returnError = "操作失败,本来就是冻结的";
                                        }
                                        else if (string.IsNullOrEmpty(freezeTip))
                                        {
                                            returnError = "操作失败,冻结理由不可为空";
                                        }
                                        else
                                        {
                                            returnText = $"成功冻结{id}平台,理由:{freezeTip}";
                                            platform.PlatformStatus = PlatformStatus.Freeze;
                                            platform.FreezeTip = freezeTip;
                                            returnTipForZuoDaoAdminer = $"对平台({id})冻结,理由:{freezeTip}";
                                            platformOperateHistory = new PlatformOperateHistory
                                            {
                                                CreatorId = id,
                                                OperateUserId = uid,
                                                PlatformUserRole = PlatformUserRole.Adminer,
                                                Time = DateTime.UtcNow,
                                                Remark = "平台被冻结"
                                            };

                                            try
                                            {
                                                await Program._botClient.SendTextMessageAsync(id, $"您的平台已被冻结,理由:{freezeTip}\n\n详情请咨询皇冠客服 @ZuoDao_KeFuBot");
                                            }
                                            catch (Exception ex)
                                            {
                                                Log.Error("告知平台被冻结时出错:" + ex.Message);
                                            }
                                        }
                                    }
                                    //解冻
                                    else if (Regex.IsMatch(text, @"^/UnFreeze[0-9]{10}$"))
                                    {
                                        // 冻结
                                        var id = Convert.ToInt64(Regex.Match(text, @"[0-9]{10}").Value);
                                        platform = await db.Platforms.FirstOrDefaultAsync(u => u.CreatorId == id);
                                        if (platform == null)
                                        {
                                            returnError = "操作失败,此平台不存在";
                                        }
                                        else if (platform.PlatformStatus != PlatformStatus.Freeze)
                                        {
                                            returnError = "操作失败,本平台并未冻结的";
                                        }
                                        else
                                        {
                                            returnText = $"成功解冻{id}平台";
                                            platform.PlatformStatus = PlatformStatus.Open;
                                            platform.FreezeTip = string.Empty;
                                            returnTipForZuoDaoAdminer = $"对平台({id})解冻";
                                            platformOperateHistory = new PlatformOperateHistory
                                            {
                                                CreatorId = id,
                                                OperateUserId = uid,
                                                PlatformUserRole = PlatformUserRole.Adminer,
                                                Time = DateTime.UtcNow,
                                                Remark = "平台被解冻"
                                            };

                                            try
                                            {
                                                await Program._botClient.SendTextMessageAsync(id, $"您的平台已被解冻");
                                            }
                                            catch (Exception ex)
                                            {
                                                Log.Error("告知平台被解冻时出错:" + ex.Message);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        returnError = "您发的命令有误!";
                                    }
                                }
                                //查看财务记录(下载表格文档)
                                else if (Regex.IsMatch(text, @"^💰 查看\([0-9]{10}\)财务记录"))
                                {
#warning 这里是向管理员返回下载表格
                                }
                                //查看平台玩家列表
                                else if (Regex.IsMatch(text, @"^👥 查看\([0-9]{10}\)平台玩家"))
                                {
                                    //>进去还能看到玩家财务记录
                                }
                                //查看红包盘口信息
                                else if (Regex.IsMatch(text, @"^盘口:\([0-9]{10}\)的红包"))
                                {
                                    //点击后可以看到盘口信息,和盘口历史记录 /还有冻结/解冻操作
                                }
                                #endregion
                                else
                                {
                                    returnText = "尊贵的皇冠管理员:";
                                    inputBtn.Add([new KeyboardButton("🔢 全部平台列表")]);
                                    inputBtn.Add([new KeyboardButton("⚙️ 全局系统配置")]);
                                    if (db.PlatformFinanceHistorys.Any(u => u.FinanceStatus == FinanceStatus.WaitingConfirmation))
                                        inputBtn.Add([new KeyboardButton("💸 平台提现审批")]);
                                }

                                inputBtn.Add([new KeyboardButton("↩️ 返回顶级菜单")]);
                            }
                            //未创建平台的访客
                            else if (platform == null)
                            {
                                if (db.Platforms.Any(u => u.FinancerId == uid))
                                {
                                    returnText = "您是其他博彩平台群的工作人员,请前往贵平台的机器人进行操作!";
                                }
                                else
                                {
                                    //用户发了绑定机器人Api Token
                                    if (Regex.IsMatch(text, @"[0-9]{10}:[0-9a-zA-Z-_]{35}"))
                                    {
                                        botApiToken = text;
                                        TelegramBotClient? bc = null;
                                        try
                                        {
                                            bc = new TelegramBotClient(botApiToken);
                                            bc.StartReceiving(updateHandler: PlatformBot.PlatformHandleUpdateAsync, pollingErrorHandler: Helper.PollingErrorHandler, receiverOptions: new ReceiverOptions() { ThrowPendingUpdates = true });
                                        }
                                        catch (Exception ex)
                                        {
                                            Log.Error("用户发的机器人Api Token无效" + ex.Message);
                                        }

                                        if (bc == null)
                                        {
                                            returnError = "绑定失败,输入的机器人API Token无效\n\n你如果还没有机器人?<a href='https://t.me/BotFather'>去创建机器人</a>";
                                        }
                                        else if (db.Platforms.Any(u => u.BotApiToken == botApiToken))
                                        {
                                            returnError = "绑定失败,此机器人Api Token已经绑定过其他博彩平台,不能重复绑定!";
                                        }
                                        else
                                        {
                                            returnTipForZuoDaoAdminer = $"@{user.Username} {user.FirstName}{user.LastName} (Id:{user.Id})绑定了机器人Api Token创建了平台";
                                            platform = new Platform
                                            {
                                                CreatorId = uid,
                                                BotApiToken = botApiToken,
                                                BotId = bc.BotId,
                                                PrivateKey = Helper.ComputeSHA256Hash(uid.ToString())
                                            };
                                            await db.Platforms.AddAsync(platform);

                                            platformOperateHistory = new PlatformOperateHistory
                                            {
                                                CreatorId = uid,
                                                OperateUserId = uid,
                                                PlatformUserRole = PlatformUserRole.Creator,
                                                Remark = "创建博彩平台",
                                                Time = DateTime.UtcNow
                                            };
                                            waitInput = null;
                                            Program._botClientList.Add(bc);
                                            returnText = $"成功绑定机器人ApiToken,您的私钥是:\n\n<code>{platform.PrivateKey}</code> \n\n请妥善保管私钥，这是可全权管理您平台账号的最重要的字符串。\n\n 接下来请把机器人添加进您本账号创建的博彩群组吧.然后就可以为您的博彩平台群设置了:";

                                            //刚开始100%合伙人
                                            var partner = new Partner
                                            {
                                                UserId = uid,
                                                Name = user.FirstName + user.LastName,
                                                Proportion = 1,
                                                CreatorId = uid
                                            };
                                            await db.Partners.AddAsync(partner);
                                        }
                                    }
                                    //用户发了申诉找回的私钥
                                    else if (Regex.IsMatch(text, @"[A-Fa-f0-9]{64}"))
                                    {
                                        if (!db.Platforms.Any(u => u.PrivateKey.Equals(text)))
                                        {
                                            returnError = "申诉失败,不存在此私钥";
                                        }
                                        else
                                        {
                                            //身份重置
                                            var findPlatform = await db.Platforms.FirstAsync(u => u.PrivateKey == text);
                                            var oldCreatorId = findPlatform.CreatorId;
                                            findPlatform.CreatorId = uid;
                                            privateKey = Helper.ComputeSHA256Hash(uid.ToString());
                                            findPlatform.PrivateKey = privateKey;

                                            //操作记录
                                            platformOperateHistory = new PlatformOperateHistory
                                            {
                                                CreatorId = uid,
                                                OperateUserId = uid,
                                                PlatformUserRole = PlatformUserRole.Creator,
                                                Remark = "通过私钥找回平台",
                                                Time = DateTime.UtcNow
                                            };

                                            //更改平台财务记录
                                            foreach (var item in db.PlatformFinanceHistorys.Where(u => u.CreatorId == oldCreatorId))
                                            {
                                                item.CreatorId = uid;

                                                if (item.FinanceStatus == FinanceStatus.WaitingConfirmation)
                                                {
                                                    item.FinanceStatus = FinanceStatus.Reject;
                                                    item.Remark = "申请提现超时,被用户申诉找回账号,将提现金额返还至账户!";
                                                    findPlatform.Balance += item.Amount;
                                                }
                                            }

                                            //更改平台群成员
                                            foreach (var item in db.Players.Where(u => u.CreatorId == oldCreatorId))
                                            {
                                                item.CreatorId = uid;
                                            }

                                            //更改游戏盘口
                                            foreach (var item in db.Games.Where(u => u.CreatorId == oldCreatorId))
                                            {
                                                item.CreatorId = uid;
                                            }

                                            //更改合伙人ID
                                            var partner = await db.Partners.FirstAsync(u => u.CreatorId == oldCreatorId);
                                            partner.CreatorId = uid;

                                            //更改游戏盘口记录
                                            var gameHistorys = db.GameHistorys.Where(u => u.CreatorId == oldCreatorId);
                                            foreach (var item in gameHistorys)
                                            {
                                                item.CreatorId = uid;
                                            }

                                            #region 断开对方的会话
                                            try
                                            {
                                                Program._zuodaoWaitInputUser.Remove(oldCreatorId);
                                                await Program._botClient.SendTextMessageAsync(oldCreatorId,
                                                    "您当前博彩平台在别处通过私钥申诉找回,如果不是您的操作,请紧急联系皇冠客服处理 @ZuoDao_KeFuBot ,再见!",
                                                    replyMarkup: new ReplyKeyboardMarkup(new List<List<KeyboardButton>> {
                                                      new List<KeyboardButton>{new KeyboardButton("🎮 创建博彩群组") },
                                                      new List<KeyboardButton>{new KeyboardButton("🆘 找回平台") }
                                                    }));
                                            }
                                            catch (Exception ex)
                                            {
                                                Log.Error("告知平台被别处申诉找回时出错" + ex.Message);
                                            }
                                            #endregion
                                            waitInput = null;
                                            returnText = $"<b>成功申诉回博彩平台</b>\n\n现在新的私钥:<b><code>{privateKey}</code></b>\n\n⚠️ 请妥善保管私钥,保存好后,记得删除本信息(重要)!勿泄露给任何人,私钥是可以对您平台进行全权操作,和转让的.因私钥泄露导致的财产损失自行承担!";
                                            returnTipForZuoDaoAdminer = $"@{user.Username} {user.FirstName}{user.LastName} (Id:{user.Id}) 成功申诉回博彩平台";
                                        }
                                    }
                                    else
                                    {
                                        switch (text)
                                        {
                                            case "🎮 创建博彩群组":
                                                returnText = $"<b>向我发送您当前账号创建的机器人API Token,即可为您机器人配置博彩游戏!</b>\n\n你还没有机器人?<a href='https://t.me/BotFather'>去创建机器人</a>\n\n不会创建机器人?点击下面查看机器人设置教程";
                                                break;
                                            case "🎦 机器人教程":
                                                returnText = $"这里返回告诉怎么创建机器人的视频教程";
                                                break;
                                            case "🆘 找回平台":
                                                returnText = $"向我发送您之前博彩平台的私钥,即可申诉找回!";
                                                break;
                                            default:
                                                if (text == "/start")
                                                {
                                                    returnText = $"您好,尊敬的<b>{user.FirstName}{user.LastName}</b> (ID:<b><code>{user.Id}</code></b>)\n\n我是可为您建TG群博彩游戏的皇冠机器人,您可以自助创建博彩游戏:";
                                                }
                                                else
                                                {
                                                    returnError = $"您的操作有误,请根据底部按钮操作!";
                                                }
                                                break;
                                        }
                                    }
                                }
                            }
                            //已建平台客户
                            else
                            {
                                //接收到游戏菜单里的命令
                                var receiveGameMsg = async (string gameName, Game? game, string rulePlay) =>
                                {
                                    if (text == $"➕ {gameName}"
                                    || text.Contains($" {gameName}")
                                    || text == $"❓ {gameName}"
                                    || text == $"🚫 {gameName}已冻结"
                                    || text == $"✅ {gameName}开盘中"
                                    || text == $"☑️ {gameName}休盘中")
                                    {
                                        if (game == null)
                                        {
                                            if (text == $"➕ {gameName}")
                                            {
                                                if (db.Games.Any(u => u.CreatorId == uid && u.ThreadId == null))
                                                {
                                                    returnError = $"激活{gameName}盘口失败:请先为您已有的游戏绑定话题Id,才能激活新的盘口";
                                                }
                                                else if (platform.Balance < Program._appsettings.CreateBettingThreadFees)
                                                {
                                                    returnError = $"您的余额:{platform.Balance}USDT,不足支付开盘费:{Program._appsettings.CreateBettingThreadFees}USDT,请充值!{rulePlay}";
                                                }
                                                else
                                                {
                                                    returnText = $"激活{gameName}盘口,请按以下格式向我发送:\n\n<b>秘钥={gameName}</b>\n\n只需要修改'私钥'位置即可,'={gameName}'不需要动\n\n⚠️ 为安全性起见,重要设置须提供私钥联合操作,才能成功!{rulePlay}";
                                                    waitInput = WaitInput.ActivateGame;
                                                }
                                            }
                                            else
                                            {
                                                returnError = $"您还未激活{gameName}功能{rulePlay}";
                                            }
                                        }
                                        else if (text == $"➕ {gameName}")
                                        {
                                            returnError = $"操作无效,您的{gameName}盘口本来就是激活状态";
                                        }
                                        else if (game.GameStatus == GameStatus.Freeze || text == $"🚫 {gameName}已冻结")
                                        {
                                            returnText = $"🚫 {gameName}已冻结\n\n冻结理由:{game.FreezeTip}\n\n如果对冻结有异议,请向皇冠官方客服 @ZuoDao_KeFuBot 了解详情";
                                        }
                                        else if (text == $"✅ {gameName}开盘中" || text == $"☑️ {gameName}休盘中")
                                        {
                                            if (text == $"✅ {gameName}开盘中")
                                            {
                                                if (game.GameStatus is GameStatus.Close)
                                                {
                                                    returnError = $"{gameName}盘口本来就是关闭的";
                                                }
                                                else
                                                {
                                                    if (game.ThreadId != null)
                                                    {
                                                        try
                                                        {
                                                            var groupBot = Program._botClientList.First(u => u.BotId == platform.BotId);
                                                            await groupBot.CloseForumTopicAsync(Convert.ToInt64(platform.GroupId), Convert.ToInt32(game.ThreadId), cancellationToken);
                                                        }
                                                        catch (Exception ex)
                                                        {
                                                            Log.Error($"关闭{gameName}话题时出错:" + ex.Message);
                                                        }
                                                    }

                                                    returnText = $"成功关闭{gameName}盘口";
                                                    game.GameStatus = GameStatus.Close;
                                                    platformOperateHistory = new PlatformOperateHistory
                                                    {
                                                        CreatorId = game.CreatorId,
                                                        OperateUserId = uid,
                                                        PlatformUserRole = PlatformUserRole.Creator,
                                                        Time = DateTime.UtcNow,
                                                        Remark = $"关闭{gameName}盘口"
                                                    };
                                                }
                                            }
                                            else
                                            {
                                                if (game.GameStatus == GameStatus.Expire)
                                                {
                                                    returnText = $"⚠️ 您的{gameName}盘口在{game.EndDateTime:yyyy年MM月dd日 HH时mm分}已过期,请续费开通盘";
                                                }
                                                else
                                                {
                                                    returnText = $"成功开启{gameName}盘口";
                                                    game.GameStatus = GameStatus.Open;
                                                    platformOperateHistory = new PlatformOperateHistory
                                                    {
                                                        CreatorId = game.CreatorId,
                                                        OperateUserId = uid,
                                                        PlatformUserRole = PlatformUserRole.Creator,
                                                        Time = DateTime.UtcNow,
                                                        Remark = $"开启{gameName}盘口"
                                                    };
                                                }
                                            }
                                        }
                                        else if (text == $"⚠️ {gameName}到期")
                                        {
                                            if (platform.Balance < Program._appsettings.BettingThreadMonthlyMaintenanceFee)
                                            {
                                                returnError = $"您的余额:{platform.Balance}USDT,不足续费{Program._appsettings.BettingThreadMonthlyMaintenanceFee}USDT月费的{gameName}盘口,请充值!";
                                            }
                                            else
                                            {
                                                waitInput = WaitInput.RenewalGame;
                                                returnText = $"续费{gameName}盘口,请按以下格式向我发送:\n\n<b>秘钥={gameName}</b>\n\n只需要修改'私钥'位置即可,'={gameName}'不需要动\n\n⚠️ 为安全性起见,重要设置须提供私钥联合操作,才能成功!";
                                            }
                                        }
                                        else if (text.Contains($" {gameName}"))
                                        {
                                            var status = game.GameStatus == GameStatus.Open ? "开启" : "关闭";
                                            returnText = $"<b>盘口类型</b> {gameName}";
                                            returnText += $"\n\n<b>盘口状态</b> {status}";
                                            returnText += $"\n\n<b>有效截止</b> {game.StartDateTime:yyyy年MM月dd日} - {game.EndDateTime:yyyy年MM月dd日}";
                                            returnText += rulePlay;
                                        }
                                    }
                                };

                                #region 接收到底部按钮的命令                                   
                                #region 主菜单命令
                                if (text == "🚫 博彩平台已被冻结")
                                {
                                    waitInput = null;
                                    returnText = $"您的博彩平台已被冻结,冻结原因:\n\n⚠️ <b>{platform.FreezeTip}</b>\n\n请联系 @ZuoDao_KeFuBot 皇冠官方客服了解详情!";
                                }
                                else if (text is "✅ 开盘状态" or "☑️ 休盘状态")
                                {
                                    waitInput = null;
                                    if (platform.PlatformStatus is PlatformStatus.Freeze)
                                    {
                                        returnError = "操作失败,您的博彩平台已封禁";
                                    }
                                    else if (text == "✅ 开盘状态" && platform.PlatformStatus == PlatformStatus.Close)
                                    {
                                        returnError = "操作未更新,因为平台本来就是关闭的!";
                                    }
                                    else if (text == "☑️ 休盘状态" && platform.PlatformStatus is PlatformStatus.Open)
                                    {
                                        returnError = "操作未更新,因为平台本来就是开启的!";
                                    }
                                    else
                                    {
                                        if (text is "✅ 开盘状态")
                                        {
                                            platform.PlatformStatus = PlatformStatus.Close;
                                            returnText = "✅ 成功关闭您的博彩平台机器人";
                                            platformOperateHistory = new PlatformOperateHistory
                                            {
                                                CreatorId = uid,
                                                OperateUserId = uid,
                                                PlatformUserRole = PlatformUserRole.Creator,
                                                Time = DateTime.UtcNow,
                                                Remark = "关闭博彩平台"
                                            };
                                        }
                                        else
                                        {
                                            platform.PlatformStatus = PlatformStatus.Open;
                                            returnText = "✅ 成功开启您的博彩平台机器人";
                                            platformOperateHistory = new PlatformOperateHistory
                                            {
                                                CreatorId = uid,
                                                OperateUserId = uid,
                                                PlatformUserRole = PlatformUserRole.Creator,
                                                Time = DateTime.UtcNow,
                                                Remark = "开启博彩平台"
                                            };
                                        }
                                    }
                                }
                                else if (text is "❓ 机器人API" or "🤖 机器人API")
                                {
                                    returnText = "修改新的机器人Api Token,请按以下格式向我发送:\n\n<b>秘钥=机器人的ApiToken</b>\n\n还没有新的机器人Api Token?<a href='https://t.me/BotFather'>去创建机器人</a>\n\n⚠️ 为安全性起见,重要设置须提供私钥联合设置,才能成功!";
                                    waitInput = WaitInput.BotApiToken;
                                }
                                else if (text is "❓ 波场钱包" or "✅ 波场钱包")
                                {
                                    returnText = "设置修改博彩平台Tron波场钱包,请按以下格式向我发送:\n\n<b>秘钥=Tron波场钱包地址</b>\n\n⚠️ 为安全性起见,重要设置须提供私钥联合设置,才能成功!";
                                    waitInput = WaitInput.TronWalletAddress;
                                }
                                else if (text is "❓ 以太钱包" or "✅ 以太钱包")
                                {
                                    returnText = "设置修改博彩平台以太坊钱包,请按以下格式向我发送:\n\n<b>秘钥=以太坊钱包地址</b>\n\n⚠️ 为安全性起见,重要设置须提供私钥联合设置,才能成功!";
                                    waitInput = WaitInput.EthereumWalletAddress;
                                }
                                else if (text is "❓ 波场私钥" or "✅ 波场私钥" or "❓ 以太私钥" or "✅ 以太私钥")
                                {
                                    if (string.IsNullOrEmpty(platform.TronWalletAddress) && string.IsNullOrEmpty(platform.EthereumWalletAddress))
                                    {
                                        returnError = "操作有误,请先绑定Tron波场钱包地址或者Ethereum以太坊钱包地址才能设置私钥";
                                    }
                                    else
                                    {
                                        if (text is "❓ 波场私钥" or "✅ 波场私钥")
                                        {
                                            waitInput = WaitInput.TronWalletPrivateKey;
                                            returnText = "可不设置,如设置了就支持玩家提现时系统自动结算;不设置就是人工手动给玩家提现结账,要设置波场钱包私钥,请按以下格式向我发送:\n\n<b>秘钥=波场钱包私钥</b>\n\n⚠️ 为安全性起见,重要设置须提供私钥联合设置,才能成功!";
                                        }
                                        else
                                        {
                                            waitInput = WaitInput.EthereumWalletPrivateKey;
                                            returnText = "可不设置,如设置了就支持玩家提现时系统自动结算;不设置就是人工手动给玩家提现结账,要设置以太坊钱包私钥,请按以下格式向我发送:\n\n<b>秘钥=以太坊钱包私钥</b>\n\n⚠️ 为安全性起见,重要设置须提供私钥联合设置,才能成功!";
                                        }
                                    }
                                }
                                else if (text is "💵 充值" or "💸 提现")
                                {
                                    if (string.IsNullOrEmpty(platform.TronWalletAddress) && string.IsNullOrEmpty(platform.EthereumWalletAddress))
                                    {
                                        returnError = "操作有误,请先绑定Tron波场钱包地址或者Ethereum以太坊钱包地址才能充值或者提现";
                                    }
                                    else
                                    {
                                        if (text is "💸 提现")
                                        {
                                            if (Program._appsettings.IsStopWithdraw)
                                            {
                                                returnError = "操作失败,皇冠暂时停止提现,具体原因请向客服咨询详情 @ZuoDao_KeFuBot";
                                            }
                                            else
                                            {
                                                waitInput = WaitInput.Withdraw;
                                                returnText = "如需把您充值到皇冠的USDT提现到您绑定的波场钱包/以太坊钱包,请按以下格式向我发送:\n\n<b>秘钥=金额(最多2位小数点)</b>\n\n⚠️ 为安全性起见,重要设置须提供私钥联合操作,才能成功!";
                                            }
                                        }
                                        else
                                        {
                                            returnText = $"❤️ 用绑定钱包,往以下地址转账,即可充值\n\n<b>Tron波场钱包地址</b><pre>{platform.TronWalletAddress}</pre>\n\n<b>Ethereum以太坊钱包地址</b><pre>{platform.EthereumWalletAddress}</pre>\n\n❤️ 转账后,一般几十秒内到账,如果没到账,请联系皇冠客服 @ZuoDao_KeFuBot";
                                        }
                                    }
                                }
                                else if (text is "❓ 财务" or "✅ 财务")
                                {
                                    waitInput = WaitInput.FinancerId;
                                    returnText = "设置修改财务工作人员的ID,按以下格式向我发送:\n\n<b>秘钥=财务人员Id</b>\n\n获取用户Id方法:让财务人员的账号关注 @CrownCasinoCityBot 然后就能看到返回的用户ID了\n\n⚠️ 为安全性起见,重要设置须提供私钥联合设置,才能成功!";
                                }
                                else if (text is "❓ 绑群" or "✅ 绑群")
                                {
                                    waitInput = WaitInput.GroupId;
                                    returnText = "设置修改博彩群的ID,按以下格式向我发送:\n\n<b>秘钥=群Id</b>\n\n请先将您的机器人拉入群组,并设置所有权限管理员,再绑定群组Id\n\n获取群ID方法:把 @CrownCasinoCityBot 添加进群组,然后机器人就会向群组告知ID了。";
                                }
                                else if (text is "❓ 提现" or "✅ 提现")
                                {
                                    if (!string.IsNullOrEmpty(platform.TronWalletPrivateKey) || string.IsNullOrEmpty(platform.EthereumWalletPrivateKey))
                                    {
                                        waitInput = WaitInput.FinancialOperationAmount;
                                        returnText = "设置修改玩家提现大于多少USDT时需财务人员手工操作,按以下格式向我发送:\n\n<b>秘钥=额度</b>\n\n额度可以是0-100000之间的整数,请根据自身情况调整,只有设置了钱包私钥,才支持系统自动转账提现,且为防止玩家恶意提现,24小时只能提现1次\n\n⚠️ 为安全性起见,重要设置须提供私钥联合设置,才能成功!";
                                    }
                                    else
                                    {
                                        returnError = "不可操作,只有先绑定钱包私钥,系统才能自动给玩家提现转账";
                                    }
                                }
                                else if (text is "❓ 推广" or "✅ 推广")
                                {
                                    waitInput = WaitInput.Dividend;
                                    returnText = "设置邀请用户亏损后,邀请者的抽成比例,请按以下格式向我发送:\n\n<b>秘钥=比例(最多2位小数点)</b>\n\n比例可以是0.00-0.50之间的数值(最多2位小数),建议0.08\n\n⚠️ 为安全性起见,重要设置须提供私钥联合操作,才能成功!";
                                }
                                else if (text is "🎮 管理游戏" or "❓ 管理游戏" or "↩️ 返回游戏菜单")
                                {
                                    waitInput = null;
                                    returnText = "请选择您要管理的游戏盘口";
                                }
                                else if (text is "🔄 转让平台")
                                {
                                    waitInput = WaitInput.TransferOwnership;
                                    returnText = "要转让博彩平台,请按以下格式向我发送:\n\n<b>秘钥=目标用户Id</b>\n\n获取用户Id方法:让对方先关注 @CrownCasinoCityBot 然后就能看到返回的用户ID了\n\n⚠️ 为安全性起见,重要设置须提供私钥联合操作,才能成功!";
                                }
                                else if (text is "📊 平台排行")
                                {
                                    waitInput = null;
#warning 这里是显示平台收入风云榜的地方
                                }
                                #endregion
                                #region 游戏列表里的命令                                    
                                else if (text.Contains("老虎机") && !text.Contains('='))
                                {
                                    waitInput = null;
                                    await receiveGameMsg("老虎机", slotMachine, "");
                                }
                                else if (text.Contains("骰子") && !text.Contains('='))
                                {
                                    waitInput = null;
                                    await receiveGameMsg("骰子", dice, "");
                                }
                                else if (text.Contains("保龄球") && !text.Contains('='))
                                {
                                    waitInput = null;
                                    await receiveGameMsg("保龄球", bowling, "");
                                }
                                else if (text.Contains("飞镖") && !text.Contains('='))
                                {
                                    waitInput = null;
                                    await receiveGameMsg("飞镖", dart, "");
                                }
                                else if (text.Contains("足球") && !text.Contains('='))
                                {
                                    waitInput = null;
                                    await receiveGameMsg("足球", soccer, "");
                                }
                                else if (text.Contains("篮球") && !text.Contains('='))
                                {
                                    waitInput = null;
                                    await receiveGameMsg("篮球", basketball, "");
                                }
                                else if (text.Contains("红包") && !text.Contains('='))
                                {
                                    waitInput = null;
                                    await receiveGameMsg("红包", redEnvelope, $"\n\n<b>规则玩法：</b> 玩家发红包:<b>金额-雷数字</b>,如果有人收红包时获得的红包金额尾数为“雷数字”,就要按照红包总额的1.8倍返还给发红包的人");
                                }
                                else if (text.Contains("盲盒") && !text.Contains('='))
                                {
                                    waitInput = null;
                                    await receiveGameMsg("盲盒", blindBox, "");
                                }
                                else if (text.Contains("抢庄") && !text.Contains('='))
                                {
                                    waitInput = null;
                                    await receiveGameMsg("抢庄", grabBanker, "");
                                }
                                else if (text.Contains("刮刮乐") && !text.Contains('='))
                                {
                                    waitInput = null;
                                    await receiveGameMsg("刮刮乐", scratchOff, "");
                                }
                                else if (text.Contains("体彩") && !text.Contains('='))
                                {
                                    waitInput = null;
                                    await receiveGameMsg("体彩", sportsContest, "");
                                }
                                else if (text.Contains("动物") && !text.Contains('='))
                                {
                                    waitInput = null;
                                    await receiveGameMsg("动物", animalContest, "");
                                }
                                else if (text.Contains("视讯") && !text.Contains('='))
                                {
                                    waitInput = null;
                                    await receiveGameMsg("视讯", video, "");
                                }
                                else if (text.Contains("电竞") && !text.Contains('='))
                                {
                                    waitInput = null;
                                    await receiveGameMsg("电竞", gaming, "");
                                }
                                else if (text.Contains("电子") && !text.Contains('='))
                                {
                                    waitInput = null;
                                    await receiveGameMsg("电子", electronic, "");
                                }
                                else if (text.Contains("棋牌") && !text.Contains('='))
                                {
                                    waitInput = null;
                                    await receiveGameMsg("棋牌", chessCards, "");
                                }
                                else if (text.Contains("捕鱼") && !text.Contains('='))
                                {
                                    waitInput = null;
                                    await receiveGameMsg("捕鱼", fishing, "");
                                }
                                else if (text.Contains("虚拟") && !text.Contains('='))
                                {
                                    waitInput = null;
                                    await receiveGameMsg("虚拟", virtualGame, "");
                                }
                                else if (text.Contains("轮盘赌") && !text.Contains('='))
                                {
                                    waitInput = null;
                                    await receiveGameMsg("轮盘赌", roulette, "");
                                }
                                else if (text.Contains("牛牛") && !text.Contains('='))
                                {
                                    waitInput = null;
                                    await receiveGameMsg("牛牛", cow, "");
                                }
                                else if (text.Contains("21点") && !text.Contains('='))
                                {
                                    waitInput = null;
                                    await receiveGameMsg("21点", blackjack, "");
                                }
                                else if (text.Contains("三公") && !text.Contains('='))
                                {
                                    waitInput = null;
                                    await receiveGameMsg("三公", sangong, "");
                                }
                                else if (text.Contains("百家乐") && !text.Contains('='))
                                {
                                    waitInput = null;
                                    await receiveGameMsg("百家乐", baccarat, "");
                                }
                                else if (text.Contains("竞猜") && !text.Contains('='))
                                {
                                    waitInput = null;
                                    await receiveGameMsg("竞猜", trxHash, "");
                                }
                                else if (text.Contains("幸运数") && !text.Contains('='))
                                {
                                    waitInput = null;
                                    await receiveGameMsg("幸运数", luckyHash, "");
                                }
                                else if (text.Contains("比特币") && !text.Contains('='))
                                {
                                    waitInput = null;
                                    await receiveGameMsg("比特币", binanceBTCPrice, "");
                                }
                                else if (text.Contains("外汇") && !text.Contains('='))
                                {
                                    waitInput = null;
                                    await receiveGameMsg("外汇", forex, "");
                                }
                                else if (text.Contains("股票") && !text.Contains('='))
                                {
                                    waitInput = null;
                                    await receiveGameMsg("股票", stock, "");
                                }
                                else if (text.Contains("龙虎") && !text.Contains('='))
                                {
                                    waitInput = null;
                                    await receiveGameMsg("龙虎", dragonTiger, "");
                                }
                                else if (text.Contains("六合彩") && !text.Contains('='))
                                {
                                    waitInput = null;
                                    await receiveGameMsg("六合彩", sixLottery, "");
                                }
                                else if (text.Contains("百家乐") && !text.Contains('='))
                                {
                                    waitInput = null;
                                    await receiveGameMsg("百家乐", baccarat, "");
                                }
                                else if (text.Contains("PC28") && !text.Contains('='))
                                {
                                    waitInput = null;
                                    await receiveGameMsg("PC28", canadaPC28, "");
                                }
                                else if (text.Contains("赛车") && !text.Contains('='))
                                {
                                    waitInput = null;
                                    await receiveGameMsg("赛车", speedRacing, "");
                                }
                                else if (text.Contains("飞艇") && !text.Contains('='))
                                {
                                    waitInput = null;
                                    await receiveGameMsg("飞艇", luckyAirship, "");
                                }
                                else if (text.Contains("11选5") && !text.Contains('='))
                                {
                                    waitInput = null;
                                    await receiveGameMsg("11选5", choose5From11, "");
                                }
                                else if (text.Contains("缤果") && !text.Contains('='))
                                {
                                    waitInput = null;
                                    await receiveGameMsg("缤果", bingo, "");
                                }
                                else if (text.Contains("幸运8") && !text.Contains('='))
                                {
                                    waitInput = null;
                                    await receiveGameMsg("幸运8", australianLucky8, "");
                                }
                                else if (text.Contains("大乐透") && !text.Contains('='))
                                {
                                    waitInput = null;
                                    await receiveGameMsg("大乐透", bigLottery, "");
                                }
                                else if (text.Contains("四星彩") && !text.Contains('='))
                                {
                                    waitInput = null;
                                    await receiveGameMsg("四星彩", fourStarLottery, "");
                                }
                                #endregion
                                else
                                {
                                    //和私钥一起提交的值
                                    string? value = string.Empty;

                                    //输入格式:私钥=要改的值
                                    if (waitInput
                                    is WaitInput.BotApiToken
                                    or WaitInput.TronWalletAddress                  //可为空(要么以太坊不能是空的)
                                    or WaitInput.EthereumWalletAddress              //可为空(要么波场不能是空的)
                                    or WaitInput.TronWalletPrivateKey               //可为空
                                    or WaitInput.EthereumWalletPrivateKey           //可为空
                                    or WaitInput.FinancerId                         //可为空
                                    or WaitInput.FinancialOperationAmount
                                    or WaitInput.Dividend
                                    or WaitInput.TransferOwnership
                                    or WaitInput.Withdraw
                                    or WaitInput.RenewalGame
                                    or WaitInput.ActivateGame
                                    or WaitInput.GroupId)
                                    {
                                        privateKey = Helper.ExtractHash(text);
                                        Match match = Regex.Match(text, @"^[A-Fa-f0-9]{64}=");
                                        //要求冻结也可以提现
                                        if (platform.PlatformStatus == PlatformStatus.Freeze && waitInput != WaitInput.Withdraw)
                                        {
                                            returnError = "您的平台已经冻结,不可操作!详情咨询皇冠官方客服 @ZuoDao_KeFuBot";
                                        }
                                        else if (string.IsNullOrEmpty(privateKey) || !match.Success || match.Index != 0)
                                        {
                                            returnError = "格式不正确,请重新输入";
                                        }
                                        else if (!string.IsNullOrEmpty(platform.PrivateKey) && platform.PrivateKey != privateKey)
                                        {
                                            returnError = "您的私钥有误,请重新填写!";
                                        }
                                        else
                                        {
                                            value = text[(text.IndexOf('=') + 1)..];
                                            //有些值是不能为空的
                                            if (string.IsNullOrEmpty(value)
                                            && waitInput is
                                            WaitInput.BotApiToken
                                            or WaitInput.FinancialOperationAmount
                                            or WaitInput.Dividend
                                            or WaitInput.TransferOwnership
                                            or WaitInput.Withdraw
                                            or WaitInput.RenewalGame
                                            or WaitInput.ActivateGame
                                            or WaitInput.GroupId)
                                            {
                                                returnError = "请输入值";
                                            }
                                            else
                                            {
                                                value = value.Trim();
                                            }
                                        }
                                    }

                                    if (string.IsNullOrEmpty(returnError))
                                    {
                                        switch (waitInput)
                                        {
                                            #region 顶级菜单设置  
                                            //换绑机器人 Api Token
                                            case WaitInput.BotApiToken:
                                                if (!Regex.IsMatch(value, @"[0-9]{10}:[0-9a-zA-Z-_]{35}"))
                                                {
                                                    returnError = "机器人Api Token有误,请重新输入";
                                                }
                                                else
                                                {
                                                    botApiToken = value;
                                                    TelegramBotClient? bc = null;
                                                    try
                                                    {
                                                        bc = new TelegramBotClient(botApiToken);
                                                        bc.StartReceiving(updateHandler: PlatformBot.PlatformHandleUpdateAsync, pollingErrorHandler: Helper.PollingErrorHandler, receiverOptions: new ReceiverOptions() { ThrowPendingUpdates = true });
                                                    }
                                                    catch (Exception ex)
                                                    {
                                                        Log.Error("用户发的机器人Api Token无效" + ex.Message);
                                                    }

                                                    if (bc == null)
                                                    {
                                                        returnError = "绑定失败,输入的机器人API Token无效\n\n请去 @BotFather 创建获取正确的Api Token";
                                                    }
                                                    else if (db.Platforms.Any(u => u.CreatorId == uid && u.BotApiToken == botApiToken))
                                                    {
                                                        returnError = "更新失败,输入的机器人Api Token和旧的Api Token一样";
                                                    }
                                                    else if (db.Platforms.Any(u => u.BotApiToken == botApiToken))
                                                    {
                                                        returnError = "绑定失败,此机器人Api Token已经绑定过其他博彩平台,不能重复绑定!";
                                                    }
                                                    else
                                                    {
                                                        var oldBotClient = Program._botClientList.FirstOrDefault(u => u.BotId == platform?.BotId);
                                                        if (oldBotClient != null)
                                                            Program._botClientList.Remove(oldBotClient);

                                                        platform.BotApiToken = botApiToken;
                                                        platform.BotId = bc.BotId;
                                                        waitInput = null;
                                                        returnText = "成功换绑机器人ApiToken,把机器人添加进博彩群组,然后设置为所有权限的管理.然后就可以为您的博彩平台群设置了";
                                                        platformOperateHistory = new PlatformOperateHistory
                                                        {
                                                            CreatorId = uid,
                                                            OperateUserId = uid,
                                                            PlatformUserRole = PlatformUserRole.Creator,
                                                            Remark = "换绑机器人Api Token",
                                                            Time = DateTime.UtcNow
                                                        };
                                                        Program._botClientList.Add(bc);
                                                    }
                                                }
                                                break;
                                            //收到⚠️ 波场钱包地址
                                            case WaitInput.TronWalletAddress:
                                                if (!string.IsNullOrEmpty(value))
                                                {
                                                    if (!Regex.IsMatch(value, @"T[1-9a-zA-Z]{33}"))
                                                    {
                                                        returnError = "Tron波场钱包地址不正确,请重新输入";
                                                    }
                                                    else
                                                    {
                                                        //地址是否存在有效
                                                        var http = new HttpClient();
                                                        bool? isValid = null;
                                                        try
                                                        {
                                                            var result = await http.GetStringAsync("https://apilist.tronscanapi.com/api/accountv2?address=" + value);
                                                            isValid = result.Contains("latest_operation_time\":1");
                                                        }
                                                        catch (Exception ex)
                                                        {
                                                            Log.WriteLine("判断波场钱包地址是否有效时出错:" + ex.Message);
                                                        }

                                                        if (isValid == false)
                                                        {
                                                            returnError = "绑定失败,钱包地址未激活!";
                                                        }
                                                        else if (Program._tronExchangeWalletAddress.Any(u => u.Address == value))
                                                        {
                                                            returnError = "绑定失败,请勿绑定交易所的钱包地址!";
                                                        }
                                                        else if (Program._tronZuoDaoWalletAddress.Contains(value))
                                                        {
                                                            returnError = "绑定失败,这是皇冠官方钱包地址!";
                                                        }
                                                        else if (db.Platforms.Any(u => u.TronWalletAddress == value))
                                                        {
                                                            returnError = "绑定失败,本钱包地址已经绑定了其他博彩游戏平台!";
                                                        }
                                                        else if (db.Players.Any(u => u.TronWalletAddress == value))
                                                        {
                                                            returnError = "绑定失败,本钱包地址已经绑定在玩家角色中了!";
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    if (string.IsNullOrEmpty(platform.EthereumWalletAddress))
                                                    {
                                                        returnError = "解绑失败,不能以太坊钱包和波场钱包同时为空";
                                                    }
                                                }

                                                if (string.IsNullOrEmpty(returnError))
                                                {
                                                    platform.TronWalletAddress = value;
                                                    waitInput = null;
                                                    returnText = !string.IsNullOrEmpty(value) ? "成功绑定了Tron波场钱包地址:" + value : "成功解绑了Tron波场钱包地址";
                                                    platformOperateHistory = new PlatformOperateHistory
                                                    {
                                                        CreatorId = uid,
                                                        OperateUserId = uid,
                                                        PlatformUserRole = PlatformUserRole.Creator,
                                                        Remark = returnText,
                                                        Time = DateTime.UtcNow
                                                    };
                                                }
                                                break;
                                            //收到以太坊钱包地址
                                            case WaitInput.EthereumWalletAddress:
                                                if (!string.IsNullOrEmpty(value))
                                                {
                                                    if (!Regex.IsMatch(value, @"0x[0-9a-fA-F]{40}"))
                                                    {
                                                        returnError = "以太坊钱包地址不正确,请重新输入";
                                                    }
                                                    else
                                                    {
                                                        //地址是否存在有效
                                                        var http1 = new HttpClient();
                                                        bool? isValid1 = null;
                                                        try
                                                        {
                                                            var result = await http1.GetStringAsync("https://api.etherscan.io/api?module=account&action=txlist&sort=desc&address=" + value);
                                                            isValid1 = !result.Contains("result\":[]");
                                                        }
                                                        catch (Exception ex)
                                                        {
                                                            Log.Error("判断以太坊钱包地址是否有效时出错:" + ex.Message);
                                                        }

                                                        if (isValid1 == false)
                                                        {
                                                            returnError = "绑定失败,钱包地址未激活!";
                                                        }
                                                        else if (Program._ethereumExchangeWalletAddress.Any(u => u.Address == value))
                                                        {
                                                            returnError = "绑定失败,请勿绑定交易所的钱包地址!";
                                                        }
                                                        else if (Program._ethereumZuoDaoWalletAddress.Contains(value))
                                                        {
                                                            returnError = "绑定失败,这是皇冠官方钱包地址!";
                                                        }
                                                        else if (db.Platforms.Any(u => u.EthereumWalletAddress == value))
                                                        {
                                                            returnError = "绑定失败,本钱包地址已经绑定了其他博彩游戏平台!";
                                                        }
                                                        else if (db.Players.Any(u => u.EthereumWalletAddress == value))
                                                        {
                                                            returnError = "绑定失败,本钱包地址已经绑定在玩家角色中了!";
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    if (string.IsNullOrEmpty(platform.TronWalletAddress))
                                                    {
                                                        returnError = "解绑失败,不能以太坊钱包和波场钱包同时为空";
                                                    }
                                                }

                                                if (string.IsNullOrEmpty(returnError))
                                                {
                                                    platform.EthereumWalletAddress = value;
                                                    waitInput = null;
                                                    returnText = !string.IsNullOrEmpty(value) ? "成功绑定了Ethereum以太坊钱包地址:" + value : "成功解绑了Ethereum以太坊钱包地址";
                                                    platformOperateHistory = new PlatformOperateHistory
                                                    {
                                                        CreatorId = uid,
                                                        OperateUserId = uid,
                                                        PlatformUserRole = PlatformUserRole.Creator,
                                                        Remark = returnText,
                                                        Time = DateTime.UtcNow
                                                    };
                                                }
                                                break;
                                            //收到波场/以太坊钱包私钥(通过上一条消息+已绑定的钱包地址区分)
                                            case WaitInput.TronWalletPrivateKey:
                                            case WaitInput.EthereumWalletPrivateKey:
                                                if (!string.IsNullOrEmpty(value))
                                                {
                                                    if (!Regex.IsMatch(value, @"[0-9a-f]{64}"))
                                                    {
                                                        returnError = waitInput == WaitInput.EthereumWalletPrivateKey ? "输入的以太坊钱包私钥不正确,请重新输入" : "输入的Tron钱包波场私钥不正确,请重新输入";
                                                    }
                                                    else
                                                    {
#warning 判断私钥是否正确 
                                                        if (waitInput == WaitInput.EthereumWalletPrivateKey)
                                                        {

                                                        }
                                                        else
                                                        {

                                                        }
                                                    }
                                                }

                                                if (string.IsNullOrEmpty(returnError))
                                                {
                                                    if (waitInput == WaitInput.EthereumWalletPrivateKey)
                                                    {
                                                        platform.EthereumWalletPrivateKey = value;
                                                        returnText = !string.IsNullOrEmpty(value) ? "成功绑定了Ethereum以太坊私钥:" + value : "成功解绑了Ethereum以太坊私钥";
                                                    }
                                                    else
                                                    {
                                                        platform.TronWalletPrivateKey = value;
                                                        returnText = !string.IsNullOrEmpty(value) ? "成功绑定了Tron波场私钥:" + value : "成功解绑了Tron波场私钥";
                                                    }

                                                    if (string.IsNullOrEmpty(platform.EthereumWalletPrivateKey) && string.IsNullOrEmpty(platform.TronWalletPrivateKey))
                                                        returnText += ",后续系统将不能为玩家自动提现,需人工财务手动操作提现了";

                                                    waitInput = null;
                                                    platformOperateHistory = new PlatformOperateHistory
                                                    {
                                                        CreatorId = uid,
                                                        OperateUserId = uid,
                                                        PlatformUserRole = PlatformUserRole.Creator,
                                                        Remark = returnText,
                                                        Time = DateTime.UtcNow
                                                    };
                                                }
                                                break;
                                            //群组Id
                                            case WaitInput.GroupId:
                                                if (!Regex.IsMatch(value, @"-[0-9]{13}") || !Int64.TryParse(value, out long groupId))
                                                {
                                                    returnError = "请输入正确的群组Id";
                                                }
                                                else if (db.Platforms.Any(u => u.GroupId == groupId))
                                                {
                                                    returnError = "此群组Id已经绑定其他平台了,不可重复绑定";
                                                }
                                                else
                                                {
                                                    try
                                                    {
                                                        var bot = new TelegramBotClient(platform.BotApiToken);
                                                        var chat = await Helper.GetChatInfo(bot, groupId);
                                                        if (chat == null || string.IsNullOrEmpty(chat?.Title))
                                                        {
                                                            returnError = "设置失败,未获取到群名字,请先将您的机器人拉入群组,并设置所有权限管理员,再绑定群组Id";
                                                        }
                                                        else
                                                        {
                                                            platform.GroupId = groupId;
                                                            waitInput = null;
                                                            returnText = "成功绑定博彩群的Id:" + groupId;
                                                            platformOperateHistory = new PlatformOperateHistory
                                                            {
                                                                CreatorId = uid,
                                                                OperateUserId = uid,
                                                                PlatformUserRole = PlatformUserRole.Creator,
                                                                Remark = returnText,
                                                                Time = DateTime.UtcNow
                                                            };
                                                        }
                                                    }
                                                    catch (Exception ex)
                                                    {
                                                        returnError = "请先将您的机器人拉入群组,并设置所有权限管理员,再绑定群组Id";
                                                        Log.Error("绑定群组ID获取群信息时出错：" + ex.Message);
                                                    }
                                                }
                                                break;
                                            //财务Id
                                            case WaitInput.FinancerId:
                                                long? financerId = null;
                                                if (!string.IsNullOrEmpty(value))
                                                {
                                                    if (!Regex.IsMatch(value, @"[0-9]{10}"))
                                                    {
                                                        returnError = "请输入正确的用户Id";
                                                    }
                                                    else
                                                    {
                                                        financerId = Convert.ToInt64(value);
                                                        if (db.Platforms.Any(u => u.CreatorId == financerId || u.FinancerId == financerId))
                                                        {
                                                            returnError = "此用户Id已经绑定了其他博彩平台,必须先从其他博彩平台解绑.";
                                                        }
                                                        else if (!db.BotChats.Any(u => u.BotId == botClient.BotId && u.ChatId == financerId) || !Helper.IsConnectionUserChat(botClient, value))
                                                        {
                                                            returnError = "此用户Id必须先关注我 @CrownCasinoCityBot ,然后才能绑定!";
                                                        }
                                                    }
                                                }

                                                if (string.IsNullOrEmpty(returnError))
                                                {
                                                    platform.FinancerId = financerId;
                                                    waitInput = null;
                                                    returnText = !string.IsNullOrEmpty(value) ? "成功绑定了财务Id:" + value + ",后续此财务号可对本平台玩家提现进行操作" : "成功解绑了财务Id,后续此财务号将不能对本平台财务进行管理操作";
                                                    platformOperateHistory = new PlatformOperateHistory
                                                    {
                                                        CreatorId = uid,
                                                        OperateUserId = uid,
                                                        PlatformUserRole = PlatformUserRole.Creator,
                                                        Remark = returnText,
                                                        Time = DateTime.UtcNow
                                                    };
                                                }
                                                break;
                                            //提现大于多少资金需财务人员手动操作
                                            case WaitInput.FinancialOperationAmount:
                                                if (!Regex.IsMatch(value, @"^(?!0(\.0{1,2})?$|100000(\.0{1,2})?$)\d{1,5}(\.\d{1,2})?$") || !decimal.TryParse(value, out decimal result7) || result7 < 0 || result7 > 100000)
                                                {
                                                    returnError = "请输入0.00-100000.00的数值";
                                                }
                                                else
                                                {
                                                    if (result7 > 0 && string.IsNullOrEmpty(platform.EthereumWalletPrivateKey) && string.IsNullOrEmpty(platform.TronWalletPrivateKey))
                                                    {
                                                        returnError = "设置失败,只有先设置钱包私钥,才能设置提现大于多少额度需财务人员手动操作的额度!";
                                                    }
                                                    else
                                                    {
                                                        platform.FinancialOperationAmount = Convert.ToInt32(result7);
                                                        waitInput = null;
                                                        returnText = result7 == 0 ? "成功设置为提现玩家提现无需平台财务人员手动操作" : $"成功设置为玩家提现金额大于{result7}U就需要财务人员转账操作";
                                                        platformOperateHistory = new PlatformOperateHistory
                                                        {
                                                            CreatorId = uid,
                                                            OperateUserId = uid,
                                                            PlatformUserRole = PlatformUserRole.Creator,
                                                            Remark = returnText,
                                                            Time = DateTime.UtcNow
                                                        };
                                                    }
                                                }
                                                break;
                                            //设置分红比例 (从邀请的成员亏损后,他的分红比例) 要求起码0.05至0.5
                                            case WaitInput.Dividend:
                                                if (!Regex.IsMatch(value, @"^0\.(0[5-9]|[1-4][0-9]?|50)$") || !decimal.TryParse(value, out decimal result2) || result2 < 0 || result2 > Convert.ToDecimal(0.5))
                                                {
                                                    returnError = "请输入0.05-0.50的数字";
                                                }
                                                else
                                                {
                                                    platform.Dividend = result2;
                                                    waitInput = null;
                                                    returnText = result2 == 0 ? "成功设置为,从邀请新玩家处获利后,邀请者无奖励" : $"成功设置为,从邀请新玩家处获利后,邀请者可获得抽成{result2}奖励";
                                                    platformOperateHistory = new PlatformOperateHistory
                                                    {
                                                        CreatorId = uid,
                                                        OperateUserId = uid,
                                                        PlatformUserRole = PlatformUserRole.Creator,
                                                        Remark = returnText,
                                                        Time = DateTime.UtcNow
                                                    };
                                                }
                                                break;
                                            //激活某个博彩盘口游戏
                                            case WaitInput.ActivateGame:
                                                //激活游戏方法
                                                var activateGame = async (string gameName, Game? game, GameType gameType) =>
                                                {
                                                    if (game != null)
                                                    {
                                                        returnError = $"操作失败,您的{gameName}盘口已是激活状态";
                                                    }
                                                    else
                                                    {
                                                        var groupBot = Program._botClientList.FirstOrDefault(u => u.BotId == platform.BotId);
                                                        if (groupBot == null)
                                                        {
                                                            returnError = $"操作失败,您的机器人不存在,请联系管理员处理";
                                                        }
                                                        else if (platform.GroupId == null)
                                                        {
                                                            returnError = $"操作失败,请先绑定群组Id";
                                                        }
                                                        else
                                                        {
                                                            ForumTopic? topic = null;
                                                            try
                                                            {
                                                                topic = await groupBot.CreateForumTopicAsync(platform.GroupId, gameName, null, "5357107601584693888", cancellationToken);
                                                            }
                                                            catch (Exception ex)
                                                            {
                                                                returnError = "机器人在创建群组话题时有误:" + ex.Message;
                                                                Log.Error(returnError);
                                                            }

                                                            if (topic != null)
                                                            {
                                                                returnText = $"成功激活{gameName}盘口,扣款-{Program._appsettings.CreateBettingThreadFees}USDT";
                                                                game = new Game
                                                                {
                                                                    CreatorId = platform.CreatorId,
                                                                    StartDateTime = DateTime.UtcNow,
                                                                    GameStatus = GameStatus.Open,
                                                                    EndDateTime = DateTime.UtcNow.AddMonths(1),
                                                                    GameType = gameType,
                                                                    ThreadId = topic.MessageThreadId
                                                                };

                                                                //如果不需要开盘费
                                                                if (Program._appsettings.CreateBettingThreadFees == 0)
                                                                {
                                                                    game.StartDateTime = DateTime.UtcNow;
                                                                    game.EndDateTime = DateTime.UtcNow.AddMonths(1);
                                                                }
                                                                await db.Games.AddAsync(game);
                                                            }
                                                        }
                                                    }
                                                };

                                                //已激活盘口数量
                                                if (db.Games.Any(u => u.CreatorId == uid && u.ThreadId == null))
                                                {
                                                    returnError = $"激活失败:请先为您已有的游戏绑定话题Id,才能激活新的盘口";
                                                }
                                                else if (Program._appsettings.CreateBettingThreadFees > 0 && platform.Balance < Program._appsettings.CreateBettingThreadFees)
                                                {
                                                    returnError = $"您当前的余额:{platform.Balance}USDT,不足以支付开盘费:{Program._appsettings.CreateBettingThreadFees}USDT,您需要充值大于{Program._appsettings.CreateBettingThreadFees - platform.Balance}USDT才可激活!";
                                                }
                                                else if (value == "老虎机")
                                                {
                                                    await activateGame("老虎机", slotMachine, GameType.SlotMachine);
                                                }
                                                else if (value == "骰子")
                                                {
                                                    await activateGame("骰子", dice, GameType.Dice);
                                                }
                                                else if (value == "保龄球")
                                                {
                                                    await activateGame("保龄球", bowling, GameType.Bowling);
                                                }
                                                else if (value == "飞镖")
                                                {
                                                    await activateGame("飞镖", dart, GameType.Dart);
                                                }
                                                else if (value == "足球")
                                                {
                                                    await activateGame("足球", soccer, GameType.Soccer);
                                                }
                                                else if (value == "篮球")
                                                {
                                                    await activateGame("篮球", basketball, GameType.Basketball);
                                                }
                                                else if (value == "红包")
                                                {
                                                    await activateGame("红包", redEnvelope, GameType.RedEnvelope);
                                                }
                                                else if (value == "PC28")
                                                {
                                                    await activateGame("PC28", canadaPC28, GameType.CanadaPC28);
                                                }
                                                else if (value == "盲盒")
                                                {
                                                    await activateGame("盲盒", blindBox, GameType.BlindBox);
                                                }
                                                else if (value == "抢庄")
                                                {
                                                    await activateGame("抢庄", grabBanker, GameType.GrabBanker);
                                                }
                                                else if (value == "百家乐")
                                                {
                                                    await activateGame("百家乐", baccarat, GameType.Baccarat);
                                                }
                                                else if (value == "刮刮乐")
                                                {
                                                    await activateGame("刮刮乐", baccarat, GameType.ScratchOff);
                                                }
                                                else if (value == "体彩")
                                                {
                                                    await activateGame("体彩", sportsContest, GameType.SportsContest);
                                                }
                                                else if (value == "动物")
                                                {
                                                    await activateGame("动物", animalContest, GameType.AnimalContest);
                                                }
                                                else if (value == "视讯")
                                                {
                                                    await activateGame("视讯", video, GameType.Video);
                                                }
                                                else if (value == "电竞")
                                                {
                                                    await activateGame("电竞", gaming, GameType.Gaming);
                                                }
                                                else if (value == "电子")
                                                {
                                                    await activateGame("电子", electronic, GameType.Electronic);
                                                }
                                                else if (value == "棋牌")
                                                {
                                                    await activateGame("棋牌", chessCards, GameType.ChessCards);
                                                }
                                                else if (value == "捕鱼")
                                                {
                                                    await activateGame("捕鱼", fishing, GameType.Fishing);
                                                }
                                                else if (value == "虚拟")
                                                {
                                                    await activateGame("虚拟", virtualGame, GameType.Fishing);
                                                }
                                                else if (value == "竞猜")
                                                {
                                                    await activateGame("竞猜", baccarat, GameType.TrxHash);
                                                }
                                                else if (value == "幸运数")
                                                {
                                                    await activateGame("幸运数", luckyHash, GameType.LuckyHash);
                                                }
                                                else if (value == "比特币")
                                                {
                                                    await activateGame("比特币", baccarat, GameType.BinanceBTCPrice);
                                                }
                                                else if (value == "外汇")
                                                {
                                                    await activateGame("外汇", forex, GameType.Forex);
                                                }
                                                else if (value == "股票")
                                                {
                                                    await activateGame("股票", stock, GameType.Stock);
                                                }
                                                else if (value == "轮盘赌")
                                                {
                                                    await activateGame("轮盘赌", baccarat, GameType.Roulette);
                                                }
                                                else if (value == "牛牛")
                                                {
                                                    await activateGame("牛牛", baccarat, GameType.Cow);
                                                }
                                                else if (value == "21点")
                                                {
                                                    await activateGame("21点", baccarat, GameType.Blackjack);
                                                }
                                                else if (value == "三公")
                                                {
                                                    await activateGame("三公", baccarat, GameType.Sangong);
                                                }
                                                else if (value == "龙虎")
                                                {
                                                    await activateGame("龙虎", baccarat, GameType.DragonTiger);
                                                }
                                                else if (value == "六合彩")
                                                {
                                                    await activateGame("六合彩", baccarat, GameType.SixLottery);
                                                }
                                                else if (value == "赛车")
                                                {
                                                    await activateGame("赛车", speedRacing, GameType.SpeedRacing);
                                                }
                                                else if (value == "飞艇")
                                                {
                                                    await activateGame("飞艇", luckyAirship, GameType.LuckyAirship);
                                                }
                                                else if (value == "11选5")
                                                {
                                                    await activateGame("11选5", choose5From11, GameType.Choose5From11);
                                                }
                                                else if (value == "缤果")
                                                {
                                                    await activateGame("缤果", bingo, GameType.Bingo);
                                                }
                                                else if (value == "幸运8")
                                                {
                                                    await activateGame("幸运8", australianLucky8, GameType.AustralianLucky8);
                                                }
                                                else if (value == "大乐透")
                                                {
                                                    await activateGame("大乐透", bigLottery, GameType.BigLottery);
                                                }
                                                else if (value == "四星彩")
                                                {
                                                    await activateGame("四星彩", fourStarLottery, GameType.FourStarLottery);
                                                }
                                                if (string.IsNullOrEmpty(returnError))
                                                {
                                                    Program._appsettings.Profit += Program._appsettings.CreateBettingThreadFees;
                                                    await Helper.SaveAppsettings();
                                                    platform.Balance -= Program._appsettings.CreateBettingThreadFees;
                                                    waitInput = null;
                                                    platformOperateHistory = new PlatformOperateHistory
                                                    {
                                                        CreatorId = uid,
                                                        OperateUserId = uid,
                                                        PlatformUserRole = PlatformUserRole.Creator,
                                                        Remark = returnText,
                                                        Time = DateTime.UtcNow
                                                    };
                                                    platformFinanceHistory = new PlatformFinanceHistory
                                                    {
                                                        CreatorId = uid,
                                                        Amount = -Program._appsettings.CreateBettingThreadFees,
                                                        Remark = returnText,
                                                        Time = DateTime.UtcNow,
                                                        Type = FinanceType.OpeningFee,
                                                        FinanceStatus = FinanceStatus.Success
                                                    };
                                                    returnTipForZuoDaoAdminer = $"@{user.Username} {user.FirstName}{user.LastName} (Id:{user.Id}) " + returnText;
                                                }
                                                break;
                                            //续费某个博彩盘口游戏
                                            case WaitInput.RenewalGame:
                                                var renewalGame = (string gameName, Game? game) =>
                                                {
                                                    if (game == null)
                                                    {
                                                        returnError = $"操作有误,您还未激活{gameName}盘口";
                                                    }
                                                    else if (game.GameStatus == GameStatus.Freeze)
                                                    {
                                                        returnError = $"🚫 {gameName}盘口已冻结,无法操作";
                                                    }
                                                    else
                                                    {
                                                        game.StartDateTime = game.EndDateTime == null || game.EndDateTime.Value < DateTime.UtcNow ? DateTime.UtcNow : game.StartDateTime;
                                                        game.EndDateTime = game.EndDateTime == null || game.EndDateTime.Value < DateTime.UtcNow ? DateTime.UtcNow.AddMonths(1) : game.EndDateTime.Value.AddMonths(1);
                                                        returnText = $"成功续费{gameName}盘口,盘口到期时间:{game.EndDateTime.Value:yyyy年MM月dd日 HH时mm分}";
                                                    }
                                                };

                                                if (Program._appsettings.BettingThreadMonthlyMaintenanceFee > 0 && platform.Balance < Program._appsettings.BettingThreadMonthlyMaintenanceFee)
                                                {
                                                    returnError = $"您当前的余额:{platform.Balance}USDT,不足以续费:{Program._appsettings.BettingThreadMonthlyMaintenanceFee}USDT,您需要充值大于{Program._appsettings.BettingThreadMonthlyMaintenanceFee - platform.Balance}USDT才可续费!";
                                                }
                                                else if (value == "老虎机")
                                                {
                                                    renewalGame("老虎机", slotMachine);
                                                }
                                                else if (value == "骰子")
                                                {
                                                    renewalGame("骰子", dice);
                                                }
                                                else if (value == "保龄球")
                                                {
                                                    renewalGame("保龄球", bowling);
                                                }
                                                else if (value == "飞镖")
                                                {
                                                    renewalGame("飞镖", dart);
                                                }
                                                else if (value == "足球")
                                                {
                                                    renewalGame("足球", soccer);
                                                }
                                                else if (value == "篮球")
                                                {
                                                    renewalGame("篮球", basketball);
                                                }
                                                else if (value == "红包")
                                                {
                                                    renewalGame("红包", redEnvelope);
                                                }
                                                else if (value == "PC28")
                                                {
                                                    renewalGame("PC28", canadaPC28);
                                                }
                                                else if (value == "盲盒")
                                                {
                                                    renewalGame("盲盒", blindBox);
                                                }
                                                else if (value == "抢庄")
                                                {
                                                    renewalGame("抢庄", grabBanker);
                                                }
                                                else if (value == "刮刮乐")
                                                {
                                                    renewalGame("刮刮乐", scratchOff);
                                                }
                                                else if (value == "体彩")
                                                {
                                                    renewalGame("体彩", sportsContest);
                                                }
                                                else if (value == "动物")
                                                {
                                                    renewalGame("动物", animalContest);
                                                }
                                                else if (value == "视讯")
                                                {
                                                    renewalGame("视讯", video);
                                                }
                                                else if (value == "电竞")
                                                {
                                                    renewalGame("电竞", gaming);
                                                }
                                                else if (value == "电子")
                                                {
                                                    renewalGame("电子", electronic);
                                                }
                                                else if (value == "棋牌")
                                                {
                                                    renewalGame("棋牌", chessCards);
                                                }
                                                else if (value == "捕鱼")
                                                {
                                                    renewalGame("捕鱼", fishing);
                                                }
                                                else if (value == "虚拟")
                                                {
                                                    renewalGame("虚拟", virtualGame);
                                                }
                                                else if (value == "轮盘赌")
                                                {
                                                    renewalGame("轮盘赌", roulette);
                                                }
                                                else if (value == "牛牛")
                                                {
                                                    renewalGame("牛牛", cow);
                                                }
                                                else if (value == "21点")
                                                {
                                                    renewalGame("21点", blackjack);
                                                }
                                                else if (value == "三公")
                                                {
                                                    renewalGame("三公", sangong);
                                                }
                                                else if (value == "竞猜")
                                                {
                                                    renewalGame("竞猜", trxHash);
                                                }
                                                else if (value == "幸运数")
                                                {
                                                    renewalGame("幸运数", luckyHash);
                                                }
                                                else if (value == "比特币")
                                                {
                                                    renewalGame("比特币", binanceBTCPrice);
                                                }
                                                else if (value == "外汇")
                                                {
                                                    renewalGame("外汇", forex);
                                                }
                                                else if (value == "股票")
                                                {
                                                    renewalGame("股票", stock);
                                                }
                                                else if (value == "龙虎")
                                                {
                                                    renewalGame("龙虎", dragonTiger);
                                                }
                                                else if (value == "六合彩")
                                                {
                                                    renewalGame("六合彩", sixLottery);
                                                }
                                                else if (value == "百家乐")
                                                {
                                                    renewalGame("百家乐", baccarat);
                                                }
                                                else if (value == "赛车")
                                                {
                                                    renewalGame("赛车", speedRacing);
                                                }
                                                else if (value == "飞艇")
                                                {
                                                    renewalGame("飞艇", luckyAirship);
                                                }
                                                else if (value == "11选5")
                                                {
                                                    renewalGame("11选5", choose5From11);
                                                }
                                                else if (value == "缤果")
                                                {
                                                    renewalGame("缤果", bingo);
                                                }
                                                else if (value == "幸运8")
                                                {
                                                    renewalGame("幸运8", australianLucky8);
                                                }
                                                else if (value == "大乐透")
                                                {
                                                    renewalGame("大乐透", bigLottery);
                                                }
                                                else if (value == "四星彩")
                                                {
                                                    renewalGame("四星彩", fourStarLottery);
                                                }
                                                if (string.IsNullOrEmpty(returnError))
                                                {
                                                    returnTipForZuoDaoAdminer = $"@{user.Username} {user.FirstName}{user.LastName} (Id:{user.Id}) " + returnText;
                                                    Program._appsettings.Profit += Program._appsettings.BettingThreadMonthlyMaintenanceFee;
                                                    await Helper.SaveAppsettings();
                                                    platform.Balance -= Program._appsettings.BettingThreadMonthlyMaintenanceFee;
                                                    waitInput = null;
                                                    platformOperateHistory = new PlatformOperateHistory
                                                    {
                                                        CreatorId = uid,
                                                        OperateUserId = uid,
                                                        PlatformUserRole = PlatformUserRole.Creator,
                                                        Remark = returnText,
                                                        Time = DateTime.UtcNow
                                                    };
                                                    platformFinanceHistory = new PlatformFinanceHistory
                                                    {
                                                        CreatorId = uid,
                                                        Amount = -Program._appsettings.BettingThreadMonthlyMaintenanceFee,
                                                        Remark = returnText,
                                                        Time = DateTime.UtcNow,
                                                        Type = FinanceType.MonthlyMaintenanceFee,
                                                        FinanceStatus = FinanceStatus.Success
                                                    };
                                                }
                                                break;
                                            //转让博彩平台所有权
                                            case WaitInput.TransferOwnership:
                                                if (!Regex.IsMatch(value, @"[0-9]{10}"))
                                                {
                                                    returnError = "目标Id错误,请重新输入";
                                                }
                                                else if (!db.BotChats.Any(u => u.BotId == botClient.BotId && u.ChatId == Convert.ToInt64(value)) || !Helper.IsConnectionUserChat(botClient, value))
                                                {
                                                    returnError = "对方Id需要先关注我 @CrownCasinoCityBot ,然后才能转让!";
                                                }
                                                else if (db.Platforms.Any(u => u.CreatorId == Convert.ToInt64(value)))
                                                {
                                                    returnError = "对方已存在博彩平台,不可转让给对方!";
                                                }
                                                else
                                                {
                                                    var oldCreatorId = platform.CreatorId;
                                                    platform.CreatorId = Convert.ToInt64(value);
                                                    platform.PrivateKey = Helper.ComputeSHA256Hash(value);

                                                    //更改平台财务记录
                                                    foreach (var item in db.PlatformFinanceHistorys.Where(u => u.CreatorId == oldCreatorId))
                                                    {
                                                        item.CreatorId = platform.CreatorId;
                                                    }

                                                    //更改平台群成员
                                                    foreach (var item in db.Players.Where(u => u.CreatorId == oldCreatorId))
                                                    {
                                                        item.CreatorId = platform.CreatorId;
                                                    }

                                                    platformOperateHistory = new PlatformOperateHistory
                                                    {
                                                        CreatorId = platform.CreatorId,
                                                        OperateUserId = platform.CreatorId,
                                                        PlatformUserRole = PlatformUserRole.Creator,
                                                        Remark = $"@{user.Username} {user.FirstName}{user.LastName} (ID:{user.Id}) 转让平台到您名下",
                                                        Time = DateTime.UtcNow
                                                    };

                                                    if (games != null && games.Count != 0)
                                                    {
                                                        foreach (var item in games)
                                                        {
                                                            item.CreatorId = platform.CreatorId;
                                                        }
                                                    }

                                                    foreach (var item in db.GameHistorys.Where(u => u.CreatorId == oldCreatorId))
                                                    {
                                                        item.CreatorId = platform.CreatorId;
                                                    }

                                                    //更改合伙人ID
                                                    var partner = await db.Partners.FirstAsync(u => u.CreatorId == oldCreatorId);
                                                    partner.CreatorId = uid;

                                                    waitInput = null;
                                                    returnText = "成功转让了博彩平台";
                                                    returnTipForZuoDaoAdminer = $"@{user.Username} {user.FirstName}{user.LastName} (Id:{user.Id}) " + returnText;
                                                    try
                                                    {
                                                        await Program._botClient.SendTextMessageAsync(platform.CreatorId, platformOperateHistory.Remark + ",请点击 /start 命令刷新机器人操作界面!");
                                                    }
                                                    catch (Exception ex)
                                                    {
                                                        Log.Error("转让博彩平台后给转让目标发消息时出错:" + ex.Message);
                                                    }
                                                }
                                                break;
                                            //提现
                                            case WaitInput.Withdraw:
                                                if (db.PlatformFinanceHistorys.Any(u => u.CreatorId == platform.CreatorId && u.FinanceStatus == FinanceStatus.WaitingConfirmation))
                                                {
                                                    returnError = "操作失败,您有正在等待提现的记录,请等皇冠管理员审批!";
                                                }
                                                else if (Program._appsettings.IsStopWithdraw)
                                                {
                                                    returnError = "操作失败,皇冠暂时停止提现,具体原因请向客服咨询详情 @ZuoDao_KeFuBot";
                                                }
                                                else if (!Regex.IsMatch(value, @"^(?!0(\.0{1,2})?$|1000000000(\.0{1,2})?$)\d{1,9}(\.\d{1,2})?$") || !decimal.TryParse(value, out decimal result6) || result6 <= 0 || result6 > 1000000000)
                                                {
                                                    returnError = "提现失败,要求数值在0.00-1000000000.00之间";
                                                }
                                                else if (string.IsNullOrEmpty(platform.EthereumWalletAddress) && string.IsNullOrEmpty(platform.TronWalletAddress))
                                                {
                                                    returnError = "提现失败,您必须先设置Tron波场钱包或者Ethereum以太坊钱包其中一个";
                                                }
                                                else if (result6 > platform.Balance)
                                                {
                                                    returnError = "提现失败,提现的金额大于您的现有余额";
                                                }
                                                else
                                                {
                                                    if (Program._appsettings.IsApprovalWithdraw)
                                                    {
                                                        returnText = $"申请从皇冠提现{result6}USDT的审批已提交,请等待皇冠管理员审批!";
                                                        returnTipForZuoDaoAdminer = $"@{user.Username} {user.FirstName}{user.LastName} (Id:{user.Id}) 博彩平台申请提现{result6}USDT,请审批!";
                                                    }
                                                    else
                                                    {
                                                        if (string.IsNullOrEmpty(platform.TronWalletAddress))
                                                        {

                                                            returnText = $"成功从皇冠Tron波场钱包{Program._appsettings.TronWalletAddress}提现{result6}USDT至您的TRON波场钱包:{platform.TronWalletAddress}";
                                                            returnTipForZuoDaoAdminer = $"@{user.Username} {user.FirstName}{user.LastName} (Id:{user.Id}) 博彩平台提现了{result6}USDT至TRON波场钱包:{platform.TronWalletAddress}";
#warning 这里是TRON链给他转账
                                                        }
                                                        else
                                                        {

                                                            returnText = $"成功从皇冠Ethereum以太坊钱包{Program._appsettings.EthereumWalletAddress}提现{result6}USDT至您的Ethereum以太坊钱包:{platform.EthereumWalletAddress}";
                                                            returnTipForZuoDaoAdminer = $"@{user.Username} {user.FirstName}{user.LastName} (Id:{user.Id}) 博彩平台提现了{result6}USDT至Ethereum以太坊钱包:{platform.EthereumWalletAddress}";
#warning 这里是ETH链给他转账
                                                        }
                                                    }

                                                    waitInput = null;
                                                    platform.Balance -= result6;
                                                    platformFinanceHistory = new PlatformFinanceHistory
                                                    {
                                                        CreatorId = uid,
                                                        Amount = -result6,
                                                        Remark = returnText,
                                                        Time = DateTime.UtcNow,
                                                        Type = FinanceType.Withdraw,
                                                        FinanceStatus = !Program._appsettings.IsApprovalWithdraw ? FinanceStatus.Success : FinanceStatus.WaitingConfirmation
                                                    };
                                                    platformOperateHistory = new PlatformOperateHistory
                                                    {
                                                        CreatorId = uid,
                                                        OperateUserId = uid,
                                                        PlatformUserRole = PlatformUserRole.Creator,
                                                        Remark = returnText,
                                                        Time = DateTime.UtcNow
                                                    };
                                                }
                                                break;
                                            #endregion
                                            default:
                                                returnText = $"您好!{user.FirstName}{user.LastName}";
                                                break;
                                        }
                                    }
                                }
                                #endregion
                            }

                            Program._zuodaoWaitInputUser[uid] = waitInput;

                            //底部键盘按钮
                            if (string.IsNullOrEmpty(returnError) && !isZuoDaoAdminer)
                            {
                                if (platform == null && text is "/start" or "🎮 创建博彩群组" or "🎦 机器人教程" or "🆘 找回平台" || platform != null && returnText.Contains("成功转让了博彩平台"))
                                {
                                    if (platform == null && text is "🎮 创建博彩群组" || platform != null && returnText.Contains("成功转让了博彩平台"))
                                        inputBtn.Add([new KeyboardButton("🎦 机器人教程")]);

                                    if (platform == null && text is "/start" or "🎦 机器人教程" or "🆘 找回平台" || platform != null && returnText.Contains("成功转让了博彩平台"))
                                        inputBtn.Add([new KeyboardButton("🎮 创建博彩群组")]);

                                    inputBtn.Add([new KeyboardButton("🆘 找回平台"), new KeyboardButton("🔍 平台查询")]);
                                }
                                else if (platform != null)
                                {
                                    //平台已冻结
                                    if (platform.PlatformStatus == PlatformStatus.Freeze)
                                    {
                                        inputBtn = [[new KeyboardButton("🚫 博彩平台已被冻结")]];
                                    }
                                    //平台开启/关闭
                                    else
                                    {
                                        if (text is "🎮 管理游戏" or "❓ 管理游戏" or "↩️ 返回游戏菜单" || text.Contains("➕ "))
                                        {
                                            inputBtn = [
                                                [new KeyboardButton(video == null ? "➕ 视讯" : video.GameStatus switch { GameStatus.Freeze or GameStatus.Expire => "⚠️ 视讯", _ => "📹 视讯" }),
                                                new KeyboardButton(sportsContest == null ? "➕ 体彩" : sportsContest.GameStatus switch { GameStatus.Freeze or GameStatus.Expire => "⚠️ 体彩", _ => "🤾 体彩" })],

                                                [new KeyboardButton(gaming == null ? "➕ 电竞" : gaming.GameStatus switch { GameStatus.Freeze or GameStatus.Expire => "⚠️ 电竞", _ => "👾 电竞" }),
                                                new KeyboardButton(electronic == null ? "➕ 电子" : electronic.GameStatus switch { GameStatus.Freeze or GameStatus.Expire => "⚠️ 电子", _ => "🕹 电子" }),
                                                new KeyboardButton(chessCards == null ? "➕ 棋牌" : chessCards.GameStatus switch { GameStatus.Freeze or GameStatus.Expire => "⚠️ 棋牌", _ => "🀄️ 棋牌" })],

                                                [new KeyboardButton(fishing == null ? "➕ 捕鱼" : fishing.GameStatus switch { GameStatus.Freeze or GameStatus.Expire => "⚠️ 捕鱼", _ => "🐠 捕鱼" }),
                                                new KeyboardButton(virtualGame == null ? "➕ 虚拟" : virtualGame.GameStatus switch { GameStatus.Freeze or GameStatus.Expire => "⚠️ 虚拟", _ => "🏌🏻 虚拟" }),
                                                new KeyboardButton(animalContest == null ? "➕ 动物" : animalContest.GameStatus switch { GameStatus.Freeze or GameStatus.Expire => "⚠️ 动物", _ => "🐎 动物" })],


                                                [new KeyboardButton("⎼⎼⎼⎼⎼▾ 🎲 EMOJI表情 ▾⎼⎼⎼⎼⎼")],
                                                [new KeyboardButton(slotMachine == null ? "➕ 老虎机" : slotMachine.GameStatus switch { GameStatus.Freeze or GameStatus.Expire => "⚠️ 老虎机", _ => "🎰 老虎机" }),
                                                new KeyboardButton(dice == null ? "➕ 骰子" : dice.GameStatus switch { GameStatus.Freeze or GameStatus.Expire => "⚠️ 骰子", _ => "🎲 骰子" }),
                                                new KeyboardButton(bowling == null ? "➕ 保龄球" : bowling.GameStatus switch { GameStatus.Freeze or GameStatus.Expire => "⚠️ 保龄球", _ => "🎳 保龄球" })],

                                                [new KeyboardButton(dart == null ? "➕ 飞镖" : dart.GameStatus switch { GameStatus.Freeze or GameStatus.Expire => "⚠️ 飞镖", _ => "🎯 飞镖" }),
                                                new KeyboardButton(soccer == null ? "➕ 足球" : soccer.GameStatus switch { GameStatus.Freeze or GameStatus.Expire => "⚠️ 足球", _ => "⚽️ 足球" }),
                                                new KeyboardButton(basketball == null ? "➕ 篮球" : basketball.GameStatus switch { GameStatus.Freeze or GameStatus.Expire => "⚠️ 篮球", _ => "🏀 篮球" })],

                                                [new KeyboardButton("⎼⎼⎼⎼⎼▾ ⚔️ 玩家互博 ▾⎼⎼⎼⎼⎼")],
                                                [new KeyboardButton(redEnvelope == null ? "➕ 红包" : redEnvelope.GameStatus switch { GameStatus.Freeze or GameStatus.Expire => "⚠️ 红包", _ => "🧧 红包" }),
                                                new KeyboardButton(blindBox == null ? "➕ 盲盒" : blindBox.GameStatus switch { GameStatus.Freeze or GameStatus.Expire => "⚠️ 盲盒", _ => "💣 盲盒" }),
                                                new KeyboardButton(grabBanker == null ? "➕ 抢庄" : grabBanker.GameStatus switch { GameStatus.Freeze or GameStatus.Expire => "⚠️ 抢庄", _ => "🏃‍♂️ 抢庄" }),
                                                ],

                                                [new KeyboardButton("⎼⎼⎼⎼⎼▾ 📈 趋势涨跌 ▾⎼⎼⎼⎼⎼")],
                                                [new KeyboardButton(binanceBTCPrice == null ? "➕ 比特币" : binanceBTCPrice.GameStatus switch { GameStatus.Freeze or GameStatus.Expire => "⚠️ 比特币", _ => "💲 比特币" }),
                                                new KeyboardButton(forex == null ? "➕ 外汇" : forex.GameStatus switch { GameStatus.Freeze or GameStatus.Expire => "⚠️ 外汇", _ => "💲 外汇" }),
                                                new KeyboardButton(stock == null ? "➕ 股票" : stock.GameStatus switch { GameStatus.Freeze or GameStatus.Expire => "⚠️ 股票", _ => "💲 股票" })],

                                                [new KeyboardButton("⎼⎼⎼⎼⎼▾ ⛓️ 区块哈希 ▾⎼⎼⎼⎼⎼")],
                                                [new KeyboardButton(roulette == null ? "➕ 轮盘赌" : roulette.GameStatus switch { GameStatus.Freeze or GameStatus.Expire => "⚠️ 轮盘赌", _ => "🎡 轮盘赌" }),
                                                new KeyboardButton(cow == null ? "➕ 牛牛" : cow.GameStatus switch { GameStatus.Freeze or GameStatus.Expire => "⚠️ 牛牛", _ => "🐮 牛牛" }),
                                                new KeyboardButton(blackjack == null ? "➕ 21点" : blackjack.GameStatus switch { GameStatus.Freeze or GameStatus.Expire => "⚠️ 21点", _ => " 21点" })],

                                                [new KeyboardButton(sangong == null ? "➕ 三公" : sangong.GameStatus switch { GameStatus.Freeze or GameStatus.Expire => "⚠️ 三公", _ => "🎴 三公" }),
                                                new KeyboardButton(trxHash == null ? "➕ 竞猜" : trxHash.GameStatus switch { GameStatus.Freeze or GameStatus.Expire => "⚠️ 竞猜", _ => "#️⃣ 竞猜" }),
                                                new KeyboardButton(scratchOff == null ? "➕ 刮刮乐" : scratchOff.GameStatus switch { GameStatus.Freeze or GameStatus.Expire => "⚠️ 刮刮乐", _ => "🪒 刮刮乐" })],

                                                [new KeyboardButton(dragonTiger == null ? "➕ 龙虎" : dragonTiger.GameStatus switch { GameStatus.Freeze or GameStatus.Expire => "⚠️ 龙虎", _ => "🐯 龙虎" }),
                                                new KeyboardButton(baccarat == null ? "➕ 百家乐" : baccarat.GameStatus switch { GameStatus.Freeze or GameStatus.Expire => "⚠️ 百家乐", _ => "♣️ 百家乐" }),
                                                new KeyboardButton(luckyHash == null ? "➕ 幸运" : luckyHash.GameStatus switch { GameStatus.Freeze or GameStatus.Expire => "⚠️ 幸运", _ => "✨ 幸运" })],

                                                //[new KeyboardButton("⎼⎼⎼⎼⎼▾ 🎁 抽奖游戏 ▾⎼⎼⎼⎼⎼")], //10U夺宝\赢手机\赢汽车\砸彩蛋\大转盘\老虎机\扭蛋\娃娃机\翻牌

                                                [new KeyboardButton("⎼⎼⎼⎼⎼▾ 🎫 彩票竞猜 ▾⎼⎼⎼⎼⎼")],
                                                [new KeyboardButton(sixLottery == null ? "➕ 六合彩" : sixLottery.GameStatus switch { GameStatus.Freeze or GameStatus.Expire => "⚠️ 六合彩", _ => "🔯 六合彩" }),
                                                new KeyboardButton(canadaPC28 == null ? "➕ PC28" : canadaPC28.GameStatus switch { GameStatus.Freeze or GameStatus.Expire => "⚠️ PC28", _ => "🇨🇦 PC28" }),
                                                new KeyboardButton(speedRacing == null ? "➕ 赛车" : speedRacing.GameStatus switch { GameStatus.Freeze or GameStatus.Expire => "⚠️ 赛车", _ => "🚕 赛车" })],

                                                [new KeyboardButton(luckyAirship == null ? "➕ 飞艇" : luckyAirship.GameStatus switch { GameStatus.Freeze or GameStatus.Expire => "⚠️ 飞艇", _ => "🛥 飞艇" }),
                                                new KeyboardButton(bigLottery == null ? "➕ 大乐透" : bigLottery.GameStatus switch { GameStatus.Freeze or GameStatus.Expire => "⚠️ 大乐透", _ => "🛥 大乐透" }),
                                                new KeyboardButton(fourStarLottery == null ? "➕ 四星彩" : fourStarLottery.GameStatus switch { GameStatus.Freeze or GameStatus.Expire => "⚠️ 四星彩", _ => "🛥 四星彩" })],

                                                [new KeyboardButton(choose5From11 == null ? "➕ 11选5" : choose5From11.GameStatus switch { GameStatus.Freeze or GameStatus.Expire => "⚠️ 11选5", _ => "🔢 11选5" }),
                                                new KeyboardButton(bingo == null ? "➕ 缤果" : bingo.GameStatus switch { GameStatus.Freeze or GameStatus.Expire => "⚠️ 缤果", _ => "🎉 缤果" }),
                                                new KeyboardButton(australianLucky8 == null ? "➕ 幸运8" : australianLucky8.GameStatus switch { GameStatus.Freeze or GameStatus.Expire => "⚠️ 幸运8", _ => "🎱 幸运8" })]
                                             ];

                                            inputBtn.Add([new KeyboardButton("↩️ 返回主菜单")]);
                                        }
                                        else
                                        {
                                            var setGameKeyboardBtn = (string gameName, Game? game) =>
                                            {
                                                var setKB = text.Contains($" {gameName}")
                                                    || text == $"🚫 {gameName}已冻结"
                                                    || text == $"✅ {gameName}开盘中"
                                                    || text == $"☑️ {gameName}休盘中"
                                                    || text == $"⚠️ {gameName}到期"
                                                    //如果是激活盘口
                                                    || Regex.IsMatch(text, @"^[A-Fa-f0-9]{64}=");

                                                if (game != null && setKB)
                                                {
                                                    if (game!.GameStatus == GameStatus.Freeze)
                                                    {
                                                        inputBtn = [[new KeyboardButton($"🚫 {gameName}已冻结")]];
                                                    }
                                                    else if (game.GameStatus == GameStatus.Expire)
                                                    {
                                                        inputBtn.Add([new KeyboardButton($"⚠️ {gameName}到期")]);
                                                    }
                                                    else
                                                    {
                                                        //赔率设置，下注限额，保证金
                                                        inputBtn.Add([new KeyboardButton($"⎼⎼⎼⎼⎼ {gameName}设置 ⎼⎼⎼⎼⎼")]);
                                                        inputBtn.Add([new KeyboardButton("🔄 更新API"), new KeyboardButton("👥 更绑群组"), new KeyboardButton("💬 重置话题")]);
                                                        inputBtn.Add([new KeyboardButton("🔢 赔率设置"), new KeyboardButton("🗳 下注限额"), new KeyboardButton("🔗 WebUrl")]);                                                      
                                                        inputBtn.Add([new KeyboardButton("💳 盘口续费"), new KeyboardButton(game.GameStatus == GameStatus.Open ? "✅ 开盘状态" : "☑️ 休盘状态"), new KeyboardButton("🗑 删除盘口")]);
                                                    }
                                                    inputBtn.Add([new KeyboardButton("↩️ 返回游戏目录")]);
                                                }
                                            };


                                            if (text.Contains("老虎机"))
                                            {
                                                setGameKeyboardBtn("老虎机", slotMachine);
                                            }
                                            else if (text.Contains("骰子"))
                                            {
                                                setGameKeyboardBtn("骰子", dice);
                                            }
                                            else if (text.Contains("保龄球"))
                                            {
                                                setGameKeyboardBtn("保龄球", bowling);
                                            }
                                            else if (text.Contains("飞镖"))
                                            {
                                                setGameKeyboardBtn("飞镖", dart);
                                            }
                                            else if (text.Contains("足球"))
                                            {
                                                setGameKeyboardBtn("足球", soccer);
                                            }
                                            else if (text.Contains("篮球"))
                                            {
                                                setGameKeyboardBtn("篮球", basketball);
                                            }
                                            else if (text.Contains("红包"))
                                            {
                                                setGameKeyboardBtn("红包", redEnvelope);
                                            }
                                            else if (text.Contains("盲盒"))
                                            {
                                                setGameKeyboardBtn("盲盒", blindBox);
                                            }
                                            else if (text.Contains("抢庄"))
                                            {
                                                setGameKeyboardBtn("抢庄", grabBanker);
                                            }
                                            else if (text.Contains("刮刮乐"))
                                            {
                                                setGameKeyboardBtn("刮刮乐", scratchOff);
                                            }
                                            else if (text.Contains("体彩"))
                                            {
                                                setGameKeyboardBtn("体彩", sportsContest);
                                            }
                                            else if (text.Contains("动物"))
                                            {
                                                setGameKeyboardBtn("动物", animalContest);
                                            }
                                            else if (text.Contains("视讯"))
                                            {
                                                setGameKeyboardBtn("视讯", video);
                                            }
                                            else if (text.Contains("电竞"))
                                            {
                                                setGameKeyboardBtn("电竞", gaming);
                                            }
                                            else if (text.Contains("电子"))
                                            {
                                                setGameKeyboardBtn("电子", electronic);
                                            }
                                            else if (text.Contains("棋牌"))
                                            {
                                                setGameKeyboardBtn("棋牌", chessCards);
                                            }
                                            else if (text.Contains("捕鱼"))
                                            {
                                                setGameKeyboardBtn("捕鱼", fishing);
                                            }
                                            else if (text.Contains("虚拟"))
                                            {
                                                setGameKeyboardBtn("虚拟", virtualGame);
                                            }
                                            else if (text.Contains("轮盘赌"))
                                            {
                                                setGameKeyboardBtn("轮盘赌", roulette);
                                            }
                                            else if (text.Contains("牛牛"))
                                            {
                                                setGameKeyboardBtn("牛牛", cow);
                                            }
                                            else if (text.Contains("21点"))
                                            {
                                                setGameKeyboardBtn("21点", blackjack);
                                            }
                                            else if (text.Contains("三公"))
                                            {
                                                setGameKeyboardBtn("三公", sangong);
                                            }
                                            else if (text.Contains("百家乐"))
                                            {
                                                setGameKeyboardBtn("百家乐", baccarat);
                                            }
                                            else if (text.Contains("竞猜"))
                                            {
                                                setGameKeyboardBtn("竞猜", trxHash);
                                            }
                                            else if (text.Contains("幸运数"))
                                            {
                                                setGameKeyboardBtn("幸运数", luckyHash);
                                            }
                                            else if (text.Contains("比特币"))
                                            {
                                                setGameKeyboardBtn("比特币", binanceBTCPrice);
                                            }
                                            else if (text.Contains("外汇"))
                                            {
                                                setGameKeyboardBtn("外汇", forex);
                                            }
                                            else if (text.Contains("股票"))
                                            {
                                                setGameKeyboardBtn("股票", stock);
                                            }
                                            else if (text.Contains("龙虎"))
                                            {
                                                setGameKeyboardBtn("龙虎", dragonTiger);
                                            }
                                            else if (text.Contains("六合彩"))
                                            {
                                                setGameKeyboardBtn("六合彩", sixLottery);
                                            }
                                            else if (text.Contains("PC28"))
                                            {
                                                setGameKeyboardBtn("PC28", canadaPC28);
                                            }
                                            else if (text.Contains("赛车"))
                                            {
                                                setGameKeyboardBtn("赛车", speedRacing);
                                            }
                                            else if (text.Contains("飞艇"))
                                            {
                                                setGameKeyboardBtn("飞艇", luckyAirship);
                                            }
                                            else if (text.Contains("11选5"))
                                            {
                                                setGameKeyboardBtn("11选5", choose5From11);
                                            }
                                            else if (text.Contains("缤果"))
                                            {
                                                setGameKeyboardBtn("缤果", bingo);
                                            }
                                            else if (text.Contains("幸运8"))
                                            {
                                                setGameKeyboardBtn("幸运8", australianLucky8);
                                            }
                                            else if (text.Contains("大乐透"))
                                            {
                                                setGameKeyboardBtn("大乐透", bigLottery);
                                            }
                                            else if (text.Contains("四星彩"))
                                            {
                                                setGameKeyboardBtn("四星彩", fourStarLottery);
                                            }
                                            //顶级菜单按钮
                                            else if (text == "/start"
                                            || text is "↩️ 返回主菜单" or "❓ 机器人API" or "🤖 机器人API" or "✅ 开盘状态" or "☑️ 休盘状态"
                                            || !string.IsNullOrEmpty(botApiToken)
                                            || waitInput is WaitInput.TronWalletAddress
                                            or WaitInput.TronWalletPrivateKey
                                            or WaitInput.EthereumWalletAddress
                                            or WaitInput.EthereumWalletPrivateKey
                                            or WaitInput.GroupId
                                            or WaitInput.FinancerId
                                            or WaitInput.FinancialOperationAmount
                                            or WaitInput.Dividend
                                            or WaitInput.BotApiToken
                                            or WaitInput.TransferOwnership)
                                            {
                                                if (text is "❓ 机器人API" or "🤖 机器人API")
                                                    inputBtn.Add([new KeyboardButton("🎦 机器人教程")]);
                                                inputBtn.Add([new KeyboardButton(games?.Any(u => u.GameStatus == GameStatus.Expire || u.GameStatus == GameStatus.Freeze) == true ? "❓ 管理游戏" : "🎮 管理游戏")]);
                                                inputBtn.Add([
                                                    //另外绑定推广机器人这些
                                                    new KeyboardButton("🤖 机器人"),
                                                    new KeyboardButton("💳 钱包"),
                                                      //抽水水续费
                                                    new KeyboardButton("💲 抽佣"),
                                                   ]);
                                                inputBtn.Add([
                                                    new KeyboardButton(platform.GroupId == null ? "❓ 绑群" : "👥 绑群"),
                                                    new KeyboardButton(platform.ChannelId == null ? "❓ 频道" : "🗣 频道"),
                                                    new KeyboardButton("📢 公告"),
                                                   ]);
                                                inputBtn.Add([new KeyboardButton("💵 充值"), new KeyboardButton("💸 提现"), new KeyboardButton("💰 担保")]);
                                                //inputBtn.Add([new KeyboardButton(string.IsNullOrEmpty(platform.TronWalletAddress) ? "❓ 波场钱包" : "✅ 波场钱包"), new KeyboardButton(string.IsNullOrEmpty(platform.TronWalletPrivateKey) ? "❓ 波场私钥" : "✅ 波场私钥")]);
                                                //inputBtn.Add([new KeyboardButton(string.IsNullOrEmpty(platform.EthereumWalletAddress) ? "❓ 以太钱包" : "✅ 以太钱包"), new KeyboardButton(string.IsNullOrEmpty(platform.EthereumWalletPrivateKey) ? "❓ 以太私钥" : "✅ 以太私钥")]);
                                                inputBtn.Add([
                                                   //邀请分红设置
                                                   new KeyboardButton(platform.Dividend == 0 ? "❓ 推广" : "✅ 推广"),
                                                   new KeyboardButton(platform.FinancialOperationAmount == 0 ? "❓ 下分" : "✅ 下分"),
                                                   //充值送金?首充送金?流水返水?
                                                   new KeyboardButton("🎁 福利")
                                                   ]);
                                                inputBtn.Add([
                                                    new KeyboardButton("👾 造势"),
                                                    new KeyboardButton("📈 投资"),
                                                    //是否删除投注/是否每局隔5分钟群主自动参与发红包和盲盒/是否禁止闲聊/语言
                                                    new KeyboardButton("🔧 系统"),
                                                    ]);
                                                inputBtn.Add([
                                                    new KeyboardButton(string.IsNullOrEmpty(platform.ServerIds) ? "❓ 客服" : "✅ 客服"),
                                                    new KeyboardButton(platform.FinancerId == null ? "❓ 财务" : "✅ 财务"),
                                                    new KeyboardButton("🤝 股东"),
                                                    ]);
                                                inputBtn.Add([new KeyboardButton("🔄 转让"), new KeyboardButton("📊 排行"), new KeyboardButton(platform.PlatformStatus == PlatformStatus.Open ? "✅ 营业" : "☑️ 休业")]);
                                            }
                                        }
                                    }
                                }
                            }

                            //返回提示给皇冠管理员
                            if (!string.IsNullOrEmpty(returnTipForZuoDaoAdminer))
                                SendMessageToZuoDaoAdminers(returnTipForZuoDaoAdminer, uid);

                            //如果出错了
                            if (!string.IsNullOrEmpty(returnError))
                            {
                                Helper.DeleteMessage(botClient, update, 10, returnError, cancellationToken);
                            }
                            else
                            {
                                if (platformOperateHistory != null)
                                    await db.PlatformOperateHistorys.AddAsync(platformOperateHistory);

                                if (platformFinanceHistory != null)
                                    await db.PlatformFinanceHistorys.AddAsync(platformFinanceHistory);

                                // 检查上下文中是否有已更改的实体
                                bool hasChanges = db.ChangeTracker.Entries().Any(e => e.State == EntityState.Added || e.State == EntityState.Modified || e.State == EntityState.Deleted);
                                if (hasChanges)
                                    //执行数据库保存
                                    await db.SaveChangesAsync();

                                try
                                {
                                    await Program._botClient.SendTextMessageAsync(
                                        chatId: chatId,
                                        text: returnText,
                                        parseMode: ParseMode.Html,
                                        disableWebPagePreview: true,
                                        replyMarkup: msgBtn.Count != 0 ? new InlineKeyboardMarkup(msgBtn) : new ReplyKeyboardMarkup(inputBtn)
                                        {
                                            //是否自动调整按钮行高
                                            ResizeKeyboard = true,
                                            //点击按钮后隐藏按钮
                                            OneTimeKeyboard = false,
                                            //是否隐藏折叠按钮
                                            IsPersistent = false
                                        });
                                }
                                catch (Exception ex)
                                {
                                    Log.Error("机器人私聊收到信息,返回时出错:" + ex.Message);
                                }
                            }
                        }
                        //群组接到信息时
                        else if (msg.Chat.Type is ChatType.Group or ChatType.Supergroup)
                        {

                        }
                        //频道收到信息时
                        else if (msg.Chat.Type is ChatType.Channel)
                        {

                        }
                        //执行完要删除
                        Program._runingUserId.Remove(uid);
                        break;
                    //收到前台按钮回调
                    case UpdateType.CallbackQuery:
                        break;
                    //用户发了帖子
                    case UpdateType.ChannelPost:
                        break;
                    //私聊被停用/重新启用
                    case UpdateType.MyChatMember:
                        break;
                    //聊天成员的状态是 在聊天中更新。机器人必须是聊天中的管理员，并且必须显式 在列表中指定接收这些内容
                    case UpdateType.ChatMember:
                        break;
                    //用户加入群组
                    case UpdateType.ChatJoinRequest:
                        break;
                    case UpdateType.Unknown:
                        break;
                    //在聊天编辑框输入@CrownCasinoCityBot触发
                    case UpdateType.InlineQuery:
                        break;
                    //由用户选择并发送给其聊天伙伴的内联查询的结果 InlineKeyboardButton.WithSwitchInlineQueryChosenChat后触发此处
                    case UpdateType.ChosenInlineResult:
                        break;
                    //编辑消息后触发
                    case UpdateType.EditedMessage:
                        break;
                    //编辑频道帖子后触发
                    case UpdateType.EditedChannelPost:
                        break;
                    //发货查询
                    case UpdateType.ShippingQuery:
                        break;
                    //结账前查询
                    case UpdateType.PreCheckoutQuery:
                        break;
                    //投票数据有变动时
                    case UpdateType.Poll:
                        break;
                    //用户投了票 非匿名投票才能PollAnswer
                    case UpdateType.PollAnswer:
                        break;
                    default:
                        break;
                }

            }, cancellationToken);
        }

        //给皇冠管理员发消息
        public static void SendMessageToZuoDaoAdminers(string text, long? excludeId = null, IReplyMarkup? replyMarkup = null)
        {
            _ = Task.Run(async () =>
            {
                foreach (var adminerId in Program._appsettings.AdminerIds)
                {
                    if (excludeId != null && adminerId == excludeId)
                        continue;
                    try
                    {
                        await Program._botClient.SendTextMessageAsync(chatId: adminerId, text: text, parseMode: ParseMode.Html, disableNotification: false, replyMarkup: replyMarkup);
                    }
                    catch (Exception ex)
                    {
                        Log.Error("给皇冠管理员们发消息时出错:" + ex.Message);
                    }
                }
            });
        }
    }
}
