using ResourceContracts;
namespace KubeAgent.Services;

public interface IEventPublisher
{
    Task PublishAsync(ResourceProcessorEvent events, CancellationToken cancellation);
}
