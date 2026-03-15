using Telegram.Bot.Framework.Abstractions;

namespace Telegram.Bot.Framework
{
    public abstract class BotBase : IBot
    {
        private string _username;

        protected BotBase(string username, ITelegramBotClient client)
        {
            Username = username;
            Client = client;
        }

        protected BotBase(BotOptions options)
        {
            Username = options.Username;
            Client = new TelegramBotClient(
                new TelegramBotClientOptions(options.ApiToken)
                {
                    UseProxy = options.UseProxy,
                    ProxySettings = options.ProxySettings,
                });
        }

        public ITelegramBotClient Client { get; }

        public string Username
        {
            get
            {
                _username ??= Client.GetMeAsync().GetAwaiter().GetResult().Username;
                return _username;
            }
            private set
            {
                _username = value;
            }
        }
    }
}