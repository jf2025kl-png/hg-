using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;




//using Tron.FullNodeHttpApi;
using 皇冠娱乐.Games;

namespace 皇冠娱乐
{
    public static class PlatformBot
    {
        //特码数                              20-9
        //public static readonly string _tema = @"^([5-9]|[1-9][0-9]{1,2}|1000)(\-|@|买|buy)([0-9]|[1-9][0-9]{1,2})$";
        //包号(只要有)                         20-5/6
        //public static readonly string _baohaoor = @"^([5-9]|[1-9][0-9]{1,2}|1000)(\-|@|买|buy)([0-9]|[1-9][0-9])((/([0-9]|[1-9][0-9]))+)?$";
        //包号(必须有)                         20-5&6
        //public static readonly string _baohaomust = @"^([5-9]|[1-9][0-9]{1,2}|1000)(\-|@|买|buy)([0-9]|[1-9][0-9])((&([0-9]|[1-9][0-9]))+)?$";
        //定位胆                              20-8=3;4=1
        //public static readonly string _dingwei = @"^([5-9]|[1-9][0-9]{1,2}|1000)(\-|@|买|buy)^(([0-9]|[1-9][0-9])=([0-9]|[1-9][0-9]);?)+$";
        //冠亚和值是极小,极大,大,小,中           20-^xs
        //public static readonly string _guanyajidaxiaozhong = @"^([5-9]|[1-9][0-9]{1,2}|1000)(\-|@|买|buy)\^(xs|sx|xl|lx|l|s|m)$";
        //冠亚和值                            20-^=15
        //public static readonly string _guanyasum = @"^([5-9]|[1-9][0-9]{1,2}|1000)(\-|@|买|buy)\^=([1-9]|[1-9][0-9])$";
        //前2\3名                             20-^8>5
        //public static readonly string _guanyaji = @"^([5-9]|[1-9][0-9]{1,2}|1000)(\-|@|买|buy)\^([0-9]|[1-9][0-9])((>([0-9]|[1-9][0-9]))+)?$";
        //前2\3名(且顺子)                      20-^7+8+9
        //public static readonly string _guanyajishunzi = @"^([5-9]|[1-9][0-9]{1,2}|1000)(\-|@|买|buy)\^([0-9]|[1-9][0-9])((\+([0-9]|[1-9][0-9]))+)?$";
        //前组2\3(不定名次)                    20-^8&3
        //public static readonly string _guanyajibudingmingci = @"^([5-9]|[1-9][0-9]{1,2}|1000)(\-|@|买|buy)\^([0-9]|[1-9][0-9])((&([0-9]|[1-9][0-9]))+)?$";

        //平台机器人收到信息时执行的方法
        public static async Task PlatformHandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            using var db = new DataContext();
            //对话Id 
            Chat? chat = null;
            //主题Id  
            int? threadId = null;
            //玩家Id
            User? user = null;
            //聊天信息
            Message? msg = null;
            CallbackQuery? cq = null;
            switch (update.Type)
            {
                case UpdateType.Message:
                    msg = update.Message!;
                    chat = msg.Chat;
                    threadId = msg.MessageThreadId;
                    user = msg.From!;
                    break;
                case UpdateType.CallbackQuery:
                    cq = update.CallbackQuery!;
                    chat = cq.Message?.Chat;
                    threadId = cq.Message?.MessageThreadId;
                    msg = cq.Message;
                    user = cq.From;
                    break;
                case UpdateType.EditedMessage:
                    chat = update.EditedMessage?.Chat;
                    threadId = update.EditedMessage?.MessageThreadId;
                    user = update.EditedMessage?.From!;
                    msg = update.EditedMessage;
                    break;
                case UpdateType.ChatMember:
                    chat = update.ChatMember?.Chat;
                    user = update.ChatMember?.From!;
                    break;
                case UpdateType.ChatJoinRequest:
                    chat = update.ChatJoinRequest?.Chat;
                    user = update.ChatJoinRequest?.From!;
                    break;
                default:
                    break;
            }
            if (chat == null
                || user == null
                || msg != null && string.IsNullOrEmpty(msg.Text) && string.IsNullOrEmpty(msg.Caption) && msg.Dice == null)
                return;

            //玩家
            Player? player = null;
            //返回给当前用户的信息
            var returnText = string.Empty;
            //返回隔几秒就删除的信息
            var returnDelText = string.Empty;
            //返回的出错信息
            string returnError = string.Empty;
            //底部键盘按钮
            List<List<KeyboardButton>> inputBtn = [];
            //信息按钮
            List<List<InlineKeyboardButton>> msgBtn = [];
            //博彩平台操作记录
            PlatformOperateHistory? platformOperateHistory = null;
            //玩家财务记录
            PlayerFinanceHistory? playerFinanceHistory = null;
            //向平台工作人员发的通知消息
            string? returnTipToWorker = string.Empty;

            //是否已经有错误信息了
            var isError = () => !string.IsNullOrEmpty(returnError) || !string.IsNullOrEmpty(returnDelText);
            //平台
            var platform = await db.Platforms.FirstAsync(u => u.BotId == botClient.BotId, cancellationToken: cancellationToken);
            //角色
            PlatformUserRole? role = null;
            if (Program._appsettings.AdminerIds.Contains(user.Id))
            {
                role = PlatformUserRole.Adminer;
            }
            else if (platform.CreatorId == user.Id)
            {
                role = PlatformUserRole.Creator;
            }
            else if (platform.FinancerId == user.Id)
            {
                role = PlatformUserRole.Financer;
            }

            player = await db.Players.FirstOrDefaultAsync(u => u.UserId == user.Id && u.CreatorId == platform.CreatorId, cancellationToken: cancellationToken);
            if (player == null)
            {
                var playwrPrivateKey = Helper.ComputeSHA256Hash(platform.CreatorId.ToString() + user.Id.ToString());
                player = new Player
                {
                    CreatorId = platform.CreatorId,
#warning 如果有邀请者这里就要写
                    //InviterId = ,
                    IsTryModel = true,
                    RewardBalance = 5000,
                    UserId = user.Id,
                    PlayerStatus = PlayerStatus.Normal,
                    PrivateKey = playwrPrivateKey,
                    IsHidePrivateKey = false,
                    Time = DateTime.UtcNow,
                };
                await db.Players.AddAsync(player, cancellationToken);
                await db.SaveChangesAsync(cancellationToken);
            }
            else
            {
                if (player.PlayerStatus is PlayerStatus.Freeze)
                    returnDelText = "您的账号已被冻结";
            }

            if (string.IsNullOrEmpty(returnDelText) && platform.GroupId == null)
                returnDelText = "机器人未绑定群Id,暂不提供服务!";

            if (string.IsNullOrEmpty(returnDelText) && update.Type == UpdateType.Message)
            {
                if (!db.BotChats.Any(u => u.BotId == botClient.BotId && u.ChatId == update.Message!.Chat.Id))
                {
                    await db.BotChats.AddAsync(new BotChat { BotId = Convert.ToInt64(botClient.BotId), ChatId = update.Message!.Chat.Id }, cancellationToken);
                    await db.SaveChangesAsync(cancellationToken);
                }
            }

            if (string.IsNullOrEmpty(returnDelText) && Program._runingUserId.Contains(user.Id))
                returnDelText = "请勿频繁操作,请稍等";

