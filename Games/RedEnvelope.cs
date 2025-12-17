using Microsoft.EntityFrameworkCore;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace 皇冠娱乐.Games
{
    /// <summary>
    /// 红包
    /// </summary>
    public static class RedEnvelope
    {
        //系统10分钟派发一次,或者用户派发
        public static async Task Send(ITelegramBotClient botClient, DataContext db, Platform platform, long chatId, int gameId,
            Player player, decimal amount, Message? msg, int? threadId, CancellationToken cancellationToken)
        {
            //中雷尾数
            int lastNum = !string.IsNullOrEmpty(msg?.Text) ? Convert.ToInt32(msg.Text[(msg.Text.IndexOf('-') + 1)..]) : new Random().Next(0, 10);
            var senderName = msg?.From != null ? msg.From.FirstName + msg.From.LastName : "群主";
            var returnText =
                $"👤 <b>【{senderName}】</b>发了个红包,大家快来抢啊~" +
                $"\n\n🧧 <b>红包总额</b> : <b>{amount} USDT</b>" +
                $"\n\n💣 <b>领到尾数是 {lastNum} 的返 1.8 倍给包主</b>";

            //信息按钮
            List<List<InlineKeyboardButton>> msgBtn = [];
            msgBtn.Add([InlineKeyboardButton.WithCallbackData($"👉   抢{amount}U红包 0/6   👈", $"redEnvelope?playerId={player.PlayerId}&amount={amount}&lastNum={lastNum}&receipts=0")]);

            using var fileStream = new FileStream("抢红包.jpg", FileMode.Open, FileAccess.Read);
            var stream = new InputFileStream(content: fileStream, fileName: Path.GetFileName("抢红包.jpg"));

            try
            {
                var redEnvelopeMsg = await botClient.SendPhotoAsync(chatId, photo: stream, messageThreadId: threadId, parseMode: ParseMode.Html, caption: returnText, replyMarkup: new InlineKeyboardMarkup(msgBtn), cancellationToken: cancellationToken);
                var redEnvelopeGameHistory = new GameHistory
                {
                    Time = DateTime.UtcNow,
                    EndTime = DateTime.UtcNow.AddHours(2),
                    Status = GameHistoryStatus.Ongoing,
                    GroupId = chatId,
                    MessageThreadId = redEnvelopeMsg.MessageThreadId,
                    MessageId = redEnvelopeMsg.MessageId,
                    GameId = gameId,
                    CreatorId = platform.CreatorId,
                    CommissionRate = 0.05M,
                    BetAmount = amount,
                    PlayerId = player.PlayerId
                };
                await db.GameHistorys.AddAsync(redEnvelopeGameHistory, cancellationToken);
                await db.SaveChangesAsync(cancellationToken);
                var finance = new PlayerFinanceHistory
                {
                    Time = DateTime.UtcNow,
                    Name = senderName,
                    FinanceStatus = FinanceStatus.Freeze,
                    Type = FinanceType.RedEnvelope,
                    Remark = "发红包费用",
                    GameId = gameId,
                    GameMessageId = redEnvelopeMsg.MessageId,
                    PlayerId = player.PlayerId
                };
                _ = await Helper.MinusBalance(db, amount, player, finance, cancellationToken);
                await db.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                Log.Error("返回发红包信息时出错:" + ex.Message);
            }
        }

        //接收
        public static async Task Receive(ITelegramBotClient botClient, DataContext db, Platform platform, Game game, GameHistory gameHistory, Player player, Message msg,
           decimal amount, decimal compensationAmount, int receipts, long sendRedEnvelopePlayerId, int lastNum, CancellationToken cancellationToken)
        {
            //预先冻结,预防中雷没钱赔
            var finance = new PlayerFinanceHistory
            {
                Time = DateTime.UtcNow,
                Name = msg.From?.FirstName + msg.From?.LastName,
                FinanceStatus = FinanceStatus.Freeze,
                Type = FinanceType.RedEnvelope,
                Remark = "抢红包时,为了预防玩家无钱赔偿给发包者,需预先冻结红包金额的1.8倍金额,如果中雷将作为赔偿返给发包者,如果未中雷,本金额将原路返还给您!",
                GameId = game.Id,
                GameMessageId = msg!.MessageId,
                PlayerId = player.PlayerId
            };
            player = await Helper.MinusBalance(db, compensationAmount, player, finance, cancellationToken);

            var playerFinanceHistory = db.PlayerFinanceHistorys.Where(u => u.GameMessageId == msg.MessageId).OrderBy(u => u.Time).AsEnumerable();

            if (playerFinanceHistory.Count() < 6)
            {
                var sender = playerFinanceHistory.First();
                sender.FinanceStatus = FinanceStatus.Success;
                var returnText = $"👤 <b>【{sender.Name}】</b>发了个红包,大家快来抢啊~" +
                      $"\n\n🧧 <b>红包总额</b> : <b>{amount} USDT</b>" +
                      $"\n\n💣 <b>领到尾数是 {lastNum} 的返 1.8 倍给包主</b>" +
                      $"\n\n--------------<b>领取玩家</b>--------------";

                //信息按钮
                List<List<InlineKeyboardButton>> msgBtn = [];
                msgBtn.Add([InlineKeyboardButton.WithCallbackData($"👉   抢{amount}U红包 {receipts + 1}/6   👈", $"redEnvelope?playerId={sendRedEnvelopePlayerId}&amount={amount}&lastNum={lastNum}&receipts={receipts + 1}")]);

                try
                {
                    await botClient.EditMessageCaptionAsync(chatId: gameHistory.GroupId, msg!.MessageId, returnText, parseMode: ParseMode.Html, null, new InlineKeyboardMarkup(msgBtn), cancellationToken);

                }
                catch (Exception ex)
                {
                    Log.Error("点击抢红包返回信息时出错:" + ex.Message);
                }
                await Helper.SaveAppsettings();
                await db.SaveChangesAsync(cancellationToken);
            }
            else
            {
                await End(botClient, db, platform, game, gameHistory, msg, amount, lastNum, playerFinanceHistory, cancellationToken);
            }
        }

        //结束
        public static async Task End(ITelegramBotClient botClient, DataContext db, Platform platform, Game game, GameHistory gameHistory, Message msg,
           decimal amount, int lastNum, IEnumerable<PlayerFinanceHistory> playerFinanceHistory, CancellationToken cancellationToken)
        {
            var sender = playerFinanceHistory.First();
            sender.FinanceStatus = FinanceStatus.Success;
            //信息按钮
            List<List<InlineKeyboardButton>> msgBtn = [];

            var returnText = $"👤 <b>【{sender.Name}】</b>发了个红包,大家快来抢啊~" +
                  $"\n\n🧧 <b>红包总额</b> : <b>{amount} USDT</b>" +
                  $"\n\n💣 <b>领到尾数是 {lastNum} 的返 1.8 倍给包主</b>" +
                  $"\n\n--------------<b>领取玩家</b>--------------";

            //分隔的随机金额
            var amounts = playerFinanceHistory.Count() <= 6 ? [] : SplitIntIntoRandomDoubles(amount);
            //判断金额是否有顺子和豹子号的赔付重要值
            List<int> compensations = [];
            for (int i = 1; i < playerFinanceHistory.Count(); i++)
            {
                var item = playerFinanceHistory.ElementAt(i);

                string ranking = string.Empty;
                switch (i)
                {
                    case 0:
                        ranking = "1️⃣";
                        break;
                    case 1:
                        ranking = "2️⃣";
                        break;
                    case 2:
                        ranking = "3️⃣";
                        break;
                    case 3:
                        ranking = "4️⃣";
                        break;
                    case 4:
                        ranking = "5️⃣";
                        break;
                    case 5:
                        ranking = "6️⃣";
                        break;
                }

                if (playerFinanceHistory.Count() <= 5)
                {
                    returnText += $"\n\n{ranking} <b>待揭晓$</b> 💵 - <code>{item.Name}</code>";
                }
                else
                {
                    //获得的红包金额
                    var redEnvelopeAmount = amounts[i - 1];
                    var amountStr = redEnvelopeAmount.ToString().Replace(".", "");
                    var sequentialLength = Helper.IsSequential(redEnvelopeAmount);
                    var digitsSameLength = Helper.AllDigitsSame(redEnvelopeAmount);

                    //3顺子
                    if (sequentialLength == 3)
                    {
                        compensations.Add(7);
                    }
                    //4顺子
                    else if (sequentialLength == 4)
                    {
                        compensations.Add(158);
                    }
                    //5顺子
                    else if (sequentialLength == 5)
                    {
                        compensations.Add(1989);
                    }
                    //3豹子
                    else if (digitsSameLength == 3)
                    {
                        compensations.Add(5);
                    }
                    //4豹子
                    else if (digitsSameLength == 4)
                    {
                        compensations.Add(105);
                    }
                    //5豹子
                    else if (digitsSameLength == 5)
                    {
                        compensations.Add(1105);
                    }
                    else
                    {
                        compensations.Add(0);
                    }

                    var icon = redEnvelopeAmount.ToString().EndsWith(lastNum.ToString()) ? "💣" : "💵";
                    returnText += $"\n\n{ranking} <b>{redEnvelopeAmount} $</b> {icon} - <code>{item.Name}</code>";

                    var gamePlayer = await Helper.PlayerWinningFromOpponent(db, platform, game, item, "红包", Convert.ToInt32(gameHistory.PlayerId), redEnvelopeAmount, "红包盈利", msg!.MessageId, cancellationToken, 0.05M, 0.05M);

                    //盈利了
                    if (icon == "💵")
                    {
                        //解冻抢红包时的中雷红包
                        item.FinanceStatus = FinanceStatus.Return;
                        gamePlayer.Balance += item.Amount;
                        gamePlayer.RewardBalance += item.BonusAmount;
                    }
                    //中雷了
                    else
                    {
                        item.FinanceStatus = FinanceStatus.Success;
                    }
                }
            }

            if (compensations.Any(u => u > 0))
            {
                returnText += $"\n\n----------🎉<b>幸运玩家</b>🎉----------";

                //总赔偿倍数
                var totalMultiple = compensations.Sum();
                //奖池里要扣除的金额
                decimal deductPoolAmount = 0;
                //判断金额是否存在豹子和顺子
                for (int i = 1; i < playerFinanceHistory.Count(); i++)
                {
                    //赔偿占比数值
                    var multiple = compensations[i - 1];
                    if (multiple == 0)
                        continue;

                    //财务记录
                    var item = playerFinanceHistory.ElementAt(i);
                    //金额字符串长度
                    var redEnvelopeAmount = amounts[i - 1].ToString().Replace(".", "");
                    //奖金
                    decimal bonus = 0;
                    //备注
                    string remark = string.Empty;
                    //玩家
                    var gamePlayer = await db.Players.FirstAsync(u => u.PlayerId == item.PlayerId && u.CreatorId == platform.CreatorId, cancellationToken: cancellationToken);
                    //顺子还是豹子
                    var shunOrBao = string.Empty;
                    if (compensations.Count(u => u > 0) == 1)
                    {
                        //如果领的红包金额是本局玩家中唯一的顺子或者豹子:
                        //3顺 / 豹 清空30%奖池
                        //4顺 / 豹 清空50%奖池
                        //5顺 / 豹 清空70%奖池
                        switch (redEnvelopeAmount.Length)
                        {
                            case 3:
                                bonus = game.PrizePool * Convert.ToDecimal(0.3);
                                shunOrBao = multiple == 7 ? "顺子" : "豹子";
                                remark = $"抢红包时金额数是3{shunOrBao},奖励奖池的30%奖金";
                                returnText += $"\n\n🎉 {item.Name} - 红包金额:<b>{amounts[i - 1]}</b>💲 (3顺子) 独享奖池30%奖池奖金<b>{bonus} USDT</b>";
                                break;
                            case 4:
                                bonus = game.PrizePool * Convert.ToDecimal(0.5);
                                shunOrBao = multiple == 158 ? "顺子" : "豹子";
                                remark = $"抢红包时金额数是4{shunOrBao},奖励奖池的50%奖金";
                                returnText += $"\n\n🎉 {item.Name} - 红包金额:<b>{amounts[i - 1]}</b>💲 (4顺子) 独享奖池50%奖池奖金<b>{bonus} USDT</b>";
                                break;
                            case 5:
                                bonus = game.PrizePool * Convert.ToDecimal(0.7);
                                shunOrBao = multiple == 1989 ? "顺子" : "豹子";
                                remark = $"抢红包时金额数是5{shunOrBao},奖励奖池的70%奖金";
                                returnText += $"\n\n🎉 {item.Name} - 红包金额:<b>{amounts[i - 1]}</b>💲 (5顺子) 独享奖池70%奖池奖金<b>{bonus} USDT</b>";
                                break;
                            default:
                                break;
                        }
                        deductPoolAmount = bonus;
                    }
                    else
                    {
                        //如果遇到和他人同样存在顺子或者豹子时清空70 % 奖池
                        //根据数值比例瓜分奖池: 比如一人7,一人5,那就是7 / 12 = 58.333 % 5 / 12 = 41.66 %
                        //金额类型   瓜分比例
                        //    3顺   7
                        //    4顺   158
                        //    5顺   1989
                        //    3豹   5
                        //    4豹   105
                        //    5豹   1105

                        //可以瓜分奖池里的70%
                        var canCarveUpBonus = game.PrizePool * Convert.ToDecimal(0.7);
                        //和他人瓜分占比
                        var proportion = Convert.ToDecimal(multiple) / Convert.ToDecimal(totalMultiple);
                        bonus = canCarveUpBonus * proportion;
                        switch (redEnvelopeAmount.Length)
                        {
                            case 3:
                                shunOrBao = multiple is 7 or 5 ? "顺子" : "豹子";
                                remark = $"抢红包时金额数是3{shunOrBao},和他人瓜分奖池的70%奖金,您可获得" + bonus + "USDT";
                                returnText += $"\n\n🎉 {item.Name} - 红包金额:<b>{amounts[i - 1]}</b>💲 (3{shunOrBao}) 和他人瓜分总奖池里的70%奖金,获得{proportion:0.00}%≈<b>{bonus} USDT</b>";
                                break;
                            case 4:
                                shunOrBao = multiple is 158 or 105 ? "顺子" : "豹子";
                                remark = $"抢红包时金额数是4{shunOrBao},和他人瓜分奖池的70%奖金,您可获得" + bonus + "USDT";
                                returnText += $"\n\n🎉 {item.Name} - 红包金额:<b>{amounts[i - 1]}</b>💲 (4{shunOrBao}) 和他人瓜分总奖池里的70%奖金,获得{proportion:0.00}%≈<b>{bonus} USDT</b>";
                                break;
                            case 5:
                                shunOrBao = multiple is 1989 or 1105 ? "顺子" : "豹子";
                                remark = $"抢红包时金额数是5{shunOrBao},和他人瓜分奖池的70%奖金,您可获得" + bonus + "USDT";
                                returnText += $"\n\n🎉 {item.Name} - 红包金额:<b>{amounts[i - 1]}</b>💲 (5{shunOrBao}) 和他人瓜分总奖池里的70%奖金,获得{proportion:0.00}%≈<b>{bonus} USDT</b>";
                                break;
                            default:
                                break;
                        }
                        deductPoolAmount += bonus;
                    }

                    gamePlayer.Balance += bonus;
                    await db.PlayerFinanceHistorys.AddAsync(new PlayerFinanceHistory
                    {
                        FinanceStatus = FinanceStatus.Success,
                        Time = DateTime.UtcNow,
                        Type = FinanceType.RedEnvelope,
                        Amount = bonus,
                        Remark = remark,
                        OtherId = Convert.ToInt32(gameHistory.PlayerId),
                        GameId = game.Id,
                        GameMessageId = msg!.MessageId,
                        PlayerId = gamePlayer.PlayerId
                    }, cancellationToken);
                }
                //扣除奖池里的金额
                game.PrizePool -= deductPoolAmount;
            }

            //中雷人数
            var losers = amounts.Count(u => u.ToString().EndsWith(lastNum.ToString()));
            if (losers > 0)
            {
                //赚了多少钱
                var profit = Convert.ToDecimal(losers) * amount * Convert.ToDecimal(1.8) - amount;
                returnText += $"\n\n🎉 <b>本局红包已抢完!包主盈利</b> : <b>{profit}</b> 💲";
                //发包者赚的金额
                var sendPlayerProfit = Convert.ToDecimal(losers) * amount * Convert.ToDecimal(1.8);
                //发包者
                _ = await Helper.PlayerWinningFromOpponent(db, platform, game, sender, "红包", sender.PlayerId, sendPlayerProfit, $"发红包有{losers}个玩家金额尾数{lastNum}中雷了,1.8倍返给您", msg!.MessageId, cancellationToken, 0.05M, 0.05M);
            }
            else
            {
                returnText += $"\n\n🎉 <b>本局红包已抢完!包主未盈利,抢包玩家大获全胜!</b>";
            }

            returnText += $"\n\n❤️ 为更好服务众玩家,对本局盈利玩家收取盈利的5%作手续费";

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

            gameHistory.Status = GameHistoryStatus.End;
            gameHistory.EndTime = DateTime.UtcNow;
            gameHistory.BetAmount = amount + (Convert.ToDecimal(losers) * amount * Convert.ToDecimal(1.8));
            try
            {
                await botClient.EditMessageCaptionAsync(chatId: gameHistory.GroupId, msg!.MessageId, returnText, parseMode: ParseMode.Html, null, new InlineKeyboardMarkup(msgBtn), cancellationToken);

            }
            catch (Exception ex)
            {
                Log.Error("点击抢红包返回信息时出错:" + ex.Message);
            }
            await Helper.SaveAppsettings();
            await db.SaveChangesAsync(cancellationToken);
        }

        static readonly Random random = new();

        /// <summary>
        /// 随机将红包金额拆分成6个随机数
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
        static decimal[] SplitIntIntoRandomDoubles(decimal num)
        {
            decimal[] result = new decimal[6];
            // 生成5个随机数
            for (int i = 0; i < 5; i++)
            {
                decimal max = num / (6 - i); // 调整随机数的范围
                decimal randomDouble = Convert.ToDecimal(random.NextDouble()) * max;
                randomDouble = Math.Round(randomDouble, 2);
                result[i] = randomDouble;
                num -= randomDouble;
            }

            // 最后一个数保证和为原始整数
            result[5] = Math.Round(num, 2);
            return result;
        }
    }
}
