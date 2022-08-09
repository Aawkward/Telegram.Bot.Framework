using System.Threading.Tasks;

namespace Telegram.Bot.Framework.Abstractions
{
    /// <summary>
    /// Processes an update
    /// </summary>
    public interface IUpdateHandler
    {
        /// <summary>
        /// Predicate where determines update can be handled or not.
        /// </summary>
        /// <param name="context">Instance of <see cref="IUpdateContext"/></param>
        /// <returns>True if update can be handled</returns>
        bool CanHandle(IUpdateContext context);
        
        /// <summary>
        /// Handles the update for bot. This method will be called only if CanHandleUpdate returns <value>true</value>
        /// </summary>
        /// <param name="context">Instance of <see cref="IUpdateContext"/></param>
        /// <param name="next">Next update delegate</param>
        /// <returns>Result of handling this update</returns>
        Task HandleAsync(IUpdateContext context, UpdateDelegate next);
    }
}