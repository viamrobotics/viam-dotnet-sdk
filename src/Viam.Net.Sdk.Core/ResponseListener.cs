namespace Viam.Net.Sdk.Core;

using Grpc.Core;

interface ResponseListener<TResponse> {
    void OnClose(Status status, Grpc.Core.Metadata trailers);
    void OnHeaders(Grpc.Core.Metadata headers);
    void OnMessage(TResponse message);
    void OnReady();
}