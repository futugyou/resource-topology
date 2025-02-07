using CommunityToolkit.Aspire.Hosting.Dapr;
using Microsoft.Extensions.Configuration;

var builder = DistributedApplication.CreateBuilder(args);
var daprSupported = builder.Configuration.GetValue<string>("DaprSupported");

var rabbitmq = builder.AddRabbitMQ("rabbitmq");

var mongo = builder.AddMongoDB("mongo")
                   .WithLifetime(ContainerLifetime.Persistent);

var awsDB = builder.ExecutionContext.IsRunMode
    ? mongo.AddDatabase("Mongodb")
    : builder.AddConnectionString("Mongodb");

var awsProject = builder.AddProject<Projects.AwsAgent>("aws-agent")
.WithReference(awsDB)
.WaitFor(awsDB)
.WithEnvironment("DaprSupported", daprSupported);

DaprSidecarOptions awsSidecarOptions = new()
{
    AppId = "aws-agent",
    DaprGrpcPort = 50001,
    MetricsPort = 9090,
    LogLevel = "warn",
    EnableApiLogging = true,
};

if (daprSupported == "true")
{
    var awsAgentConfig = builder.AddDaprComponent("aws-agent-config", "configuration");
    var awsAgentSecret = builder.AddDaprComponent("aws-agent-secret", "secretstores");
    var awsAgentState = builder.AddDaprStateStore("aws-agent-state");
    var resourceAgent = builder.AddDaprPubSub("resource-agent");

    awsProject.WithDaprSidecar(awsSidecarOptions)
       .WithReference(awsAgentConfig)
       .WithReference(awsAgentSecret)
       .WithReference(awsAgentState)
       .WithReference(resourceAgent);
}

var kubeProject = builder.AddProject<Projects.KubeAgent>("k8s")
.WithEnvironment("DaprSupported", daprSupported);

DaprSidecarOptions kubeSidecarOptions = new()
{
    AppId = "kube-agent",
    DaprGrpcPort = 50001,
    MetricsPort = 9090,
    LogLevel = "warn",
    EnableApiLogging = true,
};

if (daprSupported == "true")
{
    kubeProject.WithDaprSidecar(kubeSidecarOptions);
}
else
{
    kubeProject.WithReference(rabbitmq).WaitFor(rabbitmq);
}


var managerProject = builder.AddProject<Projects.ResourceManager>("resource-manager")
.WithReference(rabbitmq)
.WaitFor(rabbitmq)
.WithEnvironment("DaprSupported", daprSupported);

DaprSidecarOptions managerSidecarOptions = new()
{
    AppId = "resource-manager",
    DaprGrpcPort = 50001,
    AppPort = 5000,
    MetricsPort = 9090,
    LogLevel = "warn",
    EnableApiLogging = true,
};

if (daprSupported == "true")
{
    managerProject.WithDaprSidecar(managerSidecarOptions);
}

builder.Build().Run();
