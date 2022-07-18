using Proto.Rpc.Webrtc.V1;

namespace Viam.Net.Sdk.Core
{
    public interface WebRTCClientStreamContainer
    {
        void OnResponse(Response response);
    }
}