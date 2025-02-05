using ResourceContracts;
namespace KubeAgent.Services;

public class NServiceBusPublisher(IMessageSession messageSession, ILogger<NServiceBusPublisher> logger) : IEventPublisher
{
    public async Task PublishAsync(ResourceProcessorEvent events, CancellationToken cancellation)
    {
        try
        {
            PublishOptions publishOptions = new();
            await messageSession.Publish(events, publishOptions, cancellation);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to publish event");
        }
    }
}
