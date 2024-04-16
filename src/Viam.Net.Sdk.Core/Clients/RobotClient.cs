using System.Collections.Generic;
using System.Threading.Tasks;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using Viam.Common.V1;
using Viam.Robot.V1;
using Status = Viam.Robot.V1.Status;

namespace Viam.Net.Sdk.Core.Clients
{
    public class RobotClient : ViamClient
    {
        private readonly RobotService.RobotServiceClient _robotServiceClient;

        protected internal RobotClient(ILogger logger, ViamChannel channel) : base(logger, channel)
        {
            _robotServiceClient = new RobotService.RobotServiceClient(channel);
        }

        public async Task<RepeatedField<Operation>> GetOperationsAsync()
        {
            var result = await _robotServiceClient.GetOperationsAsync(new GetOperationsRequest())
                                                  .ConfigureAwait(false);
            return result.Operations;
        }

        public RepeatedField<Operation> GetOperations()
        {
            return _robotServiceClient.GetOperations(new GetOperationsRequest()).Operations;
        }

        public async Task<RepeatedField<Session>> GetSessionsAsync()
        {
            var result = await _robotServiceClient.GetSessionsAsync(new GetSessionsRequest())
                                                  .ConfigureAwait(false);

            return result.Sessions;
        }

        public RepeatedField<Session> GetSessions()
        {
            return _robotServiceClient.GetSessions(new GetSessionsRequest()).Sessions;
        }

        public async Task<RepeatedField<ResourceName>> ResourceNamesAsync()
        {
            var result = await _robotServiceClient.ResourceNamesAsync(new ResourceNamesRequest()).ConfigureAwait(false);
            return result
                   .Resources;
        }

        public RepeatedField<ResourceName> ResourceNames()
        {
            return _robotServiceClient.ResourceNames(new ResourceNamesRequest())
                                      .Resources;
        }

        public async Task<RepeatedField<ResourceRPCSubtype>> GetResourceRPCSubtypesAsync()
        {
            var result = await _robotServiceClient.ResourceRPCSubtypesAsync(new ResourceRPCSubtypesRequest())
                                                  .ConfigureAwait(false);

            return result.ResourceRpcSubtypes;
        }

        public RepeatedField<ResourceRPCSubtype> GetResourceRPCSubtypes()
        {
            return _robotServiceClient.ResourceRPCSubtypes(new ResourceRPCSubtypesRequest())
                                      .ResourceRpcSubtypes;
        }

        public async Task CancelOperationAsync(string operationId)
        {
            await _robotServiceClient.CancelOperationAsync(new CancelOperationRequest() { Id = operationId })
                                     .ConfigureAwait(false);
        }

        public void CancelOperation(string operationId)
        {
            _robotServiceClient.CancelOperation(new CancelOperationRequest() { Id = operationId });
        }

        public async Task BlockForOperationAsync(string operationId)
        {
            await _robotServiceClient
                               .BlockForOperationAsync(new BlockForOperationRequest() { Id = operationId })
                               .ConfigureAwait(false);
        }

        public void BlockForOperation(string operationId)
        {
            _robotServiceClient.BlockForOperation(new BlockForOperationRequest() { Id = operationId });
        }

        public async Task<RepeatedField<Discovery>> DiscoverComponentsAsync(IEnumerable<DiscoveryQuery>? queries = null)
        {
            var query = new DiscoverComponentsRequest();
            if (queries != null)
            {
                query.Queries.AddRange(queries);
            }
            var result = await _robotServiceClient.DiscoverComponentsAsync(query).ConfigureAwait(false);
            return result.Discovery;
        }

        public RepeatedField<Discovery> DiscoverComponents(IEnumerable<DiscoveryQuery>? queries = null)
        {
            var query = new DiscoverComponentsRequest();
            if (queries != null)
            {
                query.Queries.AddRange(queries);
            }
            return _robotServiceClient.DiscoverComponents(query)
                                      .Discovery;
        }

        public async Task<RepeatedField<FrameSystemConfig>> FrameSystemConfigAsync(IEnumerable<Transform>? supplementalTransforms = null)
        {
            var request = new FrameSystemConfigRequest();
            if (supplementalTransforms != null)
            {
                request.SupplementalTransforms.AddRange(supplementalTransforms);
            }
            var result = await _robotServiceClient.FrameSystemConfigAsync(request)
                                                  .ConfigureAwait(false);

            return result.FrameSystemConfigs;
        }

        public RepeatedField<FrameSystemConfig> FrameSystemConfig(IEnumerable<Transform>? supplementalTransforms)
        {
            var request = new FrameSystemConfigRequest();
            if (supplementalTransforms != null)
            {
                request.SupplementalTransforms.AddRange(supplementalTransforms);
            }
            return _robotServiceClient.FrameSystemConfig(request)
                                      .FrameSystemConfigs;
        }

        public async Task<PoseInFrame> TransformPoseAsync(string destination, PoseInFrame source, IEnumerable<Transform> supplementalTransforms)
        {
            var request = new TransformPoseRequest() { Destination = destination, Source = source };
            request.SupplementalTransforms.AddRange(supplementalTransforms);
            var result = await _robotServiceClient.TransformPoseAsync(request)
                                                  .ConfigureAwait(false);
            return result.Pose;
        }

