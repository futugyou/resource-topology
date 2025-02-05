
namespace KubeAgent.Extensions;

public static class Extensions
{
    public static IHostApplicationBuilder AddApplicationServices(this IHostApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        #region NServiceBus
        var endpointConfiguration = new EndpointConfiguration("kube-agent");
        endpointConfiguration.EnableOpenTelemetry();
        endpointConfiguration.EnableOutbox();
        #endregion

        builder.AddServiceDefaults();

        #region NServiceBus
        var connectionString = builder.Configuration.GetConnectionString("rabbitmq");
        var transport = new RabbitMQTransport(RoutingTopology.Conventional(QueueType.Quorum), connectionString)
        {
            TransportTransactionMode = TransportTransactionMode.ReceiveOnly
        };

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

        builder.Services.AddSingleton<ISerializer, JsonSerializerService>();
        builder.Services.AddSingleton<IRestartResourceTracker, RestartResourceTracker>();

        #region Resource Discovery
        builder.Services.AddOptions<ResourcesSetting>().Bind(builder.Configuration.GetSection(nameof(ResourcesSetting)));
        builder.Services.AddSingleton<IResourceDiscovery, ResourceDiscovery>();
        builder.Services.AddSingleton<AdditionDiscoveryProvider>();
        builder.Services.AddSingleton<IDiscoveryProvider, OptionDiscoveryProvider>();
        builder.Services.AddSingleton<IDiscoveryProvider>(sp => sp.GetRequiredService<AdditionDiscoveryProvider>());
        builder.Services.AddSingleton<IAdditionResourceProvider>(sp => sp.GetRequiredService<AdditionDiscoveryProvider>());
        #endregion

        #region Resource processor
        builder.Services.AddKeyedSingleton<IDataProcessor<Resource>, GeneralResourceProcessor>("General");
        builder.Services.AddHostedService<ProcessorWorker>();
        #endregion

        #region Resource Monitor
        builder.Services.AddOptions<MonitorOptions>().Bind(builder.Configuration.GetSection(nameof(MonitorOptions)));
        builder.Services.AddSingleton<IResourceMonitor, GeneralMonitor>();
        builder.Services.AddSingleton<IResourceMonitorManager, ResourceMonitorManager>();
        builder.Services.AddHostedService<MonitorWorker>();
        #endregion

        #region k8s client provider
        builder.Services.AddOptions<KubernetesClientOptions>().BindConfiguration(nameof(KubernetesClientOptions));
        builder.Services.AddSingleton<IKubernetesClientProvider, KubernetesClientProvider>();
        #endregion

        #region event publisher
        builder.Services.AddKeyedSingleton<IPublisher, NServiceBusPublisher>("NServiceBus");
        builder.Services.AddKeyedSingleton<IPublisher, DaprPublisher>("Dapr");
        builder.Services.AddOptions<PublisherOption>().BindConfiguration(nameof(PublisherOption));
        builder.Services.AddSingleton<PublisherFactory>();
        #endregion

        return builder;
    }
}
