using Grpc.Core;
using grpcSignalingService = Proto.Rpc.Webrtc.V1.SignalingService;

namespace Viam.ModularResources.Services
{
    public class SignalingService : grpcSignalingService.SignalingServiceBase
    {
        public override Task Call(Proto.Rpc.Webrtc.V1.CallRequest request,
                                  IServerStreamWriter<Proto.Rpc.Webrtc.V1.CallResponse> responseStream,
                                  ServerCallContext context) =>
            throw new RpcException(new Status(StatusCode.Unimplemented, "Method is not implemented"));

        public override Task<Proto.Rpc.Webrtc.V1.CallUpdateResponse> CallUpdate(
            Proto.Rpc.Webrtc.V1.CallUpdateRequest request,
            ServerCallContext context) =>
            throw new RpcException(new Status(StatusCode.Unimplemented, "Method is not implemented"));

        public override Task Answer(IAsyncStreamReader<Proto.Rpc.Webrtc.V1.AnswerResponse> requestStream,
                                    IServerStreamWriter<Proto.Rpc.Webrtc.V1.AnswerRequest> responseStream,
                                    ServerCallContext context) =>
            throw new RpcException(new Status(StatusCode.Unimplemented, "Method is not implemented"));

        public override Task<Proto.Rpc.Webrtc.V1.OptionalWebRTCConfigResponse> OptionalWebRTCConfig(
            Proto.Rpc.Webrtc.V1.OptionalWebRTCConfigRequest request,
            ServerCallContext context) =>
            throw new RpcException(new Status(StatusCode.Unimplemented, "Method is not implemented"));
    }
}
