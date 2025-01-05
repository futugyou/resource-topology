namespace KubeAgent.Extensions;

public static class Extensions
{
    public static IHostApplicationBuilder AddApplicationServices(this IHostApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.Services.AddOptions<AgentOptions>().BindConfiguration(nameof(AgentOptions));
        builder.Services.AddOptions<MonitorSetting>().Bind(builder.Configuration.GetSection(nameof(MonitorSetting)));

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

        builder.Services.AddSingleton<IResourceDiscovery, ResourceDiscovery>();
        builder.Services.AddSingleton<AdditionDiscoveryProvider>();
        builder.Services.AddSingleton<IDiscoveryProvider, OptionDiscoveryProvider>();
        builder.Services.AddSingleton<IDiscoveryProvider>(sp => sp.GetRequiredService<AdditionDiscoveryProvider>());
        builder.Services.AddSingleton<IAdditionResourceProvider>(sp => sp.GetRequiredService<AdditionDiscoveryProvider>());

        builder.Services.AddSingleton<IRestartResourceTracker, RestartResourceTracker>();

        builder.Services.AddKeyedSingleton<IDataProcessor<Resource>, GeneralProcessor>("General");

        builder.Services.AddSingleton<IResourceMonitor, GeneralMonitor>();
        builder.Services.AddSingleton<IResourceMonitorManager, ResourceMonitorManager>();
        builder.Services.AddHostedService<MonitorWorker>();
        builder.Services.AddHostedService<ProcessorWorker>();

        return builder;
    }
}
