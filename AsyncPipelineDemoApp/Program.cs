using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AsyncPipelineDemo;
using Autofac;
using Autofac.Features.Variance;

namespace AsyncPipelineDemoApp
{
    public static class Program
    {
        private static IContainer autofacContainer;

        public static void Main(string[] args)
        {
            InitAutofacContainer();
            Task.Run(async () =>
            {
                using (var container = autofacContainer.BeginLifetimeScope())
                {
                    var pipeline = container.Resolve<IRequestPipeline>();
                    var result = await pipeline.ExecuteAsync(new AddItem.Command
                    {
                        Data = "foobar",
                    });

                    await pipeline.ExecuteAsync(new DeleteItem.Command
                    {
                        Data = "foobar",
                    });
                }
            }).Wait();
        }

        private static void InitAutofacContainer()
        {
            var builder = new ContainerBuilder();

            builder.RegisterAssemblyTypes(typeof(IRequestPipeline).GetTypeInfo().Assembly)
                   .AsImplementedInterfaces();

            builder.RegisterType<AutofacServiceProvider>()
                   .AsImplementedInterfaces();

            //builder.RegisterSource(new ContravariantRegistrationSource());

            // generic pipelines
            builder.RegisterGeneric(typeof(GenericPipelineBehavior<,>))
                   .As(typeof(IPipelineBehavior<,>))
                   .InstancePerLifetimeScope();
            builder.RegisterGeneric(typeof(AnotherGenericPipelineBehavior<,>))
                   .As(typeof(IPipelineBehavior<,>))
                   .InstancePerLifetimeScope();
            builder.RegisterGeneric(typeof(MarkerInterfacePipelineBehavior<>))
                   .As(typeof(IPipelineBehavior<,>))
                   .InstancePerLifetimeScope();

            // register handlers
            builder.RegisterAssemblyTypes(typeof(Program).Assembly)
                   .AsClosedTypesOf(typeof(IAsyncRequestHandler<,>))
                   .AsImplementedInterfaces();
            builder.RegisterAssemblyTypes(typeof(Program).Assembly)
                   .AsClosedTypesOf(typeof(IAsyncRequestHandler<>))
                   .AsImplementedInterfaces();

            autofacContainer = builder.Build();
        }
    }

    public sealed class GenericPipelineBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    {
        public GenericPipelineBehavior()
        {

        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            // before
            var result = await next(request, cancellationToken);
            // after
            return result;
        }
    }

    public sealed class AnotherGenericPipelineBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    {
        public AnotherGenericPipelineBehavior()
        {

        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            // before
            var result = await next(request, cancellationToken);
            // after
            return result;
        }
    }

    public sealed class MarkerInterfacePipelineBehavior<TResponse> : IPipelineBehavior<IMarkerInterface, TResponse>
    {
        public MarkerInterfacePipelineBehavior()
        {

        }

        public async Task<TResponse> Handle(IMarkerInterface request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            // before
            var result = await next(request, cancellationToken);
            // after
            return result;
        }
    }

    public interface IMarkerInterface { }
    public interface ICommand : IRequest, IMarkerInterface { }
    public interface ICommand<out TResponse> : IRequest<TResponse> { }

    public sealed class AddItem
    {
        public sealed class Command : ICommand<bool>
        {
            public string Data { get; set; }
        }

        public sealed class Handler : IAsyncRequestHandler<Command, bool>
        {
            public async Task<bool> HandleAsync(Command request, CancellationToken cancellationToken)
            {
                await Task.CompletedTask;

                return true;
            }
        }
    }
    public sealed class DeleteItem
    {
        public sealed class Command : ICommand
        {
            public string Data { get; set; }
        }

        public sealed class Handler : IAsyncRequestHandler<Command>
        {
            public Task HandleAsync(Command request, CancellationToken cancellationToken)
            {
                return Task.CompletedTask;
            }
        }
    }
}
