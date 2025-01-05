
using System.Threading.Channels;
using KubeAgent.V1.Monitor;

namespace KubeAgent.V1.Processor;

public class CustomResourceProcessor(ILogger<CustomResourceProcessor> logger, IKubernetes client, [FromKeyedServices("General")] IResourceProcessor processor) : BaseMonitor(logger, processor), IResourceProcessor
{
    readonly Channel<Resource> channel = Channel.CreateUnbounded<Resource>();
    readonly Type targetType = typeof(Monitor.GeneralCustomResource);

    public async Task CollectingData(Resource data, CancellationToken cancellation)
    {
        logger.LogInformation("collecting custom resource: {group} {apiVersion} {plural}", data.Group, data.ApiVersion, data.Plural);
        await channel.Writer.WriteAsync(data, cancellation);
    }

    public async Task Complete(CancellationToken cancellation)
    {
        channel.Writer.Complete();
        await channel.Reader.Completion;
    }

    public async Task ProcessingData(CancellationToken cancellation)
    {
        var batch = new List<Resource>();

        while (channel.Reader.TryRead(out var data))
        {
            batch.Add(data);
        }

        if (batch.Count > 0)
        {
            await ProcessBatch(batch, cancellation);
        }
    }

    private Task ProcessBatch(List<Resource> batch, CancellationToken cancellation)
    {
        foreach (var data in batch)
        {
            logger.LogInformation("processing custom resource: {group} {apiVersion} {plural}", data.Group, data.ApiVersion, data.Plural);
            var group = data.Group;
            var version = data.ApiVersion;
            var plural = data.Plural;

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
                            // this catch block is for the case when the deserialization fails.
                            logger.LogError("custom processor on event error: {error}", (ex.InnerException ?? ex).Message);
                        }
                    },
                    onError: (ex) =>
                    {
                        //TODO: handle 'Attempted to read past the end of the stream' error.
                        logger.LogError("custom processor error: {error}", (ex.InnerException ?? ex).Message);
                    },
                    onClosed: () =>
                    {
                        logger.LogInformation("closed processing custom resource: {group} {apiVersion} {plural}", data.Group, data.ApiVersion, data.Plural);
                    });
            }
            catch (Exception ex)
            {
                // Because there is no await ListClusterCustomObjectWithHttpMessagesAsync execution, it is difficult for the code to run into this catch
                logger.LogError("custom processor watcher error: {error}", (ex.InnerException ?? ex).Message);
            }
        }

        return Task.CompletedTask;
    }
}