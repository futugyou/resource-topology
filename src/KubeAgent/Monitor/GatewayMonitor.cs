
namespace KubeAgent.Monitor;

// TODO: fill all models
// TODO: other gateway models (https://github.com/kubernetes-sigs/gateway-api/blob/main/apis/v1/gateway_types.go)
public class GatewayMonitor(ILogger<GatewayMonitor> logger, IKubernetes client, ProcessorFactory factory) : BaseMonitor(logger, factory.GetResourceProcessor()), IResourceMonitor
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

public abstract class CustomResource : IKubernetesObject<V1ObjectMeta>, IMetadata<V1ObjectMeta>
{
    [JsonPropertyName("metadata")]
    public required V1ObjectMeta Metadata { get; set; }

    [JsonPropertyName("apiVersion")]
    public required string ApiVersion { get; set; }

    [JsonPropertyName("kind")]
    public required string Kind { get; set; }
}

public abstract class CustomResource<TSpec, TStatus> : CustomResource
{
    [JsonPropertyName("spec")]
    public required TSpec Spec { get; set; }

    [JsonPropertyName("status")]
    public required TStatus Status { get; set; }
}

public class CustomResourceList<T> : KubernetesObject where T : CustomResource
{
    public required V1ListMeta Metadata { get; set; }
    public List<T> Items { get; set; } = [];
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
    public required string GatewayClassName { get; set; }

    [JsonPropertyName("listeners")]
    public required Listener[] Listeners { get; set; }

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