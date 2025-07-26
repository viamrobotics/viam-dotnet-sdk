using Google.Api;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;

using Grpc.Core;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

using Viam.Common.V1;
using Viam.Core.Logging;
using Viam.Core.Resources;
using Viam.Core.Resources.Components.Arm;
using Viam.Core.Resources.Components.Base;
using Viam.Core.Resources.Components.Board;
using Viam.Core.Resources.Components.Camera;
using Viam.Core.Resources.Components.Encoder;
using Viam.Core.Resources.Components.Gantry;
using Viam.Core.Resources.Components.Gripper;
using Viam.Core.Resources.Components.InputController;
using Viam.Core.Resources.Components.Motor;
using Viam.Core.Resources.Components.MovementSensor;
using Viam.Core.Resources.Components.PowerSensor;
using Viam.Core.Resources.Components.Sensor;
using Viam.Core.Resources.Components.Servo;
using Viam.Core.Utils;
using Viam.Robot.V1;

namespace Viam.Core.Clients
{
    public class MachineClientBase : IMachineClient
    {
        protected readonly ILogger<MachineClientBase> Logger;
        private readonly RobotService.RobotServiceClient _robotServiceClient;
        private readonly ServiceProvider _services;
        private readonly SemaphoreSlim _disposeLock = new(1, 1);
        private bool _isDisposed;

        private MachineClientBase(ILogger<MachineClientBase> logger, ViamChannel channel)
        {
            Logger = logger;
            _robotServiceClient = new RobotService.RobotServiceClient(channel);

            // This is overwritten in the other constructors, the nullability checks suck sometimes
            _services = new ServiceCollection().BuildServiceProvider();
        }

        protected internal MachineClientBase(ILogger<MachineClientBase> logger, ViamChannel channel,
            ServiceProvider services)
            : this(logger, channel)
        {
            _services = services;
        }

        protected internal MachineClientBase(ILoggerFactory loggerFactory, ViamChannel channel)
            : this(loggerFactory.CreateLogger<MachineClientBase>(), channel)
        {
            Logger = loggerFactory.CreateLogger<MachineClientBase>();
            _robotServiceClient = new RobotService.RobotServiceClient(channel);

            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton(this);
            serviceCollection.AddSingleton(channel);
            serviceCollection.AddSingleton(loggerFactory);
            serviceCollection.AddSingleton(typeof(ILogger<>), typeof(Logger<>));
            var resourceNames = _robotServiceClient.ResourceNames(new ResourceNamesRequest(),
                deadline: TimeSpan.FromSeconds(5).ToDeadline()); // Preload the resource names to ensure the channel is ready

            // Register the built-in component types
            foreach (var resourceName in resourceNames.Resources.Where(x => x.Type is "component"))
            {
                switch (resourceName.Subtype)
                {
                    case "arm":
                        serviceCollection.AddKeyedTransient<IArmClient, ArmClient>(resourceName, (_, _) => new ArmClient(resourceName, channel, loggerFactory.CreateLogger<ArmClient>()));
                        break;
                    case "base":
                        serviceCollection.AddKeyedTransient<IBaseClient, BaseClient>(resourceName, (_, _) => new BaseClient(resourceName, channel, loggerFactory.CreateLogger<BaseClient>()));
                        break;
                    case "board":
                        serviceCollection.AddKeyedTransient<IBoardClient, BoardClient>(resourceName, (_, _) => new BoardClient(resourceName, channel, loggerFactory.CreateLogger<BoardClient>()));
                        break;
                    case "camera":
                        serviceCollection.AddKeyedTransient<ICameraClient, CameraClient>(resourceName, (_, _) => new CameraClient(resourceName, channel, loggerFactory.CreateLogger<CameraClient>()));
                        break;
                    case "encoder":
                        serviceCollection.AddKeyedTransient<IEncoderClient, EncoderClient>(resourceName, (_, _) => new EncoderClient(resourceName, channel, loggerFactory.CreateLogger<EncoderClient>()));
                        break;
                    case "gantry":
                        serviceCollection.AddKeyedTransient<IGantryClient, GantryClient>(resourceName, (_, _) => new GantryClient(resourceName, channel, loggerFactory.CreateLogger<GantryClient>()));
                        break;
                    case "gripper":
                        serviceCollection.AddKeyedTransient<IGripperClient, GripperClient>(resourceName, (_, _) => new GripperClient(resourceName, channel, loggerFactory.CreateLogger<GripperClient>()));
                        break;
                    case "input_controller":
                        serviceCollection.AddKeyedTransient<IInputControllerClient, InputControllerClient>(resourceName, (_, _) => new InputControllerClient(resourceName, channel, loggerFactory.CreateLogger<InputControllerClient>()));
                        break;
                    case "motor":
                        serviceCollection.AddKeyedTransient<IMotorClient, MotorClient>(resourceName, (_, _) => new MotorClient(resourceName, channel, loggerFactory.CreateLogger<MotorClient>()));
                        break;
                    case "movement_sensor":
                        serviceCollection.AddKeyedTransient<IMovementSensorClient, MovementSensorClient>(resourceName, (_, _) => new MovementSensorClient(resourceName, channel, loggerFactory.CreateLogger<MovementSensorClient>()));
                        break;
                    case "power_sensor":
                        serviceCollection.AddKeyedTransient<IPowerSensorClient, PowerSensorClient>(resourceName, (_, _) => new PowerSensorClient(resourceName, channel, loggerFactory.CreateLogger<PowerSensorClient>()));
                        break;
                    case "sensor":
                        serviceCollection.AddKeyedTransient<ISensorClient, SensorClient>(resourceName, (_, _) => new SensorClient(resourceName, channel, loggerFactory.CreateLogger<SensorClient>()));
                        break;
                    case "servo":
                        serviceCollection.AddKeyedTransient<IServoClient, ServoClient>(resourceName, (_, _) => new ServoClient(resourceName, channel, loggerFactory.CreateLogger<ServoClient>()));
                        break;
                    default:
                        Logger.LogWarning("Unknown resource {Resource}", resourceName);
                        break;
                }

            }

            // Now we can build the provider
            _services = serviceCollection.BuildServiceProvider();
        }

