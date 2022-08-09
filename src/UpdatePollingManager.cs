using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot.Framework.Abstractions;
using Telegram.Bot.Requests;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Telegram.Bot.Framework
{
    /// <summary>
    /// Update poll manager.
    /// </summary>
    /// <typeparam name="TBot"></typeparam>
    public class UpdatePollingManager<TBot> : IUpdatePollingManager<TBot>
             where TBot : IBot
    {
        private readonly UpdateDelegate _updateDelegate;
        private readonly IBotServiceProvider _rootProvider;

        public UpdatePollingManager(
            IBotBuilder botBuilder,
            IBotServiceProvider rootProvider)
        {
            _updateDelegate = botBuilder.Build();
            _rootProvider = rootProvider;
        }

        public UpdatePollingManager(
            UpdateDelegate updateDelegate,
            IBotServiceProvider rootProvider)
        {
            _updateDelegate = updateDelegate;
            _rootProvider = rootProvider;
        }

        public async Task RunAsync(
            GetUpdatesRequest requestParams = default,
            CancellationToken cancellationToken = default)
        {
            var bot = (TBot)_rootProvider.GetService(typeof(TBot));

            await bot.Client.DeleteWebhookAsync(true, cancellationToken)
                .ConfigureAwait(false);

            requestParams ??= new GetUpdatesRequest
            {
                Offset = 0,
                Timeout = 500,
                AllowedUpdates = Array.Empty<UpdateType>(),
            };

            while (!cancellationToken.IsCancellationRequested)
            {
                var updates = await bot.Client.MakeRequestAsync(
                    requestParams,
                    cancellationToken
                ).ConfigureAwait(false);

                // async mode: doen't work
                //var tasks = new List<Task>(updates.Length);

                //foreach (var update in updates)
                //{
                //    tasks.Add(ProcessUpdateAsync(bot, update));
                //}

                //await Task.WhenAll(tasks).ConfigureAwait(false);

                foreach (var update in updates)
                {
                    await ProcessUpdateAsync(bot, update).ConfigureAwait(false);
                }


                if (updates.Length > 0)
                {
                    requestParams.Offset = updates[^1].Id + 1;
                }
            }

            cancellationToken.ThrowIfCancellationRequested();
        }
        private async Task ProcessUpdateAsync(TBot bot, Update updateItem)
        {
            using var scopeProvider = _rootProvider.CreateScope();
            var context = new UpdateContext(bot, updateItem, scopeProvider);
            await _updateDelegate(context).ConfigureAwait(false);
        }
    }
}