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
        private static readonly string connectionString = ConfigurationManager.ConnectionStrings["AdventureWorks"].ConnectionString;

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

            // note: don't do this for real
            var staticDbContext = new AdventureWorksDbContext(connectionString);

            configuration.SingleInstanceFactory = type =>
            {
                if (type == typeof(DbContext))
                    return staticDbContext;
                return null;
            };

            return configuration;
        });

        private DemoAPICommandProcessorConfiguration() { }

        public CommandModuleCatalog CommandModuleCatalog { get; private set; }
        public Action<CommandContext> CompiledCommandHandlerPipeline { get; private set; }
        public SingleInstanceFactory SingleInstanceFactory { get; private set; }
    }
}