        public PoseInFrame TransformPose(string destination, PoseInFrame source, IEnumerable<Transform> supplementalTransforms)
        {
            var request = new TransformPoseRequest() { Destination = destination, Source = source };
            request.SupplementalTransforms.AddRange(supplementalTransforms);
            return _robotServiceClient.TransformPose(request).Pose;
        }

        public async Task<ByteString> TransformPCDAsync(string source, string destination, ByteString pointCloudPcd)
        {
            var request = new TransformPCDRequest() { Source = source, Destination = destination, PointCloudPcd = pointCloudPcd };
            var result = await _robotServiceClient.TransformPCDAsync(request)
                               .ConfigureAwait(false);
            return result.PointCloudPcd;
        }

        public ByteString TransformPCD(string source, string destination, ByteString pointCloudPcd)
        {
            var request = new TransformPCDRequest() { Source = source, Destination = destination, PointCloudPcd = pointCloudPcd };
            return _robotServiceClient.TransformPCD(request)
                                      .PointCloudPcd;
        }

        public async Task<RepeatedField<Status>> GetStatusAsync(IEnumerable<ResourceName>? resourceNames = null)
        {
            var request = new GetStatusRequest();
            if (resourceNames != null)
            {
                request.ResourceNames.AddRange(resourceNames);
            }
            var result = await _robotServiceClient.GetStatusAsync(request)
                                                  .ConfigureAwait(false);

            return result.Status;
        }

        public RepeatedField<Status> GetStatus(IEnumerable<ResourceName>? resourceNames = null)
        {
            var request = new GetStatusRequest();
            if (resourceNames != null)
            {
                request.ResourceNames.AddRange(resourceNames);
            }
            return _robotServiceClient.GetStatus(request).Status;
        }

        public IAsyncStreamReader<StreamStatusResponse> StreamStatus(Duration every, IEnumerable<ResourceName>? resourceNames = null)
        {
            var request = new StreamStatusRequest() { Every = every };
            if (resourceNames != null)
            {
                request.ResourceNames.AddRange(resourceNames);
            }

            return _robotServiceClient.StreamStatus(request)
                                      .ResponseStream;
        }

        public async Task StopAllAsync(IEnumerable<StopExtraParameters>? extraParameters = null)
        {
            var request = new StopAllRequest();
            if (extraParameters != null)
            {
                request.Extra.AddRange(extraParameters);
            }

            await _robotServiceClient.StopAllAsync(request)
                               .ConfigureAwait(false);
        }

        public void StopAll(IEnumerable<StopExtraParameters>? extraParameters = null)
        {
            var request = new StopAllRequest();
            if (extraParameters != null)
            {
                request.Extra.AddRange(extraParameters);
            }

            _robotServiceClient.StopAll(request);
        }

        public async Task<(string Id, Duration HeartbeatWindow)> StartSessionAsync(string? resumeToken = null)
        {
            var result = await _robotServiceClient.StartSessionAsync(new StartSessionRequest() { Resume = resumeToken })
                               .ConfigureAwait(false);

            return (result.Id, result.HeartbeatWindow);
        }

        public (string Id, Duration HeartbeatWindow) StartSession(string? resumeToken = null)
        {
            var result = _robotServiceClient.StartSession(new StartSessionRequest() { Resume = resumeToken });

            return (result.Id, result.HeartbeatWindow);
        }

        public async Task SendSessionHeartbeatAsync(string sessionId)
        {
            var request = new SendSessionHeartbeatRequest() { Id = sessionId };
            await _robotServiceClient.SendSessionHeartbeatAsync(request)
                                                  .ConfigureAwait(false);
        }

        public void SendSessionHeartbeat(string sessionId)
        {
            var request = new SendSessionHeartbeatRequest() { Id = sessionId };
            _robotServiceClient.SendSessionHeartbeat(request);
        }

        public async Task LogAsync(IEnumerable<LogEntry> logs)
        {
            var request = new LogRequest();
            request.Logs.AddRange(logs);
            await _robotServiceClient.LogAsync(request).ConfigureAwait(false);
        }

        public void Log(IEnumerable<LogEntry> logs)
        {
            var request = new LogRequest();
            request.Logs.AddRange(logs);
            _robotServiceClient.Log(request);
        }

        public async Task<CloudMetadata> GetCloudMetadataAsync()
        {
            var result = await _robotServiceClient.GetCloudMetadataAsync(new GetCloudMetadataRequest())
                                                  .ConfigureAwait(false);
            return new CloudMetadata(result.PrimaryOrgId, result.LocationId, result.MachineId, result.MachinePartId, result.RobotPartId);
        }

        public CloudMetadata GetCloudMetadata()
        {
            var result = _robotServiceClient.GetCloudMetadata(new GetCloudMetadataRequest());
            return new CloudMetadata(result.PrimaryOrgId, result.LocationId, result.MachineId, result.MachinePartId, result.RobotPartId);
        }
    }
}
