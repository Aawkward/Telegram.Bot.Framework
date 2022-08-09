using System.Threading.Tasks;

namespace Telegram.Bot.Framework.Abstractions
{
    public abstract class UpdateHandlerBase : IUpdateHandler
    {
        public virtual bool CanHandle(IUpdateContext context) => true;
        public abstract Task HandleAsync(IUpdateContext context, UpdateDelegate next);
    }
}