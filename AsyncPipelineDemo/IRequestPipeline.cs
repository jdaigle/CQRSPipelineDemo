using System.Threading;
using System.Threading.Tasks;

namespace AsyncPipelineDemo
{
    public interface IRequestPipeline
    {
        Task<TResponse> ExecuteAsync<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default(CancellationToken));

        Task ExecuteAsync(IRequest request, CancellationToken cancellationToken = default(CancellationToken));
    }
}
