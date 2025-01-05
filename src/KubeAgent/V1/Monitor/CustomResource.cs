
using KubeAgent.V1.Processor;

namespace KubeAgent.V1.Monitor;

public class GeneralCustomResource : IKubernetesObject<V1ObjectMeta>
{
    [JsonPropertyName("apiVersion")]
    public string ApiVersion { get; set; } = "";
    [JsonPropertyName("kind")]
    public string Kind { get; set; } = "";
    [JsonPropertyName("spec")]
    public object Spec { get; set; } = new();
    [JsonPropertyName("status")]
    public object Status { get; set; } = new();
    [JsonPropertyName("metadata")]
    public V1ObjectMeta Metadata { get; set; } = new();
}

public class Metadata
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = "";

    [JsonPropertyName("namespace")]
    public string Namespace { get; set; } = "";

    [JsonPropertyName("labels")]
    public Dictionary<string, string> Labels { get; set; } = [];

    [JsonPropertyName("annotations")]
    public Dictionary<string, string> Annotations { get; set; } = [];
}

public abstract class CustomResource : KubernetesObject, IKubernetesObject<V1ObjectMeta>
{
    [JsonPropertyName("metadata")]
    public V1ObjectMeta Metadata { get; set; } = new();
}

public abstract class CustomResource<TSpec> : CustomResource, ISpec<TSpec> where TSpec : new()
{
    [JsonPropertyName("spec")]
    public TSpec Spec { get; set; } = new();
}

public abstract class CustomResource<TSpec, TStatus> : CustomResource<TSpec>, IStatus<TStatus> where TSpec : new() where TStatus : new()
{
    [JsonPropertyName("status")]
    public TStatus Status { get; set; } = new();
}

public class CustomResourceList<T> : KubernetesObject where T : IKubernetesObject
{
    public V1ListMeta Metadata { get; set; } = new();
    public IList<T> Items { get; set; } = [];
}