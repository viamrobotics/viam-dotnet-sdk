using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;

using Grpc.Core;

using Microsoft.Extensions.Logging;
using Viam.Common.V1;
using Viam.Core.Resources;
using Viam.Robot.V1;

using Status = Viam.Robot.V1.Status;

namespace Viam.Core.Clients
{
    public class RobotClientBase
    {
        internal readonly ViamChannel Channel;
        private readonly RobotService.RobotServiceClient _robotServiceClient;
        private readonly ResourceManager _resourceManager;

        protected internal RobotClientBase(ILoggerFactory loggerFactory, ViamChannel channel)
        {
            Channel = channel;
            _robotServiceClient = new RobotService.RobotServiceClient(channel);
            _resourceManager = new ResourceManager(loggerFactory.CreateLogger<ResourceManager>());
        }

        protected Task RefreshAsync() => _resourceManager.RefreshAsync(this);

        public T GetComponent<T>(ViamResourceName resourceName) where T : ResourceBase => (T)_resourceManager.GetResource(resourceName);

        public async Task<Operation[]> GetOperationsAsync()
        {
            var result = await _robotServiceClient.GetOperationsAsync(new GetOperationsRequest())
                                                  ;
            return result.Operations.ToArray();
        }


        public async Task<Session[]> GetSessionsAsync()
        {
            var result = await _robotServiceClient.GetSessionsAsync(new GetSessionsRequest())
                                                  ;

            return result.Sessions.ToArray();
        }

        public async Task<ViamResourceName[]> ResourceNamesAsync()
        {
            var result = await _robotServiceClient.ResourceNamesAsync(new ResourceNamesRequest());

            // ReSharper disable once SuspiciousTypeConversion.Global
            // There is an implicit conversion between these types
            return result.Resources.Cast<ViamResourceName>()
                         .ToArray();
        }

        public async Task<ResourceRPCSubtype[]> GetResourceRPCSubtypesAsync()
        {
            var result = await _robotServiceClient.ResourceRPCSubtypesAsync(new ResourceRPCSubtypesRequest())
                                                  ;

            return result.ResourceRpcSubtypes.ToArray();
        }

        public async Task CancelOperationAsync(string operationId)
        {
            await _robotServiceClient.CancelOperationAsync(new CancelOperationRequest() { Id = operationId });
        }

        public async Task BlockForOperationAsync(string operationId)
        {
            await _robotServiceClient
                               .BlockForOperationAsync(new BlockForOperationRequest() { Id = operationId });
        }

        public void BlockForOperation(string operationId)
        {
            _robotServiceClient.BlockForOperation(new BlockForOperationRequest() { Id = operationId });
        }

        public async Task<Discovery[]> DiscoverComponentsAsync(IEnumerable<DiscoveryQuery>? queries = null)
        {
            var query = new DiscoverComponentsRequest();
            if (queries != null)
            {
                query.Queries.AddRange(queries);
            }
            var result = await _robotServiceClient.DiscoverComponentsAsync(query);
            return result.Discovery.ToArray();
        }


        public async Task<FrameSystemConfig[]> FrameSystemConfigAsync(IEnumerable<Transform>? supplementalTransforms = null)
        {
            var request = new FrameSystemConfigRequest();
            if (supplementalTransforms != null)
            {
                request.SupplementalTransforms.AddRange(supplementalTransforms);
            }
            var result = await _robotServiceClient.FrameSystemConfigAsync(request);

            return result.FrameSystemConfigs.ToArray();
        }

        public async Task<PoseInFrame> TransformPoseAsync(string destination, PoseInFrame source, IEnumerable<Transform> supplementalTransforms)
        {
            var request = new TransformPoseRequest() { Destination = destination, Source = source };
            request.SupplementalTransforms.AddRange(supplementalTransforms);
            var result = await _robotServiceClient.TransformPoseAsync(request)
                                                  ;
            return result.Pose;
        }

        public async Task<ByteString> TransformPCDAsync(string source, string destination, ByteString pointCloudPcd)
        {
            var request = new TransformPCDRequest() { Source = source, Destination = destination, PointCloudPcd = pointCloudPcd };
            var result = await _robotServiceClient.TransformPCDAsync(request)
                               ;
            return result.PointCloudPcd;
        }

        public async Task<Status[]> GetStatusAsync(IEnumerable<ResourceName>? resourceNames = null)
        {
            var request = new GetStatusRequest();
            if (resourceNames != null)
            {
                request.ResourceNames.AddRange(resourceNames);
            }
            var result = await _robotServiceClient.GetStatusAsync(request)
                                                  ;

            return result.Status.ToArray();
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
                               ;
        }

        public async Task<(string Id, Duration HeartbeatWindow)> StartSessionAsync(string? resumeToken = null)
        {
            var result = await _robotServiceClient.StartSessionAsync(new StartSessionRequest() { Resume = resumeToken });

            return (result.Id, result.HeartbeatWindow);
        }

        public async Task SendSessionHeartbeatAsync(string sessionId)
        {
            var request = new SendSessionHeartbeatRequest() { Id = sessionId };
            await _robotServiceClient.SendSessionHeartbeatAsync(request)
                                                  ;
        }

        public async Task LogAsync(IEnumerable<LogEntry> logs)
        {
            var request = new LogRequest();
            request.Logs.AddRange(logs);
            await _robotServiceClient.LogAsync(request);
        }


        public async Task<CloudMetadata> GetCloudMetadataAsync()
        {
            var result = await _robotServiceClient.GetCloudMetadataAsync(new GetCloudMetadataRequest())
                                                  ;
            return new CloudMetadata(result.PrimaryOrgId, result.LocationId, result.MachineId, result.MachinePartId, result.RobotPartId);
        }

    }
}
