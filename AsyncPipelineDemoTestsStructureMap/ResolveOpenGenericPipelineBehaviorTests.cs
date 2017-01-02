using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AsyncPipelineDemo;
using NUnit.Framework;
using StructureMap;

namespace AsyncPipelineDemoTestsStructureMap
{
    [TestFixture]
    public sealed class ResolveOpenGenericPipelineBehaviorTests
    {
        [Test]
        public void Can_Resolve_OpenGenericPipelineBehaviors()
        {
            var container = InitContainer();

            var behaviorsForCommandWithResult = container.GetAllInstances<IPipelineBehavior<CommandWithResult, bool>>().ToArray();

            Assert.AreEqual(2, behaviorsForCommandWithResult.Length);
            Assert.AreEqual(typeof(GenericPipelineBehavior<CommandWithResult, bool>), behaviorsForCommandWithResult[0].GetType());
            Assert.AreEqual(typeof(CommandPipelineBehavior<CommandWithResult,bool>), behaviorsForCommandWithResult[1].GetType());

            var behaviorsForCommandWithoutResult = container.GetAllInstances<IPipelineBehavior<CommandWithoutResult, NullValue>>().ToArray();
            Assert.AreEqual(3, behaviorsForCommandWithoutResult.Length);
            Assert.AreEqual(typeof(GenericPipelineBehavior<CommandWithoutResult, NullValue>), behaviorsForCommandWithoutResult[0].GetType());
            Assert.AreEqual(typeof(MarkerInterface1PipelineBehavior<CommandWithoutResult, NullValue>), behaviorsForCommandWithoutResult[1].GetType());
            Assert.AreEqual(typeof(CommandPipelineBehavior<CommandWithoutResult, NullValue>), behaviorsForCommandWithoutResult[2].GetType());
        }

        private IContainer InitContainer()
        {
            return new Container(cfg =>
            {
                // the fully open GenericPipelineBehavior<,> will resolve for _any_ TRequest/TResponse
                cfg.For(typeof(IPipelineBehavior<,>)).Add(typeof(GenericPipelineBehavior<,>));

                // We can effectively limit the scope that a pipeline behavior applies to
                // based on the type of TRequest using marker interfaces

                // this behavior will resolve only for MarkerInterface1/TResponse
                cfg.For(typeof(IPipelineBehavior<,>)).Add(typeof(MarkerInterface1PipelineBehavior<,>));

                // this behavior will resolve only for ICommandInterface/TResponse
                cfg.For(typeof(IPipelineBehavior<,>)).Add(typeof(CommandPipelineBehavior<,>));
            });
        }

        public sealed class GenericPipelineBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        {
            public Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }
        }

        public sealed class CommandPipelineBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
            where TRequest : ICommandInterface
        {
            public Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }
        }

        public sealed class MarkerInterface1PipelineBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
            where TRequest : MarkerInterface1
        {
            public Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }
        }

        public interface ICommandInterface { }

        public interface MarkerInterface1 { }

        public class CommandWithoutResult : IRequest, MarkerInterface1, ICommandInterface { }

        public class CommandWithResult : IRequest<int>, ICommandInterface { }
    }
}
