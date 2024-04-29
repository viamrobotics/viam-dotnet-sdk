using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using Viam.Client.Dialing;
using Viam.Client.Options;
using Viam.Core;
using Viam.Core.Clients;
using Viam.Core.Logging;

namespace Viam.Client.Clients
{
    public sealed class AppClient : AppClientBase
    {
        private AppClient(ILoggerFactory loggerFactory, ViamChannel channel)
            : base(loggerFactory, channel)
        {
        }

        /// <summary>
        /// Create an AppClient from the given <paramref name="options"/>
        /// </summary>
        /// <param name="options">The <see cref="ViamClientOptions"/> to use when creating the client</param>
        /// <returns>An instance of a <see cref="AppClient"/></returns>
        public static async ValueTask<AppClient> AtAddressAsync(ViamClientOptions options)
        {
            var dialer = new Dialer(options.LoggerFactory);
            var channel = options.DisableWebRtc
                              ? await dialer.DialGrpcDirectAsync(options.ToGrpcDialOptions())
                              : await dialer.DialWebRtcDirectAsync(options.ToWebRtcDialOptions());
            var client = new AppClient(options.LoggerFactory, channel);
            return client;
        }

        /// <summary>
        /// Create an AppClient from the given <paramref name="loggerFactory"/> and <paramref name="channel"/>
        /// </summary>
        /// <param name="loggerFactory">A <see cref="ILoggerFactory"/> used internally to create more <see cref="ILogger"/> to surface internal log messages</param>
        /// <param name="channel">The <see cref="ViamChannel"/> to provide communication to the gRPC server</param>
        /// <returns></returns>
        public static ValueTask<AppClient> WithChannel(ILoggerFactory loggerFactory, ViamChannel channel)
        {
            var client = new AppClient(loggerFactory, channel);
            return new ValueTask<AppClient>(client);
        }
    }
}
