using System.Buffers;
using System.Diagnostics;

using Grpc.Core;

using Microsoft.Extensions.Logging;

using Proto.Rpc.Webrtc.V1;

using Viam.Core.Logging;

namespace Viam.Client.WebRTC
{
    // TODO: Refactor so that _responseListener is set in the constructor
    internal class WebRtcClientStream<TRequest, TResponse>(
        Method<TRequest, TResponse> method,
        Proto.Rpc.Webrtc.V1.Stream stream,
        WebRtcClientChannel grpcChannel,
        Action<ulong> onDone,
        ILogger logger)
        : IWebRtcClientStreamContainer
        where TResponse : class
    {
        // see golang/client_stream.go
        private const int MaxRequestMessagePacketDataSize = 16373;

        private readonly WebRtcBaseStream _baseStream = new(stream, onDone, logger);
        private IResponseListener<TResponse>? _responseListener;

        private bool _headersReceived;
        private bool _trailersReceived;

        public AsyncUnaryCall<TResponse> UnaryCall(CallOptions options, TRequest request)
        {
            logger.LogMethodInvocationStart(parameters: options);
            var respHeaders = new TaskCompletionSource<global::Grpc.Core.Metadata>();
            var result = new TaskCompletionSource<TResponse>();
            var onStatus = new ManualResetEventSlim<Status>();
            var onTrailers = new ManualResetEventSlim<global::Grpc.Core.Metadata>();
            var onReady = new ManualResetEventSlim(false);

            var listener = new FuncCallListener(
                (status, md) =>
                {
                    logger.LogWebRtcClose(nameof(AsyncUnaryCall<TResponse>), status.StatusCode, status.Detail);
                    if (status.StatusCode != StatusCode.OK)
                    {
                        var ex = new Exception($"Code={status.StatusCode} Message={status.Detail}");
                        result.SetException(ex);
                        respHeaders.SetException(ex);
                        throw ex;
                    }

                    onStatus.SetResult(status);
                    onTrailers.SetResult(md);
                },
                (md) =>
                {
                    logger.LogWebRtcMetadataResponse(nameof(AsyncUnaryCall<TResponse>), md);
                    respHeaders.SetResult(md);
                },
                (msg) =>
                {
                    logger.LogWebRtcResponseReceived(nameof(AsyncUnaryCall<TResponse>));
                    logger.LogWebRtcResponseReceivedWithResponse(nameof(AsyncUnaryCall<TResponse>), msg);
                    result.SetResult(msg);
                },
                () =>
                {
                    logger.LogWebRtcCallOnReady(nameof(AsyncUnaryCall<TResponse>));
                    onReady.Set();
                }
            );

            _responseListener = listener;
            try
            {
                logger.LogWebRtcCallStartRequest(nameof(AsyncUnaryCall<TResponse>));
                Start(listener, options.Headers);

                logger.LogWebRtcWaitForOnReady(nameof(AsyncUnaryCall<TResponse>));
                onReady.Wait();
                logger.LogWebRtcWaitForOnReadySuccess(nameof(AsyncUnaryCall<TResponse>));

                SendMessage(request);

                logger.LogWebRtcCallHalfClose(nameof(AsyncUnaryCall<TResponse>));
                HalfClose();
            }
            catch (Exception ex)
            {
                logger.LogMethodInvocationFailure(ex);
                _baseStream.CloseWithReceiveError(ex);
                // Make sure this bubbles up to the caller
                throw;
            }

            logger.LogMethodInvocationSuccess();

            return new AsyncUnaryCall<TResponse>(
                responseAsync: result.Task,
                responseHeadersAsync: respHeaders.Task,
                getStatusFunc: () => onStatus.Wait(),
                getTrailersFunc: () => onTrailers.Wait(),
                disposeAction: () => logger.LogTrace("Disposing UnaryCall")
            );
        }

        public AsyncServerStreamingCall<TResponse> ServerStreamingCall(CallOptions options, TRequest request)
        {
            logger.LogMethodInvocationStart(parameters: options);
            var ready = new TaskCompletionSource<bool>();
            var respHeaders = new TaskCompletionSource<global::Grpc.Core.Metadata>();
            var result = new TaskCompletionSource<TResponse>();
            var onStatus = new ManualResetEventSlim<Status>();
            var onTrailers = new ManualResetEventSlim<global::Grpc.Core.Metadata>();
            var onReady = new ManualResetEventSlim(false);

            var streamReader = new AsyncStreamReader();
            // TODO(erd): need an async version?
            var listener = new FuncCallListener(
                (status, md) =>
                {
                    logger.LogWebRtcClose(nameof(AsyncServerStreamingCall<TResponse>), status.StatusCode, status.Detail);
                    if (status.StatusCode != StatusCode.OK)
                    {
                        var ex = new Exception($"Code={status.StatusCode} Message={status.Detail}");
                        ready.SetException(ex);
                        result.SetException(ex);
                        respHeaders.SetException(ex);
                        streamReader.SetException(ex);
                    }

                    onStatus.SetResult(status);
                    onTrailers.SetResult(md);
                },
                (md) =>
                {
                    logger.LogWebRtcMetadataResponse(nameof(AsyncServerStreamingCall<TResponse>), md);
                    respHeaders.SetResult(md);
                },
                (msg) =>
                {
                    logger.LogWebRtcResponseReceived(nameof(AsyncServerStreamingCall<TResponse>));
                    logger.LogWebRtcResponseReceivedWithResponse(nameof(AsyncServerStreamingCall<TResponse>), msg);
                    streamReader.SetNext(msg);
                },
                () =>
                {
                    logger.LogWebRtcCallOnReady(nameof(AsyncServerStreamingCall<TResponse>));
                    onReady.Set();
                }
            );

            _responseListener = listener;
            try
            {
                logger.LogWebRtcCallStartRequest(nameof(AsyncServerStreamingCall<TResponse>));
                Start(listener, options.Headers);

                logger.LogWebRtcWaitForOnReady(nameof(AsyncServerStreamingCall<TResponse>));
                onReady.Wait();
                logger.LogWebRtcWaitForOnReadySuccess(nameof(AsyncServerStreamingCall<TResponse>));

                SendMessage(request);

                logger.LogWebRtcCallHalfClose(nameof(AsyncServerStreamingCall<TResponse>));
                HalfClose();
            }
            catch (Exception ex)
            {
                logger.LogMethodInvocationFailure(ex);
                _baseStream.CloseWithReceiveError(ex);
                // Make sure this bubbles up to the caller
                throw;
            }

            logger.LogMethodInvocationSuccess();

            return new AsyncServerStreamingCall<TResponse>(
                responseStream: streamReader,
                responseHeadersAsync: respHeaders.Task,
                getStatusFunc: () => onStatus.Wait(),
                getTrailersFunc: () => onTrailers.Wait(),
                disposeAction: () => logger.LogTrace("Disposing ServerStreamingCall")
            );
        }

        public AsyncClientStreamingCall<TRequest, TResponse> AsyncClientStreamingCall(CallOptions options)
        {
            logger.LogMethodInvocationStart(parameters: options);
            var ready = new ManualResetEventSlim<bool>();
            var respHeaders = new TaskCompletionSource<global::Grpc.Core.Metadata>();
            var result = new TaskCompletionSource<TResponse>();
            var onStatus = new ManualResetEventSlim<Status>();
            var onTrailers = new ManualResetEventSlim<global::Grpc.Core.Metadata>();

            var streamReader = new AsyncStreamReader();
            var streamWriter = new ClientStreamWriter(this, ready);
            var listener = new FuncCallListener(
                (status, md) =>
                {
                    if (status.StatusCode != StatusCode.OK)
                    {
                        var ex = new Exception($"Code={status.StatusCode} Message={status.Detail}");
                        ready.SetException(ex);
                        result.SetException(ex);
                        respHeaders.SetException(ex);
                        streamReader.SetException(ex);
                    }

                    onStatus.SetResult(status);
                    onTrailers.SetResult(md);
                },
                (md) =>
                {
                    logger.LogWebRtcMetadataResponse(nameof(AsyncServerStreamingCall<TResponse>), md);
                    respHeaders.SetResult(md);
                },
                (msg) =>
                {
                    ready.Wait();
                    logger.LogWebRtcResponseReceived(nameof(AsyncUnaryCall<TResponse>));
                    logger.LogWebRtcResponseReceivedWithResponse(nameof(AsyncUnaryCall<TResponse>), msg);
                    result.SetResult(msg);
                },
                () =>
                {
                    ready.SetResult(true);
                }
            );

            _responseListener = listener;
            try
            {
                Start(listener, options.Headers);
            }
            catch (Exception ex)
            {
                logger.LogMethodInvocationFailure(ex);
                _baseStream.CloseWithReceiveError(ex);
                // Make sure this bubbles up to the caller
                throw;
            }

            logger.LogMethodInvocationSuccess();

            return new AsyncClientStreamingCall<TRequest, TResponse>(
                requestStream: streamWriter,
                responseAsync: result.Task,
                responseHeadersAsync: respHeaders.Task,
                getStatusFunc: () => onStatus.Wait(),
                getTrailersFunc: () => onTrailers.Wait(),
                disposeAction: () => logger.LogTrace("Disposing ClientStreamingCall")
            );
        }

        public AsyncDuplexStreamingCall<TRequest, TResponse> DuplexStreamingCall(CallOptions options)
        {
            logger.LogMethodInvocationStart(parameters: options);
            var ready = new ManualResetEventSlim<bool>();
            var respHeaders = new TaskCompletionSource<global::Grpc.Core.Metadata>();
            var result = new TaskCompletionSource<TResponse>();
            var onStatus = new ManualResetEventSlim<Status>();
            var onTrailers = new ManualResetEventSlim<global::Grpc.Core.Metadata>();

            var streamReader = new AsyncStreamReader();
            var streamWriter = new ClientStreamWriter(this, ready);
            var listener = new FuncCallListener(
                (status, md) =>
                {
                    if (status.StatusCode != StatusCode.OK)
                    {
                        var ex = new Exception($"Code={status.StatusCode} Message={status.Detail}");
                        ready.SetException(ex);
                        result.SetException(ex);
                        respHeaders.SetException(ex);
                        streamReader.SetException(ex);
                    }

                    onStatus.SetResult(status);
                    onTrailers.SetResult(md);
                },
                (md) =>
                {
                    logger.LogWebRtcMetadataResponse(nameof(AsyncServerStreamingCall<TResponse>), md);
                    respHeaders.SetResult(md);
                },
                (msg) =>
                {
                    ready.Wait();
                    logger.LogWebRtcResponseReceived(nameof(AsyncUnaryCall<TResponse>));
                    logger.LogWebRtcResponseReceivedWithResponse(nameof(AsyncUnaryCall<TResponse>), msg);
                    streamReader.SetNext(msg);
                },
                () =>
                {
                    ready.SetResult(true);
                }
            );

            _responseListener = listener;
            try
            {
                Start(listener, options.Headers);
            }
            catch (Exception ex)
            {
                logger.LogMethodInvocationFailure(ex);
                _baseStream.CloseWithReceiveError(ex);
                // Make sure this bubbles up to the caller
                throw;
            }

            logger.LogMethodInvocationSuccess();

            return new AsyncDuplexStreamingCall<TRequest, TResponse>(
                requestStream: streamWriter,
                responseStream: streamReader,
                responseHeadersAsync: respHeaders.Task,
                getStatusFunc: () => onStatus.Wait(),
                getTrailersFunc: () => onTrailers.Wait(),
                disposeAction: () => logger.LogTrace("Disposing DuplexStreamingCall")
            );
        }

        public void Start(IResponseListener<TResponse> responseListener, global::Grpc.Core.Metadata? headers)
        {
            var requestHeaders = new RequestHeaders
            {
                Method = method.FullName,
                Metadata = WebRtcClientChannel.FromGrpcMetadata(headers)
            };
            try
            {
                grpcChannel.WriteHeaders(_baseStream.Stream, requestHeaders);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "error writing headers");
                _baseStream.CloseWithReceiveError(ex);
            }

            responseListener.OnReady();
        }

