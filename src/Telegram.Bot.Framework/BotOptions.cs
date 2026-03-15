namespace Telegram.Bot.Framework
{
    /// <summary>
    /// Configurations for the bot.
    /// </summary>
    public class BotOptions
    {
        /// <summary>
        /// Username of the bot.
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// Optional if client not needed. Telegram API token.
        /// </summary>
        public string ApiToken { get; set; }

        /// <summary>
        /// URL path to webhook address.
        /// </summary>
        public string WebhookPath { get; set; }

        /// <summary>
        /// Use proxy or not.
        /// </summary>
        public bool UseProxy { get; set; }

        /// <summary>
        /// Proxy settings.
        /// </summary>
        public ProxySettings ProxySettings { get; set; }
    }
}