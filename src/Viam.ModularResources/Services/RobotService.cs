using Google.Protobuf.WellKnownTypes;

using Grpc.Core;
using Viam.Common.V1;
using Viam.Core.Resources;
using Viam.Core.Utils;
using Viam.Robot.V1;

using grpcRobotService = Viam.Robot.V1.RobotService;
using Status = Grpc.Core.Status;

namespace Viam.ModularResources.Services
{
    public class RobotService(ResourceManager manager, ILogger<RobotService> logger) : grpcRobotService.RobotServiceBase
    {
        public readonly ResourceManager Manager = manager;

        public override Task<ResourceNamesResponse> ResourceNames(ResourceNamesRequest request, ServerCallContext context)
        {
            var resp = new ResourceNamesResponse();
            // ReSharper disable once SuspiciousTypeConversion.Global
            // There is an implicit conversion between these types
            resp.Resources.AddRange(Manager.GetResourceNames().Cast<ResourceName>());
            return Task.FromResult(resp);
        }

        public override Task<GetStatusResponse> GetStatus(GetStatusRequest request, ServerCallContext context)
        {
            try
            {
                var resp = new GetStatusResponse();
                foreach (var resource in request.ResourceNames)
                {
                    var status = Manager.GetResource(resource)
                                        .GetStatus();

                    var s = new Viam.Robot.V1.Status { Name = resource, Status_ = new Struct() };
                    if (status.LastReconfigured.HasValue)
                        s.LastReconfigured = Timestamp.FromDateTime(status.LastReconfigured.Value);

                    s.LastReconfigured = null;

                    resp.Status.Add(s);
                }

                return Task.FromResult(resp);
            }
            catch (Exception e)
            {
                logger.LogError(e, "");
                throw new RpcException(new Status(StatusCode.Unknown, e.ToString()));
            }
        }

        public override async Task StreamStatus(StreamStatusRequest request, IServerStreamWriter<StreamStatusResponse> responseStream, ServerCallContext context)
        {
            while (true)
            {
                var resp = new StreamStatusResponse();
                foreach (var resource in request.ResourceNames)
                {
                    var status = Manager.GetResource(resource).GetStatus();
                    var s = new Viam.Robot.V1.Status { Name = resource, Status_ = new Struct() };
                    if (status.LastReconfigured.HasValue)
                        s.LastReconfigured = Timestamp.FromDateTime(status.LastReconfigured.Value);

                    s.LastReconfigured = null;

                    resp.Status.Add(s);
                }
                await responseStream.WriteAsync(resp);
            }
        }

        public override async Task<StopAllResponse> StopAll(StopAllRequest request, ServerCallContext context)
        {
            await Manager.GetResources()
                         .Select(r => r.StopResource())
                         .WhenAll();

            var resp = new StopAllResponse();
            return resp;
        }

        public override Task<GetOperationsResponse>
            GetOperations(GetOperationsRequest request, ServerCallContext context) =>
            throw new RpcException(new Status(StatusCode.Unimplemented, "Method is not implemented"));

        public override Task<ResourceRPCSubtypesResponse> ResourceRPCSubtypes(
            ResourceRPCSubtypesRequest request,
            ServerCallContext context) =>
            throw new RpcException(new Status(StatusCode.Unimplemented, "Method is not implemented"));

        public override Task<CancelOperationResponse>
            CancelOperation(CancelOperationRequest request, ServerCallContext context) =>
            throw new RpcException(new Status(StatusCode.Unimplemented, "Method is not implemented"));

        public override Task<BlockForOperationResponse> BlockForOperation(
            BlockForOperationRequest request,
            ServerCallContext context) =>
            throw new RpcException(new Status(StatusCode.Unimplemented, "Method is not implemented"));

        public override Task<FrameSystemConfigResponse> FrameSystemConfig(
            FrameSystemConfigRequest request,
            ServerCallContext context) =>
            throw new RpcException(new Status(StatusCode.Unimplemented, "Method is not implemented"));

        public override Task<TransformPoseResponse>
            TransformPose(TransformPoseRequest request, ServerCallContext context) =>
            throw new RpcException(new Status(StatusCode.Unimplemented, "Method is not implemented"));

        public override Task<DiscoverComponentsResponse> DiscoverComponents(
            DiscoverComponentsRequest request,
            ServerCallContext context) =>
            throw new RpcException(new Status(StatusCode.Unimplemented, "Method is not implemented"));

        public override Task<GetSessionsResponse>
            GetSessions(GetSessionsRequest request, ServerCallContext context) =>
            throw new RpcException(new Status(StatusCode.Unimplemented, "Method is not implemented"));

        public override Task<StartSessionResponse>
            StartSession(StartSessionRequest request, ServerCallContext context) =>
            throw new RpcException(new Status(StatusCode.Unimplemented, "Method is not implemented"));

        public override Task<SendSessionHeartbeatResponse> SendSessionHeartbeat(
            SendSessionHeartbeatRequest request,
            ServerCallContext context) =>
            throw new RpcException(new Status(StatusCode.Unimplemented, "Method is not implemented"));

        public override Task<TransformPCDResponse>
            TransformPCD(TransformPCDRequest request, ServerCallContext context) =>
            throw new RpcException(new Status(StatusCode.Unimplemented, "Method is not implemented"));

        public override Task<LogResponse> Log(LogRequest request, ServerCallContext context) =>
            throw new RpcException(new Status(StatusCode.Unimplemented, "Method is not implemented"));

        public override Task<GetCloudMetadataResponse>
            GetCloudMetadata(GetCloudMetadataRequest request, ServerCallContext context) =>
            throw new RpcException(new Status(StatusCode.Unimplemented, "Method is not implemented"));
    }
}
