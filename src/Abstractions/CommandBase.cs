using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Telegram.Bot.Framework.Abstractions
{
    /// <summary>
    /// Base handler implementation for a commandName such as "/start"
    /// </summary>
    public abstract class CommandBase : IUpdateHandler
    {
        #region Fields

        private readonly string _commandName;

        #endregion
        
        #region Ctor

        protected CommandBase(string commandName)
        {
            if (commandName.StartsWith("/"))
                throw new ArgumentException("Command name must not start with '/'.", nameof(commandName));
            
            _commandName = commandName;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Command handler method.
        /// </summary>
        /// <param name="context">Update context</param>
        /// <param name="next">Next update delegate</param>
        /// <param name="args">Command arguments</param>
        protected abstract Task HandleAsync(IUpdateContext context, UpdateDelegate next, string[] args);

        /// <summary>
        /// Command predicate method.
        /// </summary>
        /// <param name="context">Update context</param>
        /// <returns>True if method can handle.</returns>
        protected virtual bool CanHandleCommand(IUpdateContext context) => true;
        
        /// <summary>
        /// Determines command can be handled.
        /// </summary>
        /// <param name="context">Update context</param>
        /// <returns>True if command can be handled.</returns>
        public bool CanHandle(IUpdateContext context)
        {
            var message = context.Update?.Message;
            
            if (message == null)
                return false;
            
            if (string.IsNullOrWhiteSpace(message.Text))
                return false;

            var isCommand = message.Entities?.FirstOrDefault()?.Type == MessageEntityType.BotCommand;
            if (!isCommand)
                return false;

            var isCommandFromGroup = Regex.IsMatch(
                message.EntityValues.First(),
                $@"^/{_commandName}(?:@{context.Bot.Username})?$",
                RegexOptions.IgnoreCase);

            if (!isCommandFromGroup)
                return false;

            return CanHandleCommand(context);
        }

        /// <summary>
        /// Handles command.
        /// </summary>
        /// <param name="context">Update context</param>
        /// <param name="next">Next update delegate</param>
        public Task HandleAsync(IUpdateContext context, UpdateDelegate next)
        {
            return HandleAsync(context, next, ParseCommandArgs(context.Update.Message));
        }

        /// <summary>
        /// Parses command arguments from message.
        /// </summary>
        /// <param name="message">Bot message</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Thrown if parameter <paramref name="message"/> is null.</exception>
        public static string[] ParseCommandArgs(Message message)
        {
            if (message is null)
                throw new ArgumentNullException(nameof(message));
            
            if (message.Entities?.FirstOrDefault()?.Type != MessageEntityType.BotCommand)
                return Array.Empty<string>();

            var argsList = new List<string>();
            var allArgs = message.Text[message.Entities[0].Length..].TrimStart();
            argsList.Add(allArgs);

            var expandedArgs = Regex.Split(allArgs, @"\s+");
            if (expandedArgs.Length > 1)
            {
                argsList.AddRange(expandedArgs);
            }

            var args = argsList
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .ToArray();

            return args;
        }

        #endregion
    }
}