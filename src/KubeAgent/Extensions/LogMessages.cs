
namespace Microsoft.Extensions.Logging;

public static partial class LogMessages
{

    [LoggerMessage(EventId = 1001, Message = "kube processor worker running at: {time}", Level = LogLevel.Information)]
    public static partial void ProcessorWorkerRunning(this ILogger logger, string time);

    [LoggerMessage(EventId = 1002, Message = "kube processor worker error at: {time} ", Level = LogLevel.Error)]
    public static partial void ProcessorWorkerError(this ILogger logger, string time);

    [LoggerMessage(EventId = 1003, Message = "kube processor worker end at: {time} ", Level = LogLevel.Information)]
    public static partial void ProcessorWorkerEnd(this ILogger logger, string time);

    [LoggerMessage(EventId = 1004, Message = "kube processor worker stop at: {time} ", Level = LogLevel.Warning)]
    public static partial void ProcessorWorkerStop(this ILogger logger, string time);


    [LoggerMessage(EventId = 2001, Message = "monitor worker running at: {time}", Level = LogLevel.Information)]
    public static partial void MonitorWorkerRunning(this ILogger logger, string time);

    [LoggerMessage(EventId = 2002, Message = "monitor worker error at: {time} ", Level = LogLevel.Error)]
    public static partial void MonitorWorkerError(this ILogger logger, string time);

    [LoggerMessage(EventId = 2003, Message = "monitor worker end at: {time} ", Level = LogLevel.Information)]
    public static partial void MonitorWorkerEnd(this ILogger logger, string time);

    [LoggerMessage(EventId = 2004, Message = "monitor worker stop at: {time} ", Level = LogLevel.Warning)]
    public static partial void MonitorWorkerStop(this ILogger logger, string time);

}
