
using System.Threading.Channels;

namespace KubeAgent.Processor;

public class CustomResourceProcessor(ILogger<CustomResourceProcessor> logger, IKubernetes client, [FromKeyedServices("Dataflow")] IResourceProcessor processor) : BaseMonitor(logger, processor), IResourceProcessor
{
    readonly Channel<Resource> channel = Channel.CreateUnbounded<Resource>();

    public async Task CollectingData(Resource data, CancellationToken cancellation)
    {
        Console.WriteLine($"Collecting data: {data.Group} {data.ApiVersion} {data.Plural}");
        await channel.Writer.WriteAsync(data, cancellation);
    }

    public async Task Complete(CancellationToken cancellation)
    {
        channel.Writer.Complete();
        await channel.Reader.Completion;
    }

    public Task ProcessingData(CancellationToken cancellation)
    {
        var batch = new List<Resource>();

        while (channel.Reader.TryRead(out var data))
        {
            Console.WriteLine($"Processing data: {data.Group} {data.ApiVersion} {data.Plural}");
            var group = data.Group;
            var version = data.ApiVersion;
            var plural = data.Plural;
            var targetType = typeof(GeneralCustomResource);

            try
            {
                var resources = client.CustomObjects.ListClusterCustomObjectWithHttpMessagesAsync(group, version, plural, watch: true, cancellationToken: cancellation);
                resources.Watch<object, object>(
                   onEvent: async (type, item) =>
                   {
                       try
                       {
                           var json = JsonSerializer.Serialize(item);
                           object? deserializedObject = JsonSerializer.Deserialize(json, targetType);

                           if (deserializedObject is IKubernetesObject<V1ObjectMeta> kubernetesObject)
                           {
                               await HandlerResourceChange(type, kubernetesObject, cancellation);
                           }
                       }
                       catch (Exception ex)
                       {
                           logger.LogError("crd processor error: {error}", (ex.InnerException ?? ex).Message);
                       }

                   },
                  onError: (ex) =>
                  {
                      Console.WriteLine($"Error: {group} {version} {plural} {ex.Message}");
                  },
                  onClosed: () =>
                  {
                      Console.WriteLine($"Closed: {group} {version} {plural} {targetType}");
                  });
            }
            catch (Exception ex)
            {
                logger.LogError("crd processor error: {error}", (ex.InnerException ?? ex).Message);
            }

        }

        return Task.CompletedTask;
    }
}