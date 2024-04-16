using Proto.Rpc.Webrtc.V1;

namespace Viam.Net.Sdk.Core.WebRTC
{
    public interface IWebRTCClientStreamContainer
    {
        void OnResponse(Response response);
    }
}