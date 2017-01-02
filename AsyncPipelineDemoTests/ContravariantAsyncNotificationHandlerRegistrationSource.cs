using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Autofac;
using Autofac.Builder;
using Autofac.Core;
using static AsyncPipelineDemoTests.ResolveManyContravariantHandlersTests;

namespace AsyncPipelineDemoTests
{
    public class ContravariantAsyncNotificationHandlerRegistrationSource : IRegistrationSource
    {
        public IEnumerable<IComponentRegistration> RegistrationsFor(
          Service service,
          Func<Service, IEnumerable<IComponentRegistration>> registrationAccessor)
        {
            var swt = service as IServiceWithType;
            if (swt == null
                || !swt.ServiceType.IsGenericType
                || swt.ServiceType.GetGenericTypeDefinition() != typeof(IAsyncNotificationHandler<>))
            {
                // It's not a request for the base handler type, so skip it.
                return Enumerable.Empty<IComponentRegistration>();
            }

            var args = swt.ServiceType.GetTypeInfo().GenericTypeArguments;
            var definition = swt.ServiceType.GetGenericTypeDefinition();
            var contravariantParameter = args[0];

            var possibleSubstitutions = GetTypesAssignableFrom(contravariantParameter).ToList();

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

        private static IEnumerable<Type> GetTypesAssignableFrom(Type type)
        {
            return GetBagOfTypesAssignableFrom(type)
                .Distinct();
        }

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

        public bool IsAdapterForIndividualComponents { get { return false; } }
    }
}