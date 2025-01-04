namespace KubeAgent.Discovery;

public interface IResourceDiscovery
{
    Task<IEnumerable<MonitoredResource>> GetMonitoredResourcesAsync(CancellationToken cancellation);
}

public class MonitoredResource
{
    public string KubeApiVersion { get; set; } = "";
    public string KubeKind { get; set; } = "";
    public string KubeGroup { get; set; } = "";
    public string KubePluralName { get; set; } = "";
    public required Type ReflectionType { get; set; }

    public string ID()
    {
        return $"{KubePluralName}/{KubeKind}/{KubeGroup}/{KubeApiVersion}";
    }
}
