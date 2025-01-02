using KubeAgent.MonitorV2;
using KubeAgent.WorkerV2;

namespace KubeAgent.Logging;

public static class LogMessages
{
    public static readonly Action<ILogger, DateTimeOffset, Exception?> ProcessorWorkerRunning = LoggerMessage.Define<DateTimeOffset>(
        LogLevel.Information, new EventId(1, nameof(WorkerV2.ProcessorWorker)), "kube processor worker running at: {time}");
    public static readonly Action<ILogger, DateTimeOffset, string, Exception?> ProcessorWorkerError = LoggerMessage.Define<DateTimeOffset, string>(
        LogLevel.Error, new EventId(2, nameof(WorkerV2.ProcessorWorker)), "kube processor worker running at: {time}, and get an error: {error}");
    public static readonly Action<ILogger, DateTimeOffset, Exception?> ProcessorWorkerEnd = LoggerMessage.Define<DateTimeOffset>(
        LogLevel.Information, new EventId(3, nameof(WorkerV2.ProcessorWorker)), "kube processor worker end at: {time}");
    public static readonly Action<ILogger, DateTimeOffset, Exception?> ProcessorWorkerStop = LoggerMessage.Define<DateTimeOffset>(
        LogLevel.Information, new EventId(4, nameof(WorkerV2.ProcessorWorker)), "kube processor worker stop at: {time}");

    public static readonly Action<ILogger, DateTimeOffset, Exception?> MonitorWorkerRunning = LoggerMessage.Define<DateTimeOffset>(
        LogLevel.Information, new EventId(1, nameof(MonitorWorker)), "resource worker running at: {time}");
    public static readonly Action<ILogger, DateTimeOffset, string, Exception?> MonitorWorkerError = LoggerMessage.Define<DateTimeOffset, string>(
        LogLevel.Error, new EventId(2, nameof(MonitorWorker)), "resource worker running at: {time}, and get an error: {error}");
    public static readonly Action<ILogger, DateTimeOffset, Exception?> MonitorWorkerEnd = LoggerMessage.Define<DateTimeOffset>(
        LogLevel.Information, new EventId(3, nameof(MonitorWorker)), "resource worker end at: {time}");
    public static readonly Action<ILogger, DateTimeOffset, Exception?> MonitorWorkerStop = LoggerMessage.Define<DateTimeOffset>(
        LogLevel.Information, new EventId(4, nameof(MonitorWorker)), "resource worker stop at: {time}");

    public static readonly Action<ILogger, int, Exception?> GeneralProcessorProcessingBatch = LoggerMessage.Define<int>(
        LogLevel.Information, new EventId(1, nameof(GeneralProcessor)), "processing batch with {count} items.");
    public static readonly Action<ILogger, string, string, string, Exception?> GeneralProcessorHandlingResource = LoggerMessage.Define<string, string, string>(
        LogLevel.Information, new EventId(2, nameof(GeneralProcessor)), "resource processor handling: {kind} {name} {operate}");

    public static readonly Action<ILogger, string, string, string, Exception?> CustomResourceProcessorCollectingResource = LoggerMessage.Define<string, string, string>(
        LogLevel.Information, new EventId(1, nameof(CustomResourceProcessor)), "collecting custom resource: {group} {apiVersion} {plural}");
    public static readonly Action<ILogger, string, string, string, Exception?> CustomResourceProcessorProcessingResource = LoggerMessage.Define<string, string, string>(
        LogLevel.Information, new EventId(2, nameof(CustomResourceProcessor)), "processing custom resource: {group} {apiVersion} {plural}");
    public static readonly Action<ILogger, string, Exception?> CustomResourceProcessorOnEventError = LoggerMessage.Define<string>(
        LogLevel.Error, new EventId(3, nameof(CustomResourceProcessor)), "custom processor on event error: {error}");
    public static readonly Action<ILogger, string, Exception?> CustomResourceProcessorError = LoggerMessage.Define<string>(
        LogLevel.Error, new EventId(4, nameof(CustomResourceProcessor)), "custom processor error: {error}");
    public static readonly Action<ILogger, string, string, string, Exception?> CustomResourceProcessorClosed = LoggerMessage.Define<string, string, string>(
        LogLevel.Information, new EventId(5, nameof(CustomResourceProcessor)), "closed processing custom resource: {group} {apiVersion} {plural}");
    public static readonly Action<ILogger, string, Exception?> CustomResourceProcessorWatcherError = LoggerMessage.Define<string>(
        LogLevel.Error, new EventId(6, nameof(CustomResourceProcessor)), "custom processor watcher error: {error}");

    public static readonly Action<ILogger, string, Exception?> ResourceMonitorManagerStopMonitoring = LoggerMessage.Define<string>(
        LogLevel.Information, new EventId(1, nameof(ResourceMonitorManager)), "stop monitoring: {resourceId}");

    public static readonly Action<ILogger, string, string, string, Exception?> GeneralMonitorClosed = LoggerMessage.Define<string, string, string>(
        LogLevel.Information, new EventId(1, nameof(MonitorV2.GeneralMonitor)), "closed: {group} {version} {plural}");
    public static readonly Action<ILogger, string, Exception?> GeneralMonitorStopMonitoring = LoggerMessage.Define<string>(
        LogLevel.Information, new EventId(2, nameof(MonitorV2.GeneralMonitor)), "stop monitoring: {resourceId}");
    public static readonly Action<ILogger, string, string, string, Exception?> GeneralMonitorEvent = LoggerMessage.Define<string, string, string>(
        LogLevel.Information, new EventId(3, nameof(MonitorV2.GeneralMonitor)), "event: {type} {kind} {name}");
    public static readonly Action<ILogger, string, string, Exception?> GeneralMonitorOnEventError =
    LoggerMessage.Define<string, string>(
        LogLevel.Error,
        new EventId(4, nameof(MonitorV2.GeneralMonitor)),
        "OnEvent error: {resource} {error}");

}
