using System;
using System.Runtime.CompilerServices;

using Microsoft.Extensions.Logging;

using Viam.Common.V1;
using Viam.Core.Grpc;
using Viam.Core.Resources;
using Viam.Core.Utils;

namespace Viam.Core.Logging
{
    internal static partial class LogMessages
    {
        /*
         * 1000 - Method Invocation Logging
         * 1100 - Registry Logging
         * 1200 - ResourceManager Logging
         * 2000 - Communications Logging
         * 3000 - ModularResources Logging (Found in Viam.ModularResources)
         * 4000 - Client Method Logging
         *      4000 - Robot Client
         *      4100 - Arm Client
         *      4100 - Base Client
         *      4200 - Board Client
         *      4300 - Camera Client
         *      4400 - Encoder Client
         *      4500 - Gantry Client
         *      4600 - Generic Client
         *      4700 - Gripper Client
         *      4800 - InputController Client
         *      4900 - Motor Client
         *      5000 - MovementSensor Client
         *      5100 - PowerSensor Client
         *      5200 - Sensor Client
         *      5300 - Servo Client
         *      5400 - App Client
         *  
         */

        #region Method Invocation Logging
        [LoggerMessage(EventId = 1000, Message = "Invoking {Method} with {Parameters}", Level = LogLevel.Trace, SkipEnabledCheck = true)]
        private static partial void LogMethodInvocationStartImpl(this ILogger logger, string method, string parameters);

        // We separate the caller from the implementation to explicitly guard against unnecessary calls to .ToLogParameters() when the logger isn't enabled
        internal static void LogMethodInvocationStart(this ILogger logger,
                                                              [CallerMemberName] string method = "unknown",
                                                              params object?[]? parameters)
        {
            if (logger.IsEnabled(LogLevel.Trace))
            {
                logger.LogMethodInvocationStartImpl(method, parameters.ToLogFormat());
            }
        }

        [LoggerMessage(EventId = 1001, Message = "Done executing {Method}. Results {Results}", Level = LogLevel.Trace)]
        private static partial void LogMethodInvocationSuccessImpl(this ILogger logger, string method, string results);
        
        // We separate the caller from the implementation to explicitly guard against unnecessary calls to .ToLogParameters() when the logger isn't enabled
        internal static void LogMethodInvocationSuccess(this ILogger logger,
                                                        [CallerMemberName] string method = "unknown",
                                                        params object?[]? results)
        {
            if (logger.IsEnabled(LogLevel.Trace))
            {
                logger.LogMethodInvocationSuccessImpl(method, results.ToLogFormat());
            }
        }

        [LoggerMessage(EventId = 1002, Message = "An exception occurred while executing {Method}", Level = LogLevel.Debug)]
        internal static partial void LogMethodInvocationFailure(this ILogger logger, Exception? exception = null, [CallerMemberName] string method = "unknown");
        #endregion

        #region Registry Logging
        [LoggerMessage(EventId = 1100, Message = "Getting resource by {SubType} in registry", Level = LogLevel.Debug)]
        internal static partial void LogRegistryResourceGet(this ILogger logger, SubType subType);

        [LoggerMessage(EventId = 1101, Message = "Found resource by {SubType} in registry", Level = LogLevel.Debug)]
        internal static partial void LogRegistryResourceGetSuccess(this ILogger logger, SubType subType);

        [LoggerMessage(EventId = 1102, Message = "Did not find resource by {SubType} in registry", Level = LogLevel.Debug)]
        internal static partial void LogRegistryResourceGetFailure(this ILogger logger, SubType subType);

        [LoggerMessage(EventId = 1103, Message = "Getting ResourceCreator by {SubType} and {Model} in registry", Level = LogLevel.Debug)]
        internal static partial void LogRegistryResourceCreatorGet(this ILogger logger, SubType subType, Model model);

