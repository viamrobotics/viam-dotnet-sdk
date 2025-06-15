using Microsoft.Extensions.Logging;
using Viam.Client.Clients;
using Viam.Client.Dialing;

namespace Viam.Core.Test.Components
{
    internal class BoardClientTests
    {
        private ViamMachineClient? _robotClient;

        [SetUp]
        public async Task Setup()
        {
            using var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddConsole();
                builder.SetMinimumLevel(LogLevel.Trace);
            });

            var machineAddress = Environment.GetEnvironmentVariable("ROBOT_ADDRESS")
                                 ?? throw new InvalidOperationException("Missing Environment Variable");
            var apiKey = Environment.GetEnvironmentVariable("VIAM_API_KEY") ??
                         throw new InvalidOperationException("Missing Environment Variable");
            var apiKeyId = Environment.GetEnvironmentVariable("VIAM_API_KEY_ID")
                           ?? throw new InvalidOperationException("Missing Environment Variable");

            var robotClientOptions = DialOptions
                .FromAddress(machineAddress)
                .WithLogging(loggerFactory)
                .WithApiCredentials(apiKey, apiKeyId)
                .SetDisableWebRtc();

            _robotClient = await ViamMachineClient.CreateFromDialOptions(robotClientOptions);
        }
    }
}