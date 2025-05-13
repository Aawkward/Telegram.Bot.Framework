using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System;
using Telegram.Bot.Framework.Abstractions;

namespace Telegram.Bot.Framework
{
    internal class BotServiceProvider : IBotServiceProvider
    {
        private readonly IServiceProvider _container;
        private readonly IServiceScope _scope;

        public BotServiceProvider(IApplicationBuilder app)
        {
            _container = app.ApplicationServices;
        }

        public BotServiceProvider(IServiceScope scope)
        {
            _scope = scope;
        }

        public object GetService(Type serviceType)
        {
            if (_scope != null)
            {
                return _scope.ServiceProvider.GetService(serviceType);
            }
            else if (_container != null)
            {
                try
                {
                    return _container.GetService(serviceType);
                }
                catch
                {
                    return _container.CreateScope().ServiceProvider.GetService(serviceType);
                }
            }
            else throw new NullReferenceException("Scope and container are null.");
        }

        public IBotServiceProvider CreateScope()
        {
            return new BotServiceProvider(_container.CreateScope());
        }

        public void Dispose()
        {
            _scope?.Dispose();
        }
    }
}