        public void Cancel(string message, Exception cause)
        {
            _baseStream.CloseWithReceiveError(new Exception(message, cause));
        }

        public void HalfClose()
        {
            WriteMessage(true, null);
        }

        private void SendMessage(TRequest message)
        {
            logger.LogWebRtcSendMessage();
            try
            {
                if (message == null)
                {
                    WriteMessage(false, null);
                    // TODO(erd): correct to early return?
                    return;
                }

                var ctx = new SimpleSerializationContext();
                method.RequestMarshaller.ContextualSerializer.Invoke(message, ctx);
                WriteMessage(false, ctx.Data);
            }
            catch (Exception ex)
            {
                // TODO(erd): make sure another send can't happen after close...
                // TODO(erd): why does this not propagate to user when this happens? need to set right fut?
                _baseStream.CloseWithReceiveError(ex);
                throw;
            }
        }

        private void WriteMessage(bool eos, byte[]? msgBytes)
        {
            if (msgBytes == null || msgBytes.Length == 0)
            {
                var packet = new PacketMessage { Eom = true };
                var requestMessage = new RequestMessage
                {
                    HasMessage = msgBytes != null,
                    PacketMessage = packet,
                    Eos = eos
                };
                grpcChannel.WriteMessage(_baseStream.Stream, requestMessage);
                return;
            }

            while (msgBytes.Length != 0)
            {
                var amountToSend = Math.Min(msgBytes.Length, MaxRequestMessagePacketDataSize);
                var packet = new PacketMessage
                {
                    Data = Google.Protobuf.ByteString.CopyFrom(msgBytes[..amountToSend]),
                };
                msgBytes = msgBytes[amountToSend..];
                if (msgBytes.Length == 0)
                {
                    packet.Eom = true;
                }

                var requestMessage = new RequestMessage
                {
                    HasMessage = true,
                    PacketMessage = packet,
                    Eos = eos
                };
                grpcChannel.WriteMessage(_baseStream.Stream, requestMessage);
            }
        }

