using ResourceContracts;
namespace KubeAgent.Publisher;

public class NServiceBusPublisher(IMessageSession messageSession, ILogger<NServiceBusPublisher> logger) : IPublisher
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
