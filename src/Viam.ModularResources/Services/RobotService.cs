using Grpc.Core;
using Viam.Core.Resources.Components;
using Viam.Robot.V1;

using grpcRobotService = Viam.Robot.V1.RobotService;
using Status = Grpc.Core.Status;

namespace Viam.ModularResources.Services
{
    public class RobotService(IServiceProvider services, ILogger<RobotService> logger)
        : grpcRobotService.RobotServiceBase
    {
        public override Task<ResourceNamesResponse> ResourceNames(ResourceNamesRequest request,
            ServerCallContext context)
        {
            logger.LogError("Invoking {MethodName}", nameof(ResourceNames));
            var resp = new ResourceNamesResponse();
            return Task.FromResult(resp);
        }

        [Obsolete("Pending removal from the API")]
        public override Task<GetStatusResponse> GetStatus(GetStatusRequest request, ServerCallContext context)
        {
            logger.LogError("Invoking {MethodName}", nameof(GetStatus));
            return Task.FromResult(new GetStatusResponse());
        }

        [Obsolete("Pending removal from the API")]
        public override Task StreamStatus(StreamStatusRequest request,
            IServerStreamWriter<StreamStatusResponse> responseStream, ServerCallContext context)
        {
            throw new RpcException(new Status(StatusCode.Unimplemented,
                $"Method {nameof(StreamStatus)} is not implemented"));
        }

        public override Task<StopAllResponse> StopAll(StopAllRequest request, ServerCallContext context)
        {
            throw new RpcException(new Status(StatusCode.Unimplemented,
                $"Method {nameof(StopAll)} is not implemented"));
        }

        public override Task<GetOperationsResponse>
            GetOperations(GetOperationsRequest request, ServerCallContext context)
        {
            throw new RpcException(new Status(StatusCode.Unimplemented,
                $"Method {nameof(GetOperations)} is not implemented"));
        }

        public override Task<ResourceRPCSubtypesResponse> ResourceRPCSubtypes(
            ResourceRPCSubtypesRequest request,
            ServerCallContext context)
        {
            throw new RpcException(new Status(StatusCode.Unimplemented,
                $"Method {nameof(ResourceRPCSubtypes)} is not implemented"));
        }

        public override Task<CancelOperationResponse>
            CancelOperation(CancelOperationRequest request, ServerCallContext context)
        {
            throw new RpcException(new Status(StatusCode.Unimplemented,
                $"Method {nameof(CancelOperation)} is not implemented"));
        }

        public override Task<BlockForOperationResponse> BlockForOperation(
            BlockForOperationRequest request,
            ServerCallContext context)
        {
            throw new RpcException(new Status(StatusCode.Unimplemented,
                $"Method {nameof(BlockForOperation)} is not implemented"));
        }

        public override Task<FrameSystemConfigResponse> FrameSystemConfig(
            FrameSystemConfigRequest request,
            ServerCallContext context)
        {
            throw new RpcException(new Status(StatusCode.Unimplemented,
                $"Method {nameof(FrameSystemConfig)} is not implemented"));
        }

        public override Task<TransformPoseResponse>
            TransformPose(TransformPoseRequest request, ServerCallContext context)
        {
            throw new RpcException(new Status(StatusCode.Unimplemented,
                $"Method {nameof(TransformPose)} is not implemented"));
        }

        public override Task<GetSessionsResponse>
            GetSessions(GetSessionsRequest request, ServerCallContext context)
        {
            throw new RpcException(new Status(StatusCode.Unimplemented,
                $"Method {nameof(GetSessions)} is not implemented"));
        }

        public override Task<StartSessionResponse>
            StartSession(StartSessionRequest request, ServerCallContext context)
        {
            throw new RpcException(new Status(StatusCode.Unimplemented,
                $"Method {nameof(StartSession)} is not implemented"));
        }

        public override Task<SendSessionHeartbeatResponse> SendSessionHeartbeat(
            SendSessionHeartbeatRequest request,
            ServerCallContext context)
        {
            throw new RpcException(new Status(StatusCode.Unimplemented,
                $"Method {nameof(SendSessionHeartbeat)} is not implemented"));
        }

        public override Task<TransformPCDResponse>
            TransformPCD(TransformPCDRequest request, ServerCallContext context)
        {
            throw new RpcException(new Status(StatusCode.Unimplemented,
                $"Method {nameof(TransformPCD)} is not implemented"));
        }

        public override Task<LogResponse> Log(LogRequest request, ServerCallContext context)
        {
            throw new RpcException(new Status(StatusCode.Unimplemented, $"Method {nameof(Log)} is not implemented"));
        }

        public override Task<GetCloudMetadataResponse>
            GetCloudMetadata(GetCloudMetadataRequest request, ServerCallContext context)
        {
            throw new RpcException(new Status(StatusCode.Unimplemented,
                $"Method {nameof(GetCloudMetadata)} is not implemented"));
        }
    }
}