        protected async Task RefreshAsync([CallerMemberName] string? caller = null)
        {
            Logger.LogManagerRefreshStart(caller);
            ThrowIfDisposed();

            var resourceNames = await ResourceNamesAsync();
            Logger.LogDebug("Filtering {ResourceCount} resources", resourceNames.Length);
            var filteredResourceName = resourceNames.Where(x => x.SubType.ResourceType is "component" or "service")
                .Where(x => x.SubType.ResourceSubType != "remote")
                .Where(x => x.SubType.ResourceSubType != SensorClient.SubType.ResourceSubType
                            || !resourceNames.Contains(new ViamResourceName(MovementSensorClient.SubType, x.Name)))
                .ToArray();
            Logger.LogDebug("Refreshing client for {ResourceCount} resources", filteredResourceName.Length);
            foreach (var resourceName in filteredResourceName)
            {
                Logger.LogDebug("Found {resourceName}", resourceName);
            }

            Logger.LogManagerRefreshFinish(caller);
        }

        public async ValueTask DisposeAsync()
        {
            await _disposeLock.WaitAsync();
            try
            {
                if (_isDisposed) return;
                Logger.LogInformation("Disposing of client");
                await _services.DisposeAsync();

                GC.SuppressFinalize(this);
                _isDisposed = true;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Failed to properly dispose of client");
            }
            finally
            {
                Logger.LogInformation("Client disposed");
                _disposeLock.Release();
            }
        }

        private void ThrowIfDisposed()
        {
            ObjectDisposedException.ThrowIf(_isDisposed, nameof(MachineClientBase));
        }

        public T GetComponent<T>(ViamResourceName resourceName) where T : IResourceBase
        {
            Logger.LogMethodInvocationStart();
            ThrowIfDisposed();
            try
            {
                var resource = _services.GetRequiredKeyedService<T>(resourceName.ToResourceName());
                Logger.LogMethodInvocationSuccess();
                return resource;
            }
            catch (Exception ex)
            {
                Logger.LogMethodInvocationFailure(ex);
                throw;
            }
        }

