namespace Viam.Net.Sdk.Core;

using Proto.Rpc.Webrtc.V1;

public interface WebRTCClientStreamContainer
{
    void OnResponse(Response response);
}
