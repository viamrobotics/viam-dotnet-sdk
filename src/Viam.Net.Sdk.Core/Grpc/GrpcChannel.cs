using Grpc.Core;

namespace Viam.Core.Grpc
{
    public class GrpcChannel(global::Grpc.Net.Client.GrpcChannel channel)
        : ViamChannel()
    {
        protected override CallInvoker GetCallInvoker() => channel.CreateCallInvoker();

        public override void Dispose()
        {
            channel?.Dispose();
        }
    }
}
