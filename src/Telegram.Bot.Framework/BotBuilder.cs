using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot.Framework.Abstractions;

namespace Telegram.Bot.Framework
{
    /// <summary>
    /// Bot builder pipeline
    /// </summary>
    public class BotBuilder : IBotBuilder
    {
        internal UpdateDelegate UpdateDelegate { get; private set; }
        private readonly ICollection<Func<UpdateDelegate, UpdateDelegate>> _components;

        /// <summary>
        /// Creates new instance of <see cref="IBotBuilder"/>
        /// </summary>
        public BotBuilder()
        {
            _components = new List<Func<UpdateDelegate, UpdateDelegate>>();
        }

        /// <summary>
        /// Adds specified update handler to pipeline.
        /// </summary>
        /// <typeparam name="THandler">Type where defines <see cref="IUpdateHandler"/></typeparam>
        /// <returns>Instance of <see cref="IBotBuilder"/></returns>
        public IBotBuilder Use<THandler>()
            where THandler : IUpdateHandler
        {
            _components.Add(next =>
            {
                return context =>
                {
                    var handler = (IUpdateHandler)context.Services.GetService(typeof(THandler));
                    return handler.CanHandle(context) ? 
                        handler.HandleAsync(context, next) : next(context);
                };
            });

            return this;
        }

        /// <summary>
        /// Adds specified update handler to pipeline.
        /// </summary>
        /// <param name="handler">Instance of <see cref="IUpdateHandler"/></param>
        /// <typeparam name="THandler">Type where defines <see cref="IUpdateHandler"/></typeparam>
        /// <returns>Instance of <see cref="IBotBuilder"/></returns>
        public IBotBuilder Use<THandler>(THandler handler)
            where THandler : IUpdateHandler
        {
            _components.Add(next =>
            {
                return context => handler.CanHandle(context) ? 
                    handler.HandleAsync(context, next) : next(context);
            });

            return this;
        }

        /// <summary>
        /// Adds specified update handler to pipeline.
        /// </summary>
        /// <returns>Instance of <see cref="IBotBuilder"/></returns>
        internal IBotBuilder Use(Type handlerType)
        {
            var handlerInterfaceType = typeof(IUpdateHandler);
            if (!handlerInterfaceType.IsAssignableFrom(handlerType))
                throw new InvalidOperationException($"Type {handlerType.Name} is not assignable to IUpdateHandler");
            
            _components.Add(next =>
            {
                return context =>
                {
                    var handler = (IUpdateHandler)context.Services.GetService(handlerType);
                    return handler.CanHandle(context) ? 
                        handler.HandleAsync(context, next) : next(context);
                };
            });

            return this;
        }

        /// <summary>
        /// Builds bot pipeline from added update handlers.
        /// </summary>
        /// <returns>Delegate <see cref="UpdateDelegate"/></returns>
        public UpdateDelegate Build()
        {
            UpdateDelegate handle = context =>
            {
                // use Logger
                Console.WriteLine("No handler for update {0} of type {1}.", context.Update.Id, context.Update.Type);
                return Task.FromResult(1);
            };

            handle = _components.Reverse()
                .Aggregate(handle, (current, component) => component(current));

            return UpdateDelegate = handle;
        }

        /// <summary>
        /// Automatically builds bot pipeline from assemblies. 
        /// </summary>
        /// <returns>Delegate <see cref="UpdateDelegate"/></returns>
        internal static UpdateDelegate BuildBotAutomatically()
        {
            var builder = new BotBuilder();
            var handlerInterfaceType = typeof(IUpdateHandler);
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            foreach (var type in assemblies.SelectMany(i => i.DefinedTypes))
            {
                if (handlerInterfaceType.IsAssignableFrom(type) &&
                    type.IsClass && !type.IsAbstract)
                {
                    builder.Use(type);
                }
            }
            
            return builder.Build();
        }
    }
}
