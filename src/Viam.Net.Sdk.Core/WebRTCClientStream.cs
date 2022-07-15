namespace Viam.Net.Sdk.Core;

using Grpc.Core;
using Proto.Rpc.Webrtc.V1;
using System.Buffers;

class WebRTCClientStream<TRequest, TResponse> : WebRTCClientStreamContainer
{
    // see golang/client_stream.go
    private const int MAX_REQUEST_MESSAGE_PACKET_DATA_SIZE = 16373;

    private readonly Method<TRequest, TResponse> _method;
    private readonly WebRTCClientChannel _channel;
    private readonly WebRTCBaseStream _baseStream;
    private ResponseListener<TResponse> _responseListener = null!;

    private bool _headersReceived = false;
    private bool _trailersReceived = false;
    private readonly NLog.Logger _logger;


    public WebRTCClientStream(
        Method<TRequest, TResponse> method,
        Stream stream,
        WebRTCClientChannel channel,
        Action<ulong> onDone,
        NLog.Logger logger
    )
    {
        _method = method;
        _channel = channel;
        _baseStream = new WebRTCBaseStream(stream, onDone, logger);
        _logger = logger;
    }

    class FuncCallListener : ResponseListener<TResponse>
    {

        private readonly Action<Status, Grpc.Core.Metadata> _onClose;
        private readonly Action<Grpc.Core.Metadata> _onHeaders;
        private readonly Action<TResponse> _onMessage;
        private readonly Action _onReady;

        public FuncCallListener(
            Action<Status, Grpc.Core.Metadata> onClose,
            Action<Grpc.Core.Metadata> onHeaders,
            Action<TResponse> onMessage,
            Action onReady
        )
        {
            _onClose = onClose;
            _onHeaders = onHeaders;
            _onMessage = onMessage;
            _onReady = onReady;
        }

        public void OnClose(Status status, Grpc.Core.Metadata trailers)
        {
            _onClose.Invoke(status, trailers);
        }
        public void OnHeaders(Grpc.Core.Metadata headers)
        {
            _onHeaders.Invoke(headers);
        }

        public void OnMessage(TResponse message)
        {
            _onMessage.Invoke(message);
        }

        public void OnReady()
        {
            _onReady.Invoke();
        }
    }

    public AsyncUnaryCall<TResponse> UnaryCall(CallOptions options, TRequest request)
    {
        var ready = new TaskCompletionSource<bool>();
        var respHeaders = new TaskCompletionSource<Grpc.Core.Metadata>();
        var result = new TaskCompletionSource<TResponse>();
        var statusFut = new TaskCompletionSource<Status>();
        var trailersFut = new TaskCompletionSource<Grpc.Core.Metadata>();

        var listener = new FuncCallListener(
            (status, md) =>
            {
                if (status.StatusCode != StatusCode.OK)
                {
                    var ex = new Exception(String.Format("Code=%d Message=%s", status.StatusCode, status.Detail));
                    ready.SetException(ex);
                    result.SetException(ex);
                    respHeaders.SetException(ex);
                }
                statusFut.SetResult(status);
                trailersFut.SetResult(md);
            },
            (md) =>
            {
                respHeaders.SetResult(md);
            },
            (msg) =>
            {
                result.SetResult(msg);
            },
            () =>
            {
                ready.SetResult(true);
            }
        );

        // TODO(erd): asyncify
        Task.Run(async () =>
        {
            try
            {
                this.Start(listener, options.Headers);
                await ready.Task.ConfigureAwait(false);
                this.SendMessage(request);
                this.HalfClose();
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                _baseStream.CloseWithRecvError(ex);
            }
        });

        return new AsyncUnaryCall<TResponse>(
            responseAsync: result.Task,
            responseHeadersAsync: respHeaders.Task,
            getStatusFunc: () =>
            {
                return statusFut.Task.ConfigureAwait(false).GetAwaiter().GetResult();
            },
            getTrailersFunc: () =>
            {
                return trailersFut.Task.ConfigureAwait(false).GetAwaiter().GetResult();
            },
            disposeAction: () =>
            {
                Console.WriteLine("I HAVE BEEN DISPOSED ACTION!!!");
            }
        );
    }

    class AsyncStreamReader : IAsyncStreamReader<TResponse>
    {

