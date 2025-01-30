
namespace Microsoft.Extensions.Logging;

public static partial class LogMessages
{
    #region ProcessorWorker
    [LoggerMessage(EventId = 1001, Message = "processor worker running at: {time}", Level = LogLevel.Information)]
    public static partial void ProcessorWorkerRunning(this ILogger logger, DateTimeOffset time);

    [LoggerMessage(EventId = 1002, Message = "processor worker error at: {time} ", Level = LogLevel.Error)]
    public static partial void ProcessorWorkerError(this ILogger logger, DateTimeOffset time, Exception ex);

    [LoggerMessage(EventId = 1003, Message = "processor worker end at: {time} ", Level = LogLevel.Information)]
    public static partial void ProcessorWorkerEnd(this ILogger logger, DateTimeOffset time);

    [LoggerMessage(EventId = 1004, Message = "processor worker stop at: {time} ", Level = LogLevel.Warning)]
    public static partial void ProcessorWorkerStop(this ILogger logger, DateTimeOffset time);
    #endregion

    #region MonitorWorker
    [LoggerMessage(EventId = 2001, Message = "monitor worker running at: {time}", Level = LogLevel.Information)]
    public static partial void MonitorWorkerRunning(this ILogger logger, DateTimeOffset time);

    [LoggerMessage(EventId = 2002, Message = "monitor worker error at: {time} ", Level = LogLevel.Error)]
    public static partial void MonitorWorkerError(this ILogger logger, DateTimeOffset time, Exception ex);

    [LoggerMessage(EventId = 2003, Message = "monitor worker end at: {time} ", Level = LogLevel.Information)]
    public static partial void MonitorWorkerEnd(this ILogger logger, DateTimeOffset time);

    [LoggerMessage(EventId = 2004, Message = "monitor worker stop at: {time} ", Level = LogLevel.Warning)]
    public static partial void MonitorWorkerStop(this ILogger logger, DateTimeOffset time);
    #endregion

    #region Addition Monitor Resource
    [LoggerMessage(EventId = 3001, Message = "addition monitor resource {id} have been added", Level = LogLevel.Information)]
    public static partial void AdditionResourceAdded(this ILogger logger, string id);
    #endregion

    #region Monitor
    [LoggerMessage(EventId = 4001, Message = "monitor resource {id} have been added", Level = LogLevel.Information)]
    public static partial void MonitorAdded(this ILogger logger, string id);

    [LoggerMessage(EventId = 4002, Message = "k8s watcher onError {id}, trigger restart", Level = LogLevel.Error)]
    public static partial void MonitorReceiveError(this ILogger logger, string id, Exception ex);

    [LoggerMessage(EventId = 4003, Message = "k8s watcher onEvent error {id}, WatchEventType Error", Level = LogLevel.Error)]
    public static partial void MonitorOnEventError(this ILogger logger, string id);

    [LoggerMessage(EventId = 4004, Message = "k8s watcher onEvent error {id}, watching type is not a  IKubernetesObject<V1ObjectMeta>", Level = LogLevel.Error)]
    public static partial void MonitorOnEventTypeError(this ILogger logger, string id);

    [LoggerMessage(EventId = 4005, Message = "k8s watcher handling onEvent error {id}", Level = LogLevel.Error)]
    public static partial void MonitorOnEventHandlingError(this ILogger logger, string id, Exception ex);

    [LoggerMessage(EventId = 4006, Message = "k8s watcher processing {id} {type}", Level = LogLevel.Information)]
    public static partial void MonitorOnEventProcessing(this ILogger logger, string id, WatchEventType type);

    [LoggerMessage(EventId = 4007, Message = "k8s watcher timeout trigger restart {id}", Level = LogLevel.Warning)]
    public static partial void MonitorTimeout(this ILogger logger, string id);

    [LoggerMessage(EventId = 4008, Message = "k8s watcher onClosed {id}", Level = LogLevel.Warning)]
    public static partial void MonitorOnClosed(this ILogger logger, string id);

    [LoggerMessage(EventId = 4009, Message = "k8s watcher stoped {id}", Level = LogLevel.Warning)]
    public static partial void MonitorStoped(this ILogger logger, string id);

    [LoggerMessage(EventId = 4010, Message = "can not found '{name}' cluster in k8s client provider", Level = LogLevel.Warning)]
    public static partial void ClientNotFound(this ILogger logger, string name);

    [LoggerMessage(EventId = 4011, Message = "k8s watcher {id} already exist", Level = LogLevel.Warning)]
    public static partial void MonitorAlreadyExist(this ILogger logger, string id);
    #endregion

    #region Resource Processor
    [LoggerMessage(EventId = 5001, Message = "processing batch with {count} items", Level = LogLevel.Information)]
    public static partial void ProcessBatch(this ILogger logger, int count);

    [LoggerMessage(EventId = 5002, Message = "error during async disposal", Level = LogLevel.Error)]
    public static partial void ProcessDisposeError(this ILogger logger, Exception ex);
    #endregion
}
