namespace KubeAgent.Discovery;

/// <summary>
/// The responsibility of IResourceDiscovery is to provide a list of resources that need to be monitored.
/// It only provides a list of resources that currently need to be monitored, not a list of resources that need to be restarted or stopped.
/// </summary>
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
    public string Source { get; set; } = "Configurations";

    public string ID()
    {
        return $"{KubePluralName}/{KubeKind}/{KubeGroup}/{KubeApiVersion}";
    }
}
