using System;
using System.Collections.Generic;

namespace CQRSPipeline.Framework
{
    public abstract class CommandModule
    {
        protected void Handle<TCommand>(Action<TCommand> action)
            where TCommand : ICommand<VoidResult>
        {
            var dispatchInfo = CommandDispatcher.CreateCommandDispatcher(action, typeof(TCommand), GetType());
            commandDispatchers.Add(dispatchInfo);
        }

        protected void Handle<TCommand, TResult>(Func<TCommand, TResult> func)
            where TCommand : ICommand<TResult>
        {
            var dispatchInfo = CommandDispatcher.CreateCommandDispatcher(func, typeof(TCommand), GetType());
            commandDispatchers.Add(dispatchInfo);
        }

        private List<CommandDispatcher> commandDispatchers = new List<CommandDispatcher>();

        public IReadOnlyList<CommandDispatcher> CommandDispatchers { get { return commandDispatchers; } }
    }
}
