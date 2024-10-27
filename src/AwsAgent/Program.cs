using Infrastructure;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.ServiceRegistration();
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
