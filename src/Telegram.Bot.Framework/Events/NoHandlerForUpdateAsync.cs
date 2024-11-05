using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot.Framework.Abstractions;

namespace Telegram.Bot.Framework.Events
{
    public delegate Task NoHandlerForUpdateAsync(object sender, IUpdateContext context, CancellationToken cancellationToken = default);
}
