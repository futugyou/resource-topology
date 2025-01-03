namespace KubeAgent.ProcessorV2;

public interface IDataProcessor<T>
{
    Task CollectingData(T data, CancellationToken cancellation);
    Task ProcessingData(CancellationToken cancellation);
    Task Complete(CancellationToken cancellation);
}