        public void OnResponse(Response resp)
        {
            switch (resp.TypeCase)
            {
                case Response.TypeOneofCase.Headers:
                    if (_headersReceived)
                    {
                        _baseStream.CloseWithReceiveError(new Exception("headers already received"));
                        return;
                    }

                    if (_trailersReceived)
                    {
                        _baseStream.CloseWithReceiveError(new Exception("headers received after trailers"));
                        return;
                    }

                    logger.LogDebug("Got response headers");
                    ProcessHeaders(resp.Headers);
                    break;
                case Response.TypeOneofCase.Message:
                    if (!_headersReceived)
                    {
                        _baseStream.CloseWithReceiveError(new Exception("headers not yet received"));
                        return;
                    }

                    if (_trailersReceived)
                    {
                        _baseStream.CloseWithReceiveError(new Exception("headers received after trailers"));
                        return;
                    }
                    logger.LogDebug("Got a response message");
                    ProcessMessage(resp.Message);
                    break;
                case Response.TypeOneofCase.Trailers:
                    logger.LogDebug("Got response trailers");
                    ProcessTrailers(resp.Trailers);
                    break;
                default:
                    logger.LogDebug("Got an unknown response type");
                    logger.LogWarning("unknown response type: {responseType}", resp.TypeCase);
                    break;
            }
        }

