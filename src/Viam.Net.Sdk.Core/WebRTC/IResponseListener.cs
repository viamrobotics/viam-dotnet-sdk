using System.Threading.Tasks;
using Grpc.Core;

namespace Viam.Core.WebRTC
{
    internal interface IResponseListener<in TResponse>
    {
        Task OnClose(Status status, Metadata trailers);
        void OnHeaders(Metadata headers);
        Task OnMessage(TResponse message);
        void OnReady();
    }
}