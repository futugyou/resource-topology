using ResourceContracts;
namespace KubeAgent.Publisher;

public class DaprPublisher(ILogger<DaprPublisher> logger, DaprClient dapr) : IPublisher
{
    public Task PublishAsync(ResourceProcessorEvent events, CancellationToken cancellation)
    {
        var bytes = JsonSerializer.SerializeToUtf8Bytes(events);
        var metadata = new Dictionary<string, string> {
                {"datacontenttype","application/json"},
                {"contentType","application/json"},
                {"ttlInSeconds","86400"},
                {"cloudevent.type", "k8s-resource"},
            };

        var upsert = new List<StateTransactionRequest>()
        {
            new(events.EventID, bytes, StateOperationType.Upsert, metadata:metadata)
        };

        return dapr.ExecuteStateTransactionAsync("kube-agent-state", upsert, cancellationToken: cancellation);
    }
}
