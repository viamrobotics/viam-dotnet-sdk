using Grpc.Core;

namespace Viam.Client.WebRTC
{
    internal interface IResponseListener<in TResponse>
    {
        void OnClose(Status status, Metadata trailers);
        void OnHeaders(Metadata headers);
        void OnMessage(TResponse message);
        void OnReady();
    }
}