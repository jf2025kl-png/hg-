using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


    using System;
    using System.Net.Http;
    using System.Threading.Tasks;
namespace 皇冠娱乐
{
    class TelegramBotHelper
    {
        private readonly string _botToken;
        private readonly string _chatId;
        private readonly HttpClient _httpClient;

        public TelegramBotHelper(string botToken, string chatId)
        {
            _botToken = botToken;
            _chatId = chatId;
            _httpClient = new HttpClient();
        }

        public async Task SendStartupMessageAsync()
        {
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            string message = $"启动成功 {timestamp}";

            string url = $"https://api.telegram.org/bot{_botToken}/sendMessage";

            var content = new FormUrlEncodedContent(new[]
            {
            new KeyValuePair<string, string>("chat_id", _chatId),
            new KeyValuePair<string, string>("text", message)
        });

            HttpResponseMessage response = await _httpClient.PostAsync(url, content);
            string result = await response.Content.ReadAsStringAsync();

            Console.WriteLine(result);
        }
    }

}
