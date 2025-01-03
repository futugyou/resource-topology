namespace KubeAgent.MonitorV2;

public interface IResourceMonitor
{
    Task StartMonitoringAsync(MonitoringContext context, CancellationToken cancellation);
    Task StopMonitoringAsync(string resourceId);
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
            ResourceVersion = resource.ResourceVersion,
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
            ResourceVersion = ResourceVersion,
            ReflectionType = ReflectionType,
        };
    }
}
