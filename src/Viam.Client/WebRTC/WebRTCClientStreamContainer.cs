using Proto.Rpc.Webrtc.V1;

namespace Viam.Client.WebRTC
{
    public interface IWebRtcClientStreamContainer
    {
        void OnResponse(Response response);
    }
}