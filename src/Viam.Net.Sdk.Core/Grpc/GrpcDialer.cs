using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.Extensions.Logging;
using Proto.Rpc.V1;
using Viam.Core.Dialing;

namespace Viam.Core.Grpc
{
    internal class GrpcDialer(ILogger logger)
    {
        public ValueTask<ViamChannel> DialDirectAsync(GrpcDialOptions dialOptions)
        {
            logger.LogDebug("Dialing GRPC direct to {address}", dialOptions.MachineAddress);

            var channelCredentialSecurity = dialOptions.Insecure
                                                ? ChannelCredentials.Insecure
                                                : ChannelCredentials.SecureSsl;

            var channelOptions = new GrpcChannelOptions() { Credentials = channelCredentialSecurity, };

            if (dialOptions.Credentials != null)
            {
                channelOptions.UnsafeUseInsecureChannelCallCredentials = dialOptions.Insecure;
                var callCredentials = CallCredentials.FromInterceptor(async (context, metadata) =>
                {
                    if (dialOptions.Credentials != null)
                    {
                        logger.LogDebug("Creating GRPC Auth Channel");
                        using var channel = global::Grpc.Net.Client.GrpcChannel.ForAddress(dialOptions.MachineAddress);
                        logger.LogDebug("Created GRPC Auth Channel");

                        var authClient = new AuthService.AuthServiceClient(channel);
                        logger.LogDebug("Created AuthServiceClient");
                        var authResponse = await authClient.AuthenticateAsync(
                                               new AuthenticateRequest()
                                               {
                                                   Entity = dialOptions.Credentials.AuthEntity,
                                                   Credentials = new Credentials()
                                                                 {
                                                                     Payload = dialOptions.Credentials.Payload,
                                                                     Type = dialOptions.Credentials.Type
                                                                 }
                                               });

                        logger.LogDebug($"Got auth response.");
                        metadata.Add("Authorization", $"Bearer {authResponse.AccessToken}");
                    }
                });

                channelOptions.Credentials = ChannelCredentials.Create(channelCredentialSecurity, callCredentials);
            }

            var channel = new GrpcChannel(global::Grpc.Net.Client.GrpcChannel.ForAddress(dialOptions.MachineAddress, channelOptions));
            return new ValueTask<ViamChannel>(channel);
        }
    }
}
