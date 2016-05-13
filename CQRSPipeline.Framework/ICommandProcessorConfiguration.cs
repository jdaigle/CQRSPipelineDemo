using System;

namespace CQRSPipeline.Framework
{
    public interface ICommandProcessorConfiguration
    {
        CommandModuleCatalog CommandModuleCatalog { get;  }
        Action<CommandContext> CompiledCommandHandlerPipeline { get; }
        ScopedInstanceFactoryFactory ScopedInstanceFactoryFactory { get; }
    }
}