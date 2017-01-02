using System.Threading;
using System.Threading.Tasks;

namespace AsyncPipelineDemo
{
    public interface IAsyncRequestHandler<TRequest, TResponse>
       where TRequest : IRequest<TResponse>
    {
        Task<TResponse> HandleAsync(TRequest request, CancellationToken cancellationToken);
    }

    public interface IAsyncRequestHandler<TRequest>
       where TRequest : IRequest
    {
        Task HandleAsync(TRequest request, CancellationToken cancellationToken);
    }
}
