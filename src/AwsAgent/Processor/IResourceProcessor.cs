namespace AwsAgent.Processor;

public interface IResourceProcessor
{
    Task ProcessingData(CancellationToken cancellation);
}