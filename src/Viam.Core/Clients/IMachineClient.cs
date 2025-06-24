using Google.Protobuf.WellKnownTypes;
using Google.Protobuf;
using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Viam.Common.V1;
using Viam.Core.Resources;
using Viam.Robot.V1;

namespace Viam.Core.Clients
{
    public interface IMachineClient : IAsyncDisposable
    {
        public T GetComponent<T>(ViamResourceName resourceName) where T : IResourceBase;

        public Task<Operation[]> GetOperationsAsync(TimeSpan? timeout = null,
            CancellationToken cancellationToken = default);

        public Task<Session[]> GetSessionsAsync(TimeSpan? timeout = null,
            CancellationToken cancellationToken = default);

        public Task<ViamResourceName[]> ResourceNamesAsync(TimeSpan? timeout = null,
            CancellationToken cancellationToken = default);

        public Task<ResourceRPCSubtype[]> GetResourceRpcSubTypesAsync(TimeSpan? timeout = null,
            CancellationToken cancellationToken = default);

        public Task CancelOperationAsync(string operationId, TimeSpan? timeout = null,
            CancellationToken cancellationToken = default);

        public Task BlockForOperationAsync(string operationId, TimeSpan? timeout = null,
            CancellationToken cancellationToken = default);

        public Task<FrameSystemConfig[]> FrameSystemConfigAsync(Transform[]? supplementalTransforms = null,
            TimeSpan? timeout = null, CancellationToken cancellationToken = default);

        public Task<PoseInFrame> TransformPoseAsync(string destination, PoseInFrame source,
            Transform[] supplementalTransforms, TimeSpan? timeout = null,
            CancellationToken cancellationToken = default);

        public Task<ByteString> TransformPcdAsync(string source, string destination, ByteString pointCloudPcd,
            TimeSpan? timeout = null, CancellationToken cancellationToken = default);

        [Obsolete("Pending removal from the API")]
        public Task<Robot.V1.Status[]> GetStatusAsync(ViamResourceName[]? resourceNames = null,
            TimeSpan? timeout = null,
            CancellationToken cancellationToken = default);

        [Obsolete("Pending removal from the API")]
        public IAsyncStreamReader<StreamStatusResponse> StreamStatus(Duration every,
            ViamResourceName[]? resourceNames = null, TimeSpan? timeout = null,
            CancellationToken cancellationToken = default);

        public Task StopAllAsync(StopExtraParameters[]? extraParameters = null, TimeSpan? timeout = null,
            CancellationToken cancellationToken = default);

        public Task<(string Id, Duration HeartbeatWindow)> StartSessionAsync(string? resumeToken = null,
            TimeSpan? timeout = null, CancellationToken cancellationToken = default);

        public Task SendSessionHeartbeatAsync(string sessionId, TimeSpan? timeout = null,
            CancellationToken cancellationToken = default);

        public Task LogAsync(IEnumerable<LogEntry> logs);

        public Task<CloudMetadata> GetCloudMetadataAsync(TimeSpan? timeout = null,
            CancellationToken cancellationToken = default);
    }
}
