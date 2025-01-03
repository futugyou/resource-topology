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

        // builder.Services.AddHostedService<ProcessorWorker>();
        // builder.Services.AddHostedService<OfficialResourceWatchWorker>();
        // builder.Services.AddHostedService<CustomResourceWatchWorker>();

        // builder.Services.AddKeyedSingleton<IResourceProcessor, GeneralProcessor>("General");
        // builder.Services.AddKeyedSingleton<IResourceProcessor, CustomResourceProcessor>("Custom");

        // builder.Services.AddSingleton<IResourceMonitor, GeneralMonitorV2>();


        builder.Services.AddSingleton<IResourceDiscovery, ResourceDiscovery>();
        builder.Services.AddSingleton<AdditionDiscoveryProvider>();
        builder.Services.AddSingleton<IDiscoveryProvider, OptionDiscoveryProvider>();
        builder.Services.AddSingleton<IDiscoveryProvider>(sp => sp.GetRequiredService<AdditionDiscoveryProvider>());
        builder.Services.AddSingleton<IAdditionResourceProvider>(sp => sp.GetRequiredService<AdditionDiscoveryProvider>());

        builder.Services.AddSingleton<IRestartResourceTracker, RestartResourceTracker>();

        builder.Services.AddKeyedSingleton<ProcessorV2.IDataProcessor<Resource>, ProcessorV2.GeneralProcessor>("General");

        builder.Services.AddSingleton<MonitorV2.IResourceMonitor, MonitorV2.GeneralMonitor>();
        builder.Services.AddSingleton<MonitorV2.IResourceMonitorManager, MonitorV2.ResourceMonitorManager>();
        builder.Services.AddHostedService<WorkerV2.MonitorWorker>();
        builder.Services.AddHostedService<WorkerV2.ProcessorWorker>();

        return builder;
    }
}
