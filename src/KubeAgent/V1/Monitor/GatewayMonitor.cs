
using KubeAgent.V1.Processor;

namespace KubeAgent.V1.Monitor;

// TODO: fill all models
// TODO: other gateway models (https://github.com/kubernetes-sigs/gateway-api/blob/main/apis/v1/gateway_types.go)
public class GatewayMonitor(ILogger<GatewayMonitor> logger, IKubernetes client, [FromKeyedServices("General")] IResourceProcessor processor) : BaseMonitor(logger, processor), IResourceMonitor
{
    public Task MonitorResource(CancellationToken cancellation)
    {
        var generic = new GenericClient(client, group: "gateway.networking.k8s.io", version: "v1", plural: "gateways");
        generic.Watch<Gateway>(
            onEvent: async (type, item) => await HandlerResourceChange(type, item, cancellation),
            onError: async (ex) => await HandlerError(MonitorResource, "ListGatewayForAllNamespacesWithHttpMessagesAsync", ex, cancellation));

        return Task.CompletedTask;
    }
}

public class GatewayList : CustomResourceList<Gateway>
{
}

public class Gateway : CustomResource<GatewaySpec, GatewayStatus>
{
    public Gateway()
    {
        ApiVersion = "gateway.networking.k8s.io/v1";
        Kind = "Gateway";
    }
}

public class GatewaySpec
{
    [JsonPropertyName("gatewayClassName")]
    public string GatewayClassName { get; set; } = "";

    [JsonPropertyName("listeners")]
    public Listener[] Listeners { get; set; } = [];

    [JsonPropertyName("addresses")]
    public GatewayAddress[] Addresses { get; set; } = [];

    [JsonPropertyName("infrastructure")]
    public GatewayInfrastructure[] Infrastructure { get; set; } = [];

    [JsonPropertyName("backendTLS")]
    public GatewayBackendTLS? BackendTLS { get; set; }
}

public class GatewayStatus : V1Status
{
    [JsonPropertyName("listeners")]
    public ListenerStatus[] Listeners { get; set; } = [];

    [JsonPropertyName("addresses")]
    public GatewayAddress[] Addresses { get; set; } = [];

    [JsonPropertyName("conditions")]
    public V1Condition[] Conditions { get; set; } = [];

}

public class ListenerStatus
{

}

public class Listener
{

}

public class GatewayAddress
{

}

public class GatewayInfrastructure
{

}

public class GatewayBackendTLS
{

}