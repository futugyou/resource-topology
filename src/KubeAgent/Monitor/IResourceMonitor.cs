namespace KubeAgent.Monitor;

public interface IResourceMonitor
{
    Task StartMonitoringAsync(MonitoringContext context, CancellationToken cancellation);
    Task<IEnumerable<WatcherInfo>> GetWatcherListAsync(CancellationToken cancellation);
    Task StopMonitoringAsync(string resourceId);
}

public class WatcherInfo
{
    public string ResourceId { get; set; } = "";
    public string ClusterName { get; set; } = "";
    public string KubeApiVersion { get; set; } = "";
    public string KubeKind { get; set; } = "";
    public string KubeGroup { get; set; } = "";
    public string KubePluralName { get; set; } = "";
    public string Namespace { get; set; } = "";
    public Type ReflectionType { get; set; } = typeof(GeneralCustomResource);
    public DateTime LastActiveTime { get; set; }
}

public class MonitoringContext
{
    public string ClusterName { get; set; } = "";
    public string KubeApiVersion { get; set; } = "";
    public string KubeKind { get; set; } = "";
    public string KubeGroup { get; set; } = "";
    public string KubePluralName { get; set; } = "";
    public string Namespace { get; set; } = "";
    public string? ResourceVersion { get; set; }
    public Type ReflectionType { get; set; } = typeof(GeneralCustomResource);
    public DateTime MonitoringStartTime { get; set; } = DateTime.UtcNow;


    public string ResourceId()
    {
        var sb = new StringBuilder();

        sb.Append(ClusterName ?? "");
        sb.Append('/');
        sb.Append(KubePluralName ?? "");
        sb.Append('/');
        sb.Append(KubeKind ?? "");
        sb.Append('/');
        sb.Append(KubeGroup ?? "");
        sb.Append('/');
        sb.Append(KubeApiVersion ?? "");
        sb.Append('/');
        sb.Append(Namespace ?? "");

        return sb.ToString();
    }
}

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