using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;

using Grpc.Core;

using Microsoft.Extensions.Logging;

using Viam.Common.V1;
using Viam.Core.Logging;
using Viam.Core.Resources;
using Viam.Core.Utils;
using Viam.Robot.V1;

using Status = Viam.Robot.V1.Status;

namespace Viam.Core.Clients
{
    public class RobotClientBase
    {
        internal readonly ViamChannel Channel;
        protected readonly ILogger<RobotClientBase> Logger;
        private readonly RobotService.RobotServiceClient _robotServiceClient;
        private readonly ResourceManager _resourceManager;

        protected internal RobotClientBase(ILoggerFactory loggerFactory, ViamChannel channel)
        {
            Logging.Logger.SetLoggerFactory(loggerFactory);
            Channel = channel;
            Logger = loggerFactory.CreateLogger<RobotClientBase>();
            _robotServiceClient = new RobotService.RobotServiceClient(channel);
            _resourceManager = new ResourceManager(loggerFactory);
        }

        [LogCall]
        protected Task RefreshAsync() => _resourceManager.RefreshAsync(this);

        [LogCall]
        public T GetComponent<T>(ViamResourceName resourceName) where T : ResourceBase => (T)_resourceManager.GetResource(resourceName);

        [LogCall]
        public async Task<Operation[]> GetOperationsAsync(TimeSpan? timeout = null, CancellationToken cancellationToken = default)
        {
            var result = await _robotServiceClient.GetOperationsAsync(new GetOperationsRequest(),
                                                                      deadline: timeout.ToDeadline(),
                                                                      cancellationToken: cancellationToken)
                                                  .ConfigureAwait(false);
            return result.Operations.ToArray();
        }

        [LogCall]
        public async Task<Session[]> GetSessionsAsync(TimeSpan? timeout = null, CancellationToken cancellationToken = default)
        {
            var result = await _robotServiceClient.GetSessionsAsync(new GetSessionsRequest(),
                                                                    deadline: timeout.ToDeadline(),
                                                                    cancellationToken: cancellationToken)
                                                  .ConfigureAwait(false);

            return result.Sessions.ToArray();
        }

        [LogCall]
        public async Task<ViamResourceName[]> ResourceNamesAsync(TimeSpan? timeout = null, CancellationToken cancellationToken = default)
        {
            var result = await _robotServiceClient.ResourceNamesAsync(new ResourceNamesRequest(),
                                                                      deadline: timeout.ToDeadline(),
                                                                      cancellationToken: cancellationToken)
                                                  .ConfigureAwait(false);

            // ReSharper disable once SuspiciousTypeConversion.Global
            // There is an implicit conversion between these types
            return result.Resources.Cast<ViamResourceName>()
                         .ToArray();
        }

        [LogCall]
        public async Task<ResourceRPCSubtype[]> GetResourceRpcSubtypesAsync(TimeSpan? timeout = null, CancellationToken cancellationToken = default)
        {
            var result = await _robotServiceClient.ResourceRPCSubtypesAsync(new ResourceRPCSubtypesRequest(),
                                                                            deadline: timeout.ToDeadline(),
                                                                            cancellationToken: cancellationToken)
                                                  .ConfigureAwait(false);

            return result.ResourceRpcSubtypes.ToArray();
        }

