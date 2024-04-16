using Microsoft.Extensions.Logging;
using Viam.Net.Sdk.Core.Clients;

namespace ViamSdk.Test
{
    public class Tests
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
                              ?? throw new InvalidOperationException();
            var apiKey = Environment.GetEnvironmentVariable("VIAM_API_KEY") ?? throw new InvalidOperationException();
            var apiKeyId = Environment.GetEnvironmentVariable("VIAM_API_KEY_ID")
                        ?? throw new InvalidOperationException();

            _robotClientOptions = ViamClientOptions
                                .FromAddress(machineAddress)
                                .WithLogger(loggerFactory.CreateLogger<ViamClient>())
                                .WithApiCredentials(apiKey, apiKeyId)
                                .WithDisableWebRtc();

            _cloudClientOptions = ViamClientOptions.FromCloud()
                                                   .WithLogger(loggerFactory.CreateLogger<ViamClient>())
                                                   .WithApiCredentials(apiKey,
                                                                       apiKeyId);
        }

        [Test]
        public async Task TestTalkGrpcToARobot()
        {
            
            var viamClient = await ViamClient.AtAddressAsync(_robotClientOptions!);
            var client = viamClient.RobotClient;
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
            var viamClient = await ViamClient.AtAddressAsync(_robotClientOptions!);
            var client = viamClient.RobotClient;

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
            
            var viamClient = await ViamClient.AtAddressAsync(_cloudClientOptions!);
            var appClient = viamClient.AppClient;
            var parts = await appClient.GetRobotPartsAsync("659c14cd-a8f5-4d16-93be-7ab7e9ad3a7a");

            Assert.That(parts, Is.Not.Null);
            Console.WriteLine(parts);
        }
    }
}