            if (!isError())
            {
                //防止重复频繁操作
                Program._runingUserId.Add(user.Id);

                //机器人私聊
                if (chat.Type == ChatType.Private)
                {
                    //收到消息事件
                    if (update.Type is UpdateType.Message && msg != null && !string.IsNullOrEmpty(msg.Text))
                    {
                        //普通玩家
                        if (role == null && player != null)
                        {
                            if (msg!.Text is "👤 我的信息")
                            {
                                var group = await Helper.GetChatInfo(botClient, Convert.ToInt64(platform.GroupId));
                                if (group != null)
                                    returnText = $"<b>进群娱乐</b> : <a href='{group.InviteLink}'><b>{group?.FirstName + group?.LastName}</b></a>";

                                returnText += $"\n\n<b>🙎‍♂️ 个人信息 ▾</b> (Id:<code>{player.PlayerId}</code>)";
                                returnText += $"\n\n<b>时间</b> : {player.Time:yyyy年MM月dd日}";
                                var userName = string.IsNullOrEmpty(user.Username) ? "" : "@" + user.Username + " ";
                                returnText += $"\n\n<b>名称</b> : {userName}{user.FirstName + user.LastName}";
                                returnText += $"\n\n<b>余额</b> : {player.Balance}USDT 获赠余额:{player.RewardBalance}";
                                if (player.InviterId != null)
                                    returnText += $"\n\n<b>邀请者</b> : {player.InviterId}";

                                if (!string.IsNullOrEmpty(player.PrivateKey) && !player.IsHidePrivateKey)
                                {
                                    returnText += $"\n\n<b>私钥</b> : {player.PrivateKey}";
                                    returnText += $"\n<b⚠️ 请及时备份私钥,私钥可全权支配你账户,和用于重要操作,一旦丢失,永远无法找回,记得备份私钥后,点击 /hidepk 永久隐藏私钥,以免账户遭黑客盗窃</b>";
                                }

                                if (player.PlayerStatus == PlayerStatus.Freeze)
                                    returnText += $"\n\n<b>状态</b> : 🚫被冻结:{player.FreezeTip}";

                                returnText += "\n\n\n<b>💰 钱包地址 ▾</b>";
                                if (!string.IsNullOrEmpty(player.TronWalletAddress))
                                    returnText += $"\n\n<b>Tron波场</b> : {player.TronWalletAddress}";
                                else
                                    returnText += $"\n\n<b>Tron波场</b> : 未绑定";

                                if (!string.IsNullOrEmpty(player.EthereumWalletAddress))
                                    returnText += $"\n\n<b>Ethereum以太坊</b> : {player.EthereumWalletAddress}";
                                else
                                    returnText += $"\n\n<b>Ethereum以太坊</b> : 未绑定";

                                returnText += $"\n\n💁 向我直接发送钱包地址,即可绑定/换绑钱包";

                                if (platform.Dividend > 0)
                                {
                                    returnText += $"\n\n<b>邀请分成</b> : 从受邀请玩家处获利可获{platform.Dividend}倍的提成";
                                }

                                returnText += "\n\n\n<b>♻️ 转让/申诉 ▾</b>";
                                returnText += $"\n\n<b>转让账号</b> : 发送:<b><code>私钥=目标用户Id</code></b>";
                                returnText += $"\n\n<b>申诉账号</b> : 发原账号私钥";

                            }
                            else if (msg.Text is "💴 充值提现")
                            {
                                returnText = $"<b>余额</b> : {player.Balance}USDT 获赠余额:{player.RewardBalance}";

                                returnText += "\n\n\n<b>💰 我的钱包地址 ▾</b>";
                                if (!string.IsNullOrEmpty(player.TronWalletAddress))
                                    returnText += $"\n\n<b>Tron波场</b> : {player.TronWalletAddress}";
                                else
                                    returnText += $"\n\n<b>Tron波场</b> : 未绑定";

                                if (!string.IsNullOrEmpty(player.EthereumWalletAddress))
                                    returnText += $"\n\n<b>Ethereum以太坊</b> : {player.EthereumWalletAddress}";
                                else
                                    returnText += $"\n\n<b>Ethereum以太坊</b> : 未绑定";

                                returnText += $"\n\n💁 向我直接发送钱包地址,即可绑定/换绑钱包";

                                returnText += "\n\n\n<b>💰 平台充值地址 ▾</b>";

                                if (!string.IsNullOrEmpty(platform.TronWalletAddress))
                                    returnText += $"\n\n<b>Tron波场</b> : {platform.TronWalletAddress}";

                                if (!string.IsNullOrEmpty(platform.EthereumWalletAddress))
                                    returnText += $"\n\n<b>Ethereum以太坊</b> : {platform.EthereumWalletAddress}";

                                returnText += $"\n\n💁 先绑定您的钱包地址,然后往平台对应的钱包地址转账,即可自动到账";

                                returnText += "\n\n\n<b>💸 提现操作 ▾</b>";
                                returnText += $"\n\n向我发送格式:<b><code>/withdraw=私钥=提现金额</code></b>";
                            }
                            else if (msg.Text is "🧮 账单记录")
                            {
                                returnText = $"<b>余额</b> : {player.Balance}USDT 获赠余额:{player.RewardBalance}";
                                returnText += "\n\n\n<b>🧮 最近账单记录 ▾</b>";
                                var records = db.PlayerFinanceHistorys.Where(u => u.PlayerId == player.PlayerId).Take(10);
                                if (!records.Any())
                                {
                                    returnText += "\n\n😟 您暂无账单记录";
                                }
                                else
                                {
                                    for (int i = 0; i < records.Count(); i++)
                                    {
                                        var item = records.ElementAt(i);

                                        var statusIco = string.Empty;
                                        switch (item.FinanceStatus)
                                        {
                                            case FinanceStatus.WaitingConfirmation:
                                                statusIco = "⏳";
                                                break;
                                            case FinanceStatus.Reject:
                                                statusIco = "🚫";
                                                break;
                                            case FinanceStatus.Success:
                                                statusIco = "✅";
                                                break;
                                            case FinanceStatus.Timeout:
                                                statusIco = "🕔";
                                                break;
                                            default:
                                                break;
                                        }
                                        var type = Helper.GetEnumDescription(item.FinanceStatus);
                                        returnText += $"\n\n{i}.{statusIco} {type} {item.Time:MM/dd hh:mm:ss}";
                                        returnText += $"\n{item.Remark}";
                                    }

                                    if (records.Count() > 10)
                                    {
#warning 这里发财务账单表格给用户
                                    }
                                }
                            }
                            else if (msg.Text is "🪧 推广赚钱" && platform.Dividend > 0)
                            {
                                returnText = $"<b>推广盈利</b> 今日盈利<b>3</b>U / 总盈利<b>300</b>U  今日邀请5人/历史邀请10人";
                                returnText += $"\n\n<b>推广链接</b> https://t.me/";
                                returnText += $"\n\n<b>邀请玩家参与游戏,可获得{platform.Dividend}的奖励</b>";
                            }
                            //隐藏私钥
                            else if (msg.Text is "/hidepk")
                            {
                                player.IsHidePrivateKey = true;
                                returnText = "成功永久隐藏私钥,请妥善保管您的私钥,一旦丢失,永远无法找回!";
                            }
                            //绑定波场钱包
                            else if (Regex.IsMatch(msg.Text, @"T[1-9a-zA-Z]{33}"))
                            {
                                //地址是否存在有效
                                var http = new HttpClient();
                                bool? isValid = null;
                                try
                                {
                                    var result = await http.GetStringAsync("https://apilist.tronscanapi.com/api/accountv2?address=" + msg.Text, cancellationToken);
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
                                else if (Program._tronExchangeWalletAddress.Any(u => u.Address == msg.Text))
                                {
                                    returnError = "绑定失败,请勿绑定交易所的钱包地址!";
                                }
                                else if (Program._tronZuoDaoWalletAddress.Contains(msg.Text))
                                {
                                    returnError = "绑定失败,这是皇冠官方钱包地址!";
                                }
                                else if (db.Platforms.Any(u => u.TronWalletAddress == msg.Text))
                                {
                                    returnError = "绑定失败,本钱包地址是博彩平台钱包,不可绑定为玩家钱包";
                                }
                                else if (db.Players.Any(u => u.TronWalletAddress == msg.Text))
                                {
                                    returnError = "绑定失败,本钱包地址已经绑定本平台,不可重复绑定";
                                }

                                if (string.IsNullOrEmpty(returnError))
                                {
                                    player.TronWalletAddress = msg.Text;
                                    returnText = "成功绑定了Tron波场钱包地址:" + msg.Text;
                                }
                            }
                            //绑定以太坊钱包
                            else if (Regex.IsMatch(msg.Text, @"0x[0-9a-fA-F]{40}"))
                            {
                                //地址是否存在有效
                                var http1 = new HttpClient();
                                bool? isValid1 = null;
                                try
                                {
                                    var result = await http1.GetStringAsync("view-source:https://api.etherscan.io/api?module=account&action=txlist&sort=desc&address=" + msg.Text, cancellationToken);
                                    isValid1 = !result.Contains("result\":[]");
                                }
                                catch (Exception ex)
                                {
                                    Log.WriteLine("判断以太坊钱包地址是否有效时出错:" + ex.Message);
                                }

                                if (isValid1 == false)
                                {
                                    returnError = "绑定失败,钱包地址未激活!";
                                }
                                else if (Program._ethereumExchangeWalletAddress.Any(u => u.Address == msg.Text))
                                {
                                    returnError = "绑定失败,请勿绑定交易所的钱包地址!";
                                }
                                else if (Program._ethereumZuoDaoWalletAddress.Contains(msg.Text))
                                {
                                    returnError = "绑定失败,这是皇冠官方钱包地址!";
                                }
                                else if (db.Platforms.Any(u => u.EthereumWalletAddress == msg.Text))
                                {
                                    returnError = "绑定失败,本钱包地址是博彩平台钱包,不可绑定为玩家钱包";
                                }
                                else if (db.Players.Any(u => u.EthereumWalletAddress == msg.Text))
                                {
                                    returnError = "绑定失败,本钱包地址已经绑定本平台,不可重复绑定";
                                }

                                if (string.IsNullOrEmpty(returnError))
                                {
                                    player.EthereumWalletAddress = msg.Text;
                                    returnText = "成功绑定了Ethereum以太坊钱包地址:" + msg.Text;
                                }
                            }
                            //申诉/转让/提现
                            else if (Regex.IsMatch(msg.Text, @"^[A-Fa-f0-9]{64}"))
                            {
                                var privateKey = Helper.ExtractHash(msg.Text);
                                var match = Regex.Match(msg.Text, @"^[A-Fa-f0-9]{64}");
                                if (player.PlayerStatus == PlayerStatus.Freeze)
                                {
                                    returnError = $"您的账号已经冻结:{player.FreezeTip}";
                                }
                                else if (string.IsNullOrEmpty(privateKey) || !match.Success || match.Index != 0)
                                {
                                    returnError = "格式不正确,请重新输入";
                                }
                                else if (!db.Players.Any(u => u.PrivateKey == match.Value))
                                {
                                    returnError = "不存在此私钥";
                                }
                                //这样代表申诉账号
                                else if (Regex.IsMatch(msg.Text, @"^[A-Fa-f0-9]{64}$"))
                                {
                                    var findefPlayer = await db.Players.FirstOrDefaultAsync(u => u.PrivateKey == match.Value && u.CreatorId == platform.CreatorId, cancellationToken: cancellationToken);
                                    if (player.Balance > 0 || player.RewardBalance > 0)
                                    {
                                        returnError = "申诉失败,您的当前账户还有余额,不可申诉,否则您当前账户余额将会丢失!";
                                    }
                                    else if (player.PrivateKey == match.Value)
                                    {
                                        returnError = "申诉失败,不可申诉您自己的私钥";
                                    }
                                    else if (findefPlayer == null)
                                    {
                                        returnError = "申诉失败,不存在此私钥用户";
                                    }
                                    else
                                    {
                                        var oldUserId = findefPlayer.UserId;

                                        findefPlayer.UserId = user.Id;
                                        privateKey = Helper.ComputeSHA256Hash(user.Id.ToString());
                                        findefPlayer.PrivateKey = privateKey;
                                        findefPlayer.IsHidePrivateKey = false;
                                        findefPlayer.FreezeTip = string.Empty;

                                        //财务记录
                                        var finances = from p in db.Players
                                                       from f in db.PlayerFinanceHistorys
                                                       where p.UserId == oldUserId && p.PlayerId == f.PlayerId && f.FinanceStatus == FinanceStatus.WaitingConfirmation
                                                       select f;
                                        foreach (var finance in finances)
                                        {
                                            finance.FinanceStatus = FinanceStatus.Reject;
                                            finance.Remark = "申请提现超时,被用户申诉找回账号,将提现金额返还至账户!";
                                            findefPlayer.Balance += finance.Amount;
                                        }
                                        #region 断开对方的会话
                                        try
                                        {
                                            await botClient.SendTextMessageAsync(oldUserId, "您的账号在别处通过私钥申诉找回,如果不是您的操作,请紧急联系客服处理,再见!", cancellationToken: cancellationToken);
                                        }
                                        catch (Exception ex)
                                        {
                                            Log.Error("告知账号被别处申诉找回时出错" + ex.Message);
                                        }
                                        #endregion
                                        returnText = $"<b>成功申诉回账号</b>\n\n现在新的私钥:<b><code>{privateKey}</code></b>\n\n⚠️ 请妥善保管私钥,保存好后,记得删除本信息(重要)!勿泄露给任何人,私钥是可以对您平台进行全权操作,和转让的.因私钥泄露导致的财产损失自行承担!";
                                        returnTipToWorker = $"@{user.Username} {user.FirstName}{user.LastName} (Id:{user.Id}) 成功申诉回账号";

                                        var delPFH = from p in db.Players
                                                     from f in db.PlayerFinanceHistorys
                                                     where p.UserId == user.Id && p.PlayerId == f.PlayerId
                                                     select f;
                                        db.PlayerFinanceHistorys.RemoveRange(delPFH);
                                        //清空当前账号的所有记录
                                        db.Players.Remove(player);
                                    }
                                }
                                //这里是要用到私钥的
                                else
                                {
                                    if (player.PrivateKey != privateKey)
                                    {
                                        returnError = "您的私钥有误,请重新填写!";
                                    }
                                    else
                                    {
                                        //和私钥一起提交的值
                                        string? value = string.Empty;
                                        value = msg.Text[(msg.Text.IndexOf('=') + 1)..];
                                        if (string.IsNullOrEmpty(value))
                                        {
                                            returnError = "请输入值";
                                        }
                                        else
                                        {
                                            value = value.Trim();
                                        }

                                        decimal withdrawAmount = 0;
                                        //转让账号
                                        if (Regex.IsMatch(value, "^[0-9]{10}$"))
                                        {
                                            //目标用户Id
                                            var targetId = Convert.ToInt32(value);
                                            if (db.Players.Any(u => u.UserId == targetId && u.CreatorId == platform.CreatorId))
                                            {
                                                returnError = "转让失败,不可转让给已在本平台群的用户";
                                            }
                                            else
                                            {
                                                player.UserId = targetId;
                                                privateKey = Helper.ComputeSHA256Hash(value);
                                                player.PrivateKey = privateKey;
                                                player.IsHidePrivateKey = false;
                                                player.FreezeTip = string.Empty;
                                                returnText = $"用户Id{user.Id}成功转让账号至Id:{targetId}";
                                                returnTipToWorker = returnText;
                                            }
                                        }
                                        //提现
                                        else if (Regex.IsMatch(value, @"^(?!0(\.0{1,2})?$|1000000000(\.0{1,2})?$)\d{1,9}(\.\d{1,2})?$") && decimal.TryParse(value, out withdrawAmount) && withdrawAmount > 0 && withdrawAmount < 1000000000)
                                        {
                                            if (platform.IsStopWithdraw)
                                            {
                                                returnError = "操作失败,暂时停止提现,具体原因请咨询客服";
                                            }
                                            else if (db.PlayerFinanceHistorys.Any(u => u.PlayerId == player.PlayerId && u.FinanceStatus == FinanceStatus.WaitingConfirmation))
                                            {
                                                returnError = "操作失败,您有正在等待提现的记录,请等皇冠管理员审批!";
                                            }
                                            else if (db.PlayerFinanceHistorys.Any(u => u.PlayerId == player.PlayerId && (DateTime.UtcNow - u.Time).TotalHours < 24))
                                            {
                                                returnError = "操作失败,24小时只能提现一次";
                                            }
                                            else if (string.IsNullOrEmpty(player.EthereumWalletAddress) && string.IsNullOrEmpty(player.TronWalletAddress))
                                            {
                                                returnError = "提现失败,您必须先设置Tron波场钱包或者Ethereum以太坊钱包其中一个";
                                            }
                                            else if (withdrawAmount > player.Balance)
                                            {
                                                returnError = "提现失败,提现的金额大于您的现有余额";
                                            }
                                            else
                                            {
                                                //要人工审核 
                                                var isNeedApproval = withdrawAmount > platform.FinancialOperationAmount || string.IsNullOrEmpty(platform.TronWalletPrivateKey) && string.IsNullOrEmpty(platform.EthereumWalletPrivateKey);
                                                if (isNeedApproval)
                                                {
                                                    returnText = $"申请提现{withdrawAmount}USDT的审批已提交,请等待审批!";
                                                    returnTipToWorker = $"@{user.Username} {user.FirstName}{user.LastName} (Id:{user.Id}) 申请提现{withdrawAmount}USDT,请审批!";
                                                }
                                                else
                                                {
                                                    if (string.IsNullOrEmpty(player.TronWalletAddress))
                                                    {

                                                        returnText = $"成功从平台Tron波场钱包{platform.TronWalletAddress}提现{withdrawAmount}USDT至您的TRON波场钱包:{player.TronWalletAddress}";
                                                        returnTipToWorker = $"@{user.Username} {user.FirstName}{user.LastName} (Id:{user.Id}) 博彩平台提现了{withdrawAmount}USDT至TRON波场钱包:{player.TronWalletAddress}";
#warning 这里是TRON链给他转账
                                                    }
                                                    else
                                                    {

                                                        returnText = $"成功从平台Ethereum以太坊钱包{platform.EthereumWalletAddress}提现{withdrawAmount}USDT至您的Ethereum以太坊钱包:{player.EthereumWalletAddress}";
                                                        returnTipToWorker = $"@{user.Username} {user.FirstName}{user.LastName} (Id:{user.Id}) 博彩平台提现了{withdrawAmount}USDT至Ethereum以太坊钱包:{player.EthereumWalletAddress}";
#warning 这里是ETH链给他转账
                                                    }
                                                }

                                                platform.Balance -= withdrawAmount;
                                                playerFinanceHistory = new PlayerFinanceHistory
                                                {
                                                    Amount = withdrawAmount,
                                                    FinanceStatus = isNeedApproval ? FinanceStatus.WaitingConfirmation : FinanceStatus.Success,
                                                    Time = DateTime.UtcNow,
                                                    PlayerId = player.PlayerId,
                                                    Type = FinanceType.Withdraw,
                                                    Remark = returnText
                                                };
                                            }
                                        }
                                        else
                                        {
                                            returnError = "操作有误";
                                        }
                                    }
                                }
                            }
                            else
                            {
                                returnError = "操作有误";
                            }
                        }
                        //平台工作人员:台主/财务
                        else if (role is PlatformUserRole.Creator or PlatformUserRole.Financer)
                        {
#warning 是人工提现，要求可以修改玩家余额

                            if (msg.Text is "🚫 平台已被冻结" || platform.PlatformStatus == PlatformStatus.Freeze)
                            {
                                returnText = $"🚫 平台已被冻结,不可再操作:{platform.FreezeTip}.详情向皇冠客服了解详情 @ZuoDao_KeFuBot";
                            }
                            else if (msg.Text is "📊 信息统计")
                            {
                                returnText += $"平台余额:" + platform.Balance;
                                returnText += $"\n❤️本余额是博彩平台在皇冠预存的余额,非博彩平台玩家余额,是用于支付盘口月费和分红的!";

                                returnText = "\n\n🏛 <b>平台信息</b> ▾";
                                if (platform.CreatorId == user.Id)
                                    returnText += $"          <a href='https://t.me/CrownCasinoCityBot'>去设置</a>";

                                returnText += $"\n\n共盈利:{platform.Profit}USDT";
                                returnText += $"\n\n绑定群Id:{platform.GroupId}";
                                returnText += $"\n\n机器人Id:{platform.BotId}";
                                returnText += $"\n\n平台状态:{platform.PlatformStatus}";
                                returnText += $"\n\nTron波场链钱包:" + platform.TronWalletAddress;
                                returnText += $"\n\nTron波场钱包私钥:" + (!string.IsNullOrEmpty(platform.TronWalletPrivateKey) ? "已绑定" : "未绑定");
                                returnText += $"\n\nEthereum以太坊链钱包:" + platform.EthereumWalletAddress;
                                returnText += $"\n\nEthereum以太坊钱包私钥:" + (!string.IsNullOrEmpty(platform.EthereumWalletPrivateKey) ? "已绑定" : "未绑定");
                                returnText += $"\n\n财务Id:" + platform.FinancerId;
                                returnText += $"\n\n邀请分红:" + platform.Dividend;
                                returnText += $"\n\n提现设置:" + (platform.IsStopWithdraw ? "暂停提现 (点击允许提现👉 /AllowWithdraw )" : "可提现 (点击暂停提现👉 /AllowWithdraw )");
                                if (!string.IsNullOrEmpty(platform.TronWalletPrivateKey) || !string.IsNullOrEmpty(platform.EthereumWalletPrivateKey))
                                    returnText += $"\n\n提现干预额度:{platform.FinancialOperationAmount}USDT以上";

                                var players = db.Players.Where(u => u.CreatorId == platform.CreatorId);
                                returnText += "\n\n\n📊 <b>数据统计</b> ▾";
                                returnText += "\n\n<b>◆玩家数据</b>";
                                returnText += $"\n\n正常玩家:{players.Count(u => u.PlayerStatus == PlayerStatus.Normal)}👤  冻结玩家:{players.Count(u => u.PlayerStatus == PlayerStatus.Freeze)}👤";
                                returnText += $"\n\n玩家:{players.Count(u => u.Balance > 0)}👤 {players.Sum(u => (double)u.Balance)}💲  奖励玩家:{players.Count(u => u.RewardBalance > 0)}👤 {players.Sum(u => (double)u.RewardBalance)}💲";
                                returnText += $"\n\n绑定钱包人数:波场钱包<b>{players.Count(u => !string.IsNullOrEmpty(u.TronWalletAddress))}</b>👤    以太坊钱包<b>{players.Count(u => !string.IsNullOrEmpty(u.EthereumWalletAddress))}</b>👤";
                                returnText += $"\n\n受邀:<b>{players.Count(u => u.InviterId != null)}</b>👤";

                                returnText += "\n\n\n📊 <b>账单下载</b> ▾";
                                returnText += "\n\n点击下载全部账单 /DownloadBill";
                            }
                            else if (msg.Text.Contains(" 提现审批") || msg.Text == "/AllowWithdraw" || msg.Text == "/ForbidWithdraw" || Regex.IsMatch(msg.Text, @"^/(aw|fw)[0-9]{1,15}$"))
                            {
                                if (msg.Text.Contains(" 提现审批"))
                                {
                                    returnText = $"提现设置:" + (platform.IsStopWithdraw ? "暂停提现 (点击允许提现👉 /AllowWithdraw )" : "可提现 (点击暂停提现👉 /AllowWithdraw )");
                                    if (!string.IsNullOrEmpty(platform.TronWalletPrivateKey) || !string.IsNullOrEmpty(platform.EthereumWalletPrivateKey))
                                        returnText += $"\n\n提现大于{platform.FinancialOperationAmount}USDT以上需干预额度";

                                    //等待提现
                                    var waitWithdraws = (from p in db.Players
                                                         from f in db.PlatformFinanceHistorys
                                                         where p.CreatorId == platform.CreatorId && p.CreatorId == f.CreatorId && f.FinanceStatus == FinanceStatus.WaitingConfirmation
                                                         select f).Take(20);
                                    if (!waitWithdraws.Any())
                                    {
                                        returnText += "\n\n⭕️ 无待审批提现记录";
                                    }
                                    else
                                    {
                                        returnText += $"\n\n\n💸 提现审批({waitWithdraws.Count()})";
                                        foreach (var item in waitWithdraws)
                                        {
                                            returnText += $"\n\n提现金额{item.Amount}USDT   🕔{item.Time:MM/dd HH:mm}";
                                            returnText += $"\n备注:{item.Remark}";
                                            returnText += $"\n提现审批:允许 /aw{item.Id}   不允许 /fw{item.Id}";
                                        }
                                    }
                                }
                                //设置为允许提现
                                else if (msg.Text == "/AllowWithdraw")
                                {
                                    platform.IsStopWithdraw = false;
                                    returnText = "成功设置为允许提现";
                                }
                                //设置为不允许提现
                                else if (msg.Text == "/ForbidWithdraw")
                                {
                                    platform.IsStopWithdraw = true;
                                    returnText = "成功设置为禁止提现";
                                }
                                //同意提现\禁止提现
                                else if (Regex.IsMatch(msg.Text, @"^/(aw|fw)[0-9]{1,15}$"))
                                {
                                    var withdrawId = Convert.ToInt32(Regex.Match(msg.Text, "[0-9]{1,15}"));
                                    var record = await (from p in db.Players
                                                        from f in db.PlayerFinanceHistorys
                                                        where p.CreatorId == platform.CreatorId && p.PlayerId == f.PlayerId && f.Id == withdrawId && f.FinanceStatus == FinanceStatus.WaitingConfirmation
                                                        select f).FirstOrDefaultAsync(cancellationToken: cancellationToken);
                                    if (record == null)
                                    {
                                        returnError = "不存在此记录,或者此记录已审核/过期.";
                                    }
                                    else
                                    {
                                        //同意订单提现
                                        if (Regex.IsMatch(msg.Text, @"^/aw[0-9]{1,15}$"))
                                        {
                                            returnText = $"审批通过,玩家{record.PlayerId}成功提现{record.Amount}USDT";
                                            returnTipToWorker = returnText;
                                            record.FinanceStatus = FinanceStatus.Success;
                                        }
                                        //拒绝订单提现
                                        else if (Regex.IsMatch(msg.Text, @"^/fw[0-9]{1,15}$"))
                                        {
                                            returnText = $"审批不通过,玩家{record.PlayerId}的申请提现{record.Amount}USDT未成功";
                                            record.FinanceStatus = FinanceStatus.Reject;
                                        }

                                        record.Remark = returnText;

                                        //告知审核结果
                                        try
                                        {
                                            await Program._botClient.SendTextMessageAsync(record.PlayerId, returnText, cancellationToken: cancellationToken);
                                        }
                                        catch (Exception ex)
                                        {
                                            Log.Error("告知玩家提现审批结果时出错:" + ex.Message);
                                        }
                                    }
                                }

                            }
                            else if (msg.Text is "🧮 账单记录" || msg.Text is "/DownloadBill")
                            {
#warning 下载记录
                                returnText = $"💸 <b>玩家财务账单</b>";
                            }
                            else if (msg.Text is "📄 红包扫雷记录")
                            {
#warning 下载记录
                            }
                            else if (msg.Text is "📄 加拿大PC28记录")
                            {
#warning 下载记录
                            }
                            else if (msg.Text is "📄 骰子记录")
                            {
#warning 下载记录
                            }
                            else if (msg.Text is "📄 老虎机记录")
                            {
#warning 下载记录
                            }
                            else if (msg.Text is "📄 盲盒记录")
                            {
#warning 下载记录
                            }
                            else if (msg.Text is "📄 百家乐记录")
                            {
#warning 下载记录
                            }
                            else if (msg.Text is "📄 赛车记录")
                            {
#warning 下载记录
                            }
                            else if (msg.Text is "📄 飞艇记录")
                            {
#warning 下载记录
                            }
                            else if (msg.Text is "📄 11选5记录")
                            {
#warning 下载记录
                            }
                            else if (msg.Text is "📄 缤果记录")
                            {
#warning 下载记录
                            }
                            else if (msg.Text is "📄 幸运8记录")
                            {
#warning 下载记录
                            }
                            //搜索某个玩家/su
                            else if (msg.Text is "👥 玩家列表" || Regex.IsMatch(msg.Text, @"^/su[0-9]{10}$"))
                            {
                                var players = db.Players.Where(u => u.CreatorId == platform.CreatorId);
                                //搜单个用户时
                                if (Regex.IsMatch(msg.Text, @"^/su[0-9]{10}$"))
                                {
                                    var userId = Convert.ToInt64(Regex.Match(msg.Text, @"[0-9]{10}").Value);
                                    players = players.Where(u => u.UserId == userId);
                                }

                                var count = players.Count();
                                var page = 0;
                                players = players.Skip(page * 10).Take(10);
                                for (int i = 0; i < players.Count(); i++)
                                {
                                    var item = players.ElementAt(i);
                                    var status = item.PlayerStatus is PlayerStatus.Freeze ? "🚫" : "";
                                    returnText += $"\n{i + 1}.{status}{item.PlayerId} 余额{item.Balance}💲  赠{item.RewardBalance}💲";
                                    returnText += $"\n玩家信息 : 【/pi{item.PlayerId}】";
                                    returnText += $"\n下载账单 : 【/df{item.PlayerId}】";
                                    if (item.PlayerStatus is PlayerStatus.Freeze)
                                    {
                                        returnText += $"\n解冻账号 : 【/uf{item.PlayerId}】";
                                    }
                                    else
                                    {
                                        returnText += $"\n解冻账号 : 【/f{item.PlayerId}】";
                                    }
                                }

                                returnText += "\n\n\n🔎搜索玩家:<b>/su用户Id</b>";
                                if (page > 0)
                                    msgBtn.Add([InlineKeyboardButton.WithCallbackData($"◀️ 上一页 ({page})", $"Skills?page=" + (page - 1))]);

                                if (count > ((page + 1) * 10))
                                    msgBtn.Add([InlineKeyboardButton.WithCallbackData($"({page + 2}) 下一页 ▶️", $"Skills?page=" + (page + 1))]);
                            }
                            else if (Regex.IsMatch(msg.Text, @"^/pi[0-9]{10}$") || Regex.IsMatch(msg.Text, @"^/df[0-9]{10}$") || Regex.IsMatch(msg.Text, @"^/uf[0-9]{10}$") || Regex.IsMatch(msg.Text, @"^/f[0-9]{10}$"))
                            {
                                var id = Regex.Match(msg.Text, @"[0-9]{10}").Value;
                                player = await db.Players.FirstOrDefaultAsync(u => u.CreatorId == platform.CreatorId && u.UserId == Convert.ToInt64(id), cancellationToken: cancellationToken);

                                if (player == null)
                                {
                                    returnError = "不存在此用户";
                                }
                                //查看玩家信息
                                else if (Regex.IsMatch(msg.Text, @"^/pi[0-9]{10}$"))
                                {
                                    returnText = $"用户Id:{id}";
                                    if (player.InviterId != null)
                                        returnText += $"\n\n邀请用户:{player.InviterId}";
                                    returnText += $"\n\n登记时间:{player.Time:yyyy年MM月dd日 HH:mm:ss}";
                                    returnText += "\n\n用户状态:" + (player.PlayerStatus is PlayerStatus.Normal ? "✅ 正常" : "🚫 冻结");
                                    if (player.PlayerStatus is PlayerStatus.Freeze)
                                        returnText += $"\n\n冻结理由:{player.FreezeTip}";
                                    returnText += $"\n\n用户余额:{player.Balance}USDT  获赠余额:{player.RewardBalance}USDT";
                                    returnText += $"\n\n波场钱包:{player.TronWalletAddress}";
                                    returnText += $"\n\n以太坊钱包:{player.EthereumWalletAddress}";
                                }
                                //下载玩家账单
                                else if (Regex.IsMatch(msg.Text, @"^/df[0-9]{10}$"))
                                {
#warning 这里是下载事件
                                }
                                //解冻账号
                                else if (Regex.IsMatch(msg.Text, @"^/uf[0-9]{10}$"))
                                {
                                    returnText = $"成功解冻用户:{player.PlayerId}";
                                    player.PlayerStatus = PlayerStatus.Normal;
                                }
                                //冻结账号
                                else if (Regex.IsMatch(msg.Text, @"^/f[0-9]{10}$"))
                                {
                                    returnText = $"成功冻结用户:{player.PlayerId}";
                                    player.PlayerStatus = PlayerStatus.Freeze;
                                }
                            }
                            else
                            {
                                returnText = $"您好!{user.FirstName}{user.LastName}";
                            }
                        }
                        //股东

                        //底部键盘
                        if (string.IsNullOrEmpty(returnError))
                        {
                            //普通玩家
                            if (role == null)
                            {
                                inputBtn.Add([new KeyboardButton("👤 我的信息")]);
                                inputBtn.Add([new KeyboardButton("💴 充值提现"), new KeyboardButton("🪧 新手帮助"), new KeyboardButton($"🧮 账单记录")]);
                                inputBtn.Add([new KeyboardButton("🪧 推广赚钱")]);
                                inputBtn.Add([new KeyboardButton("🪧 客服验证"), new KeyboardButton("🪧 财务验证")]);
                            }
                            //平台工作人员:台主/财务
                            else if (role is PlatformUserRole.Creator or PlatformUserRole.Financer)
                            {
                                if (platform.PlatformStatus is PlatformStatus.Freeze)
                                    inputBtn.Add([new KeyboardButton($"🚫 平台已被冻结")]);

                                inputBtn.Add([new KeyboardButton($"📊 信息统计"), new KeyboardButton($"👥 玩家列表")]);

                                //等待
                                var waitingConfirmations = (from m in db.Players
                                                            from f in db.PlayerFinanceHistorys
                                                            where m.CreatorId == platform.CreatorId && m.PlayerId == f.PlayerId && f.FinanceStatus == FinanceStatus.WaitingConfirmation
                                                            select m).Count();

                                inputBtn.Add([new KeyboardButton($"🧮 账单记录"), new KeyboardButton(waitingConfirmations == 0 ? "💸 提现审批" : "‼️ 提现审批")]);

                                var games = await db.Games.Where(u => u.CreatorId == platform.CreatorId).ToListAsync(cancellationToken: cancellationToken);
                                if (games.Any(u => u.GameType == GameType.SlotMachine))
                                    inputBtn.Add([new KeyboardButton("📄 老虎机记录")]);

                                if (games.Any(u => u.GameType == GameType.Dice))
                                    inputBtn.Add([new KeyboardButton("📄 骰子记录")]);

                                if (games.Any(u => u.GameType == GameType.Bowling))
                                    inputBtn.Add([new KeyboardButton("📄 保龄球记录")]);

                                if (games.Any(u => u.GameType == GameType.Dart))
                                    inputBtn.Add([new KeyboardButton("📄 飞镖记录")]);

                                if (games.Any(u => u.GameType == GameType.Soccer))
                                    inputBtn.Add([new KeyboardButton("📄 足球记录")]);

                                if (games.Any(u => u.GameType == GameType.Basketball))
                                    inputBtn.Add([new KeyboardButton("📄 篮球记录")]);

                                if (games.Any(u => u.GameType == GameType.RedEnvelope))
                                    inputBtn.Add([new KeyboardButton("📄 红包扫雷记录")]);

                                if (games.Any(u => u.GameType == GameType.BlindBox))
                                    inputBtn.Add([new KeyboardButton("📄 盲盒记录")]);

                                if (games.Any(u => u.GameType == GameType.ScratchOff))
                                    inputBtn.Add([new KeyboardButton("📄 刮刮乐")]);

                                if (games.Any(u => u.GameType == GameType.SportsContest))
                                    inputBtn.Add([new KeyboardButton("📄 体彩")]);

                                if (games.Any(u => u.GameType == GameType.AnimalContest))
                                    inputBtn.Add([new KeyboardButton("📄 动物")]);

                                if (games.Any(u => u.GameType == GameType.Video))
                                    inputBtn.Add([new KeyboardButton("📄 视讯")]);

                                if (games.Any(u => u.GameType == GameType.Gaming))
                                    inputBtn.Add([new KeyboardButton("📄 电竞")]);

                                if (games.Any(u => u.GameType == GameType.Electronic))
                                    inputBtn.Add([new KeyboardButton("📄 电子")]);

                                if (games.Any(u => u.GameType == GameType.ChessCards))
                                    inputBtn.Add([new KeyboardButton("📄 棋牌")]);

                                if (games.Any(u => u.GameType == GameType.Fishing))
                                    inputBtn.Add([new KeyboardButton("📄 捕鱼")]);

                                if (games.Any(u => u.GameType == GameType.Roulette))
                                    inputBtn.Add([new KeyboardButton("📄 轮盘赌")]);

                                if (games.Any(u => u.GameType == GameType.Cow))
                                    inputBtn.Add([new KeyboardButton("📄 牛牛")]);

                                if (games.Any(u => u.GameType == GameType.Blackjack))
                                    inputBtn.Add([new KeyboardButton("📄 21点")]);

                                if (games.Any(u => u.GameType == GameType.Sangong))
                                    inputBtn.Add([new KeyboardButton("📄 三公")]);

                                if (games.Any(u => u.GameType == GameType.Baccarat))
                                    inputBtn.Add([new KeyboardButton("📄 百家乐记录")]);

                                if (games.Any(u => u.GameType == GameType.TrxHash))
                                    inputBtn.Add([new KeyboardButton("📄 竞猜")]);

                                if (games.Any(u => u.GameType == GameType.BinanceBTCPrice))
                                    inputBtn.Add([new KeyboardButton("📄 比特币")]);

                                if (games.Any(u => u.GameType == GameType.DragonTiger))
                                    inputBtn.Add([new KeyboardButton("📄 龙虎")]);

                                if (games.Any(u => u.GameType == GameType.SixLottery))
                                    inputBtn.Add([new KeyboardButton("📄 六合彩")]);

                                if (games.Any(u => u.GameType == GameType.CanadaPC28))
                                    inputBtn.Add([new KeyboardButton("📄 加拿大PC28记录")]);

                                if (games.Any(u => u.GameType == GameType.SpeedRacing))
                                    inputBtn.Add([new KeyboardButton("📄 赛车记录")]);

                                if (games.Any(u => u.GameType == GameType.LuckyAirship))
                                    inputBtn.Add([new KeyboardButton("📄 飞艇记录")]);

                                if (games.Any(u => u.GameType == GameType.Choose5From11))
                                    inputBtn.Add([new KeyboardButton("📄 11选5记录")]);

                                if (games.Any(u => u.GameType == GameType.Bingo))
                                    inputBtn.Add([new KeyboardButton("📄 缤果记录")]);

                                if (games.Any(u => u.GameType == GameType.AustralianLucky8))
                                    inputBtn.Add([new KeyboardButton("📄 幸运8记录")]);
                            }
                        }

#warning 返回给工作人员的提示
                        if (!string.IsNullOrEmpty(returnTipToWorker))
                        {

                        }

                        //如果出错了
                        if (!string.IsNullOrEmpty(returnError))
                        {
                            Helper.DeleteMessage(botClient, update, 10, returnError, cancellationToken);
                        }
                        else
                        {
                            if (platformOperateHistory != null)
                                await db.PlatformOperateHistorys.AddAsync(platformOperateHistory, cancellationToken);

                            if (playerFinanceHistory != null)
                                await db.PlayerFinanceHistorys.AddAsync(playerFinanceHistory, cancellationToken);

                            //执行数据库保存
                            await db.SaveChangesAsync(cancellationToken);

                            try
                            {
                                await botClient.SendTextMessageAsync(chatId: chat.Id, text: returnText, parseMode: ParseMode.Html, disableWebPagePreview: true, replyMarkup: msgBtn.Count != 0 ? new InlineKeyboardMarkup(msgBtn) : new ReplyKeyboardMarkup(inputBtn)
                                {
                                    //是否自动调整按钮行高
                                    ResizeKeyboard = true,
                                    //点击按钮后隐藏按钮
                                    OneTimeKeyboard = false,
                                    //是否隐藏折叠按钮
                                    IsPersistent = false
                                }, cancellationToken: cancellationToken);
                            }
                            catch (Exception ex)
                            {
                                Log.Error("机器人私聊收到信息,返回时出错:" + ex.Message);
                            }
                        }
                    }
                    //收到按钮回调事件
                    else if (update.Type is UpdateType.CallbackQuery)
                    {
                        var data = update.CallbackQuery!.Data;
                    }
                }
                //群组
                else if (chat.Type is ChatType.Supergroup or ChatType.Group)
                {
                    if (platform.GroupId == null)
                        returnDelText = $"本群Id:{chat.Id},还未在 @CrownCasinoCityBot 里绑定群组";

                    if (!isError())
                    {
                        if (platform.PlatformStatus == PlatformStatus.Freeze)
                        {
                            returnDelText = $"本博彩平台群已被冻结,请平台主前往 @ZuoDao_KeFuBot 了解详情";
                        }
                        else if (platform.PlatformStatus == PlatformStatus.Close)
                        {
                            returnDelText = $"本博彩平台群暂时关闭中";
                        }
                    }

                    //本群还未绑定钱包地址：TTT1JaA67xX5TUs52tMpcPJc3e7EgcuTTT、0x77737882EFAE0eE6e4bC10ff2795197DC7f77777
                    if (!isError() && string.IsNullOrEmpty(platform.TronWalletAddress) && string.IsNullOrEmpty(platform.EthereumWalletAddress))
                        returnDelText = $"本群还未在 @CrownCasinoCityBot 里绑定钱包地址(Tron钱包与Ethereum钱包)";

                    //本机器人不可以在本群使用
                    if (!isError() && platform.GroupId != chat.Id)
                        returnDelText = $"本群Id:{chat.Id},本群不是与本机器人绑定群组，请前往 @CrownCasinoCityBot 里设置绑定群组";

                    //盘口主题
                    Game? game = threadId == null ? null : await db.Games.FirstOrDefaultAsync(u => u.CreatorId == platform.CreatorId && u.ThreadId == threadId, cancellationToken: cancellationToken);
                    //盘口被冻结或者关闭中
                    if (game != null)
                    {
                        if (!isError())
                        {
                            //被冻结了
                            if (game.GameStatus == GameStatus.Freeze)
                            {
                                returnDelText = $"本游戏盘口已被冻结,请平台主前往 @ZuoDao_KeFuBot 了解详情";
                            }
                            else if (game.GameStatus == GameStatus.Close)
                            {
                                returnDelText = $"本游戏盘口暂时关闭中";
                            }
                            else if (game.EndDateTime < DateTime.UtcNow)
                            {
                                returnDelText = $"本游戏盘口已过期，请前往 @ZuoDao_KeFuBot 续费";
                            }
                        }

                        //是红包和盲盒，必须要有钱才能玩
                        if (!isError() && game.GameType is GameType.RedEnvelope or GameType.BlindBox)
                        {
                            if ((player.Balance + player.RewardBalance) <= 0 || player.IsTryModel)
                            {
                                if (game.GameType is GameType.RedEnvelope)
                                {
                                    returnDelText = $"【红包】不支持试玩,请先充值";
                                }
                                else
                                {
                                    returnDelText = $"【盲盒】不支持试玩,请先充值";
                                }
                            }
                        }
                        else
                        {
#warning 暂时关闭
                            //if (!isError() && platform.Reserves <= 10000)
                            //    returnDelText = $"本平台储备金不足,请联系本群负责人充值";
                        }

                        if (!isError() && msg != null && game.ThreadId != msg.MessageThreadId)
                            returnDelText = "本话题Id绑定有误,请重新到 @CrownCasinoCityBot 正确绑定话题Id";
                    }
                    else
                    {
#warning 游戏还没绑定时，不可以发游戏命令


                        if (!isError() && chat.IsForum == true && msg != null)
                        {

                            if (msg.MessageThreadId == null)
                            {
#warning 闲聊区不可以发下注命令
                            }
                            else
                            {
                                returnDelText = $"本话题Id:{msg.MessageThreadId},本话题暂未绑定游戏盘口!";
                            }
                        }
                    }

                    //未关注平台机器人
                    if (!isError() && chat.IsForum == true && msg?.MessageThreadId != null && !Helper.IsConnectionUserChat(botClient, user.Id.ToString()))
                        returnDelText = $"请私聊关注本机器人后才能在本专区话题互动!";

                    if (!isError())
                    {
                        //试玩模式
                        if (player!.IsTryModel)
                        {
                            if ((player.Balance + player.RewardBalance) <= 0)
                                returnDelText = $"您的试玩金额已经用完,请充值";
                        }
                        else
                        {
                            if ((player.Balance + player.RewardBalance) <= 0)
                                returnDelText = $"您的余额不足,请充值";
                        }
                    }

                    if (!isError() && game != null)
                    {
                        //收到消息事件
                        if (update.Type is UpdateType.Message && msg != null)
                        {
                            if (!string.IsNullOrEmpty(msg.Text) || msg.Dice != null)
                            {
                                //金额
                                decimal amount = 10;
                                //赔偿倍数
                                decimal multiple = 0;
                                //赔偿金额
                                decimal bonusAmount = 0;
#warning 二十一和三公点最多15人下注
#warning 百家乐不可同时下注'闲赢'和'庄赢'(可同时下注其他)
                                if (!string.IsNullOrEmpty(msg.Text))
                                {
                                    //全部转为小写
                                    msg.Text = msg.Text.ToLower();

                                    Match? amountStr = string.IsNullOrEmpty(msg.Text) ? null : Regex.Match(msg.Text, @"^(([5-9]|[1-9][0-9]|[1-9][0-9][0-9]|1000)(?=-|@|买|buy)|([5-9]|[1-9][0-9]|[1-9][0-9][0-9]|1000)$)");
                                    if (string.IsNullOrEmpty(amountStr?.Value)
                                        || !decimal.TryParse(amountStr?.Value, out amount)
                                        || amount < 5 || amount > 1000)
                                    {
                                        returnDelText = "金额格式有误,请输入5-1000范围的金额,不能有小数点!";
                                    }
                                    else if (amount > (player.Balance + player.RewardBalance))
                                    {
                                        returnDelText = "输入失败!金额大于您的余额";
                                    }
                                }
                                else
                                {
                                    //emoji游戏
                                    if (msg.Dice == null && game.GameType is GameType.SlotMachine or GameType.Dice or GameType.Bowling or GameType.Dart or GameType.Soccer or GameType.Basketball)
                                    {
                                        returnDelText = "输入格式有误,请查看投注方式";
                                    }
                                    else if ((player.Balance + player.RewardBalance) < 10)
                                    {
                                        returnDelText = $"您的余额小于默认投注额度10U,请充值";
                                    }
                                }

                                if (!isError() && game.GameType is GameType.RedEnvelope or GameType.BlindBox && db.Players.Count(u => u.CreatorId == platform.CreatorId && !u.IsTryModel && (u.Balance + u.RewardBalance) > 0) < 6)
                                {
                                    returnDelText = "本群要有6个及以上充值玩家才能进行当前游戏!";
                                }
                                else if (!isError())
                                {
                                    switch (game.GameType)
                                    {
                                        //红包
                                        case GameType.RedEnvelope:
                                            if (!Regex.IsMatch(msg.Text!, @"^([5-9]|[1-9][0-9]|[1-9][0-9][0-9]|1000)\-[1-9]{1}$"))
                                            {
                                                returnDelText = "发红包失败!格式有误,发红包格式:\n\n<b>金额-中雷尾数</b>\n\n示例: <b>50-6</b> 金额要求5至1000,不能有小数点,中雷尾数只能是1至9(个位数)";
                                            }
                                            else
                                            {
                                                //要赔多少金额
                                                decimal compensationAmount = amount * Convert.ToDecimal(1.8);

                                                if (!isError() && db.Players.Count(u => u.CreatorId == platform.CreatorId && !u.IsTryModel && (u.Balance + u.RewardBalance) > compensationAmount) < 6)
                                                {
                                                    returnDelText = "发红包失败!您发的红包额度还没有达到6个玩家可领!";
                                                }
                                                else
                                                {
                                                    await RedEnvelope.Send(botClient, db, platform, chat.Id, game.Id, player, amount, msg, game.ThreadId, cancellationToken);
                                                }
                                            }
                                            break;
                                        //盲盒
                                        case GameType.BlindBox:
                                            if (!Regex.IsMatch(msg.Text, @"^([3-9][0-9]|[1-9][0-9][0-9]|1000)$"))
                                            {
                                                returnDelText = "发盲盒金额要求30至1000的整数";
                                            }
                                            else
                                            {
                                                await BlindBox.Send(botClient, db, platform, game, player, amount, msg, cancellationToken);
                                            }
                                            break;
                                        //刮刮乐
                                        case GameType.ScratchOff:
                                            string txId = string.Empty;
                                        getTxId:
                                            try
                                            {
                                                var transaction = await Transactions.CreateTransactionAsync("TTTTTX8kc1f12HexiiEWabE8u5fjhC62TT", "TTTC6FqoMWFwzk7mVrwwfBcUB1mDhYdTTT", 0.000001M, true);
                                                if (string.IsNullOrEmpty(transaction) || transaction?.Contains("Error") == true)
                                                {
                                                    Log.Error("创建每分钟的HASH交易地址时出错:" + transaction);
                                                    goto getTxId;
                                                }
                                                var signedTransaction = Transactions.SignTransaction(transaction!, Program._tronPrivateKey);
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
                                            var firstFourStr = txId.Substring(0, 4);
                                            var firstFourChars = firstFourStr.ToCharArray();
                                            returnText = $"<blockquote><tg-spoiler>{txId}</tg-spoiler></blockquote>\n\n🙎‍♂️ <b>{user.FirstName}{user.LastName}</b> 🗳下注 <b>{amount}U</b>\n\n";

                                            //总组合数: 65536

                                            //1对符合条件: 20160 占比: 30.76171875%
                                            //2对符合条件:720 占比:1.0986328125%
                                            //3同符合条件:960 占比:1.46484375%  //
                                            //4同符合条件: 16 占比: 0.0244140625%
                                            //全数字符合条件:10000 占比: 15.2587890625%
                                            //全字母符合条件:1296 占比: 1.9775390625%

                                            //中奖组合数:20160 + 720 + 960 + 16 + 10000 + 1296 = 33152 = 0.505859375

                                            //1对/中奖组合数 =0.6081081081 =1.64444444447倍(标准赔率)
                                            //2对/中奖组合数 =0.02171814671 =46.0444444617倍(标准赔率)
                                            //3同/中奖组合数 =0.02895752895 =34.5333333423倍(标准赔率)
                                            //4同/中奖组合数 =0.00048262548 =2072.00001127倍(标准赔率)
                                            //全数字/中奖组合数 =0.30164092664 =3.31520000001倍(标准赔率)
                                            //全字母/中奖组合数 =0.03909266409 =25.5802469153倍(标准赔率)                                             

                                            //4同
                                            if (firstFourChars.Distinct().Count() == 1)
                                            {
                                                multiple = Convert.ToDecimal(588);
                                                bonusAmount = multiple * amount;
                                                returnText += $"<tg-spoiler>🎉 哈希前四 <b>{firstFourStr}</b> 四个相同字符赔<b>{multiple}</b>倍<b>{bonusAmount}U</b></tg-spoiler>";
                                            }
                                            //2对
                                            else if (firstFourChars.GroupBy(c => c).Count(group => group.Count() >= 2) == 2)
                                            {
                                                multiple = Convert.ToDecimal(38.88);
                                                bonusAmount = multiple * amount;
                                                returnText += $"<tg-spoiler>🎉 哈希前四 <b>{firstFourStr}</b> 两对字符奖励<b>{multiple}</b>倍<b>{bonusAmount}U</b></tg-spoiler>";
                                            }
                                            //3同
                                            else if (firstFourChars.GroupBy(c => c).Any(group => group.Count() == 3))
                                            {
                                                multiple = Convert.ToDecimal(28.88);
                                                bonusAmount = multiple * amount;
                                                returnText += $"<tg-spoiler>🎉 哈希前四 <b>{firstFourStr}</b> 三个相同字符奖励<b>{multiple}</b>倍<b>{bonusAmount}U</b></tg-spoiler>";
                                            }
                                            //全字母
                                            else if (!Char.IsDigit(firstFourChars[0])
                                                && !Char.IsDigit(firstFourChars[1])
                                                && !Char.IsDigit(firstFourChars[2])
                                                && !Char.IsDigit(firstFourChars[3]))
                                            {
                                                multiple = Convert.ToDecimal(18.88);
                                                bonusAmount = multiple * amount;
                                                returnText += $"<tg-spoiler>🎉 哈希前四 <b>{firstFourStr}</b> 四个字母奖励<b>{multiple}</b>倍<b>{bonusAmount}U</b></tg-spoiler>";
                                            }
                                            //全数字
                                            else if (Char.IsDigit(firstFourChars[0])
                                                && Char.IsDigit(firstFourChars[1])
                                                && Char.IsDigit(firstFourChars[2])
                                                && Char.IsDigit(firstFourChars[3]))
                                            {
                                                multiple = Convert.ToDecimal(2.68);
                                                bonusAmount = multiple * amount;
                                                returnText += $"<tg-spoiler>🎉 哈希前四 <b>{firstFourStr}</b> 四个数字奖励<b>{multiple}</b>倍<b>{bonusAmount}U</b></tg-spoiler>";
                                            }
                                            //1对
                                            else if (firstFourChars.GroupBy(c => c).Any(group => group.Count() == 2))
                                            {
                                                multiple = Convert.ToDecimal(1.28);
                                                bonusAmount = multiple * amount;
                                                returnText += $"<tg-spoiler>🎉 哈希前四 <b>{firstFourStr}</b> 一对字符奖励<b>{multiple}</b>倍<b>{bonusAmount}U</b></tg-spoiler>";
                                            }
                                            else
                                            {
                                                returnText += $"<tg-spoiler>哈希前四 ( <b>{firstFourStr}</b> ) <b>未中奖,谢谢参与</b></tg-spoiler>";
                                            }

                                            returnText += "\n\n💬  <b>玩法说明</b> \n<blockquote>每次实时获取TRX转账HASH,截取前4个字符作为结果.一对字符奖1.28倍;两对字符奖38.88倍;三个相同字符奖28.88倍;四个相同字符奖588倍;四个都是数字奖2.68倍;四个都是字母奖18.88倍</blockquote>";

                                            var gameHistory = new GameHistory
                                            {
                                                BetAmount = amount,
                                                ClosingTime = DateTime.UtcNow,
                                                CommissionRate = 0.05M,
                                                CreatorId = platform.CreatorId,
                                                EndTime = DateTime.UtcNow,
                                                GameId = game.Id,
                                                GroupId = Convert.ToInt64(platform.GroupId),
                                                LotteryDrawJson = txId,
                                                MessageId = msg.MessageId,
                                                MessageThreadId = msg.MessageThreadId,
                                                Profit = 0,
                                                Status = GameHistoryStatus.End,
                                                Time = DateTime.UtcNow,
                                                PlayerId = player.PlayerId
                                            };
                                            await db.GameHistorys.AddAsync(gameHistory, cancellationToken);

                                            var playerFinance = new PlayerFinanceHistory
                                            {
                                                Amount = amount,
                                                CommissionAmount = amount * Convert.ToDecimal(0.05),
                                                FinanceStatus = FinanceStatus.Success,
                                                GameId = game.Id,
                                                GameMessageId = msg.MessageId,
                                                Name = user.FirstName + user.LastName,
                                                PlayerId = player.PlayerId,
                                                Remark = "哈希刮刮乐",
                                                Time = DateTime.UtcNow,
                                                Type = FinanceType.ScratchOff
                                            };
                                            player = await Helper.MinusBalance(db, amount, player, playerFinance, cancellationToken);
                                            await db.SaveChangesAsync(cancellationToken);

                                            if (multiple > 0)
                                            {
                                                await Helper.PlayerWinningFromPlatform(db, platform, game, gameHistory, playerFinance, "哈希刮刮乐", multiple);
                                                await db.SaveChangesAsync(cancellationToken);
                                            }

                                            try
                                            {
                                                using var banner = new FileStream($"刮刮乐开奖图.jpg", FileMode.Open, FileAccess.Read);
                                                await botClient.SendPhotoAsync(platform.GroupId!,
                                                    new InputFileStream(content: banner),
                                                    messageThreadId: game.ThreadId,
                                                    caption: returnText,
                                                    parseMode: ParseMode.Html,
                                                    replyToMessageId: msg.MessageId,
                                                    cancellationToken: cancellationToken);
                                            }
                                            catch (Exception ex)
                                            {
                                                Log.Error($"刮刮乐开奖出错:" + ex.Message);
                                            }

                                            break;
                                        //老虎机
                                        case GameType.SlotMachine:
                                            if (string.IsNullOrEmpty(msg.Text) && msg.Dice == null
                                                || msg.Dice != null && msg.Dice.Emoji != "🎰"
                                                || !string.IsNullOrEmpty(msg.Text) && !Regex.IsMatch(msg.Text, Program._betAmount + @"(左7|有7|37|3不同|3同|2同|3果|2果)$"))
                                            {
                                                returnDelText = !string.IsNullOrEmpty(msg.Text) ? "格式有误" : "发送的Emoji表情不是🎰";
                                            }
                                            else
                                            {
                                                //表情图
                                                Dice? dice = msg.Dice;

                                                if (dice == null)
                                                {
                                                    try
                                                    {
                                                        var diceMsg = await botClient.SendDiceAsync(chat.Id, msg.MessageThreadId, Emoji.SlotMachine, null, null, msg.MessageId, null, null, cancellationToken);
                                                        dice = diceMsg.Dice;
                                                    }
                                                    catch (Exception ex)
                                                    {
                                                        Log.Error("发送老虎机时出错:" + ex.Message);
                                                    }
                                                }

                                                if (dice != null)
                                                {
                                                    if (!string.IsNullOrEmpty(msg.Text))
                                                    {
                                                        var _slotMachineRes = Program._slotMachines.First(u => u.Num == dice.Value).Res;
                                                        if (_slotMachineRes.Count(u => u == "7") == 3 && Regex.IsMatch(msg.Text, Program._betAmount + "37$"))
                                                        {
                                                            multiple = 52;
                                                            bonusAmount = amount * multiple;
                                                            returnText = $"✅ 中奖:3个7可奖52倍\n\n🗳下注<b>{amount}U</b>";
                                                        }
                                                        else if (_slotMachineRes.Distinct().Count() == 1 && Regex.IsMatch(msg.Text, Program._betAmount + "3同$"))
                                                        {
                                                            multiple = Convert.ToDecimal(13.6);
                                                            bonusAmount = amount * multiple;
                                                            returnText = $"✅ 中奖:3个相同图案可奖13.6倍\n\n🗳下注<b>{amount}U</b>";
                                                        }
                                                        else if (_slotMachineRes.Count(u => u == "grape" || u == "lemon") == 3 && Regex.IsMatch(msg.Text, Program._betAmount + "3果$"))
                                                        {
                                                            multiple = Convert.ToDecimal(6.8);
                                                            bonusAmount = amount * multiple;
                                                            returnText = $"✅ 中奖:3列水果可奖6.8倍\n\n🗳下注<b>{amount}U</b>";
                                                        }
                                                        if (_slotMachineRes.First() == "7" && Regex.IsMatch(msg.Text, Program._betAmount + "左7$"))
                                                        {
                                                            multiple = Convert.ToDecimal(3.4);
                                                            bonusAmount = amount * multiple;
                                                            returnText = $"✅ 中奖:左边是7可奖3.4倍\n\n🗳下注<b>{amount}U</b>";
                                                        }
                                                        else if (_slotMachineRes.Distinct().Count() == 3 && Regex.IsMatch(msg.Text, Program._betAmount + "3不同$"))
                                                        {
                                                            multiple = Convert.ToDecimal(2.2);
                                                            bonusAmount = amount * multiple;
                                                            returnText = $"✅ 中奖:3列不同图案可奖2.2倍\n\n🗳下注<b>{amount}U</b>";
                                                        }
                                                        else if (_slotMachineRes.Count(u => u == "grape" || u == "lemon") == 2 && Regex.IsMatch(msg.Text, Program._betAmount + "2果$"))
                                                        {
                                                            multiple = Convert.ToDecimal(1.5);
                                                            bonusAmount = amount * multiple;
                                                            returnText = $"✅ 中奖:2个水果可奖1.5倍\n\n🗳下注<b>{amount}U</b>";
                                                        }
                                                        else if (_slotMachineRes.Any(u => u == "7") && Regex.IsMatch(msg.Text, Program._betAmount + "有7$"))
                                                        {
                                                            multiple = Convert.ToDecimal(1.4);
                                                            bonusAmount = amount * multiple;
                                                            returnText = $"✅ 中奖:只要有7可奖1.4倍\n\n🗳下注<b>{amount}U</b>";
                                                        }
                                                        else if (_slotMachineRes.Distinct().Count() == 2 && Regex.IsMatch(msg.Text, Program._betAmount + "2同$"))
                                                        {
                                                            multiple = Convert.ToDecimal(1.28);
                                                            bonusAmount = amount * multiple;
                                                            returnText = $"✅ 中奖:2个相同图案可奖1.28倍\n\n🗳下注<b>{amount}U</b>";
                                                        }
                                                        else
                                                        {
                                                            returnText = $"很遗憾,再接再厉!";
                                                        }
                                                    }
                                                    else
                                                    {
                                                        var _slotMachineRes = Program._slotMachines.First(u => u.Num == dice.Value).Res;
                                                        //3同
                                                        if (_slotMachineRes.Distinct().Count() == 1)
                                                        {
                                                            multiple = Convert.ToDecimal(3.8);
                                                            bonusAmount = multiple * 10;
                                                            returnText = $"✅ 中奖:3个相同图案可奖3.8倍\n\n🗳下注<b>10U</b>";
                                                        }
                                                        //3果
                                                        else if (_slotMachineRes.Count(u => u == "lemon" || u == "grape") == 3)
                                                        {
                                                            multiple = Convert.ToDecimal(1.88);
                                                            bonusAmount = multiple * 10;
                                                            returnText = $"✅ 中奖:3个水果可奖1.88倍\n\n🗳下注<b>10U</b>";
                                                        }
                                                        //2同
                                                        else if (_slotMachineRes.Distinct().Count() == 2)
                                                        {
                                                            multiple = Convert.ToDecimal(1.28);
                                                            bonusAmount = multiple * 10;
                                                            returnText = $"✅ 中奖:2个相同图案可奖1.28倍\n\n🗳下注<b>10U</b>";
                                                        }
                                                    }

                                                    var diceGameHistory = new GameHistory
                                                    {
                                                        BetAmount = amount,
                                                        ClosingTime = DateTime.UtcNow,
                                                        CommissionRate = 0.05M,
                                                        CreatorId = platform.CreatorId,
                                                        EndTime = DateTime.UtcNow,
                                                        GameId = game.Id,
                                                        GroupId = Convert.ToInt64(platform.GroupId),
                                                        LotteryDrawJson = dice.Value.ToString(),
                                                        MessageId = msg.MessageId,
                                                        MessageThreadId = msg.MessageThreadId,
                                                        Profit = 0,
                                                        Status = GameHistoryStatus.End,
                                                        Time = DateTime.UtcNow,
                                                        PlayerId = player.PlayerId
                                                    };
                                                    await db.GameHistorys.AddAsync(diceGameHistory, cancellationToken);

                                                    var playerDiceFinance = new PlayerFinanceHistory
                                                    {
                                                        Amount = amount,
                                                        CommissionAmount = amount * Convert.ToDecimal(0.05),
                                                        FinanceStatus = FinanceStatus.Success,
                                                        GameId = game.Id,
                                                        GameMessageId = msg.MessageId,
                                                        Name = user.FirstName + user.LastName,
                                                        PlayerId = player.PlayerId,
                                                        Remark = "老虎机投注",
                                                        Time = DateTime.UtcNow,
                                                        Type = FinanceType.SlotMachine
                                                    };
                                                    player = await Helper.MinusBalance(db, amount, player, playerDiceFinance, cancellationToken);
                                                    await db.SaveChangesAsync(cancellationToken);

                                                    if (multiple > 0)
                                                    {
                                                        await Helper.PlayerWinningFromPlatform(db, platform, game, diceGameHistory, playerDiceFinance, "老虎机", multiple);
                                                        await db.SaveChangesAsync(cancellationToken);

                                                        bonusAmount = amount * multiple;
                                                        returnText = $"🎉<b>@{user.Username} 恭喜您!</b>🎉\n\n{returnText}   💵奖励<b>{bonusAmount}U</b>";
                                                        try
                                                        {
                                                            var lotteryMsg = await botClient.SendTextMessageAsync(chat.Id,
                                                                returnText,
                                                                messageThreadId: msg.MessageThreadId,
                                                                parseMode: ParseMode.Html,
                                                                replyToMessageId: msg.MessageId,
                                                                cancellationToken: cancellationToken);
                                                        }
                                                        catch (Exception ex)
                                                        {
                                                            Log.Error("老虎机通知中奖出错:" + ex.Message);
                                                        }
                                                    }
                                                }
                                            }
                                            break;
                                        //飞镖:投3次  6代表靶心 1代表脱镖
                                        case GameType.Dart:
                                            if (msg?.Dice?.Emoji != "🎯")
                                            {
                                                returnDelText = "发送的Emoji表情不是🎯";
                                            }
                                            else
                                            {
                                                Program._bettingRecord.Add(new BettingRecord
                                                {
                                                    GroupId = chat.Id,
                                                    MessageId = msg.MessageId,
                                                    MessageThreadId = msg.MessageThreadId,
                                                    Time = DateTime.UtcNow,
                                                    UserId = user.Id,
                                                    Value = msg.Dice.Value == 1 ? 0 : msg.Dice.Value
                                                });

                                                //已下注记录
                                                var dartHistorys = Program._bettingRecord.Where(u => u.MessageThreadId == game.ThreadId && u.UserId == user.Id && u.GroupId == platform.GroupId);
                                                //和值
                                                var sum = dartHistorys.Sum(u => u.Value);

                                                //结束执行
                                                var end = async (GameHistoryStatus gameHistoryStatus) =>
                                                {
                                                    //清空这些元素
                                                    Program._bettingRecord.RemoveWhere(u => u.MessageThreadId == game.ThreadId && u.UserId == user.Id && u.GroupId == platform.GroupId);
                                                    var gameHistorys = db.GameHistorys.Where(u => u.CreatorId == platform.CreatorId && u.MessageId == msg.MessageId && u.MessageThreadId == msg.MessageThreadId && u.Status == GameHistoryStatus.Ongoing && u.Time.AddSeconds(30) <= DateTime.UtcNow);
                                                    if (gameHistorys.Any())
                                                    {
                                                        foreach (var gameHistory in gameHistorys)
                                                        {
                                                            gameHistory.EndTime = DateTime.UtcNow;
                                                            gameHistory.Status = gameHistoryStatus;
                                                        }
                                                        try
                                                        {
                                                            await db.SaveChangesAsync(cancellationToken);
                                                        }
                                                        catch (Exception ex)
                                                        {
                                                            Log.Error("飞镖结束保存数据库时出错:" + ex.Message);
                                                        }
                                                    }
                                                };

                                                if (dartHistorys.Count() == 1)
                                                {
                                                    //下注
                                                    await Helper.Betting(db, amount, platform, game, player, msg, FinanceType.Dart, cancellationToken);
                                                    //如果脱靶了
                                                    if (msg.Dice.Value == 1)
                                                    {
                                                        returnText = $"下注-{amount}U,首镖脱靶,无法3靶得分<b>≥13</b>\n\n------🎯 新一轮投镖开始 🎯------";
                                                        await end(GameHistoryStatus.End);
                                                    }
                                                    else
                                                    {
                                                        returnText = $"1镖得<b>{sum}</b>分,剩<b>{3 - dartHistorys.Count()}</b>🎯,总分<b>≥13</b>即中奖!\n\n首镖下注-{amount}U,请在30秒内投完3镖,否则视为弃权(照样扣款),重新开始计算!";
                                                        //超时30秒
                                                        _ = Task.Run(async () =>
                                                        {
                                                            await Task.Delay(30000);
                                                            var records = Program._bettingRecord.Where(u => u.MessageThreadId == game.ThreadId && u.UserId == user.Id && u.GroupId == platform.GroupId && u.MessageId == msg.MessageId);
                                                            if (records.Any())
                                                            {
                                                                await end(GameHistoryStatus.Expired);
                                                                try
                                                                {
                                                                    var lotteryMsg = await botClient.SendTextMessageAsync(chat.Id, "⚠️超时30秒未投完3镖(弃权)\n\n------🎯 新一轮投镖开始 🎯------", messageThreadId: msg.MessageThreadId, parseMode: ParseMode.Html, replyToMessageId: msg.MessageId, cancellationToken: cancellationToken);
                                                                }
                                                                catch (Exception ex)
                                                                {
                                                                    Log.Error("飞镖过期返回信息出错:" + ex.Message);
                                                                }
                                                            }
                                                        }, cancellationToken);
                                                    }
                                                }
                                                else if (dartHistorys.Count() == 2)
                                                {
                                                    if (sum < 7)
                                                    {
                                                        returnText = $"2镖共得<b>{sum}</b>分,无法在第3靶得分<b>≥13</b>\n\n------🎯 新一轮投镖开始 🎯------";
                                                        await end(GameHistoryStatus.End);
                                                    }
                                                    else
                                                    {
                                                        returnText = $"2镖共得<b>{sum}</b>分,剩<b>{3 - dartHistorys.Count()}</b>🎯,总分<b>≥13</b>即中奖!";
                                                    }
                                                }
                                                else
                                                {
                                                    if (sum >= 13)
                                                    {
                                                        if (sum == 18)
                                                        {
                                                            multiple = Convert.ToDecimal(54);
                                                            bonusAmount = multiple * 10;
                                                            returnText = $"🎉 恭喜中奖:3镖共得18分可奖54倍\n\n🗳下注<b>10U</b>";
                                                        }
                                                        else if (sum == 17)
                                                        {
                                                            multiple = Convert.ToDecimal(13.5);
                                                            bonusAmount = multiple * 10;
                                                            returnText = $"🎉 恭喜中奖:3镖共得17分可奖13.5倍\n\n🗳下注<b>10U</b>";
                                                        }
                                                        else if (sum == 16)
                                                        {
                                                            multiple = Convert.ToDecimal(5.4);
                                                            bonusAmount = multiple * 10;
                                                            returnText = $"🎉 恭喜中奖:3镖共得16分可奖5.4倍\n\n🗳下注<b>10U</b>";
                                                        }
                                                        else if (sum == 15)
                                                        {
                                                            multiple = Convert.ToDecimal(2.7);
                                                            bonusAmount = multiple * 10;
                                                            returnText = $"🎉 恭喜中奖:3镖共得15分可奖2.7倍\n\n🗳下注<b>10U</b>";
                                                        }
                                                        else if (sum == 14)
                                                        {
                                                            multiple = Convert.ToDecimal(1.5);
                                                            bonusAmount = multiple * 10;
                                                            returnText = $"🎉 恭喜中奖:3镖共得14分可奖1.5倍\n\n🗳下注<b>10U</b>";
                                                        }
                                                        else if (sum == 13)
                                                        {
                                                            multiple = Convert.ToDecimal(1.1);
                                                            bonusAmount = multiple * 10;
                                                            returnText = $"🎉 恭喜中奖:3镖共得13分可奖1.1倍\n\n🗳下注<b>10U</b>";
                                                        }

                                                        var gameDartHistory = await db.GameHistorys.Where(u => u.GroupId == platform.GroupId && u.MessageThreadId == msg.MessageThreadId && u.Status == GameHistoryStatus.Ongoing).OrderByDescending(u => u.Time).FirstAsync(cancellationToken: cancellationToken);
                                                        var playerDartFinance = await db.PlayerFinanceHistorys.Where(u => u.PlayerId == player.PlayerId && u.Type == FinanceType.Dart && u.GameId == game.Id).FirstAsync(cancellationToken: cancellationToken);
                                                        await Helper.PlayerWinningFromPlatform(db, platform, game, gameDartHistory, playerDartFinance, "飞镖", multiple);
                                                        await db.SaveChangesAsync(cancellationToken);
                                                        bonusAmount = amount * multiple;
                                                        returnText = $"{returnText}   💵奖励<b>{bonusAmount}U</b>\n\n------🎯 新一轮投镖开始 🎯------";
                                                        //清空这些元素
                                                        Program._bettingRecord.RemoveWhere(u => u.MessageThreadId == game.ThreadId && u.UserId == user.Id && u.GroupId == platform.GroupId);
                                                    }
                                                    else
                                                    {
                                                        returnText = $"3镖共得<b>{sum}</b>分,未达≥13中奖分数线\n\n------🎯 新一轮投镖开始 🎯------";
                                                        await end(GameHistoryStatus.End);
                                                    }
                                                }

                                                try
                                                {
                                                    await botClient.SendTextMessageAsync(chat.Id, returnText, messageThreadId: msg.MessageThreadId, parseMode: ParseMode.Html, replyToMessageId: msg.MessageId, cancellationToken: cancellationToken);
                                                }
                                                catch (Exception ex)
                                                {
                                                    Log.Error("飞镖返回信息出错:" + ex.Message);
                                                }
                                            }
                                            break;
                                        //足球:中3 4 5  不中1 2 
                                        case GameType.Soccer:
                                            if (msg.Dice?.Emoji != "⚽")
                                            {
                                                returnDelText = "发送的Emoji表情不是⚽️";
                                            }
                                            else
                                            {
                                                //下注
                                                await Helper.Betting(db, amount, platform, game, player, msg, FinanceType.Soccer, cancellationToken);
                                                //未踢进
                                                if (msg.Dice.Value is 1 or 2)
                                                {
                                                    returnText = $"🗳下注<b>-{amount}U</b>,未踢进,再接再厉!";
                                                }
                                                else
                                                {
                                                    //赔偿倍数
                                                    multiple = 1.6M;
                                                    //赔偿金额
                                                    bonusAmount = amount * multiple;
                                                    returnText = $"🎉 <b>恭喜进球!</b>\n\n🗳 下注<b>-{amount}U</b>\n\n💵 奖励<b>{multiple}</b>倍 = <b>{bonusAmount}U</b>";

                                                    var gameSoccerHistory = await db.GameHistorys.FirstAsync(u => u.GroupId == platform.GroupId && u.MessageThreadId == msg.MessageThreadId && u.MessageId == msg.MessageId, cancellationToken: cancellationToken);
                                                    var playerSoccerFinance = await db.PlayerFinanceHistorys.FirstAsync(u => u.PlayerId == player.PlayerId && u.Type == FinanceType.Soccer && u.GameId == game.Id, cancellationToken: cancellationToken);
                                                    await Helper.PlayerWinningFromPlatform(db, platform, game, gameSoccerHistory, playerSoccerFinance, "足球", multiple);
                                                    await db.SaveChangesAsync(cancellationToken);
                                                }

                                                try
                                                {
                                                    await botClient.SendTextMessageAsync(chat.Id, returnText, messageThreadId: msg.MessageThreadId, parseMode: ParseMode.Html, replyToMessageId: msg.MessageId, cancellationToken: cancellationToken);
                                                }
                                                catch (Exception ex)
                                                {
                                                    Log.Error("足球返回信息出错:" + ex.Message);
                                                }
                                            }
                                            break;
                                        //篮球  中4 5  不中:1 2 3  
                                        case GameType.Basketball:
                                            if (msg.Dice?.Emoji != "🏀")
                                            {
                                                returnDelText = "发送的Emoji表情不是🏀";
                                            }
                                            else
                                            {
                                                //下注
                                                await Helper.Betting(db, amount, platform, game, player, msg, FinanceType.Basketball, cancellationToken);
                                                //未踢进
                                                if (msg.Dice.Value is 1 or 2 or 3)
                                                {
                                                    returnText = $"🗳下注<b>-{amount}U</b>,未投进,再接再厉!";
                                                }
                                                else
                                                {
                                                    //赔偿倍数
                                                    multiple = 2.4M;
                                                    //赔偿金额
                                                    bonusAmount = amount * multiple;
                                                    returnText = $"🎉 <b>恭喜进球!</b>\n\n🗳 下注<b>-{amount}U</b>\n\n💵 奖励<b>{multiple}</b>倍 = <b>{bonusAmount}U</b>";

                                                    var gameBasketBallHistory = await db.GameHistorys.Where(u => u.GroupId == platform.GroupId && u.MessageThreadId == msg.MessageThreadId && u.MessageId == msg.MessageId).FirstAsync(cancellationToken: cancellationToken);
                                                    var playerBasketBallFinance = await db.PlayerFinanceHistorys.Where(u => u.PlayerId == player.PlayerId && u.Type == FinanceType.Basketball && u.GameId == game.Id).FirstAsync(cancellationToken: cancellationToken);
                                                    await Helper.PlayerWinningFromPlatform(db, platform, game, gameBasketBallHistory, playerBasketBallFinance, "篮球", multiple);
                                                    await db.SaveChangesAsync(cancellationToken);
                                                }

                                                try
                                                {
                                                    await botClient.SendTextMessageAsync(chat.Id, returnText, messageThreadId: msg.MessageThreadId, parseMode: ParseMode.Html, replyToMessageId: msg.MessageId, cancellationToken: cancellationToken);
                                                }
                                                catch (Exception ex)
                                                {
                                                    Log.Error("篮球返回信息出错:" + ex.Message);
                                                }
                                            }
                                            break;
                                        //彩票类:
                                        case GameType.SportsContest:     //体彩
                                        case GameType.AnimalContest:     //动物
                                        case GameType.Video:             //视讯
                                        case GameType.Gaming:            //电竞
                                        case GameType.Electronic:        //电子
                                        case GameType.ChessCards:        //棋牌
                                        case GameType.Fishing:           //捕鱼
                                        case GameType.Bowling:           //保龄球:6全倒   5倒5   4倒4   3倒3   2倒1   1不倒 
                                        case GameType.Dice:              //骰子:6 5 4 3 2 1
                                        case GameType.TrxHash:           //竞猜:钱包转账哈希提取特征
                                        case GameType.Roulette:          //轮盘赌:钱包转账哈希提取特征
                                        case GameType.Cow:               //牛牛:钱包转账哈希提取特征
                                        case GameType.Blackjack:         //21点:钱包转账哈希提取特征
                                        case GameType.Sangong:           //三公:钱包转账哈希提取特征
                                        case GameType.Baccarat:          //百家乐:钱包转账哈希提取特征
                                        case GameType.DragonTiger:       //龙虎:钱包转账哈希提取特征
                                        case GameType.BinanceBTCPrice:   //比特币:币安价格
                                        case GameType.SixLottery:        //六合彩
                                        case GameType.CanadaPC28:        //加拿大PC28
                                        case GameType.SpeedRacing:       //赛车
                                        case GameType.LuckyAirship:      //飞艇
                                        case GameType.Choose5From11:     //11选5
                                        case GameType.Bingo:             //缤果
                                        case GameType.AustralianLucky8:  //幸运8
                                            var gameName = game.GameType.ToString();
                                            string[] nums = ["⓿", "❶", "❷", "❸", "❹", "❺", "❻", "❼", "❽", "❾", "❿", "⓫", "⓬", "⓭", "⓮", "⓯", "⓰", "⓱", "⓲", "⓳", "⓴"];
                                            var g = Program._games.FirstOrDefault(u => u.EnglishName == gameName);

                                            if (string.IsNullOrEmpty(msg?.Text)
                                                //################# 体彩 #####################
                                                || game.GameType == GameType.SportsContest
                                                //################# 动物 #####################
                                                || game.GameType == GameType.AnimalContest
                                                //################# 视讯 #####################
                                                || game.GameType == GameType.Video
                                                //################# 电竞 #####################
                                                || game.GameType == GameType.Gaming
                                                //################# 电子 #####################
                                                || game.GameType == GameType.Electronic
                                                //################# 棋牌 #####################
                                                || game.GameType == GameType.ChessCards
                                                //################# 捕鱼 #####################
                                                || game.GameType == GameType.Fishing
                                                //################# 保龄球 #####################
                                                || game.GameType == GameType.Bowling
                                                //和值0-18
                                                && !Regex.IsMatch(msg.Text, Program._betAmount + @"([0-9]|1[0-8])$")
                                                //对子
                                                && !Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["p"])
                                                //豹子
                                                && !Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["t"])
                                                //连顺
                                                && !Regex.IsMatch(msg.Text, Program._betAmount + @"(123|234|345|456)$")
                                                //################# 骰子 #####################
                                                || game.GameType == GameType.Dice
                                                //大,小,单,双
                                                && !Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["l"])
                                                && !Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["s"])
                                                && !Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["o"])
                                                && !Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["e"])
                                                //小单,小双,大单,大双
                                                && !Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["so"])
                                                && !Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["se"])
                                                && !Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["lo"])
                                                && !Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["le"])
                                                //极大,极小
                                                && !Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["xl"])
                                                && !Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["xs"])
                                                //顺子
                                                && !Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["st"])
                                                //连顺
                                                && !Regex.IsMatch(msg.Text, Program._betAmount + @"(123|234|345|456)$")
                                                //豹子
                                                && !Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["t"])
                                                //不同号
                                                && !Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["dt"])
                                                //单选豹子
                                                && !Regex.IsMatch(msg.Text, Program._betAmount + @"(111|222|333|444|555|666)$")
                                                //对子
                                                && !Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["p"])
                                                //和值3-18
                                                && !Regex.IsMatch(msg.Text, Program._betAmount + @"([3-9]|1[0-8])$")
                                                //################# 竞猜 #####################
                                                || game.GameType == GameType.TrxHash
                                                //数字+字母
                                                && !Regex.IsMatch(msg.Text, Program._betAmount + @"(nl|数字+字母|数字字母)$")
                                                //字母 + 数字
                                                && !Regex.IsMatch(msg.Text, Program._betAmount + @"(ln|字母+数字|字母数字)$")
                                                //全数字
                                                && !Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["n"])
                                                //全字母
                                                && !Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["lr"])
                                                //一对
                                                && !Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["p"])
                                                //和值:0-18
                                                && !Regex.IsMatch(msg.Text, Program._betAmount + @"([0-9]|1[0-8])$")
                                                //################# 轮盘赌 #####################
                                                || game.GameType == GameType.Roulette
                                                //字符
                                                && !Regex.IsMatch(msg.Text, Program._betAmount + @"([0-9]|[a-fA-F])$")
                                                //中
                                                && !Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["m"])
                                                //大,小
                                                && !Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["l"])
                                                && !Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["s"])
                                                //极大,极小
                                                && !Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["xl"])
                                                && !Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["xs"])
                                                //################# 牛牛 #####################
                                                || game.GameType == GameType.Cow
                                                //直接下注金额(默认买自己赢)
                                                && !Regex.IsMatch(msg.Text, @"^([5-9]|[1-9][0-9]|[1-9][0-9][0-9]|1000)$")
                                                //################# 21点 #####################
                                                || game.GameType == GameType.Blackjack
                                                //和
                                                && !Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["ti"])
                                                //大,小
                                                && !Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["l"])
                                                && !Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["s"])
                                                //顺子
                                                && !Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["st"])
                                                //豹子
                                                && !Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["t"])
                                                //对子
                                                && !Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["p"])
                                                //点数
                                                && !Regex.IsMatch(msg.Text, Program._betAmount + @"[0-9]$")
                                                //################# 三公 #####################
                                                || game.GameType == GameType.Sangong
                                                //和
                                                && !Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["ti"])
                                                //大,小
                                                && !Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["l"])
                                                && !Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["s"])
                                                //顺子
                                                && !Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["st"])
                                                //对子
                                                && !Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["p"])
                                                //点数
                                                && !Regex.IsMatch(msg.Text, Program._betAmount + @"[0-9]$")
                                                //爆玖
                                                && !Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["tt"])
                                                //炸弹
                                                && !Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["bomb"])
                                                //三公
                                                && !Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["ss"])
                                                //大三公
                                                && !Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["lss"])
                                                //小三公
                                                && !Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["sss"])
                                                //混三公
                                                && !Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["mss"])
                                                //################# 百家乐 #####################
                                                || game.GameType == GameType.Baccarat
                                                //庄 闲
                                                && !Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["br"])
                                                && !Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["pr"])
                                                //和
                                                && !Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["ti"])
                                                //庄单、庄双、闲单、闲双
                                                && !Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["bao"])
                                                && !Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["bae"])
                                                && !Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["pao"])
                                                && !Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["pae"])
                                                //庄对、闲对
                                                && !Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["bap"])
                                                && !Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["pap"])
                                                //################# 龙虎 #####################
                                                || game.GameType == GameType.DragonTiger
                                                 //龙虎和
                                                 && !Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["d"])
                                                 && !Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["tr"])
                                                 && !Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["ti"])
                                                //################# 比特币 #####################
                                                || game.GameType == GameType.BinanceBTCPrice
                                                //数字00-99
                                                && !Regex.IsMatch(msg.Text, Program._betAmount + @"([0-9][0-9]|[0-9][0-9])$")
                                                //大,小,单,双
                                                && !Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["l"])
                                                && !Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["s"])
                                                && !Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["o"])
                                                && !Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["e"])
                                                //小单,小双,大单,大双
                                                && !Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["so"])
                                                && !Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["se"])
                                                && !Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["lo"])
                                                && !Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["le"])
                                                //极大,极小
                                                && !Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["xl"])
                                                && !Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["xs"])
                                                //龙虎和
                                                && !Regex.IsMatch(msg.Text, Program._betAmount + @"(龙|虎|和)$")
                                                //################# 六合彩 #####################
                                                || game.GameType == GameType.SixLottery
                                                //小单 小双 大单 大双
                                                && !Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["so"])
                                                && !Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["se"])
                                                && !Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["lo"])
                                                && !Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["le"])
                                                //合小 合大 合单 合双
                                                && !Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["scs"])
                                                && !Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["scl"])
                                                && !Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["sco"])
                                                && !Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["sce"])
                                                //红、蓝、绿
                                                && !Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["r"])
                                                && !Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["blu"])
                                                && !Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["g"])
                                                //红单、红双、红大、红小、
                                                && !Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["ro"])
                                                && !Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["re"])
                                                && !Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["rs"])
                                                && !Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["rl"])
                                                //蓝单、蓝双、蓝大、蓝小、
                                                && !Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["bo"])
                                                && !Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["be"])
                                                && !Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["bs"])
                                                && !Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["bl"])
                                                //绿单、绿双、绿大、绿小
                                                && !Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["go"])
                                                && !Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["ge"])
                                                && !Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["gs"])
                                                && !Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["gl"])
                                                //头0、头1、头2、头3、头4
                                                && !Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["h0"])
                                                && !Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["h1"])
                                                && !Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["h2"])
                                                && !Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["h3"])
                                                && !Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["h4"])
                                                //尾0、尾1、尾2、尾3、尾4、尾5、尾6、尾7、尾8、尾9
                                                && !Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["e0"])
                                                && !Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["e1"])
                                                && !Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["e2"])
                                                && !Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["e3"])
                                                && !Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["e4"])
                                                && !Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["e5"])
                                                && !Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["e6"])
                                                && !Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["e7"])
                                                && !Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["e8"])
                                                && !Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["e9"])
                                                //金、木、水、火、土
                                                && !Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["metal"])
                                                && !Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["wood"])
                                                && !Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["water"])
                                                && !Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["fire"])
                                                && !Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["earth"])
                                                //特码生肖
                                                && !Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["tr"])
                                                && !Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["ti"])
                                                && !Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["rat"])
                                                && !Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["ox"])
                                                && !Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["rabbit"])
                                                && !Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["snak"])
                                                && !Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["horse"])
                                                && !Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["goat"])
                                                && !Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["monkey"])
                                                && !Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["rooster"])
                                                && !Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["dog"])
                                                && !Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["pig"])
                                                //特码1-49
                                                && !Regex.IsMatch(msg.Text, Program._betAmount + @"([1-9]|[1-4][0-9])$")
                                                //正码
                                                && !Regex.IsMatch(msg.Text, Program._betAmount + @"(正|r)([1-9]|[1-4][0-9])$")
                                                //包码1-49之间选择6个号
                                                && !Regex.IsMatch(msg.Text, Program._betAmount + @"([1-9]|[1-4][0-9])(/([1-9]|[1-4][0-9])){5}$")
                                                //正码龙、正码兔、正码虎、正码牛、正码鼠、正码猪、正码狗、正码鸡、正码猴、正码羊、正码马、正码蛇
                                                && !Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["rd"])
                                                && !Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["rr"])
                                                && !Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["rn"])
                                                && !Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["rox"])
                                                && !Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["rra"])
                                                && !Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["rp"])
                                                && !Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["rdo"])
                                                && !Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["rro"])
                                                && !Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["rm"])
                                                && !Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["rg"])
                                                && !Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["rh"])
                                                && !Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["rsn"])
                                                //连肖
                                                && !Regex.IsMatch(msg.Text, Program._betAmount + @"(龙|兔|虎|牛|鼠|猪|狗|鸡|猴|羊|马|蛇)(\+(龙|兔|虎|牛|鼠|猪|狗|鸡|猴|羊|马|蛇)){1,4}$")
                                                //连尾
                                                && !Regex.IsMatch(msg.Text, Program._betAmount + @"连尾([0-9])(\+[0-9]){1,4}$")
                                                //连码
                                                && !Regex.IsMatch(msg.Text, Program._betAmount + @"(四全中([1-9]|[1-4][0-9])(\+([1-9]|[1-4][0-9])){3}|三全中([1-9]|[1-4][0-9])(\+([1-9]|[1-4][0-9])){2}|(三中三|三中二)([1-9]|[1-4][0-9])(\+([1-9]|[1-4][0-9])){2}|二全中([1-9]|[1-4][0-9])\+([1-9]|[1-4][0-9])|(二中二|二中特)([1-9]|[1-4][0-9])\+([1-9]|[1-4][0-9])|特串([1-9]|[1-4][0-9])\+([1-9]|[1-4][0-9]))$")
                                                //中一
                                                && !Regex.IsMatch(msg.Text, Program._betAmount + @"(三中一([1-9]|[1-4][0-9])(/([1-9]|[1-4][0-9])){2}|四中一([1-9]|[1-4][0-9])(/([1-9]|[1-4][0-9])){3}|五中一([1-9]|[1-4][0-9])(/([1-9]|[1-4][0-9])){4}|六中一([1-9]|[1-4][0-9])(/([1-9]|[1-4][0-9])){5})$")
                                                //不中
                                                && !Regex.IsMatch(msg.Text, Program._betAmount + @"(五不中([1-9]|[1-4][0-9])(&([1-9]|[1-4][0-9])){4}|六不中([1-9]|[1-4][0-9])(&([1-9]|[1-4][0-9])){5}|七不中([1-9]|[1-4][0-9])(&([1-9]|[1-4][0-9])){6}|八不中([1-9]|[1-4][0-9])(&([1-9]|[1-4][0-9])){7}|九不中([1-9]|[1-4][0-9])(&([1-9]|[1-4][0-9])){8}|十不中([1-9]|[1-4][0-9])(&([1-9]|[1-4][0-9])){9}|十一不中([1-9]|[1-4][0-9])(&([1-9]|[1-4][0-9])){10}|十二不中([1-9]|[1-4][0-9])(&([1-9]|[1-4][0-9])){11})$")
                                                //################# 加拿大PC28 #####################
                                                || game.GameType == GameType.CanadaPC28
                                                //大小,单双
                                                && !Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["l"])
                                                && !Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["s"])
                                                && !Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["o"])
                                                && !Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["e"])
                                                //豹子
                                                && !Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["t"])
                                                //小单,小双,大单,大双
                                                && !Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["so"])
                                                && !Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["se"])
                                                && !Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["lo"])
                                                && !Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["le"])
                                                //极大,极小
                                                && !Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["xl"])
                                                && !Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["xs"])
                                                //顺子
                                                && !Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["st"])
                                                //对子
                                                && !Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["p"])
                                                //数字0-27
                                                && !Regex.IsMatch(msg.Text, Program._betAmount + @"(0?|1?\d|2[0-7])$")
                                                //################# 赛车 #####################
                                                || game.GameType == GameType.SpeedRacing
                                                //大小
                                                && !Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["l"])
                                                && !Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["s"])
                                                //全单,全双
                                                && !Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["ao"])
                                                && !Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["ae"])
                                                //定位胆1-10
                                                && !Regex.IsMatch(msg.Text, Program._betAmount + @"(([1-9]|10)=([1-9]|10);?){1,9}$")
                                                //排前3名
                                                && !Regex.IsMatch(msg.Text, Program._betAmount + @"\^([1-9]|10)((>([1-9]|10)){1,2})?$")
                                                //前三顺子
                                                && !Regex.IsMatch(msg.Text, Program._betAmount + @"\^([1-9]|10)(\+([1-9]|10)){2}$")
                                                //前3和数
                                                && !Regex.IsMatch(msg.Text, Program._betAmount + @"([6-9]|1[0-9]|2[0-7])$")
                                                //################# 飞艇 #####################
                                                || game.GameType == GameType.LuckyAirship
                                                //定位
                                                && !Regex.IsMatch(msg.Text, Program._betAmount + @"(([1-9]|10)=([1-9]|10);?){1,9}$")
                                                //前三
                                                && !Regex.IsMatch(msg.Text, Program._betAmount + @"\^([1-9]|10)((>([1-9]|10)){1,2})?$")
                                                //和值区间:极小,小,大,极大
                                                && !Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["ao"]) && !Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["ae"])
                                                //中
                                                && !Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["m"])
                                                //冠、亚军和值
                                                && !Regex.IsMatch(msg.Text, Program._betAmount + @"\^=([3-9]|1[0-9])$")
                                                //################# 11选5 #####################
                                                || game.GameType == GameType.Choose5From11
                                                //包号
                                                && !Regex.IsMatch(msg.Text, Program._betAmount + @"([1-9]|1[0-1])((/([1-9]|1[0-1])){1,4})?$")
                                                //前组
                                                && !Regex.IsMatch(msg.Text, Program._betAmount + @"\^([1-9]|1[0-1])((&([1-9]|1[0-1])){1,2})?$")
                                                //排名
                                                && !Regex.IsMatch(msg.Text, Program._betAmount + @"\^([1-9]|1[0-1])((>([1-9]|1[0-1])){1,2})?$")
                                                //################# 缤果 #####################
                                                || game.GameType == GameType.Bingo
                                                //超级号
                                                && !Regex.IsMatch(msg.Text, Program._betAmount + @"(0?[1-9]|[1-7][0-9]|80)$")
                                                //大、小、单、双 
                                                && !Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["l"]) && !Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["s"]) && !Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["o"]) && !Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["e"])
                                                //包号
                                                && !Regex.IsMatch(msg.Text, Program._betAmount + @"(0?[1-9]|[1-7][0-9]|80)(/(0?[1-9]|[1-7][0-9]|80)){1,9}$")
                                                //################# 幸运8 #####################
                                                || game.GameType == GameType.AustralianLucky8
                                                //超级号
                                                && !Regex.IsMatch(msg.Text, Program._betAmount + @"([1-9]|1[0-9]|20)$")
                                                //任选
                                                && !Regex.IsMatch(msg.Text, Program._betAmount + @"([1-9]|1[0-9]|20)((&([1-9]|1[0-9]|20)){1,4})$")
                                                //排名1/2/3
                                                && !Regex.IsMatch(msg.Text, Program._betAmount + @"\^([1-9]|1[0-9]|20)(>([1-9]|1[0-9]|20)){1,2}$")
                                                //靠前2/3个
                                                && !Regex.IsMatch(msg.Text, Program._betAmount + @"\^([1-9]|1[0-9]|20)(&([1-9]|1[0-9]|20)){1,2}$"))
                                            {
                                                returnDelText = "下注失败,您输入的格式有误!每种下注格式示例:";

                                                for (int i = 0; i < g.Beetings.Count; i++)
                                                {
                                                    var item = g.Beetings[i];
                                                    returnDelText +=
                                                        $"\n\n{nums[i + 1]}.<b>{item.Options}</b>" +
                                                        $"\n格式:<b>{item.Format}</b>" +
                                                        $"\n说明:{item.Explain}";
                                                }
                                            }
                                            else if (Helper.IsRepeatBet(msg.Text))
                                            {
                                                returnDelText = "下注失败,请检查您的输入格式,请勿重复下注!";
                                            }
                                            else
                                            {
                                                var gameLotteryHistory = await db.GameHistorys
                                                    .Where(u => u.CreatorId == platform.CreatorId && u.GameId == game.Id && u.Status == GameHistoryStatus.Ongoing && u.LotteryDrawJson == null)
                                                    .OrderByDescending(u => u.Time)
                                                    .FirstOrDefaultAsync(cancellationToken: cancellationToken);
                                                if (gameLotteryHistory == null)
                                                {
                                                    returnDelText = "下注失败,系统正在准备新一期的投注局";
                                                }
                                                else if (gameLotteryHistory.ClosingTime != null || gameLotteryHistory.EndTime != null)
                                                {
                                                    returnDelText = "下注失败,封盘等待开奖中......";
                                                }
                                                else
                                                {
                                                    var finance = new PlayerFinanceHistory
                                                    {
                                                        Time = DateTime.UtcNow,
                                                        Type = (FinanceType)Enum.Parse(typeof(FinanceType), gameName),
                                                        Name = user.FirstName + user.LastName,
                                                        FinanceStatus = FinanceStatus.Success,
                                                        Remark = $"下注{gameLotteryHistory.LotteryDrawId}期",
                                                        GameId = game.Id,
                                                        GameMessageId = gameLotteryHistory.MessageId,
                                                        PlayerId = player.PlayerId
                                                    };
                                                    player = await Helper.MinusBalance(db, amount, player, finance, cancellationToken);
                                                    //下注的结果放在备注里
                                                    var value = Regex.Match(msg.Text, @"(?<=(-|@|买|buy)).+").Value;
                                                    returnText = $"✅  <b>{user.FirstName}{user.LastName}</b> 下注 <b>{gameLotteryHistory.LotteryDrawId}</b>期\n\n📤 投注指令:<b>{msg.Text}</b>\n\n";
                                                    switch (game.GameType)
                                                    {
                                                        case GameType.SportsContest:
                                                            //returnText += $"🗳 投注金额:<b>{amount}U</b>";
                                                            break;
                                                        case GameType.AnimalContest:
                                                            //returnText += $"🗳 投注金额:<b>{amount}U</b>";
                                                            break;
                                                        case GameType.Video:
                                                            //returnText += $"🗳 投注金额:<b>{amount}U</b>";
                                                            break;
                                                        case GameType.Gaming:
                                                            //returnText += $"🗳 投注金额:<b>{amount}U</b>";
                                                            break;
                                                        case GameType.Electronic:
                                                            //returnText += $"🗳 投注金额:<b>{amount}U</b>";
                                                            break;
                                                        case GameType.ChessCards:
                                                            //returnText += $"🗳 投注金额:<b>{amount}U</b>";
                                                            break;
                                                        case GameType.Fishing:
                                                            //returnText += $"🗳 投注金额:<b>{amount}U</b>";
                                                            break;
                                                        case GameType.Bowling:
                                                            //和值0-18
                                                            if (Regex.IsMatch(msg.Text, Program._betAmount + @"([0-9]|1[0-8])$"))
                                                            {
                                                                returnText += $"🗳 投注明细:<b>{amount}U 买 和值等于 {value}</b>\n\n💁 中奖情况:3次分数相加等于'{value}'";
                                                            }
                                                            //对子
                                                            else if (Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["p"]))
                                                            {
                                                                returnText += $"🗳 投注明细:<b>{amount}U 买 对子</b>\n\n💁 中奖情况:3次得分有两次一样(排除3个一样)";
                                                            }
                                                            //豹子
                                                            else if (Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["t"]))
                                                            {
                                                                returnText += $"🗳 投注明细:<b>{amount}U 买 豹子</b>\n\n💁 中奖情况:3次得分一样";
                                                            }
                                                            //连顺
                                                            else if (Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["st"]))
                                                            {
                                                                returnText += $"🗳 投注明细:<b>{amount}U 买 连顺</b>\n\n💁 中奖情况:3次得分是连续顺序";
                                                            }
                                                            break;
                                                        case GameType.Dice:
                                                            //大
                                                            if (Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["l"]))
                                                            {
                                                                returnText += $"🗳 投注明细:<b>{amount}U 买 大</b>\n\n💁 中奖情况:3次点数相加是'大(11-18)'";
                                                            }
                                                            //小
                                                            else if (Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["s"]))
                                                            {
                                                                returnText += $"🗳 投注明细:<b>{amount}U 买 小</b>\n\n💁 中奖情况:3次点数相加是'小(3-10)'";
                                                            }
                                                            //单
                                                            else if (Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["o"]))
                                                            {
                                                                returnText += $"🗳 投注明细:<b>{amount}U 买 单</b>\n\n💁 中奖情况:3次点数相加是'单(3、5、7、9、11、13、15、17其中一个)'";
                                                            }
                                                            //双
                                                            else if (Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["e"]))
                                                            {
                                                                returnText += $"🗳 投注明细:<b>{amount}U 买 双</b>\n\n💁 中奖情况:3次点数相加是'双(4、6、8、10、12、14、16、18其中一个)'";
                                                            }
                                                            //小单
                                                            else if (Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["so"]))
                                                            {
                                                                returnText += $"🗳 投注明细:<b>{amount}U 买 小单</b>\n\n💁 中奖情况:3次点数相加是'小单(3-10的奇数)'";
                                                            }
                                                            //小双
                                                            else if (Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["se"]))
                                                            {
                                                                returnText += $"🗳 投注明细:<b>{amount}U 买 小双</b>\n\n💁 中奖情况:3次点数相加是'小双(3-10的偶数)'";
                                                            }
                                                            //大单
                                                            else if (Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["lo"]))
                                                            {
                                                                returnText += $"🗳 投注明细:<b>{amount}U 买 大单</b>\n\n💁 中奖情况:3次点数相加是'大单(11-18的奇数)'";
                                                            }
                                                            //大双
                                                            else if (Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["le"]))
                                                            {
                                                                returnText += $"🗳 投注明细:<b>{amount}U 买 大双</b>\n\n💁 中奖情况:3次点数相加是'大双(11-18的偶数)'";
                                                            }
                                                            //极大
                                                            else if (Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["xl"]))
                                                            {
                                                                returnText += $"🗳 投注明细:<b>{amount}U 买 极大</b>\n\n💁 中奖情况:3次点数相加是'极大(15-18)'";
                                                            }
                                                            //极小
                                                            else if (Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["xs"]))
                                                            {
                                                                returnText += $"🗳 投注明细:<b>{amount}U 买 极小</b>\n\n💁 中奖情况:3次点数相加是'极小(3-6)'";
                                                            }
                                                            //顺子
                                                            else if (Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["st"]))
                                                            {
                                                                returnText += $"🗳 投注明细:<b>{amount}U 买顺子</b>\n\n💁 中奖情况:3次点数可组成顺子(不分先后)";
                                                            }
                                                            //连顺
                                                            else if (Regex.IsMatch(msg.Text, Program._betAmount + @"(123|234|345|456)$"))
                                                            {
                                                                returnText += $"🗳 投注明细:<b>{amount}U 买连顺</b>\n\n💁 中奖情况:3次点数能组成连续顺子";
                                                            }
                                                            //豹子
                                                            else if (Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["t"]))
                                                            {
                                                                returnText += $"🗳 投注明细:<b>{amount}U 买豹子</b>\n\n💁 中奖情况:3次点数一样";
                                                            }
                                                            //不同号
                                                            else if (Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["dt"]))
                                                            {
                                                                returnText += $"🗳 投注明细:<b>{amount}U 买不同号</b>\n\n💁 中奖情况:3次点数不一样";
                                                            }
                                                            //单选豹子
                                                            else if (Regex.IsMatch(msg.Text, Program._betAmount + @"(111|222|333|444|555|666)$"))
                                                            {
                                                                returnText += $"🗳 投注明细:<b>{amount}U 买 {value}</b>\n\n💁 中奖情况:3次点数是'{value}'";
                                                            }
                                                            //对子
                                                            else if (Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["p"]))
                                                            {
                                                                returnText += $"🗳 投注明细:<b>{amount}U 买对子</b>\n\n💁 中奖情况:3次点数有2个一样";
                                                            }
                                                            //和值3-18
                                                            else if (Regex.IsMatch(msg.Text, Program._betAmount + @"([3-9]|1[0-8])$"))
                                                            {
                                                                returnText += $"🗳 投注明细:<b>{amount}U 买和值等于 {value}</b>\n\n💁 中奖情况:3次点数相加等于'{value}'";
                                                            }
                                                            break;
                                                        case GameType.TrxHash:
                                                            //数字+字母
                                                            if (Regex.IsMatch(msg.Text, Program._betAmount + @"(nl|数字+字母|数字字母)$"))
                                                            {
                                                                returnText += $"🗳 投注明细:<b>{amount}U 买 数字+字母</b>\n\n💁 中奖情况:哈希后两位是'数字+字母'";
                                                            }
                                                            //字母+数字
                                                            else if (Regex.IsMatch(msg.Text, Program._betAmount + @"(ln|字母+数字|字母数字)$"))
                                                            {
                                                                returnText += $"🗳 投注明细:<b>{amount}U 买 字母+数字</b>\n\n💁 中奖情况:哈希后两位是'字母+数字'";
                                                            }
                                                            //全数字
                                                            else if (Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["n"]))
                                                            {
                                                                returnText += $"🗳 投注明细:<b>{amount}U 买 全数字</b>\n\n💁 中奖情况:哈希后两位都是'数字'";
                                                            }
                                                            //全字母
                                                            else if (Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["lr"]))
                                                            {
                                                                returnText += $"🗳 投注明细:<b>{amount}U 买 全字母</b>\n\n💁 中奖情况:哈希后两位都是'字母'";
                                                            }
                                                            //一对
                                                            else if (Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["p"]))
                                                            {
                                                                returnText += $"🗳 投注明细:<b>{amount}U 买 对子</b>\n\n💁 中奖情况:哈希后两位是'对子'";
                                                            }
                                                            //和值:0-18
                                                            else if (Regex.IsMatch(msg.Text, Program._betAmount + @"([0-9]|1[0-8])$"))
                                                            {
                                                                returnText += $"🗳 投注明细:<b>{amount}U 买和值等于 {value}</b>\n\n💁 中奖情况:2个数字相加等于'{value}'";
                                                            }
                                                            break;
                                                        case GameType.Roulette:
                                                            //字符                     
                                                            if (Regex.IsMatch(msg.Text, Program._betAmount + @"([0-9]|[a-fA-F])$"))
                                                            {
                                                                returnText += $"🗳 投注明细:<b>{amount}U 买 {value}</b>\n\n💁 中奖情况:轮盘结果是'中(7、8)'";
                                                            }
                                                            //中
                                                            else if (Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["m"]))
                                                            {
                                                                returnText += $"🗳 投注明细:<b>{amount}U 买 中</b>\n\n💁 中奖情况:轮盘结果是'大(10-14)'";
                                                            }
                                                            //大
                                                            else if (Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["l"]))
                                                            {
                                                                returnText += $"🗳 投注明细:<b>{amount}U 买 大</b>\n\n💁 中奖情况:轮盘结果是'大(9、a、b、c)'";
                                                            }
                                                            //小
                                                            else if (Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["s"]))
                                                            {
                                                                returnText += $"🗳 投注明细:<b>{amount}U 买 小</b>\n\n💁 中奖情况:轮盘结果是'小(3、4、5、6)'";
                                                            }
                                                            //极大
                                                            else if (Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["xl"]))
                                                            {
                                                                returnText += $"🗳 投注明细:<b>{amount}U 买 极大</b>\n\n💁 中奖情况:轮盘结果是'极大(d、e、f)'";
                                                            }
                                                            //极小
                                                            else if (Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["xs"]))
                                                            {
                                                                returnText += $"🗳 投注明细:<b>{amount}U 买 极小</b>\n\n💁 中奖情况:轮盘结果是'极小(0、1、2)'";
                                                            }
                                                            break;
                                                        case GameType.Cow:
                                                            returnText += $"🗳 投注金额:<b>{amount}U</b>\n\n💁 赢家情况:闲家赢";
                                                            break;
                                                        case GameType.Blackjack:
                                                            //庄
                                                            if (Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["br"]))
                                                            {
                                                                returnText += $"🗳 投注明细:<b>{amount}U 买 庄赢</b>\n\n💁 中奖情况:庄家赢";
                                                            }
                                                            //闲
                                                            else if (Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["pr"]))
                                                            {
                                                                returnText += $"🗳 投注明细:<b>{amount}U 买 闲赢</b>\n\n💁 中奖情况:闲家赢";
                                                            }
                                                            //和
                                                            else if (Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["ti"]))
                                                            {
                                                                returnText += $"🗳 投注明细:<b>{amount}U 买 和</b>\n\n💁 中奖情况:和局";
                                                            }
                                                            //庄单
                                                            else if (Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["bao"]))
                                                            {
                                                                returnText += $"🗳 投注明细:<b>{amount}U 买 庄单</b>\n\n💁 中奖情况:庄家是单";
                                                            }
                                                            //庄双
                                                            else if (Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["bae"]))
                                                            {
                                                                returnText += $"🗳 投注明细:<b>{amount}U 买 庄双</b>\n\n💁 中奖情况:庄家是双";
                                                            }
                                                            //闲单
                                                            else if (Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["pao"]))
                                                            {
                                                                returnText += $"🗳 投注明细:<b>{amount}U 买 闲单</b>\n\n💁 中奖情况:闲家是单";
                                                            }
                                                            //闲双
                                                            else if (Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["pae"]))
                                                            {
                                                                returnText += $"🗳 投注明细:<b>{amount}U 买 闲双</b>\n\n💁 中奖情况:闲家是双";
                                                            }
                                                            //庄对
                                                            else if (Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["bap"]))
                                                            {
                                                                returnText += $"🗳 投注明细:<b>{amount}U 买 庄对</b>\n\n💁 中奖情况:庄家有一对";
                                                            }
                                                            //闲对
                                                            else if (Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["pap"]))
                                                            {
                                                                returnText += $"🗳 投注明细:<b>{amount}U 买 闲对</b>\n\n💁 中奖情况:闲家有一对";
                                                            }
                                                            break;
                                                        case GameType.Sangong:
                                                            //和
                                                            if (Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["ti"]))
                                                            {
                                                                returnText += $"🗳 投注明细:<b>{amount}U 买 和</b>\n\n💁 中奖情况:庄家和玩家和局";
                                                            }
                                                            //大
                                                            else if (Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["l"]))
                                                            {
                                                                returnText += $"🗳 投注明细:<b>{amount}U 买 大</b>\n\n💁 中奖情况:自己牌点数比庄家大";
                                                            }
                                                            //小
                                                            else if (Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["s"]))
                                                            {
                                                                returnText += $"🗳 投注明细:<b>{amount}U 买 小</b>\n\n💁 中奖情况:自己牌点数比庄家小";
                                                            }
                                                            //顺子
                                                            else if (Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["st"]))
                                                            {
                                                                returnText += $"🗳 投注明细:<b>{amount}U 买 顺子</b>\n\n💁 中奖情况:自己牌是顺子";
                                                            }
                                                            //对子
                                                            else if (Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["p"]))
                                                            {
                                                                returnText += $"🗳 投注明细:<b>{amount}U 买 对子</b>\n\n💁 中奖情况:自己牌是对子";
                                                            }
                                                            //点数
                                                            else if (Regex.IsMatch(msg.Text, Program._betAmount + @"[0-9]$"))
                                                            {
                                                                returnText += $"🗳 投注明细:<b>{amount}U 买 {msg.Text}</b>\n\n💁 中奖情况:自己牌点数是'{value}'";
                                                            }
                                                            //爆九
                                                            else if (Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["tt"]))
                                                            {
                                                                returnText += $"🗳 投注明细:<b>{amount}U 买 爆九</b>\n\n💁 中奖情况:自己牌三个3";
                                                            }
                                                            //炸弹
                                                            else if (Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["bomb"]))
                                                            {
                                                                returnText += $"🗳 投注明细:<b>{amount}U 买 炸弹</b>\n\n💁 中奖情况:自己牌三张相同(去除333)";
                                                            }
                                                            //三公
                                                            else if (Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["ss"]))
                                                            {
                                                                returnText += $"🗳 投注明细:<b>{amount}U 买 三公</b>\n\n💁 中奖情况:自己有任意三张公仔牌";
                                                            }
                                                            //大三公
                                                            else if (Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["lss"]))
                                                            {
                                                                returnText += $"🗳 投注明细:<b>{amount}U 买 大三公</b>\n\n💁 中奖情况:自己有三张相同的公仔牌";
                                                            }
                                                            //小三公
                                                            else if (Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["sss"]))
                                                            {
                                                                returnText += $"🗳 投注明细:<b>{amount}U 买 小三公</b>\n\n💁 中奖情况:自己有三张相同的点数牌";
                                                            }
                                                            //混三公
                                                            else if (Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["mss"]))
                                                            {
                                                                returnText += $"🗳 投注明细:<b>{amount}U 买 混三公</b>\n\n💁 中奖情况:自己有三张不相同的公仔牌(可以2张一样的公仔牌)";
                                                            }
                                                            break;
                                                        case GameType.Baccarat:
                                                            //庄
                                                            if (Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["br"]))
                                                            {
                                                                returnText += $"🗳 投注明细:<b>{amount}U 买 庄赢</b>\n\n💁 中奖情况:庄家赢";
                                                            }
                                                            //闲
                                                            else if (Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["pr"]))
                                                            {
                                                                returnText += $"🗳 投注明细:<b>{amount}U 买 闲赢</b>\n\n💁 中奖情况:闲家赢";
                                                            }
                                                            //和
                                                            else if (Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["ti"]))
                                                            {
                                                                returnText += $"🗳 投注明细:<b>{amount}U 买 和</b>\n\n💁 中奖情况:和局";
                                                            }
                                                            //庄单
                                                            else if (Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["bao"]))
                                                            {
                                                                returnText += $"🗳 投注明细:<b>{amount}U 买 庄单</b>\n\n💁 中奖情况:庄家单";
                                                            }
                                                            //庄双
                                                            else if (Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["bae"]))
                                                            {
                                                                returnText += $"🗳 投注明细:<b>{amount}U 买 庄双</b>\n\n💁 中奖情况:庄家双";
                                                            }
                                                            //闲单
                                                            else if (Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["pao"]))
                                                            {
                                                                returnText += $"🗳 投注明细:<b>{amount}U 买 闲单</b>\n\n💁 中奖情况:闲家单";
                                                            }
                                                            //闲双
                                                            else if (Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["pae"]))
                                                            {
                                                                returnText += $"🗳 投注明细:<b>{amount}U 买 闲双</b>\n\n💁 中奖情况:闲家双";
                                                            }
                                                            //庄对
                                                            else if (Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["bap"]))
                                                            {
                                                                returnText += $"🗳 投注明细:<b>{amount}U 买 庄对</b>\n\n💁 中奖情况:庄家对";
                                                            }
                                                            //闲对
                                                            else if (Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["pap"]))
                                                            {
                                                                returnText += $"🗳 投注明细:<b>{amount}U 买 闲对</b>\n\n💁 中奖情况:闲家对";
                                                            }
                                                            break;
                                                        case GameType.DragonTiger:
                                                            //龙
                                                            if (Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["br"]))
                                                            {
                                                                returnText += $"🗳 投注明细:<b>{amount}U 买 龙大</b>\n\n💁 中奖情况:哈希首字大";
                                                            }
                                                            //虎
                                                            else if (Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["pr"]))
                                                            {
                                                                returnText += $"🗳 投注明细:<b>{amount}U 买 虎大</b>\n\n💁 中奖情况:哈希尾字大";
                                                            }
                                                            //和
                                                            else if (Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["ti"]))
                                                            {
                                                                returnText += $"🗳 投注明细:<b>{amount}U 买 和</b>\n\n💁 中奖情况:和局";
                                                            }
                                                            break;
                                                        case GameType.BinanceBTCPrice:
                                                            //数字00-99
                                                            if (Regex.IsMatch(msg.Text, Program._betAmount + @"([0-9][0-9]|[0-9][0-9])$"))
                                                            {
                                                                returnText += $"🗳 投注明细:<b>{amount}U 买 {value}</b>\n\n💁 中奖情况:2个数字是'{value}'";
                                                            }
                                                            //大
                                                            else if (Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["l"]))
                                                            {
                                                                returnText += $"🗳 投注明细:<b>{amount}U 买 大</b>\n\n💁 中奖情况:结果是'大(50-99)'";
                                                            }
                                                            //小
                                                            else if (Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["s"]))
                                                            {
                                                                returnText += $"🗳 投注明细:<b>{amount}U 买 小</b>\n\n💁 中奖情况:结果是'小(00-49)'";
                                                            }
                                                            //单
                                                            else if (Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["o"]))
                                                            {
                                                                returnText += $"🗳 投注明细:<b>{amount}U 买 单</b>\n\n💁 中奖情况:结果是'单(奇数)'";
                                                            }
                                                            //双
                                                            else if (Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["e"]))
                                                            {
                                                                returnText += $"🗳 投注明细:<b>{amount}U 买 双</b>\n\n💁 中奖情况:结果是'双(偶数)'";
                                                            }
                                                            //小单
                                                            else if (Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["so"]))
                                                            {
                                                                returnText += $"🗳 投注明细:<b>{amount}U 买 小单</b>\n\n💁 中奖情况:结果是'小单(00-49的单数)'";
                                                            }
                                                            //小双
                                                            else if (Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["se"]))
                                                            {
                                                                returnText += $"🗳 投注明细:<b>{amount}U 买 小双</b>\n\n💁 中奖情况:结果是'小双(00-49的双数)'";
                                                            }
                                                            //大单
                                                            else if (Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["lo"]))
                                                            {
                                                                returnText += $"🗳 投注明细:<b>{amount}U 买 大单</b>\n\n💁 中奖情况:结果是'大单(50-99的单数)'";
                                                            }
                                                            //大双
                                                            else if (Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["le"]))
                                                            {
                                                                returnText += $"🗳 投注明细:<b>{amount}U 买 大双</b>\n\n💁 中奖情况:结果是'大双(50-99的双数)'";
                                                            }
                                                            //极大
                                                            else if (Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["xl"]))
                                                            {
                                                                returnText += $"🗳 投注明细:<b>{amount}U 买 极大</b>\n\n💁 中奖情况:结果是'极大(70-99)'";
                                                            }
                                                            //极小
                                                            else if (Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["xs"]))
                                                            {
                                                                returnText += $"🗳 投注明细:<b>{amount}U 买 极小</b>\n\n💁 中奖情况:结果是'极小(0-19)'";
                                                            }
                                                            //龙
                                                            if (Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["br"]))
                                                            {
                                                                returnText += $"🗳 投注明细:<b>{amount}U 买 龙大</b>\n\n💁 中奖情况:首个数字大";
                                                            }
                                                            //虎
                                                            else if (Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["pr"]))
                                                            {
                                                                returnText += $"🗳 投注明细:<b>{amount}U 买 虎大</b>\n\n💁 中奖情况:尾数大";
                                                            }
                                                            //和
                                                            else if (Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["ti"]))
                                                            {
                                                                returnText += $"🗳 投注明细:<b>{amount}U 买 和</b>\n\n💁 中奖情况:和局";
                                                            }
                                                            break;
                                                        case GameType.SixLottery:
                                                            //特码1-49
                                                            if (Regex.IsMatch(msg.Text, Program._betAmount + @"([1-9]|[1-4][0-9])$"))
                                                            {
                                                                returnText += $"🗳 投注明细:<b>{amount}U 买特码 {value}</b>\n\n💁 中奖情况:特码是'{value}'";
                                                            }
                                                            //特码生肖
                                                            else if (Regex.IsMatch(msg.Text, Program._betAmount + @"(鼠|牛|虎|兔|龙|蛇|马|羊|猴|鸡|狗|猪)$"))
                                                            {
                                                                returnText += $"🗳 投注明细:<b>{amount}U 买特码生肖 {value}</b>\n\n💁 中奖情况:特码生肖是'{value}'";
                                                            }
                                                            //包号1-49之间选择2至6个号
                                                            else if (Regex.IsMatch(msg.Text, Program._betAmount + @"([1-9]|[1-4][0-9])(/([1-9]|[1-4][0-9])){1,5}$"))
                                                            {
                                                                returnText += "🗳 投注明细:<b>" + $"{amount}U 买正码 " + value.Replace("/", "和") + $"</b>\n\n💁 中奖情况:只要正码中有'{value.Replace("/", "或")}'";
                                                            }
                                                            break;
                                                        case GameType.CanadaPC28:
                                                            if (Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["l"]))
                                                            {
                                                                returnText += $"🗳 投注明细:<b>{amount}U 买 大</b>\n\n💁 中奖情况:3个号码的尾数之和是'大(14-27)'";
                                                            }
                                                            else if (Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["s"]))
                                                            {
                                                                returnText += $"🗳 投注明细:<b>{amount}U 买 小</b>\n\n💁 中奖情况:3个号码的尾数之和是'小(0-13)'";
                                                            }
                                                            else if (Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["o"]))
                                                            {
                                                                returnText += $"🗳 投注明细:<b>{amount}U 买 单</b>\n\n💁 中奖情况:3个号码的尾数之和是'单(0-27任何奇数)'";
                                                            }
                                                            else if (Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["e"]))
                                                            {
                                                                returnText += $"🗳 投注明细:<b>{amount}U 买 双</b>\n\n💁 中奖情况:3个号码的尾数之和是'双(0-27任何偶数)'";
                                                            }
                                                            else if (Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["so"]))
                                                            {
                                                                returnText += $"🗳 投注明细:<b>{amount}U 买 小单</b>\n\n💁 中奖情况:3个号码的尾数之和是'小单(0-13的奇数,排除0)'";
                                                            }
                                                            else if (Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["se"]))
                                                            {
                                                                returnText += $"🗳 投注明细:<b>{amount}U 买 小双</b>\n\n💁 中奖情况:3个号码的尾数之和是'小双(0-13的偶数,包括0)'";
                                                            }
                                                            else if (Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["lo"]))
                                                            {
                                                                returnText += $"🗳 投注明细:<b>{amount}U 买 大单</b>\n\n💁 中奖情况:3个号码的尾数之和是'大单(14-27的奇数)'";
                                                            }
                                                            else if (Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["le"]))
                                                            {
                                                                returnText += $"🗳 投注明细:<b>{amount}U 买 大双</b>\n\n💁 中奖情况:3个号码的尾数之和是'大双(14-27的偶数)'";
                                                            }
                                                            else if (Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["xl"]))
                                                            {
                                                                returnText += $"🗳 投注明细:<b>{amount}U 买 极大</b>\n\n💁 中奖情况:3个号码的尾数之和是'极大(22-27)'";
                                                            }
                                                            else if (Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["xs"]))
                                                            {
                                                                returnText += $"🗳 投注明细:<b>{amount}U 买 极小</b>\n\n💁 中奖情况:3个号码的尾数之和是'极小(0-5)'";
                                                            }
                                                            else if (Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["st"]))
                                                            {
                                                                returnText += $"🗳 投注明细:<b>{amount}U 买 顺子</b>\n\n💁 中奖情况:3个号码能组成顺子";
                                                            }
                                                            else if (Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["t"]))
                                                            {
                                                                returnText += $"🗳 投注明细:<b>{amount}U 买 豹子</b>\n\n💁 中奖情况:3个号码是一样的数";
                                                            }
                                                            else if (Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["p"]))
                                                            {
                                                                returnText += $"🗳 投注明细:<b>{amount}U 买 对子</b>\n\n💁 中奖情况:3个号码的有两个号是一样的数";
                                                            }
                                                            else if (Regex.IsMatch(msg.Text, Program._betAmount + @"(0?|1?\d|2[0-7])$"))
                                                            {
                                                                returnText += $"🗳 投注明细:<b>{amount}U 买和值等于 {value}</b>\n\n💁 中奖情况:3个号码的尾数之和是'{value}'";
                                                            }
                                                            break;
                                                        case GameType.SpeedRacing:
                                                            if (Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["l"]))
                                                            {
                                                                returnText += $"🗳 投注明细:<b>{amount}U 买 大</b>\n\n💁 中奖情况:前三名之和 '大于(含)21且小于(含)27'";
                                                            }
                                                            else if (Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["s"]))
                                                            {
                                                                returnText += $"🗳 投注明细:<b>{amount}U 买 小</b>\n\n💁 中奖情况:前三名之和 '大于(含)6,小于(含)12'";
                                                            }
                                                            else if (Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["ao"]))
                                                            {
                                                                returnText += $"🗳 投注明细:<b>{amount}U 买 前三都是单</b>\n\n💁 中奖情况:开奖的前三名全为'单数'";
                                                            }
                                                            else if (Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["ae"]))
                                                            {
                                                                returnText += $"🗳 投注明细:<b>{amount}U 买 前三都是双</b>\n\n💁 中奖情况:开奖的前三名全为'双数'";
                                                            }
                                                            else if (Regex.IsMatch(msg.Text, Program._betAmount + @"(([1-9]|10)=([1-9]|10);?){1,9}$"))
                                                            {
                                                                returnText += "🗳 投注明细:<b>" + $"{amount}U 买号码 " + value.Replace("=", "排在第")
                                                                    + $"</b>\n\n💁 中奖情况:开奖号码位置'{value.Replace("=", "排在第")}' (不一定需全中)";
                                                            }
                                                            else if (Regex.IsMatch(msg.Text, Program._betAmount + @"\^([1-9]|10)((>([1-9]|10)){1,2})?$"))
                                                            {
                                                                returnText += $"🗳 投注明细:<b>" + $"{amount}U 买前{value.Split('>').Length}名排名依次是" + value.Replace("^", "")
                                                                    + $"</b>\n\n💁 中奖情况:只要开奖前{value.Split('>').Length}名顺序依次是'{value.Replace("^", "")}'";
                                                            }
                                                            else if (Regex.IsMatch(msg.Text, Program._betAmount + @"\^([1-9]|10)(\+([1-9]|10)){2}$"))
                                                            {
                                                                returnText += $"🗳 投注明细:<b>" + $"{amount}U 买前三顺子 " + value.Replace("^", "")
                                                                    + $"</b>\n\n💁 中奖情况:前三名的开奖号码能组成连续顺子的三位数(排除9+10+1和1+10+9、10+1+2和2+1+10)";
                                                            }
                                                            else if (Regex.IsMatch(msg.Text, Program._betAmount + @"([6-9]|1[0-9]|2[0-7])$"))
                                                            {
                                                                returnText += $"🗳 投注明细:<b>{amount}U 买前3和值 {value}</b>\n\n💁 中奖情况:前3相加等于'{value}'";
                                                            }
                                                            break;
                                                        case GameType.LuckyAirship:
                                                            if (Regex.IsMatch(msg.Text, Program._betAmount + @"(([1-9]|10)=([1-9]|10);?){1,9}$"))
                                                            {
                                                                returnText += "🗳 投注明细:<b>" + $"{amount}U 买号码 " + value.Replace("=", "排在")
                                                                    + $"</b>\n\n💁 中奖情况:只要'{value.Replace("=", "排在第")}'和号码开奖位置一样";
                                                            }
                                                            else if (Regex.IsMatch(msg.Text, Program._betAmount + @"\^([1-9]|10)((>([1-9]|10)){1,2})?$"))
                                                            {
                                                                returnText += "🗳 投注明细:<b>" + $"{amount}U 买前{value.Split('>').Length}名排名依次是" + value.Replace("^", "")
                                                                    + $"</b>\n\n💁 中奖情况:只要开奖前{value.Split('>').Length}名顺序依次是'{value.Replace("^", "")}'";
                                                            }
                                                            else if (Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["xl"]))
                                                            {
                                                                returnText += $"🗳 投注明细:<b>{amount}U 买 极大</b>\n\n💁 中奖情况:第1名+第2名=相加之和是'极大(16-19)'";
                                                            }
                                                            else if (Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["xs"]))
                                                            {
                                                                returnText += $"🗳 投注明细:<b>{amount}U 买 极小</b>\n\n💁 中奖情况:第1名+第2名=相加之和是'极小(3-6)'";
                                                            }
                                                            else if (Regex.IsMatch(msg.Text, Program._betAmount + @"\^=([3-9]|1[0-9])$"))
                                                            {
                                                                returnText += "🗳 投注明细:<b>" + $"{amount}U 买1名+2名之和= " + value.Replace("^=", "") + $"</b>\n\n💁 中奖情况:'1名+2名之和={value}'";
                                                            }
                                                            break;
                                                        case GameType.Choose5From11:
                                                            if (Regex.IsMatch(msg.Text, Program._betAmount + @"([1-9]|1[0-1])((/([1-9]|1[0-1])){1,4})?$"))
                                                            {
                                                                if (Regex.IsMatch(value, @"^([1-9]|10)$"))
                                                                {
                                                                    returnText += $"🗳 投注明细:<b>{amount}U 买左边第一个号码是 {value}</b>\n\n💁 中奖情况:只要开奖的左边第一个号码中是'{value}'";
                                                                }
                                                                else
                                                                {
                                                                    returnText += "🗳 投注明细:<b>" + $"{amount}U 买只要有开奖就中的号码是 " + value.Replace("/", "或")
                                                                        + $"</b>\n\n💁 中奖情况:只要开奖的号码中有'{value.Replace("/", "或")}'";
                                                                }
                                                            }
                                                            else if (Regex.IsMatch(msg.Text, Program._betAmount + @"\^([1-9]|1[0-1])((&([1-9]|1[0-1])){1,2})?$"))
                                                            {
                                                                returnText += "🗳 投注明细:<b>" + $"{amount}U 买前{value.Split('&').Length}名(不分先后)的号码 " + value.Replace("^", "")
                                                                    + $"</b>\n\n💁 中奖情况:只要开奖号码买前{value.Split('&').Length}名(不分先后)的号码是'{value.Replace("^", "")}'";
                                                            }
                                                            else if (Regex.IsMatch(msg.Text, Program._betAmount + @"\^([1-9]|1[0-1])((>([1-9]|1[0-1])){1,2})?$"))
                                                            {
                                                                returnText += "🗳 投注明细:<b>" + $"{amount}U 买排名先后顺序 " + value.Replace("^", "")
                                                                    + $"</b>\n\n💁 中奖情况:只要开奖前{value.Split('>').Length}名顺序依次是'{value.Replace("^", "")}'";
                                                            }
                                                            break;
                                                        case GameType.Bingo:
                                                            if (Regex.IsMatch(msg.Text, Program._betAmount + @"(0?[1-9]|[1-7][0-9]|80)$"))
                                                            {
                                                                returnText += "🗳 投注明细:<b>" + $"{amount}U 买特码 " + value + $"</b>\n\n💁 中奖情况:开奖的特码是'{value}'";
                                                            }
                                                            else if (Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["l"]))
                                                            {
                                                                returnText += $"🗳 投注明细:<b>{amount}U 买 大</b>\n\n💁 中奖情况:开奖后'(含)13个号码以上41-80范围的数'";
                                                            }
                                                            else if (Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["s"]))
                                                            {
                                                                returnText += $"🗳 投注明细:<b>{amount}U 买 小</b>\n\n💁 中奖情况:开奖后'(含)13个号码以上01~40范围的数'";
                                                            }
                                                            else if (Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["o"]))
                                                            {
                                                                returnText += $"🗳 投注明细:<b>{amount}U 买 单</b>\n\n💁 中奖情况:开奖后'(含)13个单号'";
                                                            }
                                                            else if (Regex.IsMatch(msg.Text, Program._betAmount + Program._betItems["e"]))
                                                            {
                                                                returnText += $"🗳 投注明细:<b>{amount}U 买 双</b>\n\n💁 中奖情况:开奖后'(含)13个双号'";
                                                            }
                                                            else if (Regex.IsMatch(msg.Text, Program._betAmount + @"(0?[1-9]|[1-7][0-9]|80)(/(0?[1-9]|[1-7][0-9]|80)){1,9}$"))
                                                            {
                                                                returnText += $"🗳 投注明细:<b>{amount}U 买只要有开奖就中的号码是 " + value.Replace("/", "或")
                                                                    + $"</b>\n\n💁 中奖情况:只要开奖的号码中有'{value.Replace("/", "或")}'";
                                                            }
                                                            break;
                                                        case GameType.AustralianLucky8:
                                                            if (Regex.IsMatch(msg.Text, Program._betAmount + @"([1-9]|1[0-9]|20)$"))
                                                            {
                                                                returnText += $"🗳 投注明细:<b>{amount}U 买特码 {value}</b>\n\n💁 中奖情况:开奖的特码是'{value}'";
                                                            }
                                                            else if (Regex.IsMatch(msg.Text, Program._betAmount + @"([1-9]|1[0-9]|20)((&([1-9]|1[0-9]|20)){1,4})$"))
                                                            {
                                                                returnText += $"🗳 投注明细:<b>{amount}U 买必须开奖的号码是 " + value.Replace("&", "和")
                                                                    + $"</b>\n\n💁 中奖情况:开奖的号码同时存在'{value.Replace("&", "和")}'";
                                                            }
                                                            else if (Regex.IsMatch(msg.Text, Program._betAmount + @"\^([1-9]|1[0-9]|20)(>([1-9]|1[0-9]|20)){1,2}$"))
                                                            {
                                                                returnText += $"🗳 投注明细:<b>{amount}U 买前{value.Split('>').Length}名依次排名号码是" + value.Replace("^", "")
                                                                    + $"</b>\n\n💁 中奖情况:只要开奖前{value.Split('>').Length}名顺序依次是'{value.Replace("^", "")}'";
                                                            }
                                                            else if (Regex.IsMatch(msg.Text, Program._betAmount + @"\^([1-9]|1[0-9]|20)(&([1-9]|1[0-9]|20)){1,2}$"))
                                                            {
                                                                returnText += $"🗳 投注明细:<b>{amount}U 买前{value.Split('>').Length}名(不分先后)的号码 " + value.Replace("^", "")
                                                                    + $"</b>\n\n💁 中奖情况:只要开奖号码买前{value.Split('&').Length}名(不分先后)的号码是'{value.Replace("^", "")}'";
                                                            }
                                                            break;
                                                        default:
                                                            break;
                                                    }

                                                    finance.Remark = value;
                                                    gameLotteryHistory.BetAmount += amount;
                                                    await db.PlayerFinanceHistorys.AddAsync(finance, cancellationToken);
                                                    await db.SaveChangesAsync(cancellationToken);
                                                    try
                                                    {
                                                        await botClient.SendTextMessageAsync(chat.Id, returnText, messageThreadId: msg.MessageThreadId, parseMode: ParseMode.Html, replyToMessageId: msg.MessageId, replyMarkup: new InlineKeyboardMarkup(msgBtn), cancellationToken: cancellationToken);
                                                    }
                                                    catch (Exception ex)
                                                    {
                                                        Log.Error("彩票下注失败:" + ex.Message);
                                                    }
                                                }
                                            }
                                            break;
                                    }
                                }
                            }
                        }
                        //收到按钮回调事件
                        else if (update.Type is UpdateType.CallbackQuery && cq?.Message != null)
                        {
                            switch (game?.GameType)
                            {
                                //红包
                                case GameType.RedEnvelope:
                                    var data = cq.Data!;
                                    //发送者
                                    var sendRedEnvelopePlayerId = Convert.ToInt64(Regex.Match(data, "[0-9]{9,11}").Value);
                                    //红包金额
                                    var amount = Convert.ToDecimal(Regex.Match(data, "(?<=amount=)([5-9]|[1-9][0-9]{1,2}|1000)(?=&)").Value);
                                    //要赔多少钱:冻结
                                    var compensationAmount = amount * Convert.ToDecimal(1.8);
                                    //中雷尾数
                                    var lastNum = Convert.ToInt32(Regex.Match(data, "(?<=&lastNum=)[1-9]{1}(?=&)").Value);
                                    //已领取了多少个用户了
                                    var receipts = Convert.ToInt32(Regex.Match(data, "[0-6]$").Value);
                                    var gameHistory = await db.GameHistorys.FirstOrDefaultAsync(u => u.CreatorId == platform.CreatorId && u.MessageThreadId == msg!.MessageThreadId && u.MessageId == update.CallbackQuery!.Message!.MessageId, cancellationToken: cancellationToken);
                                    if (gameHistory == null)
                                    {
                                        returnError = "领取失败!红包不存在";
                                    }
                                    else if (sendRedEnvelopePlayerId == player.PlayerId)
                                    {
                                        returnError = $"禁抢自己发的红包";
                                    }
                                    else if ((player.Balance + player.RewardBalance) < compensationAmount)
                                    {
                                        returnError = $"您余额{player.Balance + player.RewardBalance}USDT不足以领取本红包";
                                    }
                                    else if (db.PlayerFinanceHistorys.Any(u => u.PlayerId == player.PlayerId && u.GameId == game.Id && u.GameMessageId == msg!.MessageId))
                                    {
                                        returnError = $"请勿重复领取!";
                                    }
                                    else if (gameHistory.Status == GameHistoryStatus.Expired || gameHistory.EndTime < DateTime.UtcNow)
                                    {
                                        returnError = "领取失败!红包已过期";
                                    }
                                    else if (gameHistory.Status == GameHistoryStatus.End)
                                    {
                                        returnError = "领取失败!红包已被领取完";
                                    }
                                    else
                                    {
                                        await RedEnvelope.Receive(botClient, db, platform, game, gameHistory, player, msg, amount, compensationAmount, receipts, sendRedEnvelopePlayerId, lastNum, cancellationToken);
                                    }
                                    break;
                                //盲盒
                                case GameType.BlindBox:
                                    if (cq?.Message?.ReplyMarkup?.InlineKeyboard == null || cq?.Message?.Caption == null || update.CallbackQuery?.Message == null || msg == null)
                                        return;

                                    if (cq.Message.ReplyMarkup.InlineKeyboard.Any(u => u.Any(c => c.Text != "🎁" && c.CallbackData == cq.Data)) == true)
                                    {
                                        returnError = "开盒失败!此盒子已经有人开了!";
                                    }
                                    else
                                    {
                                        //盲盒游戏记录
                                        var blindBoxGameHistory = await db.GameHistorys.FirstAsync(u => u.CreatorId == platform.CreatorId && u.MessageThreadId == msg.MessageThreadId && u.MessageId == cq.Message.MessageId, cancellationToken: cancellationToken);
                                        //庄家发起记录
                                        var bankerFinanceHistory = await db.PlayerFinanceHistorys.FirstAsync(u => u.PlayerId == blindBoxGameHistory.PlayerId && u.FinanceStatus == FinanceStatus.Freeze && u.Type == FinanceType.BlindBox && u.GameMessageId == blindBoxGameHistory.MessageId, cancellationToken: cancellationToken);
                                        //庄家发了多少金额
                                        var boxAmountTotalAmount = bankerFinanceHistory.Amount + bankerFinanceHistory.BonusAmount;

                                        if (blindBoxGameHistory == null)
                                        {
                                            returnError = "操作失败!盲盒记录不存在!";
                                        }
                                        //结束了
                                        else if (blindBoxGameHistory.EndTime < DateTime.UtcNow)
                                        {
                                            returnError = "操作失败!盲盒已经结束!";
                                        }
                                        //不够钱开盒:领取费用+点到鞭炮的赔偿费用
                                        else if ((boxAmountTotalAmount / 6) + (boxAmountTotalAmount / 3) > (player.Balance + player.RewardBalance))
                                        {
                                            returnError = "操作失败!您的余额不足以开此盒!";
                                        }
                                        else
                                        {
                                            await BlindBox.Receive(botClient, db, platform, game, cq, blindBoxGameHistory, player, bankerFinanceHistory, cancellationToken);
                                        }
                                    }
                                    break;
                                default:
                                    break;
                            }
                        }
                        //加群事件
                        else if (update.Type is UpdateType.ChatJoinRequest)
                        {

                        }
                    }
                }
                //频道
                else if (chat.Type is ChatType.Channel)
                {
                    returnError = "本机器人不支持频道!";
                }
            }

            if (!string.IsNullOrEmpty(returnDelText))
            {
                Helper.DeleteMessage(botClient, update, 10, returnDelText, cancellationToken);
            }
            else if (!string.IsNullOrEmpty(returnError))
            {
                try
                {
                    if (msg == null)
                    {
                        returnError = $"@{user.Username} {user.FirstName}{user.LastName} \n\n⚠️ {returnError}";
                    }
                    else
                    {
                        returnError = $"⚠️ {returnError}";
                    }

                    if (update.Message != null)
                    {
                        await botClient.SendTextMessageAsync(chatId: chat.Id,
                            text: returnError,
                            messageThreadId: threadId,
                            parseMode: ParseMode.Html,
                            replyToMessageId: msg?.MessageId,
                            cancellationToken: cancellationToken);
                    }
                    else if (update.CallbackQuery != null)
                    {
                        try
                        {
                            await botClient.AnswerCallbackQueryAsync(update.CallbackQuery.Id, $"{returnError}", true, null, 0, cancellationToken: cancellationToken);
                        }
                        catch (Exception ex)
                        {
                            Log.Error($"机器人:{botClient.BotId}向用户发送错误的AnswerCallbackQueryAsync时出错:" + ex.Message);
                        }
                    }

                }
                catch (Exception ex)
                {
                    Log.Error($"发送returnError：{returnError} 时出错：" + ex.Message);
                }
            }

            Program._runingUserId.Remove(user.Id);
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
