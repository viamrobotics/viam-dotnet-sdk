using Grpc.Core;

namespace Viam.Core.Grpc
{
    public class GrpcChannel(global::Grpc.Net.Client.GrpcChannel channel, string remote)
        : ViamChannel(remote)
    {
        protected override CallInvoker GetCallInvoker() => channel.CreateCallInvoker();

        public override void Dispose() => channel.Dispose();
    }
}