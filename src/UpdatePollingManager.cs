using System;
using System.Collections.Generic;
using System.Linq;
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
    public class UpdatePollingManager<TBot> : IDisposable, IUpdatePollingManager<TBot>
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

            requestParams ??= new GetUpdatesRequest
            {
                Offset = await ThrowOutPendingUpdatesAsync(bot, cancellationToken).ConfigureAwait(false),
                Timeout = (int)bot.Client.Timeout.TotalSeconds,
                Limit = 25,
                AllowedUpdates = Array.Empty<UpdateType>(),
            };

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var updates = await bot.Client.MakeRequestAsync(
                        requestParams,
                        cancellationToken
                    ).ConfigureAwait(false);

                    if (updates.Length > 0)
                    {
                        requestParams.Offset = updates[^1].Id + 1;

                        var sortedUpdates = updates.GroupBy(x => x.Message?.From.Id ?? x.CallbackQuery?.From.Id);

                        var tasks = sortedUpdates.Select(sortedUpdate => Task.Run(async () =>
                        {
                            foreach (var update in sortedUpdate)
                            {
                                await ProcessUpdateAsync(bot, update).ConfigureAwait(false);
                            }
                        }));

                        _ = Task.WhenAll(tasks).ConfigureAwait(false);
                    }
                }
                catch 
                {
                    // ignore
                }
            }

            cancellationToken.ThrowIfCancellationRequested();
        }

        private static async Task<int> ThrowOutPendingUpdatesAsync(TBot bot, 
            CancellationToken cancellationToken = default)
        {
            var request = new GetUpdatesRequest
            {
                Limit = 1,
                Offset = -1,
                Timeout = 0,
                AllowedUpdates = Array.Empty<UpdateType>(),
            };
            var updates = await bot.Client.MakeRequestAsync(request: request, cancellationToken: cancellationToken)
                .ConfigureAwait(false);

            if (updates.Length > 0)
            { 
                return updates[updates.Length - 1].Id + 1; 
            }

            return 0;
        }

        private async Task ProcessUpdateAsync(TBot bot, Update updateItem)
        {
            using var scopeProvider = _rootProvider.CreateScope();
            var context = new UpdateContext(bot, updateItem, scopeProvider);
            await _updateDelegate(context).ConfigureAwait(false);
        }

        #region IDisposable

        protected bool _disposed;

        /// <inheritdoc/>
        public virtual void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _rootProvider?.Dispose();
                }

                _disposed = true;
            }
        }
        
        ~UpdatePollingManager()
        {
            Dispose(false);
        }

        #endregion
    }
}