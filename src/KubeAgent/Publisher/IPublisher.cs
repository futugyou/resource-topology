using ResourceContracts;
namespace KubeAgent.Publisher;

public interface IPublisher
{
    Task PublishAsync(ResourceProcessorEvent events, CancellationToken cancellation);
}
