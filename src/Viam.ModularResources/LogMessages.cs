namespace Viam.ModularResources
{
    internal static partial class LogMessages
    {
        [LoggerMessage(Message = "Calling {Method} from {CallerMethod} with {Parameters}", Level = LogLevel.Trace)]
        internal static partial void LogMethodInvocationImpl(this ILogger logger, string method, string? callerMethod, object?[] parameters);

        internal static void LogMethodInvocation(this ILogger logger,
                                                 string method,
                                                 string? callerMethod,
                                                 params object?[] parameters) =>
            LogMethodInvocationImpl(logger, method, callerMethod, parameters);

    }
}
