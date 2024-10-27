using Infrastructure;
using ResourceAdapter;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.ServiceRegistration();
builder.Services.ResourceAdapterRegistration();
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
