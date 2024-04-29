using Microsoft.Extensions.Logging;
using Viam.Core.Resources;

namespace Viam.Core.Logging
{
    internal static partial class LogMessages
    {
        [LoggerMessage(EventId = 1000, Message = "Calling {Method} with ({Parameters})", Level = LogLevel.Trace)]
        internal static partial void LogMethodInvocationStart(this ILogger logger, string method, object?[] parameters);

        [LoggerMessage(EventId = 1001, Message = "Done executing {Method} with ({Parameters})", Level = LogLevel.Trace)]
        internal static partial void LogMethodInvocationFinish(this ILogger logger, string method, object?[] parameters);

        [LoggerMessage(EventId = 1002, Message = "Getting resource by {SubType} in registry", Level = LogLevel.Debug)]
        internal static partial void LogRegistryResourceGet(this ILogger logger, SubType subType);

        [LoggerMessage(EventId = 1003, Message = "Found resource by {SubType} in registry", Level = LogLevel.Debug)]
        internal static partial void LogRegistryResourceGetSuccess(this ILogger logger, SubType subType);

        [LoggerMessage(EventId = 1004, Message = "Did not find resource by {SubType} in registry", Level = LogLevel.Debug)]
        internal static partial void LogRegistryResourceGetFailure(this ILogger logger, SubType subType);

        [LoggerMessage(EventId = 1005, Message = "Getting ResourceCreator by {SubType} and {Model} in registry", Level = LogLevel.Debug)]
        internal static partial void LogRegistryResourceCreatorGet(this ILogger logger, SubType subType, Model model);

        [LoggerMessage(EventId = 1006, Message = "Found ResourceCreator by {SubType} and {Model} in registry", Level = LogLevel.Debug)]
        internal static partial void LogRegistryResourceCreatorGetSuccess(this ILogger logger, SubType subType, Model model);

        [LoggerMessage(EventId = 1007, Message = "Did not find ResourceCreator by {SubType} and {Model} in registry", Level = LogLevel.Debug)]
        internal static partial void LogRegistryResourceCreatorGetFailure(this ILogger logger, SubType subType, Model model);

        [LoggerMessage(EventId = 2000, Message = "Registering Resource {ResourceName} {Resource} from {Caller} in manager", Level = LogLevel.Debug)]
        internal static partial void LogManagerResourceRegistration(this ILogger logger,
                                                             ViamResourceName resourceName,
                                                             IResourceBase resource,
                                                             string? caller = null);

        [LoggerMessage(EventId = 2001, Message = "Getting Resource {ResourceName} from {Caller} in manager", Level = LogLevel.Debug)]
        internal static partial void LogManagerResourceGet(this ILogger logger, ViamResourceName resourceName, string? caller = null);

        [LoggerMessage(EventId = 2002, Message = "Found Resource {ResourceName} {Resource} from {Caller} in manager", Level = LogLevel.Debug)]
        internal static partial void LogManagerResourceGetSuccess(this ILogger logger, ViamResourceName resourceName, IResourceBase resource, string? caller = null);

        [LoggerMessage(EventId = 2003, Message = "Did not find Resource {ResourceName} from {Caller} in manager", Level = LogLevel.Debug)]
        internal static partial void LogManagerResourceGetFailure(this ILogger logger, ViamResourceName resourceName, string? caller = null);

        [LoggerMessage(EventId = 2004, Message = "Getting Resource {ResourceName} from {Caller} in manager", Level = LogLevel.Debug)]
        internal static partial void LogManagerResourceGetByShortName(this ILogger logger, string resourceName, string? caller = null);

        [LoggerMessage(EventId = 2005, Message = "Found Resource {ResourceName} {Resource} from {Caller} in manager", Level = LogLevel.Debug)]
        internal static partial void LogManagerResourceGetByShortNameSuccess(this ILogger logger, string resourceName, IResourceBase resource, string? caller = null);

        [LoggerMessage(EventId = 2006, Message = "Did not find Resource {ResourceName} from {Caller} in manager", Level = LogLevel.Debug)]
        internal static partial void LogManagerResourceGetByShortNameFailure(this ILogger logger, string resourceName, string? caller = null);

        [LoggerMessage(EventId = 2007, Message = "Removing Resource {ResourceName} from {Caller} in manager", Level = LogLevel.Debug)]
        internal static partial void LogManagerResourceRemove(this ILogger logger, ViamResourceName resourceName, string? caller = null);

        [LoggerMessage(EventId = 2008, Message = "Removed Resource {ResourceName} from {Caller} in manager", Level = LogLevel.Debug)]
        internal static partial void LogManagerResourceRemoveSuccess(this ILogger logger, ViamResourceName resourceName, string? caller = null);

        [LoggerMessage(EventId = 2009, Message = "Could not remove Resource {ResourceName} from {Caller} in manager", Level = LogLevel.Debug)]
        internal static partial void LogManagerResourceRemoveFailure(this ILogger logger, ViamResourceName resourceName, string? caller = null);

        [LoggerMessage(EventId = 2010, Message = "Refreshing resources from {Caller} in manager", Level = LogLevel.Debug)]
        internal static partial void LogManagerRefreshStart(this ILogger logger, string? caller = null);

        [LoggerMessage(EventId = 2011, Message = "Done refreshing resources from {Caller} in manager", Level = LogLevel.Debug)]
        internal static partial void LogManagerRefreshFinish(this ILogger logger, string? caller = null);

        [LoggerMessage(EventId = 2012, Message = "Refreshing client for {ResourceName}@{Channel} in manager", Level = LogLevel.Debug)]
        internal static partial void LogManagerCreateOrRefreshClient(this ILogger logger,
                                                                     ViamResourceName resourceName,
                                                                     ViamChannel channel);

        [LoggerMessage(EventId = 2013, Message = "Disposing manager", Level = LogLevel.Debug)]
        internal static partial void LogManagerDispose(this ILogger logger);
    }
}
