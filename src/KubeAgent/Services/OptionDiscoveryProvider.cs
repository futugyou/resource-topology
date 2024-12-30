namespace KubeAgent.Services;
public class OptionDiscoveryProvider(IEnumerable<IDiscoveryProvider> providers) : IDiscoveryProvider
{
    public int Priority => 1;

    public async Task<IEnumerable<MonitoredResource>> GetMonitoredResourcesAsync(CancellationToken cancellation)
    {
        var resourceDictionary = new Dictionary<string, MonitoredResource>();

        foreach (var provider in providers.OrderBy(p => p.Priority))
        {
            var resources = await provider.GetMonitoredResourcesAsync(cancellation);

            foreach (var resource in resources)
            {
                resourceDictionary[resource.KubePluralName + resource.KubeKind + resource.KubeGroup + resource.KubeApiVersion] = resource;
            }
        }

        return resourceDictionary.Values;
    }
}
