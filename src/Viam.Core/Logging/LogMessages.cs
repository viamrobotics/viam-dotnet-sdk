using Grpc.Core;
using Microsoft.Extensions.Logging;
using System;
using System.Runtime.CompilerServices;
using Viam.Contracts.Resources;
using Viam.Core.Grpc;
using Viam.Core.Resources;
using Viam.Core.Utils;
using Metadata = Grpc.Core.Metadata;

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

        [LoggerMessage(EventId = 1000, Message = "Invoking {Method} with {Parameters}", Level = LogLevel.Trace,
            SkipEnabledCheck = true)]
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

        [LoggerMessage(EventId = 1002, Message = "An exception occurred while executing {Method}",
            Level = LogLevel.Debug)]
        internal static partial void LogMethodInvocationFailure(this ILogger logger, Exception? exception = null,
            [CallerMemberName] string method = "unknown");

        #endregion

        #region Registry Logging

        [LoggerMessage(EventId = 1100, Message = "Getting resource by {SubType} in registry", Level = LogLevel.Debug)]
        internal static partial void LogRegistryResourceGet(this ILogger logger, SubType subType);

        [LoggerMessage(EventId = 1101, Message = "Found built-in resource by {SubType} in registry",
            Level = LogLevel.Debug)]
        internal static partial void LogRegistryBuiltInResourceGetSuccess(this ILogger logger, SubType subType);

        [LoggerMessage(EventId = 1102, Message = "Found resource by {SubType} in registry", Level = LogLevel.Debug)]
        internal static partial void LogRegistryResourceGetSuccess(this ILogger logger, SubType subType);

        [LoggerMessage(EventId = 1103, Message = "Did not find resource by {SubType} in registry",
            Level = LogLevel.Debug)]
        internal static partial void LogRegistryResourceGetFailure(this ILogger logger, SubType subType);

        [LoggerMessage(EventId = 1104, Message = "Getting ResourceCreator by {SubType} and {Model} in registry",
            Level = LogLevel.Debug)]
        internal static partial void LogRegistryResourceCreatorGet(this ILogger logger, SubType subType, Model model);

        [LoggerMessage(EventId = 1105, Message = "Found ResourceCreator by {SubType} and {Model} in registry",
            Level = LogLevel.Debug)]
        internal static partial void LogRegistryResourceCreatorGetSuccess(this ILogger logger, SubType subType,
            Model model);

        [LoggerMessage(EventId = 1106, Message = "Did not find ResourceCreator by {SubType} and {Model} in registry",
            Level = LogLevel.Debug)]
        internal static partial void LogRegistryResourceCreatorGetFailure(this ILogger logger, SubType subType,
            Model model);

        #endregion

        #region Resource Manager Logging

        [LoggerMessage(EventId = 1200,
            Message = "Registering Resource {ResourceName} {Resource} from {Caller} in manager",
            Level = LogLevel.Debug)]
        internal static partial void LogManagerResourceRegistration(this ILogger logger, ViamResourceName resourceName,
            IResourceBase resource, string? caller = null);

        [LoggerMessage(EventId = 1201, Message = "Getting Resource {ResourceName} from {Caller} in manager",
            Level = LogLevel.Debug)]
        internal static partial void LogManagerResourceGet(this ILogger logger, ViamResourceName resourceName,
            string? caller = null);

        [LoggerMessage(EventId = 1202, Message = "Found Resource {ResourceName} {Resource} from {Caller} in manager",
            Level = LogLevel.Debug)]
        internal static partial void LogManagerResourceGetSuccess(this ILogger logger, ViamResourceName resourceName,
            IResourceBase resource, string? caller = null);

        [LoggerMessage(EventId = 1203, Message = "Did not find Resource {ResourceName} from {Caller} in manager",
            Level = LogLevel.Debug)]
        internal static partial void LogManagerResourceGetFailure(this ILogger logger, ViamResourceName resourceName,
            string? caller = null);

        [LoggerMessage(EventId = 1204, Message = "Getting Resource {ResourceName} from {Caller} in manager",
            Level = LogLevel.Debug)]
        internal static partial void LogManagerResourceGetByShortName(this ILogger logger, string resourceName,
            string? caller = null);

        [LoggerMessage(EventId = 1205, Message = "Found Resource {ResourceName} {Resource} from {Caller} in manager",
            Level = LogLevel.Debug)]
        internal static partial void LogManagerResourceGetByShortNameSuccess(this ILogger logger, string resourceName,
            IResourceBase resource, string? caller = null);

        [LoggerMessage(EventId = 1206, Message = "Did not find Resource {ResourceName} from {Caller} in manager",
            Level = LogLevel.Debug)]
        internal static partial void LogManagerResourceGetByShortNameFailure(this ILogger logger, string resourceName,
            string? caller = null);

        [LoggerMessage(EventId = 1207, Message = "Removing Resource {ResourceName} from {Caller} in manager",
            Level = LogLevel.Debug)]
        internal static partial void LogManagerResourceRemove(this ILogger logger, ViamResourceName resourceName,
            string? caller = null);

        [LoggerMessage(EventId = 1208, Message = "Removed Resource {ResourceName} from {Caller} in manager",
            Level = LogLevel.Debug)]
        internal static partial void LogManagerResourceRemoveSuccess(this ILogger logger, ViamResourceName resourceName,
            string? caller = null);

        [LoggerMessage(EventId = 1209, Message = "Could not remove Resource {ResourceName} from {Caller} in manager",
            Level = LogLevel.Debug)]
        internal static partial void LogManagerResourceRemoveFailure(this ILogger logger, ViamResourceName resourceName,
            string? caller = null);

        [LoggerMessage(EventId = 1210, Message = "Refreshing resources from {Caller} in manager",
            Level = LogLevel.Debug)]
        internal static partial void LogManagerRefreshStart(this ILogger logger, string? caller = null);

        [LoggerMessage(EventId = 1211, Message = "Done refreshing resources from {Caller} in manager",
            Level = LogLevel.Debug)]
        internal static partial void LogManagerRefreshFinish(this ILogger logger, string? caller = null);

        [LoggerMessage(EventId = 1212, Message = "Refreshing client for {ResourceName}@{Channel} in manager",
            Level = LogLevel.Debug)]
        internal static partial void LogManagerCreateOrRefreshClient(this ILogger logger, ViamResourceName resourceName,
            ViamChannel channel);

        [LoggerMessage(EventId = 1213, Message = "Disposing manager", Level = LogLevel.Debug)]
        internal static partial void LogManagerDispose(this ILogger logger);

        [LoggerMessage(EventId = 1214, Message = "Registering remote resources {RemoteResourceNames}",
            Level = LogLevel.Debug)]
        internal static partial void LogManagerRegisterRemoteResources(this ILogger logger,
            ViamResourceName[] remoteResourceNames);

        [LoggerMessage(EventId = 1215, Message = "Registering remote resource {RemoteResourceName}",
            Level = LogLevel.Trace)]
        internal static partial void LogManagerRegisterRemoteResource(this ILogger logger,
            ViamResourceName remoteResourceName);

        [LoggerMessage(EventId = 1216, Message = "Registered {ResourceCount} remote resources", Level = LogLevel.Trace)]
        internal static partial void LogManagerRegisterRemoteResourcesComplete(this ILogger logger, int resourceCount);

        [LoggerMessage(EventId = 1217, Message = "Failed to register resource {ResourceName}", Level = LogLevel.Debug)]
        internal static partial void LogManagerRegisterRemoteResourcesError(this ILogger logger,
            ViamResourceName resourceName, Exception exception);

        #endregion

        #region Communication Logging

        #region Dialing Logging

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

        #region WebRTC Logging

        [LoggerMessage(EventId = 2100, Message = "Starting {ClassName} Request", Level = LogLevel.Debug)]
        internal static partial void LogWebRtcCallStartRequest(this ILogger logger, string className);

        [LoggerMessage(EventId = 2101, Message = "{ClassName} request waiting for OnReady", Level = LogLevel.Debug)]
        internal static partial void LogWebRtcWaitForOnReady(this ILogger logger, string className);

        [LoggerMessage(EventId = 2102, Message = "{ClassName} request OnReady success", Level = LogLevel.Debug)]
        internal static partial void LogWebRtcWaitForOnReadySuccess(this ILogger logger, string className);

        [LoggerMessage(EventId = 2103, Message = "{ClassName} Request HalfClose success", Level = LogLevel.Debug)]
        internal static partial void LogWebRtcCallHalfClose(this ILogger logger, string className);

        [LoggerMessage(EventId = 2104, Message = "{ClassName} Request setting ready", Level = LogLevel.Debug)]
        internal static partial void LogWebRtcCallOnReady(this ILogger logger, string className);

        [LoggerMessage(EventId = 2105, Message = "{ClassName} Request metadata received: {Metadata}",
            Level = LogLevel.Debug)]
        internal static partial void
            LogWebRtcMetadataResponse(this ILogger logger, string className, Metadata metadata);

        [LoggerMessage(EventId = 2106, Message = "{ClassName} Request response received", Level = LogLevel.Debug)]
        internal static partial void LogWebRtcResponseReceived(this ILogger logger, string className);

        [LoggerMessage(EventId = 2106, Message = "{ClassName} Request response received: {Response}",
            Level = LogLevel.Trace)]
        internal static partial void LogWebRtcResponseReceivedWithResponse(this ILogger logger, string className,
            object response);

        [LoggerMessage(EventId = 2107, Message = "{ClassName} Request close {StatusCode} {Detail}",
            Level = LogLevel.Debug)]
        internal static partial void LogWebRtcClose(this ILogger logger, string className, StatusCode statusCode,
            string detail);

        [LoggerMessage(EventId = 2108, Message = "Processing message", Level = LogLevel.Trace)]
        internal static partial void LogWebRtcProcessMessage(this ILogger logger);

        [LoggerMessage(EventId = 2109, Message = "Calling OnMessage", Level = LogLevel.Trace)]
        internal static partial void LogWebRtcCallingOnMessage(this ILogger logger);

        [LoggerMessage(EventId = 2110, Message = "Message not done yet", Level = LogLevel.Trace)]
        internal static partial void LogWebRtcMessageNotDone(this ILogger logger);

        [LoggerMessage(EventId = 2111,
            Message = "Message size {RequiredSize} is larger than current size {CurrentSize}", Level = LogLevel.Trace)]
        internal static partial void LogWebRtcPacketMessageContentsSizeExceeded(this ILogger logger, int currentSize,
            int requiredSize);

        [LoggerMessage(EventId = 2112, Message = "Copying {DataSize} bytes to array at position {Position}",
            Level = LogLevel.Trace)]
        internal static partial void LogWebRtcPacketMessageCopyingData(this ILogger logger, int dataSize, int position);

        [LoggerMessage(EventId = 2113, Message = "Done appending data", Level = LogLevel.Trace)]
        internal static partial void LogWebRtcPacketMessageDataCopyDone(this ILogger logger);

        [LoggerMessage(EventId = 2114, Message = "Disposing PacketMessageContents and returning array to pool",
            Level = LogLevel.Trace)]
        internal static partial void LogWebRtcPacketMessageDispose(this ILogger logger);

        [LoggerMessage(EventId = 2115, Message = "Growing array to {ArraySize}", Level = LogLevel.Trace)]
        internal static partial void LogWebRtcPacketMessageGrowArrayStart(this ILogger logger, int arraySize);

        [LoggerMessage(EventId = 2116, Message = "Got new array {ArraySize}", Level = LogLevel.Trace)]
        internal static partial void LogWebRtcPacketMessageNewArraySize(this ILogger logger, int arraySize);

        [LoggerMessage(EventId = 2116, Message = "Done resizing array", Level = LogLevel.Trace)]
        internal static partial void LogWebRtcPacketMessageGrowArrayEnd(this ILogger logger);

        [LoggerMessage(EventId = 2117,
            Message = "Message size {MessageSize} larger than max {MaxMessageSize}; discarding",
            Level = LogLevel.Warning)]
        internal static partial void LogWebRtcMessageSizeExceeded(this ILogger logger, int messageSize,
            int maxMessageSize);

        [LoggerMessage(EventId = 2118, Message = "Processing new packet message", Level = LogLevel.Trace)]
        internal static partial void LogWebRtcProcessNewPacketMessage(this ILogger logger);

        [LoggerMessage(EventId = 2118, Message = "Processing addition to existing packet message",
            Level = LogLevel.Trace)]
        internal static partial void LogWebRtcProcessExistingPacketMessage(this ILogger logger);

        [LoggerMessage(EventId = 2119, Message = "Processing packet message complete", Level = LogLevel.Trace)]
        internal static partial void LogWebRtcProcessPacketMessageEndOfMessage(this ILogger logger);

        [LoggerMessage(EventId = 2120, Message = "Sending WebRTC message", Level = LogLevel.Trace)]
        internal static partial void LogWebRtcSendMessage(this ILogger logger);

        #endregion

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