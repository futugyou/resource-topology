
namespace KubeAgent.Extensions;

public static class Extensions
{
    public static IHostApplicationBuilder AddApplicationServices(this IHostApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        #region NServiceBus
        var endpointConfiguration = new EndpointConfiguration("kube-agent");
        endpointConfiguration.EnableOpenTelemetry();
        #endregion

        builder.AddServiceDefaults();

        #region NServiceBus
        var connectionString = builder.Configuration.GetConnectionString("rabbitmq");
        var transport = new RabbitMQTransport(RoutingTopology.Conventional(QueueType.Quorum), connectionString);

        var routing = endpointConfiguration.UseTransport(transport);

        var persistenceConnection = builder.Configuration.GetConnectionString("Mongodb");
        var persistence = endpointConfiguration.UsePersistence<MongoPersistence>();
        persistence.DatabaseName("kube-agent");
        persistence.MongoClient(new MongoClient(persistenceConnection));

        endpointConfiguration.UseSerialization<SystemJsonSerializer>();
        endpointConfiguration.EnableInstallers();
        builder.UseNServiceBus(endpointConfiguration);
        #endregion

        builder.Services.AddAutoMapper(typeof(Program));
        AutoMapperConfig.RegisterMapper();

        builder.Services.AddOptions<AgentOptions>().BindConfiguration(nameof(AgentOptions));
        builder.Services.AddOptions<ResourcesSetting>().Bind(builder.Configuration.GetSection(nameof(ResourcesSetting)));
        builder.Services.AddOptions<MonitorOptions>().Bind(builder.Configuration.GetSection(nameof(MonitorOptions)));
        builder.Services.AddOptions<KubernetesClientOptions>().BindConfiguration(nameof(KubernetesClientOptions));

        builder.Services.AddSingleton<ISerializer, JsonSerializerService>();

        builder.Services.AddSingleton<IResourceDiscovery, ResourceDiscovery>();
        builder.Services.AddSingleton<AdditionDiscoveryProvider>();
        builder.Services.AddSingleton<IDiscoveryProvider, OptionDiscoveryProvider>();
        builder.Services.AddSingleton<IDiscoveryProvider>(sp => sp.GetRequiredService<AdditionDiscoveryProvider>());
        builder.Services.AddSingleton<IAdditionResourceProvider>(sp => sp.GetRequiredService<AdditionDiscoveryProvider>());

        builder.Services.AddSingleton<IRestartResourceTracker, RestartResourceTracker>();

        builder.Services.AddKeyedSingleton<IDataProcessor<Resource>, GeneralResourceProcessor>("General");

        builder.Services.AddSingleton<IResourceMonitor, GeneralMonitor>();
        builder.Services.AddSingleton<IResourceMonitorManager, ResourceMonitorManager>();
        builder.Services.AddHostedService<MonitorWorker>();
        builder.Services.AddHostedService<ProcessorWorker>();

        builder.Services.AddSingleton<IKubernetesClientProvider, KubernetesClientProvider>();

        return builder;
    }
}
