
using ResourceContracts;

class ResourceProcessorEventHandler : IHandleMessages<ResourceProcessorEvent>
{
    public Task Handle(ResourceProcessorEvent message, IMessageHandlerContext context)
    {
        Console.WriteLine($"event [{message.EventID}] - Successfully got.");
        foreach (var resource in message.InsertResources)
        {
            Console.WriteLine($"Insert resource [{resource.ResourceName}]");
        }
        return Task.CompletedTask;
    }
}