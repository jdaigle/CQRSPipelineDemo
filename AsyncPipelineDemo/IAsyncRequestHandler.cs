using System.Threading;
using System.Threading.Tasks;

namespace AsyncPipelineDemo
{
    public interface IAsyncRequestHandler<in TRequest, TResponse>
       where TRequest : IRequest<TResponse>
    {
        Task<TResponse> HandleAsync(TRequest request, CancellationToken cancellationToken);
    }

    public interface IAsyncRequestHandler<in TRequest>
       where TRequest : IRequest
    {
        Task HandleAsync(TRequest request, CancellationToken cancellationToken);
    }
}
