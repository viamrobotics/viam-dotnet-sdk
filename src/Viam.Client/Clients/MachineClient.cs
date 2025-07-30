using Microsoft.Extensions.Logging;
using Viam.Client.Dialing;
using Viam.Core;
using Viam.Core.Clients;

namespace Viam.Client.Clients
{
    /// <summary>
    /// A client for interacting with Viam Smart Machines
    /// </summary>
    public sealed class MachineClient : MachineClientBase
    {
        private MachineClient(ILoggerFactory loggerFactory, ViamChannel channel)
            : base(loggerFactory, channel)
        {
        }

        public static async ValueTask<IMachineClient> CreateFromDialOptions(DialOptions options)
        {
            var dialer = new Dialer(options.LoggerFactory);
            var channel = options.DisableWebRtc
                ? await dialer.DialGrpcDirectAsync(options.ToGrpcDialOptions())
                : await dialer.DialWebRtcDirectAsync(options.ToWebRtcDialOptions());
            var client = new MachineClient(options.LoggerFactory, channel);
            using var cts = new CancellationTokenSource(options.ConnectTimeout);
            await client.RefreshAsync(options.ConnectTimeout, cts.Token);
            return client;
        }

        public static async ValueTask<IMachineClient> WithChannel(ILoggerFactory loggerFactory, ViamChannel channel, CancellationToken ct = default)
        {
            var client = new MachineClient(loggerFactory, channel);
            await client.RefreshAsync(token: ct);
            return client;
        }
    }
}