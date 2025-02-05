using ResourceContracts;
namespace KubeAgent.Services;

public class DaprPublisher(ILogger<DaprPublisher> logger) : IEventPublisher
{
    public Task PublishAsync(ResourceProcessorEvent events, CancellationToken cancellation)
    {
        // TODO: send events with dapr
        logger.LogInformation("Publishing events with Dapr");
        return Task.CompletedTask;
    }
}
