using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using AsyncPipelineDemo;
using Autofac;
using System.Threading;
using Autofac.Core;
using Autofac.Builder;
using Autofac.Features.Scanning;
using System.Reflection;

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

            builder.RegisterAssemblyTypes(GetType().Assembly)
                   .PreserveExistingDefaults()
                   .AsImplementedInterfaces();

            builder.RegisterSource(new OpenGenericContravariantRegistrationSource(typeof(IAsyncNotificationHandler<>)));

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

    public class OpenGenericContravariantRegistrationSource : IRegistrationSource
    {
        private readonly Type _openGenericType;

        public OpenGenericContravariantRegistrationSource(Type openGenericType)
        {
            if (openGenericType == null) throw new ArgumentNullException(nameof(openGenericType));

            _openGenericType = openGenericType;
        }

        public IEnumerable<IComponentRegistration> RegistrationsFor(Service service, Func<Service, IEnumerable<IComponentRegistration>> registrationAccessor)
        {
            if (service == null) throw new ArgumentNullException(nameof(service));
            if (registrationAccessor == null) throw new ArgumentNullException(nameof(registrationAccessor));

            var swt = service as IServiceWithType;
            // must be a generic type
            if (swt == null || !swt.ServiceType.IsGenericType)
            {
                return Enumerable.Empty<IComponentRegistration>();
            }

            if (!swt.ServiceType.IsClosedTypeOf(_openGenericType))
            {
                return Enumerable.Empty<IComponentRegistration>();
            }

            // TODO: verify the service is an Interface with an contravariant generic parameter

            var args = swt.ServiceType.GetTypeInfo().GenericTypeArguments;
            var definition = swt.ServiceType.GetGenericTypeDefinition();
            var contravariantParameter = args[0]; // TODO: the contravariant parameter may not always be in position 0
            if (contravariantParameter.GetTypeInfo().IsValueType)
            {
                return Enumerable.Empty<IComponentRegistration>();
            }

            var possibleSubstitutions = GetTypesAssignableFrom(contravariantParameter).ToList();

            // TODO: need a where clause to ensure type does in compatible with contraints on generic parameter to prevent exceptions
            // TODO: need to handle genertic type with multiple parameters (one of with is the contravariant parameter that we're substituting)
            var variations = possibleSubstitutions
                .Select(a => definition.MakeGenericType(a))
                .ToList();

            var variantRegistrations = variations
                .SelectMany(v => registrationAccessor(swt.ChangeType(v)))
                .Where(r => !r.Metadata.ContainsKey(nameof(ContravariantAsyncNotificationHandlerRegistrationSource)))
                .ToList();

            return variantRegistrations
                 .Select(vr => RegistrationBuilder
                    .ForDelegate((c, p) => c.ResolveComponent(vr, p))
                    .Targeting(vr)
                    .As(service)
                    .WithMetadata(nameof(ContravariantAsyncNotificationHandlerRegistrationSource), true)
                    .CreateRegistration());
        }

        private static IEnumerable<Type> GetTypesAssignableFrom(Type type) => GetBagOfTypesAssignableFrom(type).Distinct();

        private static IEnumerable<Type> GetBagOfTypesAssignableFrom(Type type)
        {
            if (type.GetTypeInfo().BaseType != null)
            {
                yield return type.GetTypeInfo().BaseType;
                foreach (var fromBase in GetBagOfTypesAssignableFrom(type.GetTypeInfo().BaseType))
                {
                    yield return fromBase;
                }
            }
            else
            {
                if (type != typeof(object))
                {
                    yield return typeof(object);
                }
            }

            foreach (var ifce in type.GetTypeInfo().ImplementedInterfaces)
            {
                if (ifce != type)
                {
                    yield return ifce;
                    foreach (var fromIfce in GetBagOfTypesAssignableFrom(ifce))
                    {
                        yield return fromIfce;
                    }
                }
            }
        }

        public bool IsAdapterForIndividualComponents => true;
    }

}
