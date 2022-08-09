using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Telegram.Bot.Framework;
using Telegram.Bot.Framework.Abstractions;

namespace Microsoft.AspNetCore.Builder
{
    /// <summary>
    /// Extenstion methods for adding Telegram Bot framework to the ASP.NET Core middleware
    /// </summary>
    public static class MiddlewareExtensions
    {
        #region Webhook methods

        /// <summary>
        /// Add Telegram bot webhook handling functionality to the pipeline.
        /// </summary>
        /// <typeparam name="TBot">Type of bot</typeparam>
        /// <param name="app">Instance of <see cref="IApplicationBuilder"/></param>
        /// <returns>Instance of <see cref="IApplicationBuilder"/></returns>
        public static IApplicationBuilder UseTelegramBotWebhook<TBot>(
            this IApplicationBuilder app) where TBot : IBot
        {
            var updateDelegate = BotBuilder.BuildBotAutomatically();
            return app.UseTelegramBotWebhook<TBot>(updateDelegate);
        }
        
        /// <summary>
        /// Add Telegram bot webhook handling functionality to the pipeline
        /// </summary>
        /// <typeparam name="TBot">Type of bot</typeparam>
        /// <param name="app">Instance of <see cref="IApplicationBuilder"/></param>
        /// <param name="botBuilder">Instance of <see cref="IBotBuilder"/></param>
        /// <returns>Instance of <see cref="IApplicationBuilder"/></returns>
        public static IApplicationBuilder UseTelegramBotWebhook<TBot>(this IApplicationBuilder app,
            IBotBuilder botBuilder) where TBot : IBot
        {
            var updateDelegate = botBuilder.Build();
            return app.UseTelegramBotWebhook<TBot>(updateDelegate);
        }
        

        /// <summary>
        /// Add Telegram bot webhook handling functionality to the pipeline.
        /// </summary>
        /// <typeparam name="TBot">Type of bot</typeparam>
        /// <param name="app">Instance of <see cref="IApplicationBuilder"/></param>
        /// <param name="updateDelegate">Update delegate</param>
        /// <returns>Instance of <see cref="IApplicationBuilder"/></returns>
        public static IApplicationBuilder UseTelegramBotWebhook<TBot>(this IApplicationBuilder app,
            UpdateDelegate updateDelegate) where TBot : IBot
        {
            var options = app.ApplicationServices.GetRequiredService<IOptions<BotOptions>>();
            app.Map(
                options.Value.WebhookPath,
                builder => builder.UseMiddleware<TelegramBotMiddleware<TBot>>(updateDelegate)
            );

            return app;
        }
        
        /// <summary>
        /// Ensures webhook already set to right address.
        /// </summary>
        /// <param name="app">Instance of <see cref="IApplicationBuilder"/></param>
        /// <typeparam name="TBot">Type of bot</typeparam>
        /// <returns>Instance of <see cref="IApplicationBuilder"/></returns>
        public static IApplicationBuilder EnsureWebhookSet<TBot>(this IApplicationBuilder app)
            where TBot : IBot
        {
            using var scope = app.ApplicationServices.CreateScope();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<IBot>>();
            var bot = scope.ServiceProvider.GetRequiredService<TBot>();
            var options = scope.ServiceProvider.GetRequiredService<IOptions<BotOptions>>();
            var url = new Uri(options.Value.WebhookPath);

            logger?.LogInformation("Setting webhook for bot \"{Name}\" to URL \"{Url}\"", typeof(TBot).Name, url);

            bot.Client.SetWebhookAsync(url.AbsoluteUri)
                .GetAwaiter().GetResult();

            return app;
        }

        #endregion

        #region Long polling methods

        /// <summary>
        /// Uses long polling deployment method.
        /// </summary>
        /// <param name="app">Instance of <see cref="IApplicationBuilder"/></param>
        /// <param name="startAfter">Time when start getting updates</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <typeparam name="TBot">Type of bot</typeparam>
        /// <returns>Instance of <see cref="IApplicationBuilder"/></returns>
        public static IApplicationBuilder UseTelegramBotLongPolling<TBot>(this IApplicationBuilder app,
            TimeSpan startAfter = default,
            CancellationToken cancellationToken = default) where TBot : IBot
        {
            var updateDelegate = BotBuilder.BuildBotAutomatically();
            return app.UseTelegramBotLongPolling<TBot>(updateDelegate, startAfter, cancellationToken);
        }

        /// <summary>
        /// Uses long polling deployment method.
        /// </summary>
        /// <param name="app">Instance of <see cref="IApplicationBuilder"/></param>
        /// <param name="botBuilder">Instance of <see cref="IBotBuilder"/></param>
        /// <param name="startAfter">Time when start getting updates</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <typeparam name="TBot">Type of bot</typeparam>
        /// <returns>Instance of <see cref="IApplicationBuilder"/></returns>
        public static IApplicationBuilder UseTelegramBotLongPolling<TBot>(this IApplicationBuilder app,
            IBotBuilder botBuilder,
            TimeSpan startAfter = default,
            CancellationToken cancellationToken = default) where TBot : IBot
        {
            var updateDelegate = botBuilder.Build();
            return app.UseTelegramBotLongPolling<TBot>(updateDelegate, startAfter, cancellationToken);
        }

        /// <summary>
        /// Uses long polling deployment method.
        /// </summary>
        /// <param name="app">Instance of <see cref="IApplicationBuilder"/></param>
        /// <param name="updateDelegate">Update delegate</param>
        /// <param name="startAfter">Time when start getting updates</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <typeparam name="TBot">Type of bot</typeparam>
        /// <returns>Instance of <see cref="IApplicationBuilder"/></returns>
        public static IApplicationBuilder UseTelegramBotLongPolling<TBot>(this IApplicationBuilder app,
            UpdateDelegate updateDelegate,
            TimeSpan startAfter = default,
            CancellationToken cancellationToken = default) where TBot : IBot
        {
            if (startAfter == default)
            {
                startAfter = TimeSpan.FromSeconds(2);
            }

            var serviceProvider = new BotServiceProvider(app);
            var updateManager = new UpdatePollingManager<TBot>(updateDelegate, serviceProvider);

            Task.Run(async () =>
                {
                    await Task.Delay(startAfter, cancellationToken);
                    await updateManager.RunAsync(cancellationToken: cancellationToken);
                }, cancellationToken)
                .ContinueWith(t =>
                {
                    if (t.Exception == null)
                        return;

                    var logger = serviceProvider.GetService(typeof(ILogger<IBot>)) as ILogger<IBot>;
                    logger?.LogError("Thrown exception in UseTelegramBotLongPolling(): {Exception}", t.Exception);
                    throw t.Exception;
                }, TaskContinuationOptions.OnlyOnFaulted);

            return app;
        }

        #endregion
    }
}