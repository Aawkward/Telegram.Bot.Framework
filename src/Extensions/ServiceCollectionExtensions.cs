using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot.Framework.Abstractions;

namespace Telegram.Bot.Framework
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds bot handlers to services container.
        /// </summary>
        /// <param name="services">Instance of <see cref="IServiceCollection"/></param>
        /// <param name="configureOptions">Bot options to configure</param>
        /// <typeparam name="TBot">Type of bot</typeparam>
        /// <returns>Instance of <see cref="IServiceCollection"/></returns>
        public static IServiceCollection AddTelegramBot<TBot>(this IServiceCollection services,
            Action<BotOptions> configureOptions) where TBot : IBot
        {
            services.AddTransient(typeof(TBot))
                .Configure(configureOptions);

            return AddHandlersToContainer(services);
        }

        /// <summary>
        /// Adds bot handlers to services container.
        /// </summary>
        /// <param name="services">Instance of <see cref="IServiceCollection"/></param>
        /// <param name="options">Bot options to configure</param>
        /// <typeparam name="TBot">Type of bot</typeparam>
        /// <returns>Instance of <see cref="IServiceCollection"/></returns>
        public static IServiceCollection AddTelegramBot<TBot>(this IServiceCollection services,
            BotOptions options) where TBot : IBot
        {
            services.AddTransient(typeof(TBot))
                .Configure<BotOptions>(i =>
                {
                    i.Username = options.Username;
                    i.ApiToken = options.ApiToken;
                    i.WebhookPath = options.WebhookPath;
                });

            return AddHandlersToContainer(services);
        }

        private static IServiceCollection AddHandlersToContainer(IServiceCollection services)
        {
            var handlerInterfaceType = typeof(IUpdateHandler);
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            foreach (var type in assemblies.SelectMany(i => i.DefinedTypes))
            {
                if (handlerInterfaceType.IsAssignableFrom(type) &&
                    type.IsClass && !type.IsAbstract)
                {
                    services.AddScoped(type);
                }
            }

            return services;
        }
    }
}