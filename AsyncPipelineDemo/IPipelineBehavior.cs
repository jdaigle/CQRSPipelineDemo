using System.Threading;
using System.Threading.Tasks;

namespace AsyncPipelineDemo
{
    public interface IPipelineBehavior<in TRequest, TResponse>
    {
        Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken);
    }
}
