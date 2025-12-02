using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;

using Grpc.Core;

using Microsoft.Extensions.Logging;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

using Viam.Common.V1;
using Viam.Contracts;
using Viam.Contracts.Resources;
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
using Viam.Core.Resources.Services.VisionService;
using Viam.Core.Utils;
using Viam.Robot.V1;

namespace Viam.Core.Clients
{
    public class MachineClientBase : IMachineClient
    {
        protected readonly ILogger<MachineClientBase> Logger;

        private readonly ViamChannel _channel;
        private readonly ILoggerFactory _loggerFactory;
        private readonly RobotService.RobotServiceClient _robotServiceClient;
        private readonly Dictionary<ViamResourceName, Func<IResourceBase>> _resources = new();
        private readonly SemaphoreSlim _disposeLock = new(1, 1);
        private bool _isDisposed;

        protected internal MachineClientBase(ILoggerFactory loggerFactory, ViamChannel channel)
        {
            _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
            _channel = channel ?? throw new ArgumentNullException(nameof(channel));

            Logger = loggerFactory.CreateLogger<MachineClientBase>();
            _robotServiceClient = new RobotService.RobotServiceClient(channel);
        }

        protected async Task RefreshAsync(TimeSpan? timeout = null, CancellationToken token = default, [CallerMemberName] string? caller = null)
        {
            Logger.LogManagerRefreshStart(caller);
            ThrowIfDisposed();

            var resourceNames = await ResourceNamesAsync(timeout, token);
            RegisterResources(resourceNames);

            Logger.LogManagerRefreshFinish(caller);
        }

        public async ValueTask DisposeAsync()
        {
            await _disposeLock.WaitAsync();
            try
            {
                if (_isDisposed) return;
                Logger.LogDebug("Disposing of MachineClient");
                _channel.Dispose();

                GC.SuppressFinalize(this);
                _isDisposed = true;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Failed to properly dispose of client");
            }
            finally
            {
                Logger.LogDebug("Client disposed");
                _disposeLock.Release();
            }
        }

        private void ThrowIfDisposed()
        {
            ObjectDisposedException.ThrowIf(_isDisposed, nameof(MachineClientBase));
        }

        public async Task<T> GetComponent<T>(ViamResourceName resourceName, TimeSpan? timeout = null, CancellationToken token = default) where T : IResourceBase
        {
            Logger.LogMethodInvocationStart();
            ThrowIfDisposed();
            try
            {
                await RefreshAsync(timeout ?? TimeSpan.FromSeconds(30), token).ConfigureAwait(false);
                if (_resources.TryGetValue(resourceName, out var ctor) == false)
                    throw new ComponentNotFoundException(resourceName);

                Logger.LogMethodInvocationSuccess(results: ctor);
                return (T)ctor();

            }
            catch (InvalidOperationException)
            {
                Logger.LogDebug("Component not found: {ResourceName}", resourceName);
                throw new ComponentNotFoundException(resourceName);
            }
            catch (Exception ex)
            {
                Logger.LogMethodInvocationFailure(ex);
                throw;
            }
        }

