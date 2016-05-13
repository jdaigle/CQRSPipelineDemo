﻿using System;
using System.Diagnostics;

namespace CQRSPipeline.Framework
{
    public class CommandProcessor
    {
        public CommandProcessor(ICommandProcessorConfiguration configuration)
        {
            commandModuleCatalog = configuration.CommandModuleCatalog;
            compiledCommandHandlerPipeline = configuration.CompiledCommandHandlerPipeline;
            singleInstanceFactory = configuration.SingleInstanceFactory;
        }

        private readonly CommandModuleCatalog commandModuleCatalog;
        private readonly Action<CommandContext> compiledCommandHandlerPipeline;
        private readonly SingleInstanceFactory singleInstanceFactory;

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
                commandContext.SingleInstanceFactory = singleInstanceFactory;

                compiledCommandHandlerPipeline(commandContext);
            }
            var stopTS = Stopwatch.GetTimestamp();
            var totalProcessingTimeMilliseconds = ((double)stopTS - (double)startTS) / (double)Stopwatch.Frequency * 1000d; // = (ticks / (ticks/sec) * 1000)
        }
    }
}
