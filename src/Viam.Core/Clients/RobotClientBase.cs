using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;

using Grpc.Core;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

using Viam.Common.V1;
using Viam.Core.Logging;
using Viam.Core.Resources;
using Viam.Core.Resources.Components;
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

using Status = Viam.Robot.V1.Status;

namespace Viam.Core.Clients
{
    public class RobotClientBase : IDisposable
    {
        protected readonly ILogger<RobotClientBase> Logger;
        private readonly RobotService.RobotServiceClient _robotServiceClient;
        private readonly IServiceProvider _services;

        private RobotClientBase(ILogger<RobotClientBase> logger, ViamChannel channel)
        {
            Logger = logger;
            _robotServiceClient = new RobotService.RobotServiceClient(channel);
            
            // This is overwritten in the other constructors, the nullability checks suck sometimes
            _services = new ServiceCollection().BuildServiceProvider();
        }

        protected internal RobotClientBase(ILogger<RobotClientBase> logger, ViamChannel channel, IServiceProvider services) 
            : this(logger, channel)
        {
            _services = services;
        }

        protected internal RobotClientBase(ILoggerFactory loggerFactory, ViamChannel channel) 
            : this(loggerFactory.CreateLogger<RobotClientBase>(), channel)
        {
            Logger = loggerFactory.CreateLogger<RobotClientBase>();
            _robotServiceClient = new RobotService.RobotServiceClient(channel);
            
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton(this);
            serviceCollection.AddSingleton<ViamChannel>(channel);
            serviceCollection.AddSingleton<ILoggerFactory>(loggerFactory);
            serviceCollection.AddSingleton(typeof(ILogger<>), typeof(Logger<>));

            // Register the built-in component types
            RegisterComponent<ArmClient>(serviceCollection);
            RegisterComponent<BaseClient>(serviceCollection);
            RegisterComponent<BoardClient>(serviceCollection);
            RegisterComponent<CameraClient>(serviceCollection);
            RegisterComponent<EncoderClient>(serviceCollection);
            RegisterComponent<GantryClient>(serviceCollection);
            RegisterComponent<GripperClient>(serviceCollection);
            RegisterComponent<InputControllerClient>(serviceCollection);
            RegisterComponent<MotorClient>(serviceCollection);
            RegisterComponent<MovementSensorClient>(serviceCollection);
            RegisterComponent<PowerSensorClient>(serviceCollection);
            RegisterComponent<SensorClient>(serviceCollection);
            RegisterComponent<ServoClient>(serviceCollection);

            // Now we can build the provider
            _services = serviceCollection.BuildServiceProvider();
        }

        private void RegisterComponent<TImpl>(IServiceCollection services) where TImpl : class, IResourceBase
        {
            services.AddTransient<IResourceBase, TImpl>();
            services.AddTransient<TImpl>();
        }

        protected async Task RefreshAsync([CallerMemberName] string? caller = null)
        {
            Logger.LogManagerRefreshStart(caller);
            var resourceNames = await ResourceNamesAsync();
            Logger.LogDebug("Filtering {ResourceCount} resources", resourceNames.Length);
            var filteredResourceName = resourceNames.Where(x => x.SubType.ResourceType is "component" or "service")
                                   .Where(x => x.SubType.ResourceSubType != "remote")
                                   .Where(x => x.SubType.ResourceSubType != SensorClient.SubType.ResourceSubType
                                            || !resourceNames.Contains(new ViamResourceName(MovementSensorClient.SubType, x.Name)));
            Logger.LogDebug("Refreshing client for {ResourceCount} resources", filteredResourceName.Count());
            foreach (var resourceName in filteredResourceName)
            {
            }

            Logger.LogManagerRefreshFinish(caller);
        }

