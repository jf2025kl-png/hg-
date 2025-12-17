using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace 皇冠娱乐.Games
{
    /// <summary>
    /// 盲盒
    /// </summary>
    public static class BlindBox
    {
        //系统10分钟派发一次,或者用户派发
        public static async Task Send(ITelegramBotClient botClient, DataContext db, Platform platform, Game game,
            Player player, decimal amount, Message? msg, CancellationToken cancellationToken)
        {
            var openPrice = amount / 6;

            var senderName = msg?.From != null ? msg.From.FirstName + msg.From.LastName : "群主";
            var returnText =
                $"<b>👤 盲盒庄家 : {senderName}" +
                $"\n\n💵 盲盒金额 : {amount} USDT" +
                $"\n\n💲 开盒本金 : {openPrice:0.00} USDT" +
                $"\n\n💰 积累奖池 : {game.PrizePool} USDT</b>" +
                $"\n\n🫰 返奖率高,回报大的玩家博弈游戏";

            //信息按钮
            List<List<InlineKeyboardButton>> msgBtn = [
                [InlineKeyboardButton.WithCallbackData("🎁", "0"),
                InlineKeyboardButton.WithCallbackData("🎁", "1"),
                InlineKeyboardButton.WithCallbackData("🎁", "2"),
                InlineKeyboardButton.WithCallbackData("🎁", "3"),
                InlineKeyboardButton.WithCallbackData("🎁", "4")
                ],
                [InlineKeyboardButton.WithCallbackData("🎁", "5"),
                    InlineKeyboardButton.WithCallbackData("🎁", "6"),
                    InlineKeyboardButton.WithCallbackData("🎁", "7"),
                    InlineKeyboardButton.WithCallbackData("🎁", "8"),
                    InlineKeyboardButton.WithCallbackData("🎁", "9")
                    ]];

            var blindStream = new InputFileStream(content: new FileStream("盲盒开奖图.jpg", FileMode.Open, FileAccess.Read), fileName: Path.GetFileName("盲盒开奖图.jpg"));

            try
            {
                var blindBoxMsg = await botClient.SendPhotoAsync(game.GroupId, photo: blindStream, messageThreadId: game.ThreadId, parseMode: ParseMode.Html, caption: returnText, replyToMessageId: msg?.MessageId, replyMarkup: new InlineKeyboardMarkup(msgBtn), cancellationToken: cancellationToken);

                var blindBoxGameHistory = new GameHistory
                {
                    Time = DateTime.UtcNow,
                    ClosingTime = DateTime.UtcNow.AddMinutes(10),
                    EndTime = DateTime.UtcNow.AddMinutes(10),
                    Status = GameHistoryStatus.Ongoing,
                    GroupId = game.GroupId,
                    MessageThreadId = blindBoxMsg.MessageThreadId,
                    MessageId = blindBoxMsg.MessageId,
                    GameId = game.Id,
                    CreatorId = platform.CreatorId,
                    CommissionRate = 0.05M,
                    BetAmount = amount,
                    PlayerId = player.PlayerId
                };
                await db.GameHistorys.AddAsync(blindBoxGameHistory, cancellationToken);
                await db.SaveChangesAsync(cancellationToken);
                var finance = new PlayerFinanceHistory
                {
                    Time = DateTime.UtcNow,
                    Name = senderName,
                    FinanceStatus = FinanceStatus.Freeze,
                    Type = FinanceType.BlindBox,
                    Remark = "发盲盒费用",
                    GameId = game.Id,
                    GameMessageId = blindBoxMsg.MessageId,
                    PlayerId = player.PlayerId
                };
                player = await Helper.MinusBalance(db, amount, player, finance, cancellationToken);
                await db.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                Log.Error("返回发盲盒信息时出错:" + ex.Message);
            }

#warning 最后如果没人参与,就返回90% 金额给庄家,10% 流入奖池
#warning 到时开始算账
        }

        //接收
        public static async Task Receive(ITelegramBotClient botClient, DataContext db, Platform platform, Game game,
            CallbackQuery cq, GameHistory blindBoxGameHistory, Player player, PlayerFinanceHistory bankerFinanceHistory,
            CancellationToken cancellationToken)
        {
            decimal boxAmountTotalAmount = bankerFinanceHistory.Amount + bankerFinanceHistory.BonusAmount;
            //信息按钮
            List<List<InlineKeyboardButton>> msgBtn = [];
            //现有奖池资金
            var gamePoolAmount = game.PrizePool;
            //庄家
            var banker = await db.Players.FirstAsync(u => u.PlayerId == blindBoxGameHistory.PlayerId, cancellationToken: cancellationToken);

            var playerName = cq.From.FirstName + cq.From.LastName;
            //下注金额是盒子总额的1/6
            var openBoxPrice = boxAmountTotalAmount / 6;
            //点到鞭炮的费用总额的1/3
            var clickFirecrackerPrice = boxAmountTotalAmount / 3;
            //下注费用+冻结鞭炮费用
            player = await Helper.MinusBalance(db, openBoxPrice + clickFirecrackerPrice, player, new PlayerFinanceHistory
            {
                Time = DateTime.UtcNow,
                Name = playerName,
                FinanceStatus = FinanceStatus.Success,
                Type = FinanceType.BlindBox,
                Remark = $"开盲盒费用+预冻结点到🧨的{clickFirecrackerPrice:0.00}U费用,如果结果未点到🧨将返还{clickFirecrackerPrice:0.00}U",
                GameId = game.Id,
                GameMessageId = cq.Message!.MessageId,
                PlayerId = player.PlayerId
            }, cancellationToken);
            //下注费用50%给庄家,
            banker.Balance += (openBoxPrice / 2);
            await db.SaveChangesAsync(cancellationToken);

            string[] nums = ["1️⃣", "2️⃣", "3️⃣", "4️⃣", "5️⃣", "6️⃣"];
            var matches = Regex.Matches(cq.Message.Caption!, @"(1️⃣|2️⃣|3️⃣|4️⃣|5️⃣|6️⃣).+");
            string returnText = string.Empty;
            //还没达到6个盒子
            if (matches.Count < 5)
            {
                var topText = cq.Message.Caption!.Contains("----------领取玩家----------")
                    ? cq.Message.Caption[..cq.Message.Caption.IndexOf("\n\n----------领取玩家----------")]
                    : cq.Message.Caption[..cq.Message.Caption.IndexOf("\n\n🫰 返奖率高,回报大的玩家博弈游戏")];

                returnText = "<b>" + topText + "\n\n----------领取玩家----------</b>";

                foreach (var item in matches)
                    returnText += "\n\n" + item;

                returnText += $"\n\n{nums[matches.Count]}  {playerName}\n\n🫰 返奖率高,回报大的玩家博弈游戏";

                if (cq.Message.ReplyMarkup!.InlineKeyboard != null)
                {
                    foreach (var itema in cq.Message.ReplyMarkup.InlineKeyboard)
                    {
                        var row = new List<InlineKeyboardButton>();
                        foreach (var itemb in itema)
                        {
                            if (!string.IsNullOrEmpty(itemb.CallbackData))
                            {
                                if (itemb.CallbackData == cq.Data)
                                {
                                    row.Add(InlineKeyboardButton.WithCallbackData(nums[matches.Count], itemb.CallbackData!));
                                }
                                else
                                {
                                    row.Add(InlineKeyboardButton.WithCallbackData(itemb.Text, itemb.CallbackData!));
                                }
                            }
                        }
                        msgBtn.Add(row);
                    }
                }

                await db.SaveChangesAsync(cancellationToken);
                try
                {
                    await botClient.EditMessageCaptionAsync(chatId: cq.Message.Chat.Id, cq.Message!.MessageId, returnText, parseMode: ParseMode.Html, null, new InlineKeyboardMarkup(msgBtn), cancellationToken);
                }
                catch (Exception ex)
                {
                    Log.Error("点击抢红包返回信息时出错:" + ex.Message);
                }
            }
            //开到第六个格子了
            else
            {
                await End(botClient, db, platform, game, cq, blindBoxGameHistory, bankerFinanceHistory, gamePoolAmount, banker, cancellationToken);
            }
        }

        //结束
        public static async Task End(ITelegramBotClient botClient, DataContext db, Platform platform, Game game,
            CallbackQuery? cq, GameHistory blindBoxGameHistory, PlayerFinanceHistory bankerFinanceHistory, decimal gamePoolAmount, Player banker,
            CancellationToken cancellationToken)
        {
            string[] nums = ["1️⃣", "2️⃣", "3️⃣", "4️⃣", "5️⃣", "6️⃣"];
            var matches = Regex.Matches(cq.Message.Caption!, @"(1️⃣|2️⃣|3️⃣|4️⃣|5️⃣|6️⃣).+");
            decimal boxAmountTotalAmount = bankerFinanceHistory.Amount + bankerFinanceHistory.BonusAmount;
            var playerName = cq.From.FirstName + cq.From.LastName;
            //下注金额是盒子总额的1/6
            var openBoxPrice = boxAmountTotalAmount / 6;
            //点到鞭炮的费用总额的1/3
            var clickFirecrackerPrice = boxAmountTotalAmount / 3;
            //下注记录集合(剔除庄家发起记录)
            var betHistorys = db.PlayerFinanceHistorys.Where(u => u.Type == FinanceType.BlindBox && u.FinanceStatus == FinanceStatus.Success && u.GameId == game.Id && u.GameMessageId == cq.Message.MessageId).OrderByDescending(e => e.Time);

            blindBoxGameHistory.Status = GameHistoryStatus.End;
            blindBoxGameHistory.EndTime = DateTime.UtcNow;
            blindBoxGameHistory.ClosingTime = DateTime.UtcNow;
            //庄家发的+6个玩家开盒费用
            blindBoxGameHistory.BetAmount = boxAmountTotalAmount + (openBoxPrice * 6);

            var topText = cq.Message.Caption![..cq.Message.Caption!.IndexOf("\n\n💰 积累奖池")];
            var returnText = "<b>" + topText + "\n\n----------领取玩家----------</b>";

            List<string> list = [
                (boxAmountTotalAmount* 0.333333M).ToString(),
                                                    (boxAmountTotalAmount * 0.233333M).ToString(),
                                                    (boxAmountTotalAmount * 0.233333M).ToString(),
                                                    (boxAmountTotalAmount * 0.1M).ToString(),
                                                    (boxAmountTotalAmount * 0.1M).ToString(),
                                                    "💰", "💣", "🧨", "🧨", "空盒"];

            list = [.. list.OrderBy(x => new Random().Next())];
            //领取玩家列表
            var playerList = matches.Select(u => u.Value).ToList();
            playerList.Add($"{nums[5]}  {playerName}");

            //点到钱袋玩家序号
            int clickPurseNum = -1;
            //点到手雷玩家序号
            int clickGrenadeNum = -1;
            //最先手雷还是钱袋的图标序号
            var moneyGrabIcon = string.Empty;

            //列表项字符串:序号+用户名 (不包含金额💰💣🧨)
            List<string> sortedList = [];
            int currentI = 0;
            foreach (var itema in cq.Message.ReplyMarkup!.InlineKeyboard)
            {
                foreach (var btn in itema)
                {
                    var playerItem = playerList.FirstOrDefault(u => u.Contains(btn.Text));
                    if (playerItem != null)
                    {
                        sortedList.Add(playerItem);
                    }
                    else if (Convert.ToInt32(btn.CallbackData) == Convert.ToInt32(cq.Data))
                    {
                        sortedList.Add(playerList.Last());
                    }
                    else
                    {
                        sortedList.Add("🥡");
                    }
                    var iconMatch = Regex.Match(sortedList[currentI], @"^(1️⃣|2️⃣|3️⃣|4️⃣|5️⃣|6️⃣)");
                    if (iconMatch.Success)
                    {
                        int itemNum = -1;
                        var itemNumIcon = string.Empty;
                        switch (iconMatch.Value)
                        {
                            case "1️⃣":
                                itemNum = 1;
                                itemNumIcon = "⑴";
                                break;
                            case "2️⃣":
                                itemNum = 2;
                                itemNumIcon = "⑵";
                                break;
                            case "3️⃣":
                                itemNum = 3;
                                itemNumIcon = "⑶";
                                break;
                            case "4️⃣":
                                itemNum = 4;
                                itemNumIcon = "⑷";
                                break;
                            case "5️⃣":
                                itemNum = 5;
                                itemNumIcon = "⑸";
                                break;
                            case "6️⃣":
                                itemNum = 6;
                                itemNumIcon = "⑹";
                                break;
                            default:
                                break;
                        }

                        //点到💰:获得后面未被开盒的金额 + 奖池58%奖金:如果没人点到💰,未打开盒子金额流入奖池
                        if (list[currentI] == "💰")
                        {
                            //前面已经有人点到💣本条作废
                            if (clickGrenadeNum == -1 || clickGrenadeNum != -1 && itemNum < clickGrenadeNum)
                            {
                                clickGrenadeNum = -1;
                                clickPurseNum = itemNum;
                                moneyGrabIcon = itemNumIcon;
                            }
                        }
                        //点到💣:庄家可获得后面未被开盒金额 + 奖池28%奖金:庄家点到💣不作数,未打开盒子金额流入奖池
                        else if (list[currentI] == "💣")
                        {
                            //前面已经有人点到💰本条作废
                            if (clickPurseNum == -1 || clickPurseNum != -1 && itemNum < clickPurseNum)
                            {
                                clickPurseNum = -1;
                                clickGrenadeNum = itemNum;
                                moneyGrabIcon = itemNumIcon;
                            }
                        }
                    }
                    currentI++;
                }
            }

            //未开和后面的金额 后面的钱(以及未开盒的钱)
            decimal noOpenAndLaterTotal = 0;
            if (clickPurseNum != -1 || clickGrenadeNum != -1)
            {
                //是哪个索引开始抢钱的
                var clickIndex = clickPurseNum != -1 ? clickPurseNum : clickGrenadeNum;

                for (int i = 0; i < sortedList.Count; i++)
                {
                    var item = sortedList[i];
                    var playerIndex = -1;
                    var iconMatch = Regex.Match(item, @"^(1️⃣|2️⃣|3️⃣|4️⃣|5️⃣|6️⃣)");
                    if (iconMatch.Success)
                    {
                        switch (iconMatch.Value)
                        {
                            case "1️⃣":
                                playerIndex = 1;
                                break;
                            case "2️⃣":
                                playerIndex = 2;
                                break;
                            case "3️⃣":
                                playerIndex = 3;
                                break;
                            case "4️⃣":
                                playerIndex = 4;
                                break;
                            case "5️⃣":
                                playerIndex = 5;
                                break;
                            case "6️⃣":
                                playerIndex = 6;
                                break;
                            default:
                                break;
                        }
                    }

                    if (decimal.TryParse(list[i], out decimal boxAmount))
                    {
                        if (item == "🥡" || playerIndex >= clickIndex)
                        {
                            noOpenAndLaterTotal += boxAmount;
                        }
                    }
                }
            }

            //流入奖池额度:如果没人点到💣和💰且没人点开的盒子里的钱,鞭炮的70%
            decimal inflowPoolAmount = (openBoxPrice * 6) / 2;
            //庄家盈利:6个人开盒的一半+别人先点到💣
            decimal bankerProfit = ((openBoxPrice * 6) / 2) - boxAmountTotalAmount;

            //盈利流入奖池的0.05
            decimal profitInPoolAmount = 0;

            //玩家们分钱
            for (int i = 0; i < sortedList.Count; i++)
            {
                var playerIndex = -1;
                var iconMatch = Regex.Match(sortedList[i], @"^(1️⃣|2️⃣|3️⃣|4️⃣|5️⃣|6️⃣)");
                if (iconMatch.Success)
                {
                    switch (iconMatch.Value)
                    {
                        case "1️⃣":
                            playerIndex = 1;
                            break;
                        case "2️⃣":
                            playerIndex = 2;
                            break;
                        case "3️⃣":
                            playerIndex = 3;
                            break;
                        case "4️⃣":
                            playerIndex = 4;
                            break;
                        case "5️⃣":
                            playerIndex = 5;
                            break;
                        case "6️⃣":
                            playerIndex = 6;
                            break;
                        default:
                            break;
                    }
                }

                returnText += $"\n\n{sortedList[i]}";
                //盒子里有钱
                if (decimal.TryParse(list[i], out decimal boxAmount))
                {
                    returnText += $"  {boxAmount:0.00}U";

                    //钱被玩家|庄家拿去了
                    if (clickPurseNum > -1 || clickGrenadeNum > -1)
                    {
                        //玩家抢了
                        if (clickPurseNum > -1)
                        {
                            if (playerIndex >= clickPurseNum || sortedList[i] == "🥡")
                                returnText += $" 被<b>{moneyGrabIcon}</b>拿了";
                        }
                        //庄家抢了
                        else if (clickGrenadeNum > -1)
                        {
                            if (playerIndex >= clickGrenadeNum || sortedList[i] == "🥡")
                                returnText += $" 被庄拿了";
                        }
                    }
                    //无人点开的金额流入奖池
                    else if (sortedList[i] == "🥡")
                    {
                        returnText += " 流入奖池";
                        inflowPoolAmount += boxAmount;
                    }
                    //用户领到钱
                    else
                    {
                        var betHistory = betHistorys.ElementAt(playerIndex - 1);
                        //这里已经缴纳了0.05手续费了
                        profitInPoolAmount += boxAmount * 0.05M;
                        _ = await Helper.PlayerWinningFromOpponent(db, platform, game, betHistory, "盲盒", banker.PlayerId, boxAmount, "从开奖盲盒开到奖金", Convert.ToInt32(bankerFinanceHistory.GameMessageId), cancellationToken);
                    }
                }
                else
                {
                    returnText += $"  {list[i]}";
                    //序号列表是已有玩家点开的序号
                    if (sortedList[i] != "🥡")
                    {
                        //点到💰:获得后面未被开盒的金额 + 奖池58%奖金:如果没人点到💰,未打开盒子金额流入奖池
                        //点到💣:庄家可获得后面未被开盒金额 + 奖池28%奖金:庄家点到💣不作数,未打开盒子金额流入奖池
                        if (list[i] == "💰" && clickGrenadeNum > -1
                            || list[i] == "💣" && clickPurseNum > -1)
                        {
                            returnText += " 失效";
                        }
                        //点到🧨:赔本局盲盒总额的33.33%至奖池
                        else if (list[i] == "🧨")
                        {
                            //流入奖池
                            inflowPoolAmount += clickFirecrackerPrice;
                            returnText += $" 赔-{clickFirecrackerPrice:0.00}U";
                            //赌资另外增加鞭炮赔偿金
                            blindBoxGameHistory.BetAmount += clickFirecrackerPrice;

                            var betHistory = betHistorys.ElementAt(playerIndex - 1);
                            betHistory.Remark = $"开盲盒费用{openBoxPrice}U + 点到🧨的{clickFirecrackerPrice}U";
                        }
                    }
                }

                //没点到鞭炮:解冻返回鞭炮冻结
                if (playerIndex > -1 && list[i] != "🧨")
                {
                    var betHistory = betHistorys.ElementAt(playerIndex - 1);
                    betHistory.Amount -= clickFirecrackerPrice;
                    betHistory.Remark = $"开盲盒费用,(已返还了🧨的{clickFirecrackerPrice}U)";

                    var betHistoryPlayer = await db.Players.FirstAsync(u => u.PlayerId == betHistory.PlayerId, cancellationToken: cancellationToken);
                    betHistoryPlayer.Balance += clickFirecrackerPrice;
                }
            }

            if (noOpenAndLaterTotal > 0)
            {
                if (clickPurseNum > -1)
                {
                    //拿58%奖池
                    var poolAmount = gamePoolAmount > 0 ? gamePoolAmount * Convert.ToDecimal(0.58) : 0;
                    returnText += $"\n\n🎉 <b>第{moneyGrabIcon}玩家抢夺后面玩家盒子和未开盒的{noOpenAndLaterTotal:0.00}U+58%奖池{poolAmount:0.00}U</b>";
                    noOpenAndLaterTotal += poolAmount;
                    var betHistory = betHistorys.ElementAt(clickPurseNum - 1);
                    //这里已经缴纳了0.05手续费和流入0.05奖池了
                    profitInPoolAmount += noOpenAndLaterTotal * 0.05M;
                    _ = await Helper.PlayerWinningFromOpponent(db, platform, game, betHistory, "盲盒", banker.PlayerId, noOpenAndLaterTotal, "从开奖盲盒开到钱袋中大奖", Convert.ToInt32(bankerFinanceHistory.GameMessageId), cancellationToken);

                    game.PrizePool -= poolAmount;
                }
                else if (clickGrenadeNum > -1)
                {
                    //庄家拿了未开盒和后面的钱
                    bankerProfit += noOpenAndLaterTotal;
                    //拿28%奖池
                    var poolAmount = gamePoolAmount > 0 ? gamePoolAmount * Convert.ToDecimal(0.28) : 0;
                    returnText += $"\n\n🎉 <b>庄家获得{moneyGrabIcon}后面玩家盒子和未开盒的{noOpenAndLaterTotal:0.00}U+28%奖池{poolAmount:0.00}U</b>";
                    bankerProfit += poolAmount;
                    //这里已经缴纳了0.05手续费和流入0.05奖池了
                    profitInPoolAmount += noOpenAndLaterTotal * 0.05M;
                    _ = await Helper.PlayerWinningFromOpponent(db, platform, game, bankerFinanceHistory, "盲盒", banker.PlayerId, noOpenAndLaterTotal, "开奖盲盒坐庄盈利", Convert.ToInt32(bankerFinanceHistory.GameMessageId), cancellationToken);
                    bankerProfit -= (bankerProfit * 0.1M);
                    game.PrizePool -= poolAmount;
                }
            }

            game.PrizePool += inflowPoolAmount;
            returnText
                += $"\n\n<b>----------本局结果----------</b>"
                + $"\n\n<b>💸 流入奖池 : {(inflowPoolAmount + profitInPoolAmount):0.00}U 已累计{game.PrizePool:0.00}U</b>"
                + $"\n\n<b>🤑 庄家盈利 : {bankerProfit:0.00} USDT</b>";
            //信息按钮
            List<List<InlineKeyboardButton>> msgBtn = [];
            msgBtn.Add([
                InlineKeyboardButton.WithUrl("玩法", "https://t.me/ZuoDaoMianDian"),
                                                    InlineKeyboardButton.WithUrl("充值", "https://t.me/ZuoDaoMianDian"),
                                                    InlineKeyboardButton.WithUrl("余额", "https://t.me/ZuoDaoMianDian"),
                                                    InlineKeyboardButton.WithUrl("客服", "https://t.me/ZuoDaoMianDian")
            ]);
            msgBtn.Add([
                InlineKeyboardButton.WithUrl("发展下线", "https://t.me/ZuoDaoMianDian"),
                                                    InlineKeyboardButton.WithUrl("今日报表", "https://t.me/ZuoDaoMianDian")
            ]);

            //庄家发盲盒记录
            bankerFinanceHistory.FinanceStatus = FinanceStatus.Success;
            await Helper.SaveAppsettings();

            await db.SaveChangesAsync(cancellationToken);
            try
            {
                await botClient.EditMessageCaptionAsync(chatId: cq.Message.Chat.Id, cq.Message!.MessageId, returnText, parseMode: ParseMode.Html, null, new InlineKeyboardMarkup(msgBtn), cancellationToken);
            }
            catch (Exception ex)
            {
                Log.Error("点击抢红包返回信息时出错:" + ex.Message);
            }
        }
    }
}
