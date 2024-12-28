namespace KubeAgent.Monitor;

public class NetworkPolicyMonitor(ILogger<NetworkPolicyMonitor> logger, IKubernetes client, ProcessorFactory factory) : BaseMonitor(logger, factory.GetResourceProcessor()), IResourceMonitor
{
    public async Task MonitorResource(CancellationToken cancellation)
    {
        var resources = await client.NetworkingV1.ListNetworkPolicyForAllNamespacesWithHttpMessagesAsync(watch: true, cancellationToken: cancellation);
        resources.Watch<V1NetworkPolicy, V1NetworkPolicyList>(
            onEvent: async (type, item) => await HandlerResourceChange(type, item, cancellation),
            onError: async (ex) => await HandlerError(MonitorResource, "ListNetworkPolicyForAllNamespacesWithHttpMessagesAsync", ex, cancellation));
    }
}