        public void Dispose()
        {
            if (_services == null)
            {
                return;
            }
            if (_services is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
        
        public T GetComponent<T>(ViamResourceName resourceName) where T : IResourceBase
        {
            Logger.LogMethodInvocationStart();
            try
            {
                var resource = ActivatorUtilities.CreateInstance<T>(_services, resourceName);
                Logger.LogMethodInvocationSuccess();
                return resource;
            }
            catch (Exception ex)
            {
                Logger.LogMethodInvocationFailure(ex);
                throw;
            }
        }

        public async Task<Operation[]> GetOperationsAsync(TimeSpan? timeout = null, CancellationToken cancellationToken = default)
        {
            Logger.LogMethodInvocationStart();
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
        
        public async Task<Session[]> GetSessionsAsync(TimeSpan? timeout = null, CancellationToken cancellationToken = default)
        {
            Logger.LogMethodInvocationStart();
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
        
        public async Task<ViamResourceName[]> ResourceNamesAsync(TimeSpan? timeout = null, CancellationToken cancellationToken = default)
        {
            Logger.LogMethodInvocationStart();
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
        
        public async Task<ResourceRPCSubtype[]> GetResourceRpcSubTypesAsync(TimeSpan? timeout = null, CancellationToken cancellationToken = default)
        {
            Logger.LogMethodInvocationStart();
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
        
        public async Task CancelOperationAsync(string operationId, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
        {
            Logger.LogMethodInvocationStart(parameters: operationId);
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
        
        public async Task BlockForOperationAsync(string operationId, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
        {
            Logger.LogMethodInvocationStart(parameters: operationId);
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

        public async Task<Discovery[]> DiscoverComponentsAsync(DiscoveryQuery[]? queries = null, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
        {
            Logger.LogMethodInvocationStart(parameters: queries);
            try
            {
                var query = new DiscoverComponentsRequest();
                if (queries != null)
                {
                    query.Queries.AddRange(queries);
                }

                var result = await _robotServiceClient.DiscoverComponentsAsync(query,
                                                                               deadline: timeout.ToDeadline(),
                                                                               cancellationToken: cancellationToken)
                                                      .ConfigureAwait(false);

                Logger.LogMethodInvocationSuccess(results: result.Discovery.Count);
                return result.Discovery.ToArray();
            }
            catch (Exception ex)
            {
                Logger.LogMethodInvocationFailure(ex);
                throw;
            }
        }

        public async Task<FrameSystemConfig[]> FrameSystemConfigAsync(Transform[]? supplementalTransforms = null, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
        {
            Logger.LogMethodInvocationStart(parameters: supplementalTransforms);
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

        public async Task<PoseInFrame> TransformPoseAsync(string destination, PoseInFrame source, Transform[] supplementalTransforms, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
        {
            Logger.LogMethodInvocationStart(parameters: [destination, source, supplementalTransforms]);
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

        public async Task<ByteString> TransformPcdAsync(string source, string destination, ByteString pointCloudPcd, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
        {
            Logger.LogMethodInvocationStart(parameters:[source, destination]);
            try
            {
                var request = new TransformPCDRequest()
                              {
                                  Source = source, Destination = destination, PointCloudPcd = pointCloudPcd
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

        public async Task<Status[]> GetStatusAsync(ViamResourceName[]? resourceNames = null, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
        {
            Logger.LogMethodInvocationStart(parameters: resourceNames);
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

        public IAsyncStreamReader<StreamStatusResponse> StreamStatus(Duration every, ViamResourceName[]? resourceNames = null, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
        {
            Logger.LogMethodInvocationStart(parameters:[every, resourceNames]);
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

        public async Task StopAllAsync(StopExtraParameters[]? extraParameters = null, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
        {
            Logger.LogMethodInvocationStart(parameters: [extraParameters]);
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

        public async Task<(string Id, Duration HeartbeatWindow)> StartSessionAsync(string? resumeToken = null, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
        {
            Logger.LogMethodInvocationStart(parameters: resumeToken);
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

        public async Task SendSessionHeartbeatAsync(string sessionId, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
        {
            Logger.LogMethodInvocationStart(parameters: sessionId);
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

        public async Task<CloudMetadata> GetCloudMetadataAsync(TimeSpan? timeout = null, CancellationToken cancellationToken = default)
        {
            Logger.LogMethodInvocationStart();
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

        public override string ToString() => $"RobotClientBase+{_services.GetRequiredService<ViamChannel>()}";
    }
}
