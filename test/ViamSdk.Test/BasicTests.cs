using Microsoft.Extensions.Logging;

using Viam.Client.Clients;
using Viam.Client.Options;

namespace ViamSdk.Test
{
    public class ClientTests
    {
        private ViamClientOptions? _robotClientOptions;
        private ViamClientOptions? _cloudClientOptions;

        [SetUp]
        public void Setup()
        {
            using var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddConsole();
                builder.SetMinimumLevel(LogLevel.Trace);
            });

            var machineAddress = Environment.GetEnvironmentVariable("ROBOT_ADDRESS")
                              ?? throw new InvalidOperationException("Missing Environment Variable");
            var apiKey = Environment.GetEnvironmentVariable("VIAM_API_KEY")?? throw new InvalidOperationException("Missing Environment Variable");
            var apiKeyId = Environment.GetEnvironmentVariable("VIAM_API_KEY_ID")
                        ?? throw new InvalidOperationException("Missing Environment Variable");

            _robotClientOptions = ViamClientOptions
                                .FromAddress(machineAddress)
                                .WithLogger(loggerFactory)
                                .WithApiCredentials(apiKey, apiKeyId)
                                .WithDisableWebRtc();

            _cloudClientOptions = ViamClientOptions.FromCloud()
                                                   .WithLogger(loggerFactory)
                                                   .WithApiCredentials(apiKey,
                                                                       apiKeyId);
        }

        [Test]
        public async Task TestTalkGrpcToARobot()
        {
            var client = await RobotClient.AtAddressAsync(_robotClientOptions!);
            var resourceNames = await client.ResourceNamesAsync();
            Assert.That(resourceNames, Is.Not.Null);
            var status = await client.GetStatusAsync();
            Assert.That(status, Is.Not.Null);
            Console.WriteLine(status);
            Console.WriteLine(await client.GetCloudMetadataAsync());
        }

        [Test]
        public async Task TestTalkWebRtcToARobot()
        {
            var client = await RobotClient.AtAddressAsync(_robotClientOptions!);

            var resources = await client.ResourceNamesAsync();
            Assert.That(resources, Is.Not.Null);
            Console.WriteLine(resources);
        }

        [Test]
        public async Task TestTalkToAppViam()
        {
            using var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddConsole();
                builder.SetMinimumLevel(LogLevel.Trace);
            });
            
            var client = await AppClient.AtAddressAsync(_cloudClientOptions!);
            var parts = await client.GetRobotPartsAsync("659c14cd-a8f5-4d16-93be-7ab7e9ad3a7a");

            Assert.That(parts, Is.Not.Null);
            Console.WriteLine(parts);
        }
    }
}