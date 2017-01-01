using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AsyncPipelineDemo
{
    public sealed class AsyncRequestPipeline : IRequestPipeline
    {
        public AsyncRequestPipeline(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        private readonly IServiceProvider _serviceProvider;

        public Task<TResponse> ExecuteAsync<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken)
        {
            // TODO: cache request handler factory
            var requestHandlerFactoryType = typeof(RequestHandlerFactory<,>).MakeGenericType(request.GetType(), typeof(TResponse));
            var requestHandlerFactory = Activator.CreateInstance(requestHandlerFactoryType) as IRequestHandlerFactory<TResponse>;

            var pipeline = requestHandlerFactory.GetPipeline(_serviceProvider);

            return pipeline(request, cancellationToken);
        }

        public Task ExecuteAsync(IRequest request, CancellationToken cancellationToken)
        {
            // TODO: cache request handler factory
            var requestHandlerFactoryType = typeof(RequestHandlerFactory<>).MakeGenericType(request.GetType());
            var requestHandlerFactory = Activator.CreateInstance(requestHandlerFactoryType) as IRequestHandlerFactory<NullValue>;

            var pipeline = requestHandlerFactory.GetPipeline(_serviceProvider);

            return pipeline(request, cancellationToken);
        }
    }

    public interface IRequestHandlerFactory<TResponse>
    {
        RequestHandlerDelegate<TResponse> GetPipeline(IServiceProvider serviceProvider);
    }

    public sealed class RequestHandlerFactory<TRequest, TResponse> : IRequestHandlerFactory<TResponse>
       where TRequest : IRequest<TResponse>
    {
        public RequestHandlerDelegate<TResponse> GetPipeline(IServiceProvider serviceProvider)
        {
            var handler = serviceProvider.GetService(typeof(IAsyncRequestHandler<TRequest, TResponse>)) as IAsyncRequestHandler<TRequest, TResponse>;
            if (handler == null)
            {
                throw new InvalidOperationException($"IAsyncRequestHandler implementation not found for Request type {typeof(TRequest)}.");
            }


            RequestHandlerDelegate<TResponse> pipeline = (req, ct) =>
            {
                return handler.HandleAsync((TRequest)req, ct);
            };

            var pipelineBehaviors = serviceProvider.GetService(typeof(IEnumerable<IPipelineBehavior<TRequest, TResponse>>)) as IEnumerable<IPipelineBehavior<TRequest, TResponse>>;
            foreach (var behavior in pipelineBehaviors.Reverse())
            {
                var lastHandler = pipeline;
                pipeline = (req, ct) => behavior.Handle((TRequest)req, lastHandler, ct);
            }

            return pipeline;
        }
    }

    public sealed class RequestHandlerFactory<TRequest> : IRequestHandlerFactory<NullValue>
        where TRequest : IRequest
    {
        private static readonly Task<NullValue> CompletedNullValueTask = Task.FromResult(NullValue.Instance);

        public RequestHandlerDelegate<NullValue> GetPipeline(IServiceProvider serviceProvider)
        {
            var handler = serviceProvider.GetService(typeof(IAsyncRequestHandler<TRequest>)) as IAsyncRequestHandler<TRequest>;
            if (handler == null)
            {
                throw new InvalidOperationException($"IAsyncRequestHandler implementation not found for Request type {typeof(TRequest)}.");
            }

            RequestHandlerDelegate<NullValue> pipeline = (req, ct) =>
            {
                handler.HandleAsync((TRequest)req, ct);
                return CompletedNullValueTask;
            };

            var pipelineBehaviors = serviceProvider.GetService(typeof(IEnumerable<IPipelineBehavior<TRequest, NullValue>>)) as IEnumerable<IPipelineBehavior<TRequest, NullValue>>;
            foreach (var behavior in pipelineBehaviors.Reverse())
            {
                var lastHandler = pipeline;
                pipeline = (req, ct) => behavior.Handle((TRequest)req, lastHandler, ct);
            }

            return pipeline;
        }
    }
}
