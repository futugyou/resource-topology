namespace AwsAgent.Services;

public interface IResourceProcessor
{
    Task ProcessingData(CancellationToken cancellation);
}