namespace Viam.Net.Sdk.Core;

using Grpc.Core;

// Note: Using in lieu of InsecureCredentials until https://github.com/grpc/grpc-dotnet/pull/1802 is in.
internal sealed class UnsafeInsecureChannelCredentials : ChannelCredentials {

    private readonly CallCredentials CallCreds;

    internal UnsafeInsecureChannelCredentials(CallCredentials callCreds) {
        CallCreds = callCreds;
    }

    public override void InternalPopulateConfiguration(ChannelCredentialsConfiguratorBase configurator, object state) {
        configurator.SetCompositeCredentials(state, ChannelCredentials.Insecure, CallCreds);
    }
}