        // TODO(erd): synchronized
        private void ProcessHeaders(ResponseHeaders headers)
        {
            _headersReceived = true;
            var metadata = WebRtcClientChannel.ToGrpcMetadata(headers.Metadata);
            Debug.Assert(_responseListener != null, nameof(_responseListener) + " != null");
            _responseListener.OnHeaders(metadata);

            // TODO(erd): need?
            // close(s.headersReceived)
        }

        // TODO(erd): synchronized
        private void ProcessMessage(ResponseMessage msg)
        {
            logger.LogWebRtcProcessMessage();
            var result = _baseStream.ProcessPacketMessage(msg.PacketMessage);
            // If the result is null, the message isn't done yet
            if (result == null)
            {
                logger.LogWebRtcMessageNotDone();
                return;
            }

            // The resulting packet needs to be properly disposed of so the underlying rented array can be returned
            using (result)
            {
                var ctx = new SimpleDeserializationContext(result.Data[..result.Position]);
                var resp = method.ResponseMarshaller.ContextualDeserializer(ctx);
                logger.LogWebRtcCallingOnMessage();
                Debug.Assert(_responseListener != null, nameof(_responseListener) + " != null");
                _responseListener.OnMessage(resp);
            }
        }

        private void ProcessTrailers(ResponseTrailers trailers)
        {
            _trailersReceived = true;
            var status = StatusCode.OK;
            var msg = "";
            if (trailers.Status != null)
            {
                status = (StatusCode)trailers.Status.Code;
                msg = trailers.Status.Message;
            }

            var metadata = WebRtcClientChannel.ToGrpcMetadata(trailers.Metadata);
            Debug.Assert(_responseListener != null, nameof(_responseListener) + " != null");
            _responseListener.OnClose(new Status(status, msg), metadata);

            if (status == StatusCode.OK)
            {
                _baseStream.CloseWithReceiveError(null!);
                return;
            }
            _baseStream.CloseWithReceiveError(new Exception($"Code={status} Message={status}"));
        }