        public async Task<Operation[]> GetOperationsAsync(TimeSpan? timeout = null,
            CancellationToken cancellationToken = default)
        {
            Logger.LogMethodInvocationStart();
            ThrowIfDisposed();
            try
            {
                var result = await _robotServiceClient.GetOperationsAsync(new GetOperationsRequest(),
                        deadline: timeout.ToDeadline(),
                        cancellationToken: cancellationToken)
                    .ConfigureAwait(false);

                Logger.LogMethodInvocationSuccess(results: result.Operations.Count);
                return result.Operations.ToArray();
            }
            catch (Exception ex)
            {
                Logger.LogMethodInvocationFailure(ex);
                throw;
            }
        }

        public async Task<Session[]> GetSessionsAsync(TimeSpan? timeout = null,
            CancellationToken cancellationToken = default)
        {
            Logger.LogMethodInvocationStart();
            ThrowIfDisposed();
            try
            {
                var result = await _robotServiceClient.GetSessionsAsync(new GetSessionsRequest(),
                        deadline: timeout.ToDeadline(),
                        cancellationToken: cancellationToken)
                    .ConfigureAwait(false);

                Logger.LogMethodInvocationSuccess(results: result.Sessions.Count);
                return result.Sessions.ToArray();
            }
            catch (Exception ex)
            {
                Logger.LogMethodInvocationFailure(ex);
                throw;
            }
        }

        public async Task<ViamResourceName[]> ResourceNamesAsync(TimeSpan? timeout = null,
            CancellationToken cancellationToken = default)
        {
            Logger.LogMethodInvocationStart();
            ThrowIfDisposed();
            try
            {
                var result = await _robotServiceClient.ResourceNamesAsync(new ResourceNamesRequest(),
                        deadline: timeout.ToDeadline(),
                        cancellationToken: cancellationToken)
                    .ConfigureAwait(false);

                Logger.LogMethodInvocationSuccess(results: [result.Resources, result.Resources.Count]);
                return result.Resources.Select(x => new ViamResourceName(x))
                    .ToArray();
            }
            catch (Exception ex)
            {
                Logger.LogMethodInvocationFailure(ex);
                throw;
            }
        }

        public async Task<ResourceRPCSubtype[]> GetResourceRpcSubTypesAsync(TimeSpan? timeout = null,
            CancellationToken cancellationToken = default)
        {
            Logger.LogMethodInvocationStart();
            ThrowIfDisposed();
            try
            {
                var result = await _robotServiceClient.ResourceRPCSubtypesAsync(new ResourceRPCSubtypesRequest(),
                        deadline: timeout.ToDeadline(),
                        cancellationToken: cancellationToken)
                    .ConfigureAwait(false);

                Logger.LogMethodInvocationSuccess(results: result.ResourceRpcSubtypes.Count);
                return result.ResourceRpcSubtypes.ToArray();
            }
            catch (Exception ex)
            {
                Logger.LogMethodInvocationFailure(ex);
                throw;
            }
        }

        public async Task CancelOperationAsync(string operationId, TimeSpan? timeout = null,
            CancellationToken cancellationToken = default)
        {
            Logger.LogMethodInvocationStart(parameters: operationId);
            ThrowIfDisposed();
            try
            {
                await _robotServiceClient.CancelOperationAsync(new CancelOperationRequest() { Id = operationId },
                        deadline: timeout.ToDeadline(),
                        cancellationToken: cancellationToken)
                    .ConfigureAwait(false);

                Logger.LogMethodInvocationSuccess(results: operationId);
            }
            catch (Exception ex)
            {
                Logger.LogMethodInvocationFailure(ex);
                throw;
            }
        }

        public async Task BlockForOperationAsync(string operationId, TimeSpan? timeout = null,
            CancellationToken cancellationToken = default)
        {
            Logger.LogMethodInvocationStart(parameters: operationId);
            ThrowIfDisposed();
            try
            {
                await _robotServiceClient
                    .BlockForOperationAsync(new BlockForOperationRequest() { Id = operationId },
                        deadline: timeout.ToDeadline(),
                        cancellationToken: cancellationToken)
                    .ConfigureAwait(false);

                Logger.LogMethodInvocationSuccess(results: operationId);
            }
            catch (Exception ex)
            {
                Logger.LogMethodInvocationFailure(ex);
                throw;
            }
        }

