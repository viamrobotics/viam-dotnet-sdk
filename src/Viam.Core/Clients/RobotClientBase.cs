using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;

using Grpc.Core;

using Microsoft.Extensions.Logging;

using Viam.Common.V1;
using Viam.Core.Logging;
using Viam.Core.Resources;
using Viam.Core.Utils;
using Viam.Robot.V1;

using Status = Viam.Robot.V1.Status;

namespace Viam.Core.Clients
{
    public class RobotClientBase
    {
        internal readonly ViamChannel Channel;
        protected readonly ILogger<RobotClientBase> Logger;
        private readonly RobotService.RobotServiceClient _robotServiceClient;
        private readonly ResourceManager _resourceManager;

        protected internal RobotClientBase(ILoggerFactory loggerFactory, ViamChannel channel)
        {
            Channel = channel;
            Logger = loggerFactory.CreateLogger<RobotClientBase>();
            _robotServiceClient = new RobotService.RobotServiceClient(channel);
            _resourceManager = new ResourceManager(loggerFactory);
        }

        
        protected Task RefreshAsync() => _resourceManager.RefreshAsync(this);

        
        public T GetComponent<T>(ViamResourceName resourceName) where T : ResourceBase
        {
            Logger.LogMethodInvocationStart();
            try
            {
                var resource = (T)_resourceManager.GetResource(resourceName);
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

        public override string ToString() => $"RobotClientBase+{Channel}";
    }
}
