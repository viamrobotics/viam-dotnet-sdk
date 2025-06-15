using Microsoft.Extensions.Logging;
using Viam.Client.Dialing;
using Viam.Core;
using Viam.Core.Clients;

namespace Viam.Client.Clients
{
    /// <summary>
    /// A client for interacting with Viam Smart Machines
    /// </summary>
    public sealed class ViamMachineClient : ViamMachineClientBase
    {
        private ViamMachineClient(ILoggerFactory loggerFactory, ViamChannel channel)
            : base(loggerFactory, channel)
        {
        }

        public static async ValueTask<IViamMachineClient> CreateFromDialOptions(DialOptions options)
        {
            var dialer = new Dialer(options.LoggerFactory);
            var channel = options.DisableWebRtc
                ? await dialer.DialGrpcDirectAsync(options.ToGrpcDialOptions())
                : await dialer.DialWebRtcDirectAsync(options.ToWebRtcDialOptions());
            var client = new ViamMachineClient(options.LoggerFactory, channel);
            await client.RefreshAsync();
            return client;
        }

        public static async ValueTask<IViamMachineClient> WithChannel(ILoggerFactory loggerFactory, ViamChannel channel)
        {
            var client = new ViamMachineClient(loggerFactory, channel);
            await client.RefreshAsync();
            return client;
        }
    }
}