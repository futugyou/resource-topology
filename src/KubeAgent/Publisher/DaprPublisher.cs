using ResourceContracts;
namespace KubeAgent.Publisher;

public class DaprPublisher(ILogger<DaprPublisher> logger) : IPublisher
{
    public Task PublishAsync(ResourceProcessorEvent events, CancellationToken cancellation)
    {
        // TODO: send events with dapr
        logger.LogInformation("Publishing events with Dapr");
        return Task.CompletedTask;
    }
}
