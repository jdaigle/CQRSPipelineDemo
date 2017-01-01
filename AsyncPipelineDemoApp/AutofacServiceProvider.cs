using System;
using Autofac;

namespace AsyncPipelineDemoApp
{
    public sealed class AutofacServiceProvider : IServiceProvider
    {
        public AutofacServiceProvider(ILifetimeScope container)
        {
            _container = container;
        }

        private readonly ILifetimeScope _container;

        public object GetService(Type serviceType)
        {
            return _container.Resolve(serviceType);
        }
    }
}
