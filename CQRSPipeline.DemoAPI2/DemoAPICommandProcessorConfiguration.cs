using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using CQRSPipeline.DemoAPI.Data;
using CQRSPipeline.Framework;

namespace CQRSPipeline.DemoAPI
{
    public class DemoAPICommandProcessorConfiguration : ICommandProcessorConfiguration
    {

        public static ICommandProcessorConfiguration Instance { get { return LazyInstance.Value; } }

        private static Lazy<ICommandProcessorConfiguration> LazyInstance = new Lazy<ICommandProcessorConfiguration>(() =>
        {
            var configuration = new DemoAPICommandProcessorConfiguration();

            configuration.CommandModuleCatalog = new CommandModuleCatalog();
            configuration.CommandModuleCatalog.ScanAssembly(typeof(DemoAPICommandProcessorConfiguration).Assembly);

            var behaviors = new List<PipelinedBehavior>();
            // TODO: add other behaviors (e.g. auth, logging, etc.)
            behaviors.Add(new CommandTransactionBehavior());
            behaviors.Add(new CommandDispatcherBehavior(configuration.CommandModuleCatalog)); // always last!!!
            configuration.CompiledCommandHandlerPipeline = PipelinedBehavior.CompileMessageHandlerPipeline(behaviors);

            configuration.ScopedInstanceFactoryFactory = () => new DemoAPIScopedInstanceFactory();

            return configuration;
        });

        private DemoAPICommandProcessorConfiguration() { }

        public CommandModuleCatalog CommandModuleCatalog { get; private set; }
        public Action<CommandContext> CompiledCommandHandlerPipeline { get; private set; }
        public ScopedInstanceFactoryFactory ScopedInstanceFactoryFactory { get; private set; }
    }
}
