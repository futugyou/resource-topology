
namespace KubeAgent.Publisher;

public class PublisherFactory([FromKeyedServices("NServiceBus")] IPublisher eventPublisher, [FromKeyedServices("Dapr")] IPublisher daprPublisher, IOptionsMonitor<PublisherOptions> options)
{
    public IPublisher GetPublisher()
    {
        return options.CurrentValue?.PublisherType switch
        {
            "NServiceBus" => eventPublisher,
            "Dapr" => daprPublisher,
            _ => throw new ArgumentException("Invalid publisher type")
        };
    }
}