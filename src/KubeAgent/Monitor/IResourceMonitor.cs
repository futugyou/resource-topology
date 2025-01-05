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
    public string KubeApiVersion { get; set; } = "";
    public string KubeKind { get; set; } = "";
    public string KubeGroup { get; set; } = "";
    public string KubePluralName { get; set; } = "";
    public Type ReflectionType { get; set; } = typeof(GeneralCustomResource);
    public DateTime LastActiveTime { get; set; }
}

public class MonitoringContext
{
    public string ResourceId { get; set; } = "";
    public string KubeApiVersion { get; set; } = "";
    public string KubeKind { get; set; } = "";
    public string KubeGroup { get; set; } = "";
    public string KubePluralName { get; set; } = "";
    public string? ResourceVersion { get; set; }
    public Type ReflectionType { get; set; } = typeof(GeneralCustomResource);
    public DateTime MonitoringStartTime { get; set; } = DateTime.UtcNow;

    public static MonitoringContext FromMonitoredResource(MonitoredResource resource)
    {
        return new MonitoringContext
        {
            ResourceId = resource.ID(),
            KubeApiVersion = resource.KubeApiVersion,
            KubeKind = resource.KubeKind,
            KubeGroup = resource.KubeGroup,
            KubePluralName = resource.KubePluralName,
            ReflectionType = resource.ReflectionType,
        };
    }

    public MonitoredResource ToMonitoredResource()
    {
        return new MonitoredResource
        {
            KubeApiVersion = KubeApiVersion,
            KubeKind = KubeKind,
            KubeGroup = KubeGroup,
            KubePluralName = KubePluralName,
            ReflectionType = ReflectionType,
        };
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