
namespace KubeAgent.Services;

public class KubernetesClientProvider(IOptionsMonitor<KubernetesClientOptions> optionsMonitor) : IKubernetesClientProvider
{

    public Task<List<IKubernetes>> GetKubernetesClientsAsync(CancellationToken cancellation)
    {
        List<IKubernetes> k8sClients = [.. optionsMonitor.CurrentValue.Clients.Select(client =>
          {
              var kubernetesClientConfig = new KubernetesClientConfiguration
              {
                  Host = client.Host,
                  AccessToken = client.AccessToken,
                  ClientCertificateData = client.ClientCertificateData,
                  ClientCertificateKeyData = client.ClientCertificateKeyData
              };
              return (IKubernetes)new Kubernetes(kubernetesClientConfig);
          })];


        return Task.FromResult(k8sClients);
    }
}

public class KubernetesClientOptions
{
    public List<K8sClientConfig> Clients { get; set; } = [];
}

public class K8sClientConfig
{
    public string Host { get; set; } = null!;
    public string AccessToken { get; set; } = null!;
    public string ClientCertificateData { get; set; } = null!;
    public string ClientCertificateKeyData { get; set; } = null!;
}