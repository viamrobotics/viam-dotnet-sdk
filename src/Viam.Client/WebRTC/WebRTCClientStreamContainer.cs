using Proto.Rpc.Webrtc.V1;

namespace Viam.Client.WebRTC
{
    public interface IWebRTCClientStreamContainer
    {
        void OnResponse(Response response);
    }
}