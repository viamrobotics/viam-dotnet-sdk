using System.Buffers;
using Microsoft.Extensions.Logging;
using Viam.Client.Clients;
using Viam.Client.Dialing;

namespace Viam.Core.Test
{
    public class ClientTests
    {
        private DialOptions? _robotClientOptions;
        private DialOptions? _cloudClientOptions;
        private ILoggerFactory? _loggerFactory;

        [SetUp]
        public void Setup()
        {
            _loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddConsole();
                builder.SetMinimumLevel(LogLevel.Trace);
            });

            var machineAddress = Environment.GetEnvironmentVariable("ROBOT_ADDRESS")
                              ?? throw new InvalidOperationException("Missing Environment Variable");
            var apiKey = Environment.GetEnvironmentVariable("VIAM_API_KEY") ?? throw new InvalidOperationException("Missing Environment Variable");
            var apiKeyId = Environment.GetEnvironmentVariable("VIAM_API_KEY_ID")
                        ?? throw new InvalidOperationException("Missing Environment Variable");

            _robotClientOptions = DialOptions
                                .FromAddress(machineAddress)
                                .WithLogging(_loggerFactory)
                                .WithApiCredentials(apiKey, apiKeyId);

            _cloudClientOptions = DialOptions.FromCloud()
                                             .WithLogging(_loggerFactory)
                                             .WithApiCredentials(apiKey,
                                                                 apiKeyId);
        }

        [TearDown]
        public void TearDown()
        {
            _loggerFactory?.Dispose();
        }

        public class RentedArray<T>(int minimumSize) : IDisposable
        {
            private bool _disposed = false;
            private readonly T[] _array = ArrayPool<T>.Shared.Rent(minimumSize);
            public T[] Array
            {
                get
                {
                    if (_disposed) throw new ObjectDisposedException(nameof(Array));
                    return _array;
                }
            }

            public void Dispose()
            {
                _disposed = true;
                ArrayPool<T>.Shared.Return(_array);
            }
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

            var client = await ViamClient.CreateFromDialOptions(_cloudClientOptions!);
            var appClient = client.CreateAppClient();
            var parts = await appClient.GetRobotParts("659c14cd-a8f5-4d16-93be-7ab7e9ad3a7a");

            Assert.That(parts, Is.Not.Null);
            Console.WriteLine(parts);
        }
    }
}