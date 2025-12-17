using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System.ComponentModel;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
//using Tron.TronGridV1Api;
using static System.Net.Mime.MediaTypeNames;

namespace 皇冠娱乐
{
    public static class Helper
    {
        /// <summary>
        /// 生成助记词
        /// </summary>
        /// <param name="input">传入用户的CreaterId+</param>
        /// <returns></returns>
        public static string ComputeSHA256Hash(string input)
        {
            byte[] inputBytes = Encoding.UTF8.GetBytes(input);
            byte[] hashBytes = System.Security.Cryptography.SHA256.HashData(inputBytes);

            StringBuilder builder = new();
            for (int i = 0; i < hashBytes.Length; i++)
            {
                builder.Append(hashBytes[i].ToString("x2"));
            }

            return builder.ToString();
        }

        /// <summary>
        /// 正则提取博彩平台私钥
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string ExtractHash(string input)
        {
            Match match = Regex.Match(input, "^[A-Fa-f0-9]{64}");
            return match.Success ? match.Value : string.Empty;
        }

        /// <summary>
        /// 是否能链接到聊天
        /// </summary>
        /// <param name="chatId"></param>
        /// <returns></returns>
        public static bool IsConnectionUserChat(ITelegramBotClient botClient, string chatId)
        {
            try
            {
                var chatMember = botClient.GetChatAsync(chatId).Result;
                return chatMember != null;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 保存配置
        /// </summary>
        /// <returns></returns>
        public static async Task SaveAppsettings()
        {
            var json = JsonConvert.SerializeObject(Program._appsettings);
        save:
            try
            {
                await System.IO.File.WriteAllTextAsync("appsettings.json", json);
            }
            catch (Exception ex)
            {
                Log.WriteLine("保存appsettings.json时出错" + ex.Message);
                goto save;
            }
        }

        //获取USDT入账记录  1000000=1U
        public static async Task<List<Data>> GetUsdtTransferInList()
        {
            string? result;
        getUsdtTransferInList:
            try
            {
                result = await AccountsTransactions.GetContractTransactionInfo(address: Program._appsettings.TronWalletAddress, limit: 50, contractAddress: Program._tronUsdtContraAddress, onlyTo: true);
            }
            catch (Exception ex)
            {
                Console.WriteLine("获取USDT进账记录时出错:" + ex.Message);
                goto getUsdtTransferInList;
            }
            var resultObj = JsonConvert.DeserializeObject<UsdtTransferInList>(result);
            if (resultObj == null)
                goto getUsdtTransferInList;

            return resultObj.Data.Where(u => u.Type == "Transfer").ToList();
        }

        //过滤字符
        public static string AddBackslash(string input)
        {
            // 定义需要检查的字符数组
            char[] specialChars = ['_', '*', '[', ']', '(', ')', '~', '`', '>', '#', '+', '-', '=', '|', '{', '}', '.', '!'];

            // 对输入字符串进行遍历，如果包含特定字符，则在前面加上反斜杠
            foreach (char specialChar in specialChars)
            {
                if (input.Contains(specialChar.ToString()))
                {
                    input = input.Replace(specialChar.ToString(), "\\" + specialChar);
                }
            }

            return input;
        }

        //保持用户地址隐私
        public static string MaskString(string input)
        {
            var end = input.Substring(input.Length - 8, 8);
            return input[..8] + @"\*\*\*" + end;
        }

        //获取汇率
        public static async Task<Dictionary<string, decimal>> GetCurrencys(string currency)
        {
            var http = new HttpClient();
            dynamic? resultObj;
        st:
            try
            {
                var result = await http.GetStringAsync("https://api.exchangerate-api.com/v4/latest/" + currency);
                resultObj = JsonConvert.DeserializeObject<dynamic>(result);
            }
            catch
            {
                goto st;
            }
            return JsonConvert.DeserializeObject<Dictionary<string, decimal>>(JsonConvert.SerializeObject(resultObj!.rates));
        }

        //获取聊天信息
        public static async Task<Chat?> GetChatInfo(ITelegramBotClient botClient, long chatId)
        {
            Chat? chat = null;
            try
            {
                chat = await botClient.GetChatAsync(chatId);
            }
            catch (Exception ex)
            {
                Log.Error("获取ChatInfo时出错:" + ex.Message);
            }
            return chat;
        }

        /// <summary>
        /// 获取枚举描述
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string GetEnumDescription(Enum value)
        {
            var field = value.GetType().GetField(value.ToString());

            if (field != null)
            {
                if (Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) is DescriptionAttribute attribute)
                {
                    return attribute.Description;
                }
            }

            // 如果没有描述属性，则返回枚举值的字符串表示
            return value.ToString();
        }

        //删除别人信息
        public static void DeleteMessage(ITelegramBotClient botClient, Update update, int second, string text, CancellationToken cancellationToken)
        {
            _ = Task.Run(async () =>
            {
                if (update.CallbackQuery != null)
                {
                    try
                    {
                        await botClient.AnswerCallbackQueryAsync(update.CallbackQuery.Id, $"🚫 {text}", true, null, 0);
                    }
                    catch (Exception ex)
                    {
                        Log.Error($"机器人:{botClient.BotId}向用户发送错误的AnswerCallbackQueryAsync时出错:" + ex.Message);
                    }
                }
                else if (update.Message != null)
                {
                    if (second == 0)
                    {
                        try
                        {
                            await botClient.DeleteMessageAsync(update.Message!.Chat.Id, update.Message!.MessageId);
                        }
                        catch (Exception ex)
                        {
                            Log.Error($"机器人:{botClient.BotId}删除别人的消息时出错:" + ex.Message);
                        }
                    }
                    else
                    {
                        Message? msg = null;
                        try
                        {
                            msg = await botClient.SendTextMessageAsync(update.Message!.Chat.Id, $"🚫 {text}", parseMode: ParseMode.Html, replyToMessageId: update.Message?.MessageId, disableWebPagePreview: true);
                        }
                        catch (Exception e)
                        {
                            Log.Error("他人发了信息,告知对方违规信息时出错:" + e.Message);
                        }

                        if (msg == null)
                            return;

                        await Task.Delay(second * 1000);

                        try
                        {
                            await botClient.DeleteMessageAsync(update.Message!.Chat.Id, update.Message.MessageId);
                        }
                        catch (Exception e)
                        {
                            Log.Error("删除他人违规信息时出错:" + e.Message);
                        }

                        try
                        {
                            await botClient.DeleteMessageAsync(update.Message!.Chat.Id, msg.MessageId);
                        }
                        catch (Exception e)
                        {
                            Log.Error("他人发了违规信息,删除通知他人的信息时出错:" + e.Message);
                        }
                    }
                }
            }, cancellationToken);
        }

        //机器人出错后执行的方法
        public static Task PollingErrorHandler(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            var ErrorMessage = exception switch
            {
                ApiRequestException apiRequestException => $"机器人Id:{botClient.BotId} Telegram API错误:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.ToString()
            };
            Log.Error("机器人PollingErrorHandler报错:" + ErrorMessage);
            return Task.CompletedTask;
        }

        /// <summary>
        /// 减玩家余额(先减彩金,再减余额)
        /// </summary>
        /// <param name="amount">要减的额度</param>
        /// <param name="rewardBalance">平台赠送给玩家的彩金</param>
        /// <param name="balance">玩家余额</param>
        /// <returns></returns>
        public static async Task<Player> MinusBalance(DataContext db, decimal amount, Player player, PlayerFinanceHistory finance, CancellationToken cancellationToken)
        {
            if (amount > (player.RewardBalance + player.Balance))
                throw new Exception("在使用这个方法前,先判断下要减余额是否大于彩金和余额");

            //彩金比较多
            if (player.RewardBalance >= amount)
            {
                player.RewardBalance -= amount;
                finance.BonusAmount = amount;
            }
            else
            {
                finance.BonusAmount = player.RewardBalance;
                //剩余,还需要从余额里减
                var remaining = amount - player.RewardBalance;
                player.RewardBalance = 0;

                finance.Amount = remaining;
                player.Balance -= remaining;
            }

            await db.PlayerFinanceHistorys.AddAsync(finance, cancellationToken);
            return player;
        }

        /// <summary>
        /// 判断红包金额字符数字是否顺序
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static int IsSequential(decimal amount)
        {
            var input = amount.ToString().Replace(".", "");
            for (int i = 0; i < input.Length - 1; i++)
            {
                if (input[i + 1] != input[i] + 1)
                    return 0;
            }
            return input.Length;
        }

        /// <summary>
        /// 判断红包金额字符串数字是否相同
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static int AllDigitsSame(decimal amount)
        {
            var input = amount.ToString().Replace(".", "");
            for (int i = 0; i < input.Length - 1; i++)
            {
                if (input[i] != input[i + 1])
                    return 0;
            }
            return input.Length;
        }

        /// <summary>
        /// 判断是否下注到重复的数字
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static bool IsRepeatBet(string text)
        {
            var match = Regex.Match(text, @"(?<=-).+") ?? throw new Exception("输入字符串有误");
            var value = match.Value.Replace("^", "");
            string[] array = [];
            if (value.Contains('/'))
            {
                array = value.Split('/');
            }
            else if (value.Contains('+'))
            {
                array = value.Split('+');
            }
            else if (value.Contains('&'))
            {
                array = value.Split('&');
            }
            else if (value.Contains('>'))
            {
                array = value.Split('>');
            }
            else if (value.Contains(';'))
            {
                array = value.Split(';');

                //不可以同一个位置投放多个号
                //一个号不可以投放多个位置
                HashSet<string> num = [];
                HashSet<string> pos = [];
                foreach (var item in array)
                {
                    var sp = item.Split('=');
                    if (num.Contains(sp[0]) || pos.Contains(sp[1]))
                        return true;

                    num.Add(sp[0]);
                    pos.Add(sp[1]);
                }
            }

            return array.GroupBy(x => x).Any(g => g.Count() > 1);
        }

        /// <summary>
        /// 判断是否顺子
        /// </summary>
        /// <param name="num1"></param>
        /// <param name="num2"></param>
        /// <param name="num3"></param>
        /// <returns></returns>
        public static bool AreConsecutive(int num1, int num2, int num3)
        {
            // 首先判断三个数字是否互不相同
            if (num1 != num2 && num1 != num3 && num2 != num3)
            {
                // 判断是否是连续的数字
                int[] nums = { num1, num2, num3 };
                Array.Sort(nums);
                return nums[1] == nums[0] + 1 && nums[2] == nums[1] + 1;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 判断是否对子
        /// </summary>
        /// <param name="num1"></param>
        /// <param name="num2"></param>
        /// <param name="num3"></param>
        /// <returns></returns>
        public static bool HasExactlyTwoSameNumbers(int num1, int num2, int num3)
        {
            return (num1 == num2 && num1 != num3) || (num1 == num3 && num1 != num2) || (num2 == num3 && num1 != num2);
        }

        //生成横幅图片
        public static void CombineImages(string gameName, params string[] imagePaths)
        {
            List<Image<Rgba32>> images = [];

            // 加载图片
            using Image<Rgba32> topImage = SixLabors.ImageSharp.Image.Load<Rgba32>(gameName + "本期开奖横幅.jpg");
            // 加载剩余图片
            foreach (string path in imagePaths)
            {
                var size = topImage.Width / imagePaths.Length;
                images.Add(ResizeImage(path, size, size));
            }

            // 确定拼接后图片的尺寸
            int width = Math.Max(topImage.Width, images.Sum(img => img.Width));
            int height = topImage.Height + images.Max(img => img.Height);

            // 创建白底图片
            Image<Rgba32> whiteBackground = new(width, height);
            whiteBackground.Mutate(ctx => ctx.BackgroundColor(SixLabors.ImageSharp.Color.White));

            Image<Rgba32> outputImage = new(width, height);
            // 绘制白底图片
            outputImage.Mutate(ctx => ctx.DrawImage(whiteBackground, new Point(0, 0), 1f));
            // 绘制第一张图片在顶部
            outputImage.Mutate(ctx => ctx.DrawImage(topImage, new Point(0, 0), 1f));

            // 绘制剩余图片
            int currentX = 0;
            int currentY = topImage.Height;
            foreach (Image<Rgba32> image in images)
            {
                outputImage.Mutate(ctx =>
                {
                    ctx.DrawImage(image, new Point(currentX, currentY), 1f);
                });
                currentX += image.Width;
            }
            outputImage.Save(gameName + "开奖图.jpg");
        }

        /// <summary>
        /// 调整图片大小
        /// </summary>
        /// <param name="imagePath"></param>
        /// <param name="targetWidth"></param>
        /// <param name="targetHeight"></param>
        /// <returns></returns>
        public static Image<Rgba32> ResizeImage(string imagePath, int targetWidth, int targetHeight)
        {
            // 加载图片
            using Image<Rgba32> originalImage = SixLabors.ImageSharp.Image.Load<Rgba32>(imagePath);
            // 计算调整比例
            float widthRatio = (float)targetWidth / originalImage.Width;
            float heightRatio = (float)targetHeight / originalImage.Height;
            float ratio = Math.Min(widthRatio, heightRatio);

            // 计算调整后的宽度和高度
            int newWidth = (int)(originalImage.Width * ratio);
            int newHeight = (int)(originalImage.Height * ratio);

            // 调整大小并返回新图像
            return originalImage.Clone(x => x.Resize(newWidth, newHeight));
        }

        //下注
        public static async Task Betting(DataContext db, decimal amount, Platform platform, Game game, Player player, Message msg, FinanceType financeType, CancellationToken cancellationToken)
        {
#warning 要考虑玩家是否试玩模式

            var gameHistory = new GameHistory
            {
                BetAmount = amount,
                ClosingTime = DateTime.UtcNow,
                CommissionRate = 0.05M,
                CreatorId = platform.CreatorId,
                //EndTime,
                GameId = game.Id,
                GroupId = Convert.ToInt64(platform.GroupId),
                //LotteryDrawJson,
                MessageId = msg.MessageId,
                MessageThreadId = msg.MessageThreadId,
                Profit = 0,
                Status = GameHistoryStatus.Ongoing,
                Time = DateTime.UtcNow
            };
            await db.GameHistorys.AddAsync(gameHistory, cancellationToken);

            var playerFinance = new PlayerFinanceHistory
            {
                Amount = amount,
                CommissionAmount = amount * Convert.ToDecimal(0.05),
                FinanceStatus = FinanceStatus.Success,
                GameId = game.Id,
                GameMessageId = msg.MessageId,
                Name = msg.From?.FirstName + msg.From?.LastName,
                PlayerId = player.PlayerId,
                Remark = "下注",
                Time = DateTime.UtcNow,
                Type = financeType
            };
            //扣钱
            _ = await MinusBalance(db, amount, player, playerFinance, cancellationToken);
            try
            {
                await db.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                Log.Error("下注保存数据库时出错:" + ex.Message);
            }

        }

        /// <summary>
        /// 用户从平台赢钱了(玩家和平台博弈)
        /// </summary>
        /// <param name="db">数据库</param>
        /// <param name="platform">平台</param>
        /// <param name="game">主题游戏</param>  
        /// <param name="gameHistory">游戏*期记录(数据库里提取的,如果没有,要先增加到数据库)</param>
        /// <param name="playerFinance">玩家财务记录(数据库里提取的,如果没有,要先增加到数据库)</param>
        /// <param name="gameName">游戏名称</param>
        /// <param name="multiple">赔偿倍数</param>
        /// <param name="pfjp">获得奖池资金占比</param>
        /// <param name="pjp">盈利百分比扔进奖池</param>
        /// <param name="handlingFeeRatio">手续费占比</param>
        /// <returns></returns>
        public static async Task PlayerWinningFromPlatform(
            DataContext db,
            Platform platform,
            Game game,
            GameHistory gameHistory,
            PlayerFinanceHistory playerFinance,
            string gameName,
            decimal multiple,
            double pfjp = 0,
            double pjp = 0.05,
            double handlingFeeRatio = 0.05)
        {
#warning 要考虑玩家是否试玩模式

            gameHistory.EndTime = DateTime.UtcNow;
            gameHistory.Status = GameHistoryStatus.End;
            var botClient = Program._botClientList.First(u => u.BotId == platform.BotId);
            //下注金额
            var betAmount = playerFinance.Amount + playerFinance.BonusAmount;
            //赔偿金额
            var bonusAmount = Math.Round(betAmount * multiple, 2, MidpointRounding.AwayFromZero);
            //5%手续费
            var handlingFee = bonusAmount * Convert.ToDecimal(handlingFeeRatio);
            //扔进奖池金额
            var poolIncreaseAmount = pjp > 0 ? bonusAmount * Convert.ToDecimal(pjp) : 0;
            //奖池增加
            game.PrizePool += poolIncreaseAmount;
            //瓜分奖池额度
            var reducePoolAmount = pfjp > 0 ? game.PrizePool * Convert.ToDecimal(pfjp) : 0;
            game.PrizePool -= reducePoolAmount;
            //游戏亏赔偿金
            game.Profit -= bonusAmount;

            //最终可得到金额
            var bonus = bonusAmount - handlingFee - poolIncreaseAmount + reducePoolAmount;

            //玩家获得赔偿金
            var player = await db.Players.FirstAsync(u => u.PlayerId == playerFinance.PlayerId);
            player.Balance += bonus;
            await db.PlayerFinanceHistorys.AddAsync(new PlayerFinanceHistory
            {
                FinanceStatus = FinanceStatus.Success,
                Time = DateTime.UtcNow,
                Type = playerFinance.Type,
                Name = playerFinance.Name,
                Amount = bonus,
                CommissionAmount = 0.05M,
                Remark = $"{gameName} {betAmount}U买{playerFinance.Remark}中奖,克扣{handlingFeeRatio * 100}%手续费,扔进奖池{pjp * 100}%,从奖池瓜分{pfjp * 100}%奖金,最终中奖{bonus}U"
            });

            //邀请者也从手续费那里获得分成
            if (player.InviterId != null)
            {
                //邀请者盈利
                var inviterProfit = platform.Dividend * handlingFee;

                var inviter = await db.Players.FirstAsync(u => u.PlayerId == player.InviterId);
                inviter.Balance += inviterProfit;
                //剩余可以平台和皇冠平分的金额=缴纳的手续费 - 邀请者盈利
                handlingFee -= inviterProfit;
                //奖励邀请者
                gameHistory.RewardInviterAmount += inviterProfit;
                await db.PlayerFinanceHistorys.AddAsync(new PlayerFinanceHistory
                {
                    FinanceStatus = FinanceStatus.Success,
                    Time = DateTime.UtcNow,
                    Type = FinanceType.PromotionProfit,
                    Name = playerFinance.Name,
                    Amount = inviterProfit,
                    Remark = "推广获利",
                    GameId = game.Id,
                    GameMessageId = gameHistory.MessageId,
                    PlayerId = inviter.PlayerId
                });
            }

            //平台赚手续费 (扣掉了皇冠占比分成)
            var platformProfit = handlingFee * (1 - Program._appsettings.BettingThreadDividend);
            //赚了手续费(扣掉邀请者分成和皇冠占比分成后) 
            platform.Profit += platformProfit;
            //亏了赔偿金
            platform.Profit -= bonusAmount;

            //赚手续费(减掉了皇冠占比的) 
            game.Profit += platformProfit;

            //本局亏赔偿金
            gameHistory.Profit -= bonusAmount;
            //赚手续费(减掉了皇冠占比的)
            gameHistory.Profit += platformProfit;

            //皇冠盈利 = 手续费 - 平台盈利
            var zuodaoProfit = handlingFee - platformProfit;
            Program._appsettings.Profit += zuodaoProfit;
            platform.Balance -= zuodaoProfit;
#warning 平台财务 (这个在投注\红包\\\等等也要写)
            var platformFinance = new PlatformFinanceHistory
            {
                FinanceStatus = FinanceStatus.Success,
                Amount = -zuodaoProfit,
                Time = DateTime.UtcNow,
                Type = FinanceType.DividedInto,
                Remark = $"用户Id:{player.UserId} 在平台购买{gameName}中奖了,对其收取其5%手续费,按照皇冠分成占比{Program._appsettings.BettingThreadDividend * 100}%,皇冠应收取平台{zuodaoProfit}U",
                CreatorId = platform.CreatorId
            };
            await db.PlatformFinanceHistorys.AddAsync(platformFinance);

            //不够扣,关闭平台
            if (platform.Balance < zuodaoProfit)
            {
                platform.PlatformStatus = PlatformStatus.Close;

                try
                {
                    await botClient.SendTextMessageAsync(platform.CreatorId, "平台在皇冠的余额不足,请及时缴纳保持平台机器人正常运行,现因余额为0对您的平台进行关闭,并暂停关闭群所有话题,尽请知晓!有任何疑问请向 @ZuoDao_KeFuBot 咨询!");
                }
                catch (Exception ex)
                {
                    Log.WriteLine("扣平台费用不够,通知平台主时出错:" + ex.Message);
                }

                //关闭本平台所有话题
                var platformGameThreadIds = db.Games.Where(u => u.CreatorId == platform.CreatorId).Select(u => u.ThreadId);
                foreach (var platformGameThreadId in platformGameThreadIds)
                {
                    try
                    {
                        await botClient.CloseForumTopicAsync(platform.GroupId!, Convert.ToInt32(platformGameThreadId));
                    }
                    catch (Exception ex)
                    {
                        Log.Error("扣平台费用不够,关闭所有话题时出错:" + ex.Message);
                    }
                }

                try
                {
                    await botClient.CloseGeneralForumTopicAsync(platform.GroupId!);
                }
                catch (Exception ex)
                {
                    Log.Error("扣平台费用不够,关闭群常规话题时出错:" + ex.Message);
                }
            }
        }

        /// <summary>
        /// 玩家从玩家之前博弈对手那里赢钱了
        /// </summary>
        /// <param name="db">数据库</param>
        /// <param name="platform">平台</param>
        /// <param name="game">游戏</param>
        /// <param name="betHistory">下注记录</param>
        /// <param name="gameName">游戏名称</param>
        /// <param name="organizerId">本局博弈创建者Id</param>
        /// <param name="amount">盈利额度</param>
        /// <param name="financeRemark">财务备注</param>
        /// <param name="messageId">消息Id</param>
        /// <param name="cancellationToken"></param>
        /// <param name="handlingFeeRate">手续费率</param>
        /// <param name="prizePoolRate">进入奖池百分比</param>
        /// <returns></returns>
        public static async Task<Player> PlayerWinningFromOpponent(
            DataContext db,
            Platform platform,
            Game game, 
            PlayerFinanceHistory betHistory,
            string gameName, 
            int organizerId, 
            decimal amount, 
            string financeRemark,
            int messageId,
            CancellationToken cancellationToken,
            decimal handlingFeeRate = 0.05M, 
            decimal prizePoolRate = 0.05M)
        {
            //玩家投注了后获利
            var player = await db.Players.FirstAsync(u => u.PlayerId == betHistory.PlayerId && u.CreatorId == platform.CreatorId, cancellationToken: cancellationToken);
            player.Balance += amount;

            var playerFinance = new PlayerFinanceHistory
            {
                FinanceStatus = FinanceStatus.Success,
                Time = DateTime.UtcNow,
                Type = betHistory.Type,
                Amount = amount,
                CommissionAmount = amount * handlingFeeRate,
                Remark = financeRemark,
                OtherId = organizerId,
                GameId = game.Id,
                GameMessageId = messageId,
                PlayerId = player.PlayerId
            };
            await db.PlayerFinanceHistorys.AddAsync(playerFinance, cancellationToken);

            //盈利部分进入奖池
            if (prizePoolRate > 0)
            {
                var prizePoolAmount = amount * prizePoolRate;
                var poolFeeFinance = new PlayerFinanceHistory
                {
                    FinanceStatus = FinanceStatus.Success,
                    Time = DateTime.UtcNow,
                    Type = FinanceType.PoolFee,
                    Amount = -prizePoolAmount,
                    Remark = gameName + "盈利缴纳" + (prizePoolRate * 100) + "%投入奖池",
                    OtherId = organizerId,
                    GameId = game.Id,
                    GameMessageId = messageId,
                    PlayerId = player.PlayerId
                };
                player = await MinusBalance(db, prizePoolAmount, player, poolFeeFinance, cancellationToken);
                game.PrizePool += prizePoolAmount;
            }

            //平台收取手续费
            if (handlingFeeRate > 0)
            {
                //手续费
                var handlingFeeAmount = amount * handlingFeeRate;
                var handlingFeeFinance = new PlayerFinanceHistory
                {
                    FinanceStatus = FinanceStatus.Success,
                    Time = DateTime.UtcNow,
                    Type = FinanceType.HandlingFee,
                    Amount = -handlingFeeAmount,
                    Remark = gameName + "盈利缴纳" + (handlingFeeRate * 100) + "%手续费",
                    OtherId = organizerId,
                    GameId = game.Id,
                    GameMessageId = messageId,
                    PlayerId = player.PlayerId
                };
                player = await MinusBalance(db, handlingFeeAmount, player, handlingFeeFinance, cancellationToken);

                //邀请者盈利:从平台赚玩家的那里抽取
                if (player.InviterId != null)
                {
                    //邀请者盈利
                    var inviterProfit = platform.Dividend * handlingFeeAmount;
                   
                    var inviter = await db.Players.FirstAsync(u => u.PlayerId == player.InviterId, cancellationToken: cancellationToken);
                    inviter.Balance += inviterProfit;
                    
                    //剩余可以平台和皇冠平分的金额=缴纳的手续费 - 邀请者盈利
                    handlingFeeAmount -= inviterProfit;

                    await db.PlayerFinanceHistorys.AddAsync(new PlayerFinanceHistory
                    {
                        FinanceStatus = FinanceStatus.Success,
                        Time = DateTime.UtcNow,
                        Type = FinanceType.PromotionProfit,
                        Name = betHistory.Name,
                        Amount = inviterProfit,
                        Remark = "推广获利",
                        GameId = game.Id,
                        GameMessageId = messageId,
                        PlayerId = inviter.PlayerId
                    }, cancellationToken);
                }

                //平台盈利 = 剩余的钱 - (剩余的钱*皇冠占比)
                platform.Profit += handlingFeeAmount - (handlingFeeAmount * (1 - Program._appsettings.BettingThreadDividend));
                //皇冠盈利 = 剩余的钱-平台盈利
                Program._appsettings.Profit += handlingFeeAmount - (handlingFeeAmount * Program._appsettings.BettingThreadDividend);
            }

            return player;
        }

        /// <summary>
        /// 获取今年岁数的生肖
        /// </summary>
        /// <param name="age"></param>
        /// <returns></returns>
        public static char GetChineseZodiac(int age)
        {
            char[] zodiacs = [
                '龙', '兔', '虎', '牛', '鼠', '猪', '狗', '鸡', '猴', '羊', '马', '蛇',
                '龙', '兔', '虎', '牛', '鼠', '猪', '狗', '鸡', '猴', '羊', '马', '蛇',
                '龙', '兔', '虎', '牛', '鼠', '猪', '狗', '鸡', '猴', '羊', '马', '蛇',
                '龙', '兔', '虎', '牛', '鼠', '猪', '狗', '鸡', '猴', '羊', '马', '蛇',
                '龙', '兔', '虎', '牛', '鼠', '猪', '狗', '鸡', '猴', '羊', '马', '蛇',
                '龙', '兔', '虎', '牛', '鼠', '猪', '狗', '鸡', '猴', '羊', '马', '蛇',
            ];
            return zodiacs[age - 1];
        }
    }

    internal class AccountsTransactions
    {
        internal static async Task<string?> GetContractTransactionInfo(string address, int limit, string contractAddress, bool onlyTo)
        {
            throw new NotImplementedException();
        }
    }
}
