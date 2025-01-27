
namespace KubeAgent.Extensions;

public static class Extensions
{
    public static IHostApplicationBuilder AddApplicationServices(this IHostApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.Services.AddAutoMapper(typeof(Program));
        AutoMapperConfig.RegisterMapper();

        builder.Services.AddOptions<AgentOptions>().BindConfiguration(nameof(AgentOptions));
        builder.Services.AddOptions<ResourcesSetting>().Bind(builder.Configuration.GetSection(nameof(ResourcesSetting)));
        builder.Services.AddOptions<MonitorOptions>().Bind(builder.Configuration.GetSection(nameof(MonitorOptions)));
        builder.Services.AddOptions<KubernetesClientOptions>().BindConfiguration(nameof(KubernetesClientOptions));

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

        builder.Services.AddSingleton<ISerializer, JsonSerializerService>();

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

        builder.Services.AddSingleton<IKubernetesClientProvider, KubernetesClientProvider>();

        return builder;
    }
}
