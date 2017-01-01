using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AsyncPipelineDemo;
using Autofac;
using NUnit.Framework;

namespace AsyncPipelineDemoTests
{
    public sealed class ResolveOpenGenericPipelineBehaviorTests
    {
        [Test]
        public void Can_Resolve_OpenGenericPipelineBehaviors()
        {
            var container = InitContainer();

            var behaviorsForCommandWithResult = container.Resolve<IEnumerable<IPipelineBehavior<CommandWithResult, bool>>>().ToArray();
            Assert.AreEqual(3, behaviorsForCommandWithResult.Length);
            Assert.AreEqual(typeof(GenericPipelineBehavior<CommandWithResult, bool>), behaviorsForCommandWithResult[0].GetType());
            Assert.AreEqual(typeof(CommandPipelineBehavior<bool>), behaviorsForCommandWithResult[1].GetType());
            Assert.AreEqual(typeof(ResponseInspectorPipelineBehavior<CommandWithResult>), behaviorsForCommandWithResult[2].GetType());

            var behaviorsForCommandWithoutResult = container.Resolve<IEnumerable<IPipelineBehavior<CommandWithoutResult, NullValue>>>().ToArray();
            Assert.AreEqual(3, behaviorsForCommandWithoutResult.Length);
            Assert.AreEqual(typeof(GenericPipelineBehavior<CommandWithoutResult, NullValue>), behaviorsForCommandWithoutResult[0].GetType());
            Assert.AreEqual(typeof(MarkerInterface1PipelineBehavior<NullValue>), behaviorsForCommandWithoutResult[1].GetType());
            Assert.AreEqual(typeof(CommandPipelineBehavior<NullValue>), behaviorsForCommandWithoutResult[2].GetType());
        }

        private IContainer InitContainer()
        {
            var builder = new ContainerBuilder();

            // the order in which the pipelines are registered is the order in which they will be resolved!

            // the fully open GenericPipelineBehavior<,> will resolve for _any_ TRequest/TResponse
            builder.RegisterGeneric(typeof(GenericPipelineBehavior<,>))
                   .As(typeof(IPipelineBehavior<,>))
                   .InstancePerLifetimeScope();

            // We can effectively limit the scope that a pipeline behavior applies to
            // based on the type of TRequest using marker interfaces

            // this behavior will resolve only for MarkerInterface1/TResponse
            builder.RegisterGeneric(typeof(MarkerInterface1PipelineBehavior<>))
                   .As(typeof(IPipelineBehavior<,>))
                   .InstancePerLifetimeScope();

            // this behavior will resolve only for ICommandInterface/TResponse
            builder.RegisterGeneric(typeof(CommandPipelineBehavior<>))
                   .As(typeof(IPipelineBehavior<,>))
                   .InstancePerLifetimeScope();

            // Futher, we can limit based on the type of TResponse

            // this behavior will resolve only for TRequest/bool
            builder.RegisterGeneric(typeof(ResponseInspectorPipelineBehavior<>))
                   .As(typeof(IPipelineBehavior<,>))
                   .InstancePerLifetimeScope();

            return builder.Build();
        }

        public sealed class GenericPipelineBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        {
            public Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }
        }

        public sealed class CommandPipelineBehavior<TResponse> : IPipelineBehavior<ICommandInterface, TResponse>
        {
            public Task<TResponse> Handle(ICommandInterface request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }
        }

        public sealed class MarkerInterface1PipelineBehavior<TResponse> : IPipelineBehavior<MarkerInterface1, TResponse>
        {
            public Task<TResponse> Handle(MarkerInterface1 request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }
        }

        public sealed class ResponseInspectorPipelineBehavior<TRequest> : IPipelineBehavior<TRequest, bool>
        {
            public Task<bool> Handle(TRequest request, RequestHandlerDelegate<bool> next, CancellationToken cancellationToken)
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
