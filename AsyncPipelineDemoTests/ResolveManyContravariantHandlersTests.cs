using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using AsyncPipelineDemo;
using Autofac;
using System.Threading;

namespace AsyncPipelineDemoTests
{
    [TestFixture]
    public sealed class ResolveManyContravariantHandlersTests
    {
        [Test]
        public void Can_Resolve_OpenGenericPipelineBehaviors()
        {
            var container = InitContainer();

            var handlersForConcreteEvent1 = container.Resolve<IEnumerable<IAsyncNotificationHandler<ConcreteEvent1>>>().ToArray();
            Assert.AreEqual(2, handlersForConcreteEvent1.Length);

            handlersForConcreteEvent1 = container.Resolve<IEnumerable<IAsyncNotificationHandler<ConcreteEvent1>>>().ToArray();
            Assert.AreEqual(2, handlersForConcreteEvent1.Length);

            var handlersForConcreteEvent2 = container.Resolve<IEnumerable<IAsyncNotificationHandler<ConcreteEvent2>>>().ToArray();
            Assert.AreEqual(1, handlersForConcreteEvent2.Length);
        }

        private IContainer InitContainer()
        {
            var builder = new ContainerBuilder();

            builder.RegisterSource(new ContravariantAsyncNotificationHandlerRegistrationSource());

            builder.RegisterAssemblyTypes(GetType().Assembly)
                   .AsClosedTypesOf(typeof(IAsyncNotificationHandler<>))
                   .AsImplementedInterfaces();

            return builder.Build();
        }

        public interface INotification { }

        public interface IAsyncNotificationHandler<in TNotification>
        {
            Task HandleAsync(TNotification notification, CancellationToken cancellationToken);
        }

        public class BaseEvent { }

        public class ConcreteEvent1 : BaseEvent { }
        public class ConcreteEvent2 : BaseEvent { }

        public class ConcreteEvent1Handler : IAsyncNotificationHandler<ConcreteEvent1>
        {
            public Task HandleAsync(ConcreteEvent1 notification, CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }
        }

        public class BaseEventHandler : IAsyncNotificationHandler<BaseEvent>
        {
            public Task HandleAsync(BaseEvent notification, CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }
        }
    }

}
