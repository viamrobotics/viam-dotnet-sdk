using Microsoft.Extensions.Logging;
using Viam.Client.Dialing;
using Viam.Core;
using Viam.Core.Clients;

namespace Viam.Client.Clients
{
    public sealed class ViamClient : ViamClientBase
    {
        private ViamClient(ILoggerFactory loggerFactory, ViamChannel channel)
            : base(loggerFactory, channel)
        {
        }

        /// <summary>
        /// Create an AppClient from the given <paramref name="options"/>
        /// </summary>
        /// <param name="options">The <see cref="DialOptions"/> to use when creating the client</param>
        /// <returns>An instance of a <see cref="ViamClient"/></returns>
        public static async ValueTask<ViamClient> CreateFromDialOptions(DialOptions options)
        {
            var dialer = new Dialer(options.LoggerFactory);
            var channel = options.DisableWebRtc
                              ? await dialer.DialGrpcDirectAsync(options.ToGrpcDialOptions())
                              : await dialer.DialWebRtcDirectAsync(options.ToWebRtcDialOptions());
            var client = new ViamClient(options.LoggerFactory, channel);
            return client;
        }
    }
}
