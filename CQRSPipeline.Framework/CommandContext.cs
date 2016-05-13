using System;

namespace CQRSPipeline.Framework
{
    public abstract class CommandContext
    {
        /// <summary>
        /// The type of request currently being executed.
        /// </summary>
        public abstract Type CommandType { get; }

        public SingleInstanceFactory SingleInstanceFactory { get; internal set; }

        internal static CommandContext<TResult> FromCommand<TResult>(ICommand<TResult> command)
        {
            // TODO: possible perf issues with reflection and Activator, probably better to generate and cache an Expression
            var context = Activator.CreateInstance(typeof(CommandContext<,>).MakeGenericType(command.GetType(), typeof(TResult))) as CommandContext<TResult>;
            context.SetCommand(command);
            return context;
        }

        internal virtual object GetCommand() => null;
        internal virtual void SetCommand(object command) { }
    }

    internal abstract class CommandContext<TResult> : CommandContext
    {
        public TResult Result { get; internal set; }
    }

    internal sealed class CommandContext<TCommand, TResult> : CommandContext<TResult>
    where TCommand : ICommand<TResult>
    {
        private static readonly Type commandType = typeof(TCommand);
        public override Type CommandType => commandType;

        public TCommand Command { get; internal set; }
        internal override object GetCommand() => Command;
        internal override void SetCommand(object command) => Command = (TCommand)command;
    }
}