        [LoggerMessage(EventId = 1104, Message = "Found ResourceCreator by {SubType} and {Model} in registry", Level = LogLevel.Debug)]
        internal static partial void LogRegistryResourceCreatorGetSuccess(this ILogger logger, SubType subType, Model model);

        [LoggerMessage(EventId = 1105, Message = "Did not find ResourceCreator by {SubType} and {Model} in registry", Level = LogLevel.Debug)]
        internal static partial void LogRegistryResourceCreatorGetFailure(this ILogger logger, SubType subType, Model model);
        #endregion

        #region Resource Manager Logging
        [LoggerMessage(EventId = 1200, Message = "Registering Resource {ResourceName} {Resource} from {Caller} in manager", Level = LogLevel.Debug)]
        internal static partial void LogManagerResourceRegistration(this ILogger logger, ViamResourceName resourceName, IResourceBase resource, string? caller = null);

        [LoggerMessage(EventId = 1201, Message = "Getting Resource {ResourceName} from {Caller} in manager", Level = LogLevel.Debug)]
        internal static partial void LogManagerResourceGet(this ILogger logger, ViamResourceName resourceName, string? caller = null);

        [LoggerMessage(EventId = 1202, Message = "Found Resource {ResourceName} {Resource} from {Caller} in manager", Level = LogLevel.Debug)]
        internal static partial void LogManagerResourceGetSuccess(this ILogger logger, ViamResourceName resourceName, IResourceBase resource, string? caller = null);

        [LoggerMessage(EventId = 1203, Message = "Did not find Resource {ResourceName} from {Caller} in manager", Level = LogLevel.Debug)]
        internal static partial void LogManagerResourceGetFailure(this ILogger logger, ViamResourceName resourceName, string? caller = null);

        [LoggerMessage(EventId = 1204, Message = "Getting Resource {ResourceName} from {Caller} in manager", Level = LogLevel.Debug)]
        internal static partial void LogManagerResourceGetByShortName(this ILogger logger, string resourceName, string? caller = null);

        [LoggerMessage(EventId = 1205, Message = "Found Resource {ResourceName} {Resource} from {Caller} in manager", Level = LogLevel.Debug)]
        internal static partial void LogManagerResourceGetByShortNameSuccess(this ILogger logger, string resourceName, IResourceBase resource, string? caller = null);

        [LoggerMessage(EventId = 1206, Message = "Did not find Resource {ResourceName} from {Caller} in manager", Level = LogLevel.Debug)]
        internal static partial void LogManagerResourceGetByShortNameFailure(this ILogger logger, string resourceName, string? caller = null);

        [LoggerMessage(EventId = 1207, Message = "Removing Resource {ResourceName} from {Caller} in manager", Level = LogLevel.Debug)]
        internal static partial void LogManagerResourceRemove(this ILogger logger, ViamResourceName resourceName, string? caller = null);

        [LoggerMessage(EventId = 1208, Message = "Removed Resource {ResourceName} from {Caller} in manager", Level = LogLevel.Debug)]
        internal static partial void LogManagerResourceRemoveSuccess(this ILogger logger, ViamResourceName resourceName, string? caller = null);

        [LoggerMessage(EventId = 1209, Message = "Could not remove Resource {ResourceName} from {Caller} in manager", Level = LogLevel.Debug)]
        internal static partial void LogManagerResourceRemoveFailure(this ILogger logger, ViamResourceName resourceName, string? caller = null);

        [LoggerMessage(EventId = 1210, Message = "Refreshing resources from {Caller} in manager", Level = LogLevel.Debug)]
        internal static partial void LogManagerRefreshStart(this ILogger logger, string? caller = null);

        [LoggerMessage(EventId = 1211, Message = "Done refreshing resources from {Caller} in manager", Level = LogLevel.Debug)]
        internal static partial void LogManagerRefreshFinish(this ILogger logger, string? caller = null);

        [LoggerMessage(EventId = 1212, Message = "Refreshing client for {ResourceName}@{Channel} in manager", Level = LogLevel.Debug)]
        internal static partial void LogManagerCreateOrRefreshClient(this ILogger logger, ViamResourceName resourceName, ViamChannel channel);