        [LogCall]
        public async Task CancelOperationAsync(string operationId, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
        {
            await _robotServiceClient.CancelOperationAsync(new CancelOperationRequest() { Id = operationId },
                                                           deadline: timeout.ToDeadline(),
                                                           cancellationToken: cancellationToken)
                                     .ConfigureAwait(false);
        }

        [LogCall]
        public async Task BlockForOperationAsync(string operationId, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
        {
            await _robotServiceClient
                               .BlockForOperationAsync(new BlockForOperationRequest() { Id = operationId },
                                                       deadline: timeout.ToDeadline(),
                                                       cancellationToken: cancellationToken)
                               .ConfigureAwait(false);
        }

        [LogCall]
        public async Task<Discovery[]> DiscoverComponentsAsync(IEnumerable<DiscoveryQuery>? queries = null, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
        {
            var query = new DiscoverComponentsRequest();
            if (queries != null)
            {
                query.Queries.AddRange(queries);
            }
            var result = await _robotServiceClient.DiscoverComponentsAsync(query,
                                                                           deadline: timeout.ToDeadline(),
                                                                           cancellationToken: cancellationToken)
                                                  .ConfigureAwait(false);
            return result.Discovery.ToArray();
        }

        [LogCall]
        public async Task<FrameSystemConfig[]> FrameSystemConfigAsync(IEnumerable<Transform>? supplementalTransforms = null, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
        {
            var request = new FrameSystemConfigRequest();
            if (supplementalTransforms != null)
            {
                request.SupplementalTransforms.AddRange(supplementalTransforms);
            }
            var result = await _robotServiceClient.FrameSystemConfigAsync(request,
                                                                          deadline: timeout.ToDeadline(),
                                                                          cancellationToken: cancellationToken)
                                                  .ConfigureAwait(false);

            return result.FrameSystemConfigs.ToArray();
        }

        [LogCall]
        public async Task<PoseInFrame> TransformPoseAsync(string destination, PoseInFrame source, IEnumerable<Transform> supplementalTransforms, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
        {
            var request = new TransformPoseRequest() { Destination = destination, Source = source };
            request.SupplementalTransforms.AddRange(supplementalTransforms);
            var result = await _robotServiceClient.TransformPoseAsync(request,
                                                                      deadline: timeout.ToDeadline(),
                                                                      cancellationToken: cancellationToken)
                                                  .ConfigureAwait(false);
            return result.Pose;
        }

        [LogCall]
        public async Task<ByteString> TransformPcdAsync(string source, string destination, ByteString pointCloudPcd, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
        {
            var request = new TransformPCDRequest() { Source = source, Destination = destination, PointCloudPcd = pointCloudPcd };
            var result = await _robotServiceClient.TransformPCDAsync(request,
                                                                     deadline: timeout.ToDeadline(),
                                                                     cancellationToken: cancellationToken)
                                                  .ConfigureAwait(false);
            return result.PointCloudPcd;
        }

        [LogCall]
        public async Task<Status[]> GetStatusAsync(IEnumerable<ResourceName>? resourceNames = null, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
        {
            var request = new GetStatusRequest();
            if (resourceNames != null)
            {
                request.ResourceNames.AddRange(resourceNames);
            }
            var result = await _robotServiceClient.GetStatusAsync(request,
                                                                  deadline: timeout.ToDeadline(),
                                                                  cancellationToken: cancellationToken)
                                                  .ConfigureAwait(false);

            return result.Status.ToArray();
        }

        [LogCall]
        public IAsyncStreamReader<StreamStatusResponse> StreamStatus(Duration every, IEnumerable<ResourceName>? resourceNames = null, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
        {
            var request = new StreamStatusRequest() { Every = every };
            if (resourceNames != null)
            {
                request.ResourceNames.AddRange(resourceNames);
            }

            return _robotServiceClient.StreamStatus(request)
                                      .ResponseStream;
        }

        [LogCall]
        public async Task StopAllAsync(IEnumerable<StopExtraParameters>? extraParameters = null, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
        {
            var request = new StopAllRequest();
            if (extraParameters != null)
            {
                request.Extra.AddRange(extraParameters);
            }

            await _robotServiceClient.StopAllAsync(request,
                                                   deadline: timeout.ToDeadline(),
                                                   cancellationToken: cancellationToken)
                                     .ConfigureAwait(false);
        }

        [LogCall]
        public async Task<(string Id, Duration HeartbeatWindow)> StartSessionAsync(string? resumeToken = null, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
        {
            var result = await _robotServiceClient.StartSessionAsync(new StartSessionRequest() { Resume = resumeToken },
                                                                     deadline: timeout.ToDeadline(),
                                                                     cancellationToken: cancellationToken)
                                                  .ConfigureAwait(false);

            return (result.Id, result.HeartbeatWindow);
        }

        [LogCall]
        public async Task SendSessionHeartbeatAsync(string sessionId, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
        {
            var request = new SendSessionHeartbeatRequest() { Id = sessionId };
            await _robotServiceClient.SendSessionHeartbeatAsync(request,
                                                                deadline: timeout.ToDeadline(),
                                                                cancellationToken: cancellationToken)
                                     .ConfigureAwait(false);
        }

        [LogCall]
        public async Task LogAsync(IEnumerable<LogEntry> logs)
        {
            var request = new LogRequest();
            request.Logs.AddRange(logs);
            await _robotServiceClient.LogAsync(request);
        }

        [LogCall]
        public async Task<CloudMetadata> GetCloudMetadataAsync(TimeSpan? timeout = null, CancellationToken cancellationToken = default)
        {
            var result = await _robotServiceClient.GetCloudMetadataAsync(new GetCloudMetadataRequest(),
                                                                         deadline: timeout.ToDeadline(),
                                                                         cancellationToken: cancellationToken)
                                                  .ConfigureAwait(false);
            return new CloudMetadata(result.PrimaryOrgId, result.LocationId, result.MachineId, result.MachinePartId, result.RobotPartId);
        }

        public override string ToString() => $"RobotClientBase+{Channel}";
    }
}
