
namespace KubeAgent.Services;

public class KubernetesClientProvider(IOptionsMonitor<KubernetesClientOptions> optionsMonitor) : IKubernetesClientProvider
{
    readonly Dictionary<string, IKubernetes> k8sClients = [];

    public Task<Dictionary<string, IKubernetes>> GetKubernetesClientsAsync(CancellationToken cancellation)
    {
        foreach (var clientConfig in optionsMonitor.CurrentValue.Clients)
        {
            if (!k8sClients.ContainsKey(clientConfig.Alias))
            {
                var kubernetesClientConfig = new KubernetesClientConfiguration
                {
                    Host = clientConfig.Host,
                    AccessToken = clientConfig.AccessToken,
                    ClientCertificateData = clientConfig.ClientCertificateData,
                    ClientCertificateKeyData = clientConfig.ClientCertificateKeyData,
                    SkipTlsVerify = clientConfig.SkipTlsVerify,
                };
                k8sClients[clientConfig.Alias] = new Kubernetes(kubernetesClientConfig);
            }
        }

        return Task.FromResult(k8sClients);
    }

    public Task ReleaseClientAsync(string alias, CancellationToken cancellation)
    {
        if (k8sClients.Remove(alias, out IKubernetes? client))
        {
            client.Dispose();
        }

        return Task.CompletedTask;
    }
}

public class KubernetesClientOptions
{
    public List<K8sClientConfig> Clients { get; set; } = [];
}

public class K8sClientConfig
{
    public string Alias { get; set; } = null!;
    public string Host { get; set; } = null!;
    public string AccessToken { get; set; } = null!;
    public string ClientCertificateData { get; set; } = null!;
    public string ClientCertificateKeyData { get; set; } = null!;
    public bool SkipTlsVerify { get; set; } = false;
}