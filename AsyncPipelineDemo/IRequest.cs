namespace AsyncPipelineDemo
{
    public interface IRequest<out TResponse> { }

    public interface IRequest { }
}
