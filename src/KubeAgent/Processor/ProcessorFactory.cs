namespace KubeAgent.Processor;

// Factory and all processors are Singleton.
// Use IOptions that means ProcessorType can NOT change at runtime.
// If ProcessorType not set, choose 'Channel'
public class ProcessorFactory([FromKeyedServices("Dataflow")] IResourceProcessor flow, [FromKeyedServices("Channel")] IResourceProcessor chan, IOptions<AgentOptions> options)
{
    readonly AgentOptions agentOptions = options.Value;

    public IResourceProcessor GetResourceProcessor() => agentOptions.ProcessorType == "Channel" ? chan : flow;
}