using System.Buffers;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using Proto.Rpc.Webrtc.V1;

namespace Viam.Client.WebRTC
{
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
        private IResponseListener<TResponse> _responseListener = null!;

        private bool _headersReceived;
        private bool _trailersReceived;

        private class FuncCallListener(
            Func<Status, global::Grpc.Core.Metadata, Task> onClose,
            Action<global::Grpc.Core.Metadata> onHeaders,
            Func<TResponse, Task> onMessage,
            Action onReady)
            : IResponseListener<TResponse>
        {
            public Task OnClose(Status status, global::Grpc.Core.Metadata trailers) => onClose.Invoke(status, trailers);

            public void OnHeaders(global::Grpc.Core.Metadata headers) => onHeaders.Invoke(headers);

            public Task OnMessage(TResponse message) => onMessage.Invoke(message);

            public void OnReady() => onReady.Invoke();
        }

        public AsyncUnaryCall<TResponse> UnaryCall(CallOptions options, TRequest request)
        {
            var respHeaders = new TaskCompletionSource<global::Grpc.Core.Metadata>();
            var result = new TaskCompletionSource<TResponse>();
            var statusFut = new TaskCompletionSource<Status>();
            var trailersFut = new TaskCompletionSource<global::Grpc.Core.Metadata>();

            var listener = new FuncCallListener(
                (status, md) =>
                {
                    if (status.StatusCode != StatusCode.OK)
                    {
                        var ex = new Exception($"Code={status.StatusCode} Message={status.Detail}");
                        result.SetException(ex);
                        respHeaders.SetException(ex);
                        throw ex;
                    }

                    statusFut.SetResult(status);
                    trailersFut.SetResult(md);
                    return Task.CompletedTask;
                },
                (md) =>
                {
                    respHeaders.SetResult(md);
                },
                (msg) =>
                {
                    result.SetResult(msg);
                    return Task.CompletedTask;
                },
                () =>
                {
                    logger.LogDebug("Ready!");
                }
            );

            try
            {
                Start(listener, options.Headers);
                SendMessage(request);
                HalfClose();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while sending a message");
                _baseStream.CloseWithReceiveError(ex);
                throw;
            }

            return new AsyncUnaryCall<TResponse>(
                responseAsync: result.Task,
                responseHeadersAsync: respHeaders.Task,
                getStatusFunc: () => statusFut.Task.GetAwaiter().GetResult(),
                getTrailersFunc: () => trailersFut.Task.GetAwaiter().GetResult(),
                disposeAction: () => Console.WriteLine("I HAVE BEEN DISPOSED ACTION!!!")
            );
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
                await _currentLock.WaitAsync(cancellationToken)
                                  ;

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

            public async Task<bool> SetNext(TResponse resp)
            {
                await _currentLock.WaitAsync();
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

            public async Task<bool> SetException(Exception ex)
            {
                await _currentLock.WaitAsync();
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

        public AsyncServerStreamingCall<TResponse> ServerStreamingCall(CallOptions options, TRequest request)
        {
            var ready = new TaskCompletionSource<bool>();
            var respHeaders = new TaskCompletionSource<global::Grpc.Core.Metadata>();
            var result = new TaskCompletionSource<TResponse>();
            var statusFut = new TaskCompletionSource<Status>();
            var trailersFut = new TaskCompletionSource<global::Grpc.Core.Metadata>();

            var streamReader = new AsyncStreamReader();
            // TODO(erd): need an async version?
            var listener = new FuncCallListener(
                async (status, md) =>
                {
                    if (status.StatusCode != StatusCode.OK)
                    {
                        var ex = new Exception($"Code={status.StatusCode} Message={status.Detail}");
                        ready.SetException(ex);
                        result.SetException(ex);
                        respHeaders.SetException(ex);
                        await streamReader.SetException(ex);
                    }

                    statusFut.SetResult(status);
                    trailersFut.SetResult(md);
                },
                (md) =>
                {
                    respHeaders.SetResult(md);
                },
                async (msg) =>
                {
                    await streamReader.SetNext(msg);
                },
                () =>
                {
                    logger.LogDebug("Ready!");
                }
            );

            try
            {
                Start(listener, options.Headers);
                SendMessage(request);
                HalfClose();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while sending a message");
                _baseStream.CloseWithReceiveError(ex);
            }

            return new AsyncServerStreamingCall<TResponse>(
                responseStream: streamReader,
                responseHeadersAsync: respHeaders.Task,
                getStatusFunc: () => statusFut.Task.GetAwaiter().GetResult(),
                getTrailersFunc: () => trailersFut.Task.GetAwaiter().GetResult(),
                disposeAction: () => Console.WriteLine("I HAVE BEEN DISPOSED ACTION!!!")
            );
        }

        private class ClientStreamWriter(WebRtcClientStream<TRequest, TResponse> clientStream, Task<bool> ready)
            : IClientStreamWriter<TRequest>
        {
            private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

            public WriteOptions? WriteOptions { get; set; }

            public async Task WriteAsync(TRequest message)
            {
                await ready;
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
                await ready;
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

        public AsyncClientStreamingCall<TRequest, TResponse> AsyncClientStreamingCall(CallOptions options)
        {
            var ready = new TaskCompletionSource<bool>();
            var respHeaders = new TaskCompletionSource<global::Grpc.Core.Metadata>();
            var result = new TaskCompletionSource<TResponse>();
            var statusFut = new TaskCompletionSource<Status>();
            var trailersFut = new TaskCompletionSource<global::Grpc.Core.Metadata>();

            var streamReader = new AsyncStreamReader();
            var streamWriter = new ClientStreamWriter(this, ready.Task);
            var listener = new FuncCallListener(
                async (status, md) =>
                {
                    if (status.StatusCode != StatusCode.OK)
                    {
                        var ex = new Exception($"Code={status.StatusCode} Message={status.Detail}");
                        ready.SetException(ex);
                        result.SetException(ex);
                        respHeaders.SetException(ex);
                        await streamReader.SetException(ex);
                    }

                    statusFut.SetResult(status);
                    trailersFut.SetResult(md);
                },
                (md) =>
                {
                    respHeaders.SetResult(md);
                },
                async (msg) =>
                {
                    await ready.Task;
                    result.SetResult(msg);
                },
                () =>
                {
                    ready.SetResult(true);
                }
            );

            try
            {
                Start(listener, options.Headers);
            }
            catch (Exception ex)
            {
                // TODO: Clarify this error message
                logger.LogError(ex, "An error occurred");
                _baseStream.CloseWithReceiveError(ex);
            }

            return new AsyncClientStreamingCall<TRequest, TResponse>(
                requestStream: streamWriter,
                responseAsync: result.Task,
                responseHeadersAsync: respHeaders.Task,
                getStatusFunc: () => statusFut.Task.GetAwaiter().GetResult(),
                getTrailersFunc: () => trailersFut.Task.GetAwaiter().GetResult(),
                disposeAction: () =>
                {
                    Console.WriteLine("I HAVE BEEN DISPOSED ACTION!!!");
                }
            );
        }

        public AsyncDuplexStreamingCall<TRequest, TResponse> DuplexStreamingCall(CallOptions options)
        {
            var ready = new TaskCompletionSource<bool>();
            var respHeaders = new TaskCompletionSource<global::Grpc.Core.Metadata>();
            var result = new TaskCompletionSource<TResponse>();
            var statusFut = new TaskCompletionSource<Status>();
            var trailersFut = new TaskCompletionSource<global::Grpc.Core.Metadata>();

            var streamReader = new AsyncStreamReader();
            var streamWriter = new ClientStreamWriter(this, ready.Task);
            var listener = new FuncCallListener(
                async (status, md) =>
                {
                    if (status.StatusCode != StatusCode.OK)
                    {
                        var ex = new Exception($"Code={status.StatusCode} Message={status.Detail}");
                        ready.SetException(ex);
                        result.SetException(ex);
                        respHeaders.SetException(ex);
                        await streamReader.SetException(ex);
                    }

                    statusFut.SetResult(status);
                    trailersFut.SetResult(md);
                },
                (md) =>
                {
                    respHeaders.SetResult(md);
                },
                async (msg) =>
                {
                    await ready.Task;
                    await streamReader.SetNext(msg);
                },
                () =>
                {
                    ready.SetResult(true);
                }
            );

            try
            {
                Start(listener, options.Headers);
            }
            catch (Exception ex)
            {
                // TODO: Clarify this error message
                logger.LogError(ex, "An error occurred");
                _baseStream.CloseWithReceiveError(ex);
            }

            return new AsyncDuplexStreamingCall<TRequest, TResponse>(
                requestStream: streamWriter,
                responseStream: streamReader,
                responseHeadersAsync: respHeaders.Task,
                getStatusFunc: () => statusFut.Task.GetAwaiter().GetResult(),
                getTrailersFunc: () => trailersFut.Task.GetAwaiter().GetResult(),
                disposeAction: () =>
                {
                    Console.WriteLine("I HAVE BEEN DISPOSED ACTION!!!");
                }
            );
        }

        public void Start(IResponseListener<TResponse> responseListener, global::Grpc.Core.Metadata? headers)
        {
            _responseListener = responseListener;
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

            _responseListener.OnReady();
        }

        public void Cancel(string message, Exception cause)
        {
            _baseStream.CloseWithReceiveError(new Exception(message, cause));
        }

        public void HalfClose()
        {
            WriteMessage(true, null);
        }

        private class SimpleSerializationContext : SerializationContext
        {
            public byte[] Data { get; private set; } = [];
            private readonly ArrayBufferWriter<byte> _bufferWriter = new();

            public override void Complete(byte[] payload)
            {
                Data = payload;
            }

            public override IBufferWriter<byte> GetBufferWriter() => _bufferWriter;

            public override void Complete()
            {
                Data = _bufferWriter.WrittenSpan.ToArray();
            }
        }

        private void SendMessage(TRequest message)
        {
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

                    ProcessMessage(resp.Message);
                    break;
                case Response.TypeOneofCase.Trailers:
                    ProcessTrailers(resp.Trailers);
                    break;
                default:
                    logger.LogWarning("unknown response type: {responseType}", resp.TypeCase);
                    break;
            }
        }

        // TODO(erd): synchronized
        private void ProcessHeaders(ResponseHeaders headers)
        {
            _headersReceived = true;
            var metadata = WebRtcClientChannel.ToGrpcMetadata(headers.Metadata);
            _responseListener.OnHeaders(metadata);

            // TODO(erd): need?
            // close(s.headersReceived)
        }

        private class SimpleDeserializationContext(byte[] data) : DeserializationContext
        {
            public override int PayloadLength => data.Length;

            // TODO: Determine if we really need this, as it is expensive.
            public override byte[] PayloadAsNewBuffer() => data.ToArray();

            public override ReadOnlySequence<byte> PayloadAsReadOnlySequence() => new(data);
        }

        // TODO(erd): synchronized
        private void ProcessMessage(ResponseMessage msg)
        {
            var result = _baseStream.ProcessPacketMessage(msg.PacketMessage);
            // If the result is null, the message isn't done yet
            if (result == null)
            {
                return;
            }

            // The resulting packet needs to be properly disposed of so the underlying rented array can be returned
            using (result)
            {
                var ctx = new SimpleDeserializationContext(result.Data);
                var resp = method.ResponseMarshaller.ContextualDeserializer(ctx);
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
            _responseListener.OnClose(new Status(status, msg), metadata);

            if (status == StatusCode.OK)
            {
                _baseStream.CloseWithReceiveError(null!);
                return;
            }
            _baseStream.CloseWithReceiveError(new Exception($"Code={status} Message={status}"));
        }
    }
}