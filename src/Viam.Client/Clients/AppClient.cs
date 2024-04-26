using System.Threading.Tasks;

using Google.Protobuf.Collections;

using Microsoft.Extensions.Logging;

using Viam.App.V1;
using Viam.Client.Dialing;
using Viam.Client.Options;
using Viam.Core;
using Viam.Core.Clients;

namespace Viam.Client.Clients
{
    public sealed class AppClient : AppClientBase
    {
        private AppClient(ILoggerFactory loggerFactory, ViamChannel channel) : base(loggerFactory, channel)
        {
        }

        public static async ValueTask<AppClient> AtAddressAsync(ViamClientOptions options)
        {
            var dialer = new Dialer(options.LoggerFactory);
            var channel = options.DisableWebRtc
                              ? await dialer.DialGrpcDirectAsync(options.ToGrpcDialOptions())
                              : await dialer.DialWebRtcDirectAsync(options.ToWebRtcDialOptions());
            var client = new AppClient(options.LoggerFactory, channel);
            return client;
        }

        public static ValueTask<AppClient> WithChannel(ILoggerFactory loggerFactory, ViamChannel channel)
        {
            var client = new AppClient(loggerFactory, channel);
            return new ValueTask<AppClient>(client);
        }
    }
}