        public async Task<IResourceBase> GetComponent(ViamResourceName resourceName, TimeSpan? timeout = null, CancellationToken token = default)
        {
            Logger.LogMethodInvocationStart();
            ThrowIfDisposed();
            try
            {
                await RefreshAsync(timeout ?? TimeSpan.FromSeconds(30), token).ConfigureAwait(false);
                if (_resources.TryGetValue(resourceName, out var ctor) == false)
                    throw new ComponentNotFoundException(resourceName);

                Logger.LogMethodInvocationSuccess(results: ctor);
                return ctor();

            }
            catch (InvalidOperationException)
            {
                Logger.LogDebug("Component not found: {ResourceName}", resourceName);
                throw new ComponentNotFoundException(resourceName);
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

        public async Task LogAsync(IEnumerable<LogEntry> logs, CancellationToken ct)
        {
            Logger.LogMethodInvocationStart();
            ThrowIfDisposed();
            try
            {
                var request = new LogRequest();
                request.Logs.AddRange(logs);
                await _robotServiceClient.LogAsync(request, cancellationToken: ct);
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
#pragma warning disable CS0612 // Type or member is obsolete
                    result.RobotPartId);
#pragma warning restore CS0612 // Type or member is obsolete

                Logger.LogMethodInvocationSuccess(results: metadata);
                return metadata;
            }
            catch (Exception ex)
            {
                Logger.LogMethodInvocationFailure(ex);
                throw;
            }
        }

        public override string ToString() => $"MachineClientBase+{_channel}";

        private void RegisterResources(ViamResourceName[] resourceNames)
        {
            Logger.LogDebug("Registering resources: {ResourceCount}", resourceNames.Length);
            var filteredResourceName = resourceNames.Where(x => x.SubType.ResourceType is "component" or "service")
                .Where(x => x.SubType.ResourceSubType != "remote");
            // Register the built-in component types
            foreach (var resourceName in filteredResourceName)
            {
                if (_resources.ContainsKey(resourceName))
                {
                    Logger.LogTrace("Resource {ResourceName} already registered", resourceName);
                    continue;
                }
                Logger.LogDebug("Registering resource: {ResourceName}", resourceName);
                switch (resourceName.SubType.ResourceSubType)
                {
                    case "arm":
                        _resources.TryAdd(resourceName, () => new ArmClient(resourceName, _channel, _loggerFactory.CreateLogger<ArmClient>()));
                        break;
                    case "base":
                        _resources.TryAdd(resourceName, () => new BaseClient(resourceName, _channel, _loggerFactory.CreateLogger<BaseClient>()));
                        break;
                    case "board":
                        _resources.TryAdd(resourceName, () => new BoardClient(resourceName, _channel, _loggerFactory.CreateLogger<BoardClient>()));
                        break;
                    case "camera":
                        _resources.TryAdd(resourceName, () => new CameraClient(resourceName, _channel, _loggerFactory.CreateLogger<CameraClient>()));
                        break;
                    case "encoder":
                        _resources.TryAdd(resourceName, () => new EncoderClient(resourceName, _channel, _loggerFactory.CreateLogger<EncoderClient>()));
                        break;
                    case "gantry":
                        _resources.TryAdd(resourceName, () => new GantryClient(resourceName, _channel, _loggerFactory.CreateLogger<GantryClient>()));
                        break;
                    case "gripper":
                        _resources.TryAdd(resourceName, () => new GripperClient(resourceName, _channel, _loggerFactory.CreateLogger<GripperClient>()));
                        break;
                    case "input_controller":
                        _resources.TryAdd(resourceName, () => new InputControllerClient(resourceName, _channel, _loggerFactory.CreateLogger<InputControllerClient>()));
                        break;
                    case "motor":
                        _resources.TryAdd(resourceName, () => new MotorClient(resourceName, _channel, _loggerFactory.CreateLogger<MotorClient>()));
                        break;
                    case "movement_sensor":
                        _resources.TryAdd(resourceName, () => new MovementSensorClient(resourceName, _channel, _loggerFactory.CreateLogger<MovementSensorClient>()));
                        break;
                    case "power_sensor":
                        _resources.TryAdd(resourceName, () => new PowerSensorClient(resourceName, _channel, _loggerFactory.CreateLogger<PowerSensorClient>()));
                        break;
                    case "sensor":
                        _resources.TryAdd(resourceName, () => new SensorClient(resourceName, _channel, _loggerFactory.CreateLogger<SensorClient>()));
                        break;
                    case "servo":
                        _resources.TryAdd(resourceName, () => new ServoClient(resourceName, _channel, _loggerFactory.CreateLogger<ServoClient>()));
                        break;
                    case "base_remote_control":
                        Logger.LogDebug("BaseRemoteControl services are not yet supported by this client");
                        break;
                    case "data_manager":
                        Logger.LogDebug("DataManager services are not yet supported by this client");
                        break;
                    case "discovery":
                        Logger.LogDebug("Discovery services are not yet supported by this client");
                        break;
                    case "generic":
                        Logger.LogDebug("Generic services are not yet supported by this client");
                        break;
                    case "mlmodel":
                        Logger.LogDebug("MLModel services are not yet supported by this client");
                        break;
                    case "motion":
                        Logger.LogDebug("Motion services are not yet supported by this client");
                        break;
                    case "navigation":
                        Logger.LogDebug("Navigation services are not yet supported by this client");
                        break;
                    case "shell":
                        Logger.LogDebug("Shell services are not yet supported by this client");
                        break;
                    case "slam":
                        Logger.LogDebug("SLAM services are not yet supported by this client");
                        break;
                    case "vision":
                        _resources.TryAdd(resourceName, () => new VisionServiceClient(resourceName, _channel, _loggerFactory.CreateLogger<VisionServiceClient>()));
                        break;
                    case "world_state_store":
                        Logger.LogDebug("WorldStateStore services are not yet supported by this client");
                        break;
                    default:
                        Logger.LogWarning("Unknown resource {Resource}", resourceName);
                        break;
                }
            }
        }
    }
}