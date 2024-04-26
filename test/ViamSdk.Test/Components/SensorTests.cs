using Google.Protobuf.WellKnownTypes;

using Microsoft.Extensions.Logging;

using Viam.App.V1;
using Viam.Common.V1;
using Viam.Client.Options;
using Viam.Core.Resources;
using Viam.Core.Resources.Components;

using Model = Viam.Core.Resources.Model;
using Viam.Client.Clients;

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
            var apiKey = Environment.GetEnvironmentVariable("VIAM_API_KEY")
                      ?? throw new InvalidOperationException("Missing Environment Variable");
            var apiKeyId = Environment.GetEnvironmentVariable("VIAM_API_KEY_ID")
                        ?? throw new InvalidOperationException("Missing Environment Variable");

            var robotClientOptions = ViamClientOptions
                                  .FromAddress(machineAddress)
                                  .WithLogger(loggerFactory)
                                  .WithApiCredentials(apiKey, apiKeyId)
                                  .WithDisableWebRtc();

            _robotClient = await RobotClient.AtAddressAsync(robotClientOptions);
        }

        [Test]
        public async Task Test_GetSensor()
        {
            var sensor = Sensor.FromRobot(_robotClient!, "temp");
            var readings = await sensor.GetReadings();
            Assert.That(readings, Is.Not.Null);
        }
    }
}
