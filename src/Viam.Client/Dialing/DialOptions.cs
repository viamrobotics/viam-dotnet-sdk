using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Viam.Client.WebRTC;
using Viam.Core.Grpc;

namespace Viam.Client.Dialing
{
    public class DialOptions
    {
        internal ILoggerFactory LoggerFactory { get; private set; } = new NullLoggerFactory();
        public string MachineAddress { get; init; }
        public Uri? SignalingAddress { get; private set; }
        public Credentials? Credentials { get; private set; }
        public bool DisableWebRtc { get; private set; }
        public bool Insecure { get; private set; }
        public int Port { get; private set; } = 8080;

        private DialOptions(string machineAddress)
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
                Insecure,
                false,
                Credentials);

        public static DialOptions FromAddress(string machineAddress) =>
            new(machineAddress);

        public static DialOptions FromCloud() =>
            new DialOptions("https://app.viam.com").WithPort(443).SetDisableWebRtc();

        public DialOptions WithApiCredentials(string apiKey, string apiKeyId)
        {
            Credentials = new ApiCredentials(apiKey, apiKeyId);
            return this;
        }

        public DialOptions WithSignalingAddress(Uri signalingAddress)
        {
            SignalingAddress = signalingAddress;
            return this;
        }

        public DialOptions WithSignalingAddress(string signalingAddress) =>
            WithSignalingAddress(new Uri(signalingAddress));

        public DialOptions SetDisableWebRtc(bool disable = true)
        {
            DisableWebRtc = disable;
            return this;
        }

        public DialOptions SetInsecure(bool insecure = true)
        {
            Insecure = insecure;
            return this;
        }

        public DialOptions WithPort(int port = 8080)
        {
            Port = port;
            return this;
        }

        public DialOptions WithLogging(ILoggerFactory loggerFactory)
        {
            LoggerFactory = loggerFactory;
            return this;
        }
    }
}