namespace KubeAgent.Extensions;

public static class Extensions
{
    public static IHostApplicationBuilder AddApplicationServices(this IHostApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.Services.AddOptions<AgentOptions>().BindConfiguration(nameof(AgentOptions));

        builder.Services.AddSingleton<IKubernetes>(sp =>
        {
            var optionsMonitor = sp.GetRequiredService<IOptionsMonitor<AgentOptions>>();
            var serviceOption = optionsMonitor.CurrentValue!;
            KubernetesClientConfiguration kubernetesClientConfig;
            if (File.Exists(serviceOption.KubeconfigPath))
            {
                kubernetesClientConfig = KubernetesClientConfiguration.BuildConfigFromConfigFile(serviceOption.KubeconfigPath);
            }
            else
            {
                kubernetesClientConfig = KubernetesClientConfiguration.BuildDefaultConfig();
            }

            return new Kubernetes(kubernetesClientConfig);
        });

        builder.Services.AddHostedService<Worker>();
        builder.Services.AddHostedService<WatchWorker>();

        return builder;
    }
}
