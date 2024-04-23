using Proto.Rpc.Webrtc.V1;

namespace Viam.Core.WebRTC
{
    public interface IWebRTCClientStreamContainer
    {
        void OnResponse(Response response);
    }
}