        [LoggerMessage(EventId = 1213, Message = "Disposing manager", Level = LogLevel.Debug)]
        internal static partial void LogManagerDispose(this ILogger logger);

        [LoggerMessage(EventId = 1214, Message = "Registering remote resources {RemoteResourceNames}", Level = LogLevel.Debug)]
        internal static partial void LogManagerRegisterRemoteResources(this ILogger logger, ViamResourceName[] remoteResourceNames);

        [LoggerMessage(EventId = 1215, Message = "Registering remote resource {RemoteResourceName}", Level = LogLevel.Trace)]
        internal static partial void LogManagerRegisterRemoteResource(this ILogger logger, ViamResourceName remoteResourceName);

        [LoggerMessage(EventId = 1216, Message = "Registered {ResourceCount} remote resources", Level = LogLevel.Trace)]
        internal static partial void LogManagerRegisterRemoteResourcesComplete(this ILogger logger, int resourceCount);

        [LoggerMessage(EventId = 1217, Message = "Failed to register resource {ResourceName}", Level = LogLevel.Debug)]
        internal static partial void LogManagerRegisterRemoteResourcesError(this ILogger logger, ViamResourceName resourceName, Exception exception);
        #endregion

        #region Dialer Logging
        [LoggerMessage(EventId = 2000, Message = "Dialing {Options}", Level = LogLevel.Information)]
        internal static partial void LogDialDirect(this ILogger logger, GrpcDialOptions options);

        [LoggerMessage(EventId = 2001, Message = "Using Unix socket {Socket}", Level = LogLevel.Information)]
        internal static partial void LogDialUnixSocket(this ILogger logger, string socket);

        [LoggerMessage(EventId = 2002, Message = "Creating GRPC auth channel", Level = LogLevel.Debug)]
        internal static partial void LogDialCreateAuthChannel(this ILogger logger);

        [LoggerMessage(EventId = 2003, Message = "Dialing GRPC auth channel", Level = LogLevel.Debug)]
        internal static partial void LogDialDialingAuthChannel(this ILogger logger);

        [LoggerMessage(EventId = 2004, Message = "Dialed GRPC auth channel successfully", Level = LogLevel.Trace)]
        internal static partial void LogDialDialingAuthChannelSuccess(this ILogger logger);

        [LoggerMessage(EventId = 2005, Message = "Creating AuthServiceClient", Level = LogLevel.Trace)]
        internal static partial void LogDialCreateAuthClient(this ILogger logger);

        [LoggerMessage(EventId = 2006, Message = "Created AuthServiceClient successfully", Level = LogLevel.Trace)]
        internal static partial void LogDialCreateAuthClientSuccess(this ILogger logger);

        [LoggerMessage(EventId = 2007, Message = "Starting authenticate request with {Entity}", Level = LogLevel.Trace)]
        internal static partial void LogDialAuthStart(this ILogger logger, string entity);

        [LoggerMessage(EventId = 2008, Message = "Starting authenticate request with {Entity}", Level = LogLevel.Trace)]
        internal static partial void LogDialAuthSuccess(this ILogger logger, string entity);

        [LoggerMessage(EventId = 2009, Message = "Finished creating GRPC channel", Level = LogLevel.Trace)]
        internal static partial void LogDialComplete(this ILogger logger);

        [LoggerMessage(EventId = 2010, Message = "ICE Gathering State is now {State}", Level = LogLevel.Trace)]
        internal static partial void LogIceGatheringStateChange(this ILogger logger, string state);
        #endregion

        #region Client Method Logging
        #region Robot Client

        #endregion

        #region Arm Client

        #endregion

        #region Base Client

        #endregion

        #region App Client
        [LoggerMessage(EventId = 5400, Message = "", Level = LogLevel.Trace)]
        internal static partial void LogAppClientPlaceholder(this ILogger logger);
        #endregion
        #endregion
    }
}