        private bool _complete = false;
        private TResponse? _current;
        private TaskCompletionSource<TResponse> _next = new TaskCompletionSource<TResponse>();
        private readonly SemaphoreSlim _currentSema = new SemaphoreSlim(1, 1);

        public TResponse Current { get { return _current!; } }

        public async Task<bool> MoveNext(CancellationToken cancellationToken)
        {
            if (_complete)
            { // otherwise _next may throw forever as well
                return false;
            }
            _current = await _next.Task;
            if (_current == null)
            {
                _complete = true;
            }
            _next = new TaskCompletionSource<TResponse>();
            _currentSema.Release();
            return _current != null;
        }

        public async Task<bool> SetNext(TResponse resp)
        {
            await _currentSema;
            _next.SetResult(resp);
            return true;
        }

        public async Task<bool> SetException(Exception ex)
        {
            await _currentSema;
            _next.SetException(ex);
            return true;
        }
    }

    public AsyncServerStreamingCall<TResponse> ServerStreamingCall(CallOptions options, TRequest request)
    {
        var ready = new TaskCompletionSource<bool>();
        var respHeaders = new TaskCompletionSource<Grpc.Core.Metadata>();
        var result = new TaskCompletionSource<TResponse>();
        var statusFut = new TaskCompletionSource<Status>();
        var trailersFut = new TaskCompletionSource<Grpc.Core.Metadata>();

        var streamReader = new AsyncStreamReader();
        // TODO(erd): need an async version?
        var listener = new FuncCallListener(
            async (status, md) =>
            {
                if (status.StatusCode != StatusCode.OK)
                {
                    var ex = new Exception(String.Format("Code=%d Message=%s", status.StatusCode, status.Detail));
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
                ready.SetResult(true);
            }
        );

        // TODO(erd): asyncify
        Task.Run(async () =>
        {
            try
            {
                this.Start(listener, options.Headers);
                await ready.Task.ConfigureAwait(false);
                this.SendMessage(request);
                this.HalfClose();
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                _baseStream.CloseWithRecvError(ex);
            }
        });

        return new AsyncServerStreamingCall<TResponse>(
            responseStream: streamReader,
            responseHeadersAsync: respHeaders.Task,
            getStatusFunc: () =>
            {
                return statusFut.Task.ConfigureAwait(false).GetAwaiter().GetResult();
            },
            getTrailersFunc: () =>
            {
                return trailersFut.Task.ConfigureAwait(false).GetAwaiter().GetResult();
            },
            disposeAction: () =>
            {
                Console.WriteLine("I HAVE BEEN DISPOSED ACTION!!!");
            }
        );
    }

    class ClientStreamWriter : IClientStreamWriter<TRequest>
    {

        private readonly WebRTCClientStream<TRequest, TResponse> _clientStream;
        private readonly SemaphoreSlim _currentSema = new SemaphoreSlim(1, 1);
        private readonly Task<bool> _ready;

        public ClientStreamWriter(WebRTCClientStream<TRequest, TResponse> clientStream, Task<bool> ready)
        {
            _clientStream = clientStream;
            _ready = ready;
        }

        public WriteOptions? WriteOptions { get; set; }

        public async Task WriteAsync(TRequest message)
        {
            await WriteAsync(message, null);
        }

        // TODO(erd): use cancellationToken
        public async Task WriteAsync(TRequest message, CancellationToken? cancellationToken)
        {
            await _ready.ConfigureAwait(false);
            using (await _currentSema)
            {
                _clientStream.SendMessage(message);
                return;
            }
        }

        public async Task CompleteAsync()
        {
            await _ready.ConfigureAwait(false);
            using (await _currentSema)
            {
                _clientStream.HalfClose();
                return;
            }
        }
    }

    public AsyncClientStreamingCall<TRequest, TResponse> AsyncClientStreamingCall(CallOptions options)
    {
        var ready = new TaskCompletionSource<bool>();
        var respHeaders = new TaskCompletionSource<Grpc.Core.Metadata>();
        var result = new TaskCompletionSource<TResponse>();
        var statusFut = new TaskCompletionSource<Status>();
        var trailersFut = new TaskCompletionSource<Grpc.Core.Metadata>();

        var streamReader = new AsyncStreamReader();
        var streamWriter = new ClientStreamWriter(this, ready.Task);
        var listener = new FuncCallListener(
            async (status, md) =>
            {
                if (status.StatusCode != StatusCode.OK)
                {
                    var ex = new Exception(String.Format("Code=%d Message=%s", status.StatusCode, status.Detail));
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
                await ready.Task.ConfigureAwait(false);
                result.SetResult(msg);
            },
            () =>
            {
                ready.SetResult(true);
            }
        );

        // TODO(erd): asyncify
        Task.Run(() =>
        {
            try
            {
                this.Start(listener, options.Headers);
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                _baseStream.CloseWithRecvError(ex);
            }
        });

        return new AsyncClientStreamingCall<TRequest, TResponse>(
            requestStream: streamWriter,
            responseAsync: result.Task,
            responseHeadersAsync: respHeaders.Task,
            getStatusFunc: () =>
            {
                return statusFut.Task.ConfigureAwait(false).GetAwaiter().GetResult();
            },
            getTrailersFunc: () =>
            {
                return trailersFut.Task.ConfigureAwait(false).GetAwaiter().GetResult();
            },
            disposeAction: () =>
            {
                Console.WriteLine("I HAVE BEEN DISPOSED ACTION!!!");
            }
        );
    }

    public AsyncDuplexStreamingCall<TRequest, TResponse> DuplexStreamingCall(CallOptions options)
    {
        var ready = new TaskCompletionSource<bool>();
        var respHeaders = new TaskCompletionSource<Grpc.Core.Metadata>();
        var result = new TaskCompletionSource<TResponse>();
        var statusFut = new TaskCompletionSource<Status>();
        var trailersFut = new TaskCompletionSource<Grpc.Core.Metadata>();

        var streamReader = new AsyncStreamReader();
        var streamWriter = new ClientStreamWriter(this, ready.Task);
        var listener = new FuncCallListener(
            async (status, md) =>
            {
                if (status.StatusCode != StatusCode.OK)
                {
                    var ex = new Exception(String.Format("Code=%d Message=%s", status.StatusCode, status.Detail));
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
                await ready.Task.ConfigureAwait(false);
                await streamReader.SetNext(msg);
            },
            () =>
            {
                ready.SetResult(true);
            }
        );

        // TODO(erd): asyncify
        Task.Run(() =>
        {
            try
            {
                this.Start(listener, options.Headers);
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                _baseStream.CloseWithRecvError(ex);
            }
        });

        return new AsyncDuplexStreamingCall<TRequest, TResponse>(
            requestStream: streamWriter,
            responseStream: streamReader,
            responseHeadersAsync: respHeaders.Task,
            getStatusFunc: () =>
            {
                return statusFut.Task.ConfigureAwait(false).GetAwaiter().GetResult();
            },
            getTrailersFunc: () =>
            {
                return trailersFut.Task.ConfigureAwait(false).GetAwaiter().GetResult();
            },
            disposeAction: () =>
            {
                Console.WriteLine("I HAVE BEEN DISPOSED ACTION!!!");
            }
        );
    }

    public void Start(ResponseListener<TResponse> responseListener, Grpc.Core.Metadata? headers)
    {
        _responseListener = responseListener;
        var requestHeaders = new RequestHeaders
        {
            Method = _method.FullName,
            Metadata = WebRTCClientChannel.FromGRPCMetadata(headers)
        };
        try
        {
            _channel.WriteHeaders(_baseStream._stream, requestHeaders);
        }
        catch (Exception ex)
        {
            _logger.Warn("error writing headers: " + ex);
            _baseStream.CloseWithRecvError(ex);
        }

        _responseListener.OnReady();
    }

    public void Cancel(string message, Exception cause)
    {
        _baseStream.CloseWithRecvError(new Exception(message, cause));
    }

    public void HalfClose()
    {
        WriteMessage(true, null);
    }

    class SimpleSerializationContext : SerializationContext
    {

        public byte[] Data { get; set; } = new byte[] { };
        private readonly ArrayBufferWriter<byte> bufferWriter = new ArrayBufferWriter<byte>();

        public override void Complete(byte[] payload)
        {
            Data = payload;
        }

        public override IBufferWriter<byte> GetBufferWriter()
        {
            return bufferWriter;
        }

        public override void Complete()
        {
            Data = bufferWriter.WrittenSpan.ToArray();
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

            using var stream = new MemoryStream();
            var ctx = new SimpleSerializationContext();
            _method.RequestMarshaller.ContextualSerializer.Invoke(message, ctx);
            WriteMessage(false, ctx.Data.ToList());
        }
        catch (Exception ex)
        {
            // TODO(erd): make sure another send can't happen after close...
            // TODO(erd): why does this not propagate to user when this happens? need to set right fut?
            _baseStream.CloseWithRecvError(ex);
        }
    }

    private void WriteMessage(bool eos, List<Byte>? msgBytes)
    {
        if (msgBytes == null || msgBytes.Count == 0)
        {
            var packet = new PacketMessage { Eom = true };
            var requestMessage = new RequestMessage
            {
                HasMessage = msgBytes != null,
                PacketMessage = packet,
                Eos = eos
            };
            _channel.WriteMessage(_baseStream._stream, requestMessage);
            return;
        }

        while (msgBytes.Count != 0)
        {
            var amountToSend = Math.Min(msgBytes.Count, MAX_REQUEST_MESSAGE_PACKET_DATA_SIZE);
            var packet = new PacketMessage
            {
                Data = Google.Protobuf.ByteString.CopyFrom(msgBytes.GetRange(0, amountToSend).ToArray()),
            };
            msgBytes = msgBytes.GetRange(amountToSend..^0);
            if (msgBytes.Count == 0)
            {
                packet.Eom = true;
            }
            var requestMessage = new RequestMessage
            {
                HasMessage = true,
                PacketMessage = packet,
                Eos = eos
            };
            _channel.WriteMessage(_baseStream._stream, requestMessage);
        }
    }

    public void OnResponse(Response resp)
    {
        switch (resp.TypeCase)
        {
            case Response.TypeOneofCase.Headers:
                if (_headersReceived)
                {
                    _baseStream.CloseWithRecvError(new Exception("headers already received"));
                    return;
                }
                if (_trailersReceived)
                {
                    _baseStream.CloseWithRecvError(new Exception("headers received after trailers"));
                    return;
                }
                ProcessHeaders(resp.Headers);
                break;
            case Response.TypeOneofCase.Message:
                if (!_headersReceived)
                {
                    _baseStream.CloseWithRecvError(new Exception("headers not yet received"));
                    return;
                }
                if (_trailersReceived)
                {
                    _baseStream.CloseWithRecvError(new Exception("headers received after trailers"));
                    return;
                }
                ProcessMessage(resp.Message);
                break;
            case Response.TypeOneofCase.Trailers:
                ProcessTrailers(resp.Trailers);
                break;
            default:
                _logger.Warn("unknown response type: " + resp.TypeCase.ToString());
                break;
        }
    }

    // TODO(erd): synchronized
    private void ProcessHeaders(ResponseHeaders headers)
    {
        _headersReceived = true;
        var metadata = WebRTCClientChannel.ToGRPCMetadata(headers.Metadata);
        _responseListener.OnHeaders(metadata);
        // TODO(erd): need?
        // close(s.headersReceived)
    }

    class SimpleDeserializationContext : DeserializationContext
    {

        private readonly byte[] _data;

        public SimpleDeserializationContext(byte[] data)
        {
            _data = data;
        }

        public override int PayloadLength { get { return _data.Length; } }

        public override byte[] PayloadAsNewBuffer()
        {
            return _data.ToArray();
        }

        public override System.Buffers.ReadOnlySequence<byte> PayloadAsReadOnlySequence()
        {
            return new ReadOnlySequence<byte>(_data);
        }
    }

    // TODO(erd): synchronized
    private void ProcessMessage(ResponseMessage msg)
    {
        var result = _baseStream.ProcessPacketMessage(msg.PacketMessage);
        if (result == null)
        {
            return;
        }
        var ctx = new SimpleDeserializationContext(result.ToArray());
        var resp = _method.ResponseMarshaller.ContextualDeserializer(ctx);
        _responseListener.OnMessage(resp);
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

        var metadata = WebRTCClientChannel.ToGRPCMetadata(trailers.Metadata);
        _responseListener.OnClose(new Status(status, msg), metadata);

        if (status == StatusCode.OK)
        {
            _baseStream.CloseWithRecvError(null!);
            return;
        }
        _baseStream.CloseWithRecvError(new Exception(String.Format("Code=%d Message=%s", status, msg)));
    }
}