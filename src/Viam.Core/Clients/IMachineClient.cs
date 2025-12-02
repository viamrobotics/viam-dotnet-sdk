using Google.Protobuf.WellKnownTypes;
using Google.Protobuf;
using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Viam.Common.V1;
using Viam.Contracts.Resources;
using Viam.Core.Resources;
using Viam.Robot.V1;

namespace Viam.Core.Clients
{
    public interface IMachineClient : IAsyncDisposable
    {
        /// <summary>
        /// Get a component of the specified type by its resource name.
        /// </summary>
        /// <typeparam name="T">The type of the component</typeparam>
        /// <param name="resourceName">The name of the component</param>
        /// <param name="timeout">The timeout to use while constructing the component</param>
        /// <param name="token">The <see cref="CancellationToken"/> to use for the async operation</param>
        /// <returns>The <typeparamref name="T"/> component</returns>
        public Task<T> GetComponent<T>(ViamResourceName resourceName, TimeSpan? timeout = null,
            CancellationToken token = default) where T : IResourceBase;

        public Task<IResourceBase> GetComponent(ViamResourceName resourceName, TimeSpan? timeout = null,
            CancellationToken token = default);

        public Task<Operation[]> GetOperationsAsync(TimeSpan? timeout = null,
            CancellationToken token = default);

        public Task<Session[]> GetSessionsAsync(TimeSpan? timeout = null,
            CancellationToken token = default);

        public Task<ViamResourceName[]> ResourceNamesAsync(TimeSpan? timeout = null,
            CancellationToken token = default);

        public Task<ResourceRPCSubtype[]> GetResourceRpcSubTypesAsync(TimeSpan? timeout = null,
            CancellationToken token = default);

        public Task CancelOperationAsync(string operationId, TimeSpan? timeout = null,
            CancellationToken token = default);

        public Task BlockForOperationAsync(string operationId, TimeSpan? timeout = null,
            CancellationToken token = default);

        public Task<FrameSystemConfig[]> FrameSystemConfigAsync(Transform[]? supplementalTransforms = null,
            TimeSpan? timeout = null, CancellationToken token = default);

        public Task<PoseInFrame> TransformPoseAsync(string destination, PoseInFrame source,
            Transform[] supplementalTransforms, TimeSpan? timeout = null,
            CancellationToken token = default);

        public Task<ByteString> TransformPcdAsync(string source, string destination, ByteString pointCloudPcd,
            TimeSpan? timeout = null, CancellationToken token = default);

        [Obsolete("Pending removal from the API")]
        public Task<Robot.V1.Status[]> GetStatusAsync(ViamResourceName[]? resourceNames = null,
            TimeSpan? timeout = null,
            CancellationToken token = default);

        [Obsolete("Pending removal from the API")]
        public IAsyncStreamReader<StreamStatusResponse> StreamStatus(Duration every,
            ViamResourceName[]? resourceNames = null, TimeSpan? timeout = null,
            CancellationToken token = default);

        public Task StopAllAsync(StopExtraParameters[]? extraParameters = null, TimeSpan? timeout = null,
            CancellationToken token = default);

        public Task<(string Id, Duration HeartbeatWindow)> StartSessionAsync(string? resumeToken = null,
            TimeSpan? timeout = null, CancellationToken token = default);

        public Task SendSessionHeartbeatAsync(string sessionId, TimeSpan? timeout = null,
            CancellationToken token = default);

        public Task LogAsync(IEnumerable<LogEntry> logs, CancellationToken ct = default);

        public Task<CloudMetadata> GetCloudMetadataAsync(TimeSpan? timeout = null,
            CancellationToken token = default);
    }
}
