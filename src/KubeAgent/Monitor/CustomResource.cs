
namespace KubeAgent.Monitor;

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
    public IList<T> Items { get; set; } = new List<T>();
}