using System;
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
        private Update[] _updates = Array.Empty<Update>();

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

            await bot.Client.DeleteWebhookAsync(false, cancellationToken)
                .ConfigureAwait(false);

            requestParams ??= new GetUpdatesRequest
            {
                Offset = 0,
                Timeout = 500,
                Limit = 100,
                AllowedUpdates = Array.Empty<UpdateType>(),
            };

            while (!cancellationToken.IsCancellationRequested)
            {
                var updates = await bot.Client.MakeRequestAsync(
                    requestParams,
                    cancellationToken
                ).ConfigureAwait(false);

                if (updates.Length > 0)
                {
                    requestParams.Offset = updates.Max(x => x.Id) + 1;

                    if (HasEqualsCallbackQueries(updates, _updates))
                    {
                        continue;
                    }

                    _updates = updates;

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

            cancellationToken.ThrowIfCancellationRequested();
        }

        private async Task ProcessUpdateAsync(TBot bot, Update updateItem)
        {
            using var scopeProvider = _rootProvider.CreateScope();
            var context = new UpdateContext(bot, updateItem, scopeProvider);
            await _updateDelegate(context).ConfigureAwait(false);
        }

        #region Static methods

        private static bool HasEqualsCallbackQueries(Update[] first, Update[] second)
        {
            if (first.Length != second.Length) return false;

            for (int i = 0; i < first.Length; i++)
            {
                if (first[i].CallbackQuery?.ChatInstance 
                    != second[i].CallbackQuery?.ChatInstance) return false;

                if (first[i].CallbackQuery?.Data 
                    != second[i].CallbackQuery?.Data) return false;

                if (first[i].CallbackQuery?.From 
                    != second[i].CallbackQuery?.From) return false;
            }

            return true;
        }

        #endregion

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