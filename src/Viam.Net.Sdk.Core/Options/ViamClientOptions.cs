using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using SIPSorcery.Net;
using Viam.Core.Clients;
using Viam.Core.Dialing;

namespace Viam.Core.Options
{
    public class ViamClientOptions
    {
        internal ILogger Logger { get; private set; } = new NullLogger<RobotClient>();
        public string MachineAddress { get; init; }
        public Uri? SignalingAddress { get; private set; }
        public Credentials? Credentials { get; private set; }
        public bool DisableWebRtc { get; private set; }
        public bool Insecure { get; private set; }
        public WebRtcOptions? WebRtcOptions { get; private set; }
        public int Port { get; private set; } = 8080;

        private ViamClientOptions(string machineAddress)
        {
            try
            {
                var uri = new Uri(machineAddress);
                MachineAddress = uri.Host;
                Insecure = uri.Scheme == "http";
            }
            catch (UriFormatException)
            {
                MachineAddress = machineAddress;
            }
        }

        private Uri MachineAddressToUri() => new($"{(Insecure ? "http://" : "https://")}{MachineAddress}:{Port}");

        internal GrpcDialOptions ToGrpcDialOptions() => new(MachineAddressToUri(), Insecure, Credentials, Port);

        internal WebRtcDialOptions ToWebRtcDialOptions() =>
            new(SignalingAddress ?? new Uri("https://app.viam.com"),
                MachineAddress,
                new(SignalingAddress ?? new Uri("https://app.viam.com"), Insecure, Credentials, 443),
                WebRtcOptions ?? WebRtcOptions.Default,
                Insecure,
                Credentials);

        public static ViamClientOptions FromAddress(string machineAddress) =>
            new(machineAddress);

        public static ViamClientOptions FromCloud() => new ViamClientOptions("https://app.viam.com").WithPort(443).WithDisableWebRtc(true);

        public ViamClientOptions WithApiCredentials(string apiKey, string apiKeyId)
        {

            Credentials = new ApiCredentials(apiKey, apiKeyId);
            return this;
        }

        public ViamClientOptions WithSignalingAddress(Uri signalingAddress)
        {
            SignalingAddress = signalingAddress;
            return this;
        }

        public ViamClientOptions WithSignalingAddress(string signalingAddress) =>
            WithSignalingAddress(new Uri(signalingAddress));

        public ViamClientOptions WithDisableWebRtc(bool disable = true)
        {
            DisableWebRtc = disable;
            return this;
        }

        public ViamClientOptions WithInsecure(bool insecure = true)
        {
            Insecure = insecure;
            return this;
        }

        public ViamClientOptions WithPort(int port = 8080)
        {
            Port = port;
            return this;
        }

        public ViamClientOptions WithLogger(ILogger logger)
        {
            Logger = logger;
            return this;
        }
    }

    public record WebRtcOptions(RTCConfiguration RtcConfig, string? ExternalAuthEntity = null, bool DisableTrickleIce = false)
    {
        private static readonly RTCConfiguration DefaultRtcConfig = new()
        {
            iceServers = [new RTCIceServer { urls = "stun:global.stun.twilio.com:3478?transport=udp" }]
        };

        public static WebRtcOptions Default = new(DefaultRtcConfig);
    }
}