        private class FuncCallListener(
            Action<Status, global::Grpc.Core.Metadata> onClose,
            Action<global::Grpc.Core.Metadata> onHeaders,
            Action<TResponse> onMessage,
            Action onReady)
            : IResponseListener<TResponse>
        {
            public void OnClose(Status status, global::Grpc.Core.Metadata trailers) => onClose.Invoke(status, trailers);

            public void OnHeaders(global::Grpc.Core.Metadata headers) => onHeaders.Invoke(headers);

            public void OnMessage(TResponse message) => onMessage.Invoke(message);

            public void OnReady() => onReady.Invoke();
        }

        private class AsyncStreamReader : IAsyncStreamReader<TResponse>
        {
            private readonly SemaphoreSlim _currentLock = new(1, 1);

            private bool _complete;
            private TResponse? _current;
            private TaskCompletionSource<TResponse> _next = new();

            public TResponse Current => _current!;

            public async Task<bool> MoveNext(CancellationToken cancellationToken)
            {
                await _currentLock.WaitAsync(cancellationToken);

                try
                {
                    if (_complete)
                    {
                        // otherwise _next may throw forever as well
                        return false;
                    }

                    _current = await _next.Task;
                    if (_current == null)
                    {
                        _complete = true;
                    }

                    _next = new TaskCompletionSource<TResponse>();
                    return _current != null;
                }
                finally
                {
                    _currentLock.Release();
                }
            }

            public bool SetNext(TResponse resp)
            {
                _currentLock.Wait();
                try
                {
                    _next.SetResult(resp);
                    return true;
                }
                finally
                {
                    _currentLock.Release();
                }
            }

            public bool SetException(Exception ex)
            {
                _currentLock.Wait();
                try
                {
                    _next.SetException(ex);
                    return true;
                }
                finally
                {
                    _currentLock.Release();
                }
            }
        }

        private class ClientStreamWriter(WebRtcClientStream<TRequest, TResponse> clientStream, ManualResetEventSlim<bool> ready)
            : IClientStreamWriter<TRequest>
        {
            private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

            public WriteOptions? WriteOptions { get; set; }

            public async Task WriteAsync(TRequest message)
            {
                ready.Wait();
                await _semaphore.WaitAsync();
                try
                {
                    clientStream.SendMessage(message);
                }
                finally
                {
                    _semaphore.Release();
                }
            }

            public async Task CompleteAsync()
            {
                ready.Wait();
                await _semaphore.WaitAsync();
                try
                {
                    clientStream.HalfClose();
                }
                finally
                {
                    _semaphore.Release();
                }
            }
        }

        private class SimpleSerializationContext : SerializationContext
        {
            public byte[] Data { get; private set; } = [];
#if NET6_0_OR_GREATER
            private readonly ArrayBufferWriter<byte> _bufferWriter = new();
#else
            private readonly Utils.MyArrayBufferWriter<byte> _bufferWriter = new();
#endif

            public override void Complete(byte[] payload)
            {
                Data = payload;
            }

            public override IBufferWriter<byte> GetBufferWriter() => _bufferWriter;

            public override void Complete()
            {
#if NET6_0_OR_GREATER
                Data = _bufferWriter.WrittenSpan.ToArray();
#else
                Data = _bufferWriter.ToArray();
#endif
            }
        }

        private class SimpleDeserializationContext(byte[] data) : DeserializationContext
        {
            public override int PayloadLength => data.Length;

            // TODO: Determine if we really need this, as it is expensive.
            public override byte[] PayloadAsNewBuffer() => data.ToArray();

            public override ReadOnlySequence<byte> PayloadAsReadOnlySequence() => new(data);
        }
    }
}