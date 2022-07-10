namespace Viam.Net.Sdk.Core;

using Grpc.Core;

public abstract class GrpcChannel : ChannelBase, IDisposable {
    protected GrpcChannel(string target) : base(target) {

    }

    public ConnectivityState State { get; }

    public abstract void Dispose();

    // Note(erd): this is a smelly hack
    internal class Wrapped : GrpcChannel {
        Grpc.Net.Client.GrpcChannel _channel;

        public Wrapped(Grpc.Net.Client.GrpcChannel channel) : base("doesnotmatter") {
            _channel = channel;
        }

        public override CallInvoker CreateCallInvoker() {
            return _channel.CreateCallInvoker();
        }

        public override void Dispose() {
            _channel.Dispose();
        }
    }
}

