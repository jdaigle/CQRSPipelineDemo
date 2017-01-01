using System.Threading;
using System.Threading.Tasks;

namespace AsyncPipelineDemo
{
    public delegate Task<TResponse> RequestHandlerDelegate<TResponse>(object request, CancellationToken cancellationToken);
}
