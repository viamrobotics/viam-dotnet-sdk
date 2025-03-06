using System;
using System.Net.Http;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

using Grpc.Core;
using Grpc.Net.Client;

using Microsoft.Extensions.Logging;

using Proto.Rpc.V1;

using Viam.Core.Logging;

namespace Viam.Core.Grpc
{
    public record GrpcDialOptions(
        Uri MachineAddress,
        bool Insecure = false,
        Credentials? Credentials = null,
        int Port = 8080)
    {
        public override string ToString() => $"Address: {MachineAddress}, Insecure: {Insecure}, Credentials: {Credentials}, Port: {Port}";
    }

    /// <summary>
    /// A Dialer that uses GRPC to connect to the Smart Machine
    /// </summary>
    /// <param name="logger">The <see cref="ILogger{GrpcDialer}"/> to use for state logging</param>
    internal class GrpcDialer(ILogger<GrpcDialer> logger, ILoggerFactory loggerFactory)
    {
        
        public ValueTask<ViamChannel> DialDirectAsync(GrpcDialOptions dialOptions, CancellationToken cancellationToken = default, [CallerMemberName] string? caller = null)
        {
            logger.LogDialDirect(dialOptions);
            var channelCredentialSecurity = dialOptions.Insecure
                                                ? ChannelCredentials.Insecure
                                                : ChannelCredentials.SecureSsl;

            var channelOptions = new GrpcChannelOptions()
                                 {
                                     //Credentials = channelCredentialSecurity,
                                     LoggerFactory = loggerFactory
                                 };
            var address = dialOptions.MachineAddress;
            
            // Custom transport implementation for UnixDomainSockets
            if (dialOptions.MachineAddress.IsFile)
            {
                // This is required because .NET 6.0 is the earliest version supported by the GRPC library and has domain socket support.
#if NET6_0_OR_GREATER
                var socketPath = dialOptions.MachineAddress.LocalPath;
                logger.LogDialUnixSocket(socketPath);
                if (OperatingSystem.IsLinux() == false)
                    throw new PlatformNotSupportedException("Only Linux is supported for domain sockets.");

                var endpoint = new UnixDomainSocketEndPoint(socketPath);
                var factory = new UnixDomainSocketsConnectionFactory(endpoint, logger);
                HttpMessageHandler handler = new SocketsHttpHandler()
                                             {
                                                 ConnectCallback = factory.ConnectAsync,
                                                 UseProxy = false,
                                                 AllowAutoRedirect = false
                                             };
                channelOptions.HttpHandler = handler;
                // This is needed to stop the GrpcChannel from trying to dial the actual file, which will fail
                address = new Uri($"http://localhost:9090");
#else
                throw new PlatformNotSupportedException("Unix Domain Sockets are only supported on .NET 6.0 or greater.");
#endif
            }

            if (dialOptions.Credentials != null)
            {
                logger.LogDialCreateAuthChannel();
                channelOptions.UnsafeUseInsecureChannelCallCredentials = dialOptions.Insecure;
                var callCredentials = CallCredentials.FromInterceptor(async (context, metadata) =>
                {
                    if (dialOptions.Credentials != null)
                    {
                        logger.LogDialDialingAuthChannel();
                        using var channel = global::Grpc.Net.Client.GrpcChannel.ForAddress(dialOptions.MachineAddress);
                        logger.LogDialDialingAuthChannelSuccess();

                        logger.LogDialCreateAuthClient();
                        var authClient = new AuthService.AuthServiceClient(channel);
                        logger.LogDialCreateAuthClientSuccess();

                        logger.LogDialAuthStart(dialOptions.Credentials.AuthEntity);
                        var authResponse = await authClient.AuthenticateAsync(
                                               new AuthenticateRequest()
                                               {
                                                   Entity = dialOptions.Credentials.AuthEntity,
                                                   Credentials = new Proto.Rpc.V1.Credentials()
                                                   {
                                                       Payload = dialOptions.Credentials.Payload,
                                                       Type = dialOptions.Credentials.Type
                                                   }
                                               });

                        logger.LogDialAuthSuccess(dialOptions.Credentials.AuthEntity);
                        metadata.Add("Authorization", $"Bearer {authResponse.AccessToken}");
                    }
                });

                channelOptions.Credentials = ChannelCredentials.Create(channelCredentialSecurity, callCredentials);
            }

            var channel = new GrpcChannel(global::Grpc.Net.Client.GrpcChannel.ForAddress(address, channelOptions), address.ToString());
            logger.LogDialComplete();
            return new ValueTask<ViamChannel>(channel);
        }
    }
}
