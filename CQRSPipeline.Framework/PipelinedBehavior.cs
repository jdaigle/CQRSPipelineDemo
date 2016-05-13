using System;
using System.Collections.Generic;
using System.Linq;

namespace CQRSPipeline.Framework
{
    public abstract class PipelinedBehavior
    {
        public abstract void Invoke(CommandContext context, Action<CommandContext> next);

        private static readonly Action<CommandContext> EmptyAction = m => { };

        public static Action<CommandContext> CompileMessageHandlerPipeline(IEnumerable<PipelinedBehavior> behaviors)
        {
            if (!behaviors.Any())
            {
                return EmptyAction;
            }
            var behavior = behaviors.First();
            var compiledInnerBehaviors = CompileMessageHandlerPipeline(behaviors.Skip(1));
            return m => behavior.Invoke(m, compiledInnerBehaviors);
        }
    }
}
