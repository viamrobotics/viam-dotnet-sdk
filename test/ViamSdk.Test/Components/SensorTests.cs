using Microsoft.Extensions.Logging;
using Viam.Net.Sdk.Core.Clients;
using Viam.Net.Sdk.Core.Options;
using Viam.Net.Sdk.Core.Resources.Components;

namespace ViamSdk.Test.Components
{
    internal class SensorTests
    {
        private RobotClient? _robotClient;
        
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
            var apiKey = Environment.GetEnvironmentVariable("VIAM_API_KEY")?? throw new InvalidOperationException("Missing Environment Variable");
            var apiKeyId = Environment.GetEnvironmentVariable("VIAM_API_KEY_ID")
                        ?? throw new InvalidOperationException("Missing Environment Variable");

            var robotClientOptions = ViamClientOptions
                                  .FromAddress(machineAddress)
                                  .WithLogger(loggerFactory.CreateLogger<RobotClient>())
                                  .WithApiCredentials(apiKey, apiKeyId)
                                  .WithDisableWebRtc();

            _robotClient = await RobotClient.AtAddressAsync(robotClientOptions);
        }

        [Test]
        public async Task GetSensor()
        {
            var sensor = Sensor.FromRobot(_robotClient!, "temp");
            var readings = await sensor.GetReadingsAsync();
            Assert.That(readings, Is.Not.Null);
        }
    }
}
