using System;

namespace CQRSPipeline.Framework
{
    public class CommandDispatcherBehavior : PipelinedBehavior
    {
        public CommandDispatcherBehavior(CommandModuleCatalog commandModuleCatalog)
        {
            this.commandModuleCatalog = commandModuleCatalog;
        }

        private readonly CommandModuleCatalog commandModuleCatalog;

        public override void Invoke(CommandContext context, Action<CommandContext> next)
        {
            var dispatcher = commandModuleCatalog.GetDispatcher(context.CommandType);
            var result = dispatcher.Execute(context.GetCommand(), context.SingleInstanceFactory);
            context.SetResult(result); // TODO: boxing

            next(context); // it's best practice to call next, even though this is likely the most inner behavior to execute
        }
    }
}
