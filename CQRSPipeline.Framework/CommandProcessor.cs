using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CQRSPipeline.Framework
{
    public class CommandProcessor
    {
        private readonly CommandModuleCatalog commandModuleCatalog;
        private readonly Action<CommandContext> compiledCommandHandlerPipeline;

        public TResult Execute<TResult>(ICommand<TResult> command)
        {
            var commandContext = CommandContext.FromCommand(command);
            Execute(commandContext);
            return commandContext.Result;
        }

        private void Execute(CommandContext commandContext)
        {
            var startTS = Stopwatch.GetTimestamp();
            {
                // TODO: if using container, wrap in a child container

                commandContext.SingleInstanceFactory = t => null;

                compiledCommandHandlerPipeline(commandContext);
            }
            var stopTS = Stopwatch.GetTimestamp();
            var totalProcessingTimeMilliseconds = ((double)stopTS - (double)startTS) / Stopwatch.Frequency * 1000d; // = (ticks / (ticks/sec) * 1000)
        }
    }
}
