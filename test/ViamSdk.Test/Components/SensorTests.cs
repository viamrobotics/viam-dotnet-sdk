using Google.Protobuf.WellKnownTypes;

using Microsoft.Extensions.Logging;

using Viam.App.V1;
using Viam.Common.V1;
using Viam.Core.Clients;
using Viam.Core.Options;
using Viam.Core.Resources;
using Viam.Core.Resources.Components;

using Model = Viam.Core.Resources.Model;

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
                                  .WithLogger(loggerFactory.CreateLogger<RobotClient>())
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

        [Test]
        public async Task Test_ModularSensor()
        {
            Registry.RegisterResourceCreator(Sensor.SubType,
                                             new Model(new ModelFamily("viam", "sensor"), "mySensor"),
                                             new ResourceCreatorRegistration((config, dependencies) => new ModularSensor(config, dependencies), ModularSensor.ValidateConfig));
        }

        public class ModularSensor(ComponentConfig config, IDictionary<ResourceName, ResourceBase> dependencies) : ISensor
        {
            public static string[] ValidateConfig(ComponentConfig config) => Array.Empty<string>();

            public ValueTask<IDictionary<string, object?>> DoCommand(
                IDictionary<string, object> command,
                TimeSpan? timeout = null)
            {
                if (command.TryGetValue("command", out var cmd))
                {
                    Console.WriteLine(cmd);
                }

                return new ValueTask<IDictionary<string, object?>>(new Dictionary<string, object?>());
            }

            public ValueTask<IDictionary<string, object?>> GetReadings(Struct? extra = null,
                                                                            TimeSpan? timeout = null,
                                                                            CancellationToken cancellationToken =
                                                                                default)
            {
                var dict = new Dictionary<string, object?>() { { "Hello", "World" } };
                return new ValueTask<IDictionary<string, object?>>(dict);
            }
        }
    }
}
