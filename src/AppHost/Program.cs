using Aspire.Hosting.Dapr;

var builder = DistributedApplication.CreateBuilder(args);

var rabbitmq = builder.AddRabbitMQ("rabbitmq");

DaprSidecarOptions awsSidecarOptions = new()
{
    AppId = "aws-agent",
    DaprGrpcPort = 50001,
    MetricsPort = 9090,
    LogLevel = "warn",
    EnableApiLogging = true,
};

var mongo = builder.AddMongoDB("mongo")
                   .WithLifetime(ContainerLifetime.Persistent);

var awsDB = builder.ExecutionContext.IsRunMode
    ? mongo.AddDatabase("Mongodb")
    : builder.AddConnectionString("Mongodb");

builder.AddProject<Projects.AwsAgent>("aws-agent")
.WithDaprSidecar(awsSidecarOptions)
.WithReference(awsDB)
.WaitFor(awsDB);

builder.AddProject<Projects.KubeAgent>("k8s")
.WithReference(rabbitmq)
.WaitFor(rabbitmq);

DaprSidecarOptions managerSidecarOptions = new()
{
    AppId = "resource-manager",
    DaprGrpcPort = 50001,
    AppPort = 5000,
    MetricsPort = 9090,
    LogLevel = "warn",
    EnableApiLogging = true,
};
builder.AddProject<Projects.ResourceManager>("resource-manager")
.WithDaprSidecar(managerSidecarOptions)
.WithReference(rabbitmq)
.WaitFor(rabbitmq);

builder.Build().Run();
