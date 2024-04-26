using System;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading.Tasks;

using Grpc.Core;
using Grpc.Net.Client;

using Microsoft.Extensions.Logging;

using Proto.Rpc.V1;

namespace Viam.Core.Grpc
{
    public record GrpcDialOptions(Uri MachineAddress, bool Insecure = false, Viam.Core.Options.Credentials? Credentials = null, int port = 8080);

    public class GrpcDialer(ILogger<GrpcDialer> logger)
    {
        public ValueTask<ViamChannel> DialDirectAsync(GrpcDialOptions dialOptions)
        {
            logger.LogDebug("Dialing GRPC direct to {address}", dialOptions.MachineAddress);

            var channelCredentialSecurity = dialOptions.Insecure
                                                ? ChannelCredentials.Insecure
                                                : ChannelCredentials.SecureSsl;

            var channelOptions = new GrpcChannelOptions() { Credentials = channelCredentialSecurity };
            var address = dialOptions.MachineAddress;
            // Custom transport implementation for UnixDomainSockets
            if (dialOptions.MachineAddress.IsFile)
            {
                if (OperatingSystem.IsLinux() == false)
                    throw new PlatformNotSupportedException("Only Linux is supported for domain sockets.");

                var endpoint = new UnixDomainSocketEndPoint(dialOptions.MachineAddress.ToString());
                var factory = new UnixDomainSocketsConnectionFactory(endpoint);
                var handler = new SocketsHttpHandler();
                handler.ConnectCallback = factory.ConnectAsync;
                channelOptions.HttpHandler = handler;
                // This is needed to stop the GrpcChannel from trying to dial the actual file, which will fail
                address = new Uri("http://localhost");
            }

            if (dialOptions.Credentials != null)
            {
                logger.LogDebug("Setting up GRPC Auth Channel");
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

            var channel = new GrpcChannel(global::Grpc.Net.Client.GrpcChannel.ForAddress(address, channelOptions));
            return new ValueTask<ViamChannel>(channel);
        }
    }
}
