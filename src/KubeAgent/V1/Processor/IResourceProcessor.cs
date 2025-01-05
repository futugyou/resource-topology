namespace KubeAgent.V1.Processor;

public interface IResourceProcessor
{
    Task CollectingData(Resource data, CancellationToken cancellation);
    Task ProcessingData(CancellationToken cancellation);
    Task Complete(CancellationToken cancellation);
}