        public async Task<FrameSystemConfig[]> FrameSystemConfigAsync(Transform[]? supplementalTransforms = null,
            TimeSpan? timeout = null, CancellationToken cancellationToken = default)
        {
            Logger.LogMethodInvocationStart(parameters: supplementalTransforms);
            ThrowIfDisposed();
            try
            {
                var request = new FrameSystemConfigRequest();
                if (supplementalTransforms != null)
                {
                    request.SupplementalTransforms.AddRange(supplementalTransforms);
                }

                var result = await _robotServiceClient.FrameSystemConfigAsync(request,
                        deadline: timeout.ToDeadline(),
                        cancellationToken: cancellationToken)
                    .ConfigureAwait(false);

                Logger.LogMethodInvocationSuccess(results: result.FrameSystemConfigs.Count);
                return result.FrameSystemConfigs.ToArray();
            }
            catch (Exception ex)
            {
                Logger.LogMethodInvocationFailure(ex);
                throw;
            }
        }

        public async Task<PoseInFrame> TransformPoseAsync(string destination, PoseInFrame source,
            Transform[] supplementalTransforms, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
        {
            Logger.LogMethodInvocationStart(parameters: [destination, source, supplementalTransforms]);
            ThrowIfDisposed();
            try
            {
                var request = new TransformPoseRequest() { Destination = destination, Source = source };
                request.SupplementalTransforms.AddRange(supplementalTransforms);
                var result = await _robotServiceClient.TransformPoseAsync(request,
                        deadline: timeout.ToDeadline(),
                        cancellationToken: cancellationToken)
                    .ConfigureAwait(false);

                Logger.LogMethodInvocationSuccess();
                return result.Pose;
            }
            catch (Exception ex)
            {
                Logger.LogMethodInvocationFailure(ex);
                throw;
            }
        }

        public async Task<ByteString> TransformPcdAsync(string source, string destination, ByteString pointCloudPcd,
            TimeSpan? timeout = null, CancellationToken cancellationToken = default)
        {
            Logger.LogMethodInvocationStart(parameters: [source, destination]);
            ThrowIfDisposed();
            try
            {
                var request = new TransformPCDRequest()
                {
                    Source = source,
                    Destination = destination,
                    PointCloudPcd = pointCloudPcd
                };

                var result = await _robotServiceClient.TransformPCDAsync(request,
                        deadline: timeout.ToDeadline(),
                        cancellationToken: cancellationToken)
                    .ConfigureAwait(false);

                Logger.LogMethodInvocationSuccess();
                return result.PointCloudPcd;
            }
            catch (Exception ex)
            {
                Logger.LogMethodInvocationFailure(ex);
                throw;
            }
        }

        [Obsolete("Pending removal from the API")]
        public async Task<Robot.V1.Status[]> GetStatusAsync(ViamResourceName[]? resourceNames = null, TimeSpan? timeout = null,
            CancellationToken cancellationToken = default)
        {
            Logger.LogMethodInvocationStart(parameters: resourceNames);
            ThrowIfDisposed();
            try
            {
                var request = new GetStatusRequest();
                if (resourceNames != null)
                {
                    request.ResourceNames.AddRange(resourceNames.Select(x => x.ToResourceName()));
                }

                var result = await _robotServiceClient.GetStatusAsync(request,
                        deadline: timeout.ToDeadline(),
                        cancellationToken: cancellationToken)
                    .ConfigureAwait(false);

                Logger.LogMethodInvocationSuccess(results: result.Status.Count);
                return result.Status.ToArray();
            }
            catch (Exception ex)
            {
                Logger.LogMethodInvocationFailure(ex);
                throw;
            }
        }

        [Obsolete("Pending removal from the API")]
        public IAsyncStreamReader<StreamStatusResponse> StreamStatus(Duration every,
            ViamResourceName[]? resourceNames = null, TimeSpan? timeout = null,
            CancellationToken cancellationToken = default)
        {
            Logger.LogMethodInvocationStart(parameters: [every, resourceNames]);
            ThrowIfDisposed();
            try
            {
                var request = new StreamStatusRequest() { Every = every };
                if (resourceNames != null)
                {
                    request.ResourceNames.AddRange(resourceNames.Select(x => x.ToResourceName()));
                }

                var res = _robotServiceClient.StreamStatus(request)
                    .ResponseStream;

                Logger.LogMethodInvocationSuccess();
                return res;
            }
            catch (Exception ex)
            {
                Logger.LogMethodInvocationFailure(ex);
                throw;
            }
        }

        public async Task StopAllAsync(StopExtraParameters[]? extraParameters = null, TimeSpan? timeout = null,
            CancellationToken cancellationToken = default)
        {
            Logger.LogMethodInvocationStart(parameters: [extraParameters]);
            ThrowIfDisposed();
            try
            {
                var request = new StopAllRequest();
                if (extraParameters != null)
                {
                    request.Extra.AddRange(extraParameters);
                }

                await _robotServiceClient.StopAllAsync(request,
                        deadline: timeout.ToDeadline(),
                        cancellationToken: cancellationToken)
                    .ConfigureAwait(false);

                Logger.LogMethodInvocationSuccess();
            }
            catch (Exception ex)
            {
                Logger.LogMethodInvocationFailure(ex);
                throw;
            }
        }

        public async Task<(string Id, Duration HeartbeatWindow)> StartSessionAsync(string? resumeToken = null,
            TimeSpan? timeout = null, CancellationToken cancellationToken = default)
        {
            Logger.LogMethodInvocationStart(parameters: resumeToken);
            ThrowIfDisposed();
            try
            {
                var result = await _robotServiceClient.StartSessionAsync(
                        new StartSessionRequest() { Resume = resumeToken },
                        deadline: timeout.ToDeadline(),
                        cancellationToken: cancellationToken)
                    .ConfigureAwait(false);

                Logger.LogMethodInvocationSuccess(result.Id);
                return (result.Id, result.HeartbeatWindow);
            }
            catch (Exception ex)
            {
                Logger.LogMethodInvocationFailure(ex);
                throw;
            }
        }

        public async Task SendSessionHeartbeatAsync(string sessionId, TimeSpan? timeout = null,
            CancellationToken cancellationToken = default)
        {
            Logger.LogMethodInvocationStart(parameters: sessionId);
            ThrowIfDisposed();
            try
            {
                var request = new SendSessionHeartbeatRequest() { Id = sessionId };
                await _robotServiceClient.SendSessionHeartbeatAsync(request,
                        deadline: timeout.ToDeadline(),
                        cancellationToken: cancellationToken)
                    .ConfigureAwait(false);

                Logger.LogMethodInvocationSuccess(results: sessionId);
            }
            catch (Exception ex)
            {
                Logger.LogMethodInvocationFailure(ex);
                throw;
            }
        }

        public async Task LogAsync(IEnumerable<LogEntry> logs)
        {
            Logger.LogMethodInvocationStart();
            ThrowIfDisposed();
            try
            {
                var request = new LogRequest();
                request.Logs.AddRange(logs);
                await _robotServiceClient.LogAsync(request);
                Logger.LogMethodInvocationSuccess();
            }
            catch (Exception ex)
            {
                Logger.LogMethodInvocationFailure(ex);
                throw;
            }
        }

        public async Task<CloudMetadata> GetCloudMetadataAsync(TimeSpan? timeout = null,
            CancellationToken cancellationToken = default)
        {
            Logger.LogMethodInvocationStart();
            ThrowIfDisposed();
            try
            {
                var result = await _robotServiceClient.GetCloudMetadataAsync(new GetCloudMetadataRequest(),
                        deadline: timeout.ToDeadline(),
                        cancellationToken: cancellationToken)
                    .ConfigureAwait(false);

                var metadata = new CloudMetadata(result.PrimaryOrgId,
                    result.LocationId,
                    result.MachineId,
                    result.MachinePartId,
                    result.RobotPartId);

                Logger.LogMethodInvocationSuccess(results: metadata);
                return metadata;
            }
            catch (Exception ex)
            {
                Logger.LogMethodInvocationFailure(ex);
                throw;
            }
        }

        public override string ToString() => $"MachineClientBase+{_services.GetRequiredService<ViamChannel>()}";
    }
}