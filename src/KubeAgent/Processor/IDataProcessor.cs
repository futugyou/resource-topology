namespace KubeAgent.Processor;

public interface IDataProcessor<T>
{
    Task CollectingData(T data, CancellationToken cancellation);
    Task ProcessingData(CancellationToken cancellation);
    Task Complete(CancellationToken cancellation);
}