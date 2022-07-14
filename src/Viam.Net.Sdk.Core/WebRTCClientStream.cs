namespace Viam.Net.Sdk.Core;

using Grpc.Core;
using Proto.Rpc.Webrtc.V1;
using System.Buffers;

class WebRTCClientStream<TRequest, TResponse> : WebRTCClientStreamContainer {
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
    ) {
        _method = method;
        _channel = channel;
        _baseStream = new WebRTCBaseStream(stream, onDone, logger);
        _logger = logger;
    }

    class FuncCallListener : ResponseListener<TResponse> {

        private readonly Action<Status, Grpc.Core.Metadata> _onClose;
        private readonly Action<Grpc.Core.Metadata> _onHeaders;
        private readonly Action<TResponse> _onMessage;
        private readonly Action _onReady;

        public FuncCallListener(
            Action<Status, Grpc.Core.Metadata> onClose,
            Action<Grpc.Core.Metadata> onHeaders,
            Action<TResponse> onMessage,
            Action onReady
        ) {
            _onClose = onClose;
            _onHeaders = onHeaders;
            _onMessage = onMessage;
            _onReady = onReady;
        }

        public void OnClose(Status status, Grpc.Core.Metadata trailers) {
            _onClose.Invoke(status, trailers);
        }
        public void OnHeaders(Grpc.Core.Metadata headers) {
            _onHeaders.Invoke(headers);
        }

        public void OnMessage(TResponse message) {
            _onMessage.Invoke(message);
        }

        public void OnReady() {
            _onReady.Invoke();
        }
    }

    public AsyncUnaryCall<TResponse> UnaryCall(CallOptions options, TRequest request) {
        var ready = new TaskCompletionSource<bool>();
        var respHeaders = new TaskCompletionSource<Grpc.Core.Metadata>();
        var result = new TaskCompletionSource<TResponse>();
        var statusFut = new TaskCompletionSource<Status>();
        var trailersFut = new TaskCompletionSource<Grpc.Core.Metadata>();

        var listener = new FuncCallListener(
            (status, md) => {
                if (status.StatusCode != StatusCode.OK) {
                    var ex = new Exception(String.Format("Code=%d Message=%s", status.StatusCode, status.Detail));
                    ready.SetException(ex);
                    result.SetException(ex);
                    respHeaders.SetException(ex);
                }
                statusFut.SetResult(status);
                trailersFut.SetResult(md);
            },
            (md) => {
                respHeaders.SetResult(md);
            },
            (msg) => {
                result.SetResult(msg);
            },
            () => {
                ready.SetResult(true);
            }
        );

        // TODO(erd): asyncify
        Task.Run(async () => {
            try {
                this.Start(listener, options.Headers);
                await ready.Task;
                this.SendMessage(request);
                this.HalfClose();
            } catch (Exception ex) {
                _logger.Error(ex);
                _baseStream.CloseWithRecvError(ex);
            }
        });

        return new AsyncUnaryCall<TResponse>(
            responseAsync: result.Task,
            responseHeadersAsync: respHeaders.Task,
            getStatusFunc: () => {
                return statusFut.Task.GetAwaiter().GetResult();
            },
            getTrailersFunc: () => {
                return trailersFut.Task.GetAwaiter().GetResult();
            },
            disposeAction: () => {
                Console.WriteLine("I HAVE BEEN DISPOSED ACTION!!!");
            }
        );
    }

    public void Start(ResponseListener<TResponse> responseListener, Grpc.Core.Metadata? headers) {
        _responseListener = responseListener;
        var requestHeaders = new RequestHeaders {
                Method = _method.FullName,
                Metadata = WebRTCClientChannel.FromGRPCMetadata(headers)
        };
        try {
            _channel.WriteHeaders(_baseStream._stream, requestHeaders);
        } catch (Exception ex) {
            _logger.Warn("error writing headers: " + ex);
            _baseStream.CloseWithRecvError(ex);
        }

        _responseListener.OnReady();
    }

    public void Cancel(string message, Exception cause) {
        _baseStream.CloseWithRecvError(new Exception(message, cause));
    }

    public void HalfClose() {
        WriteMessage(true, null);
    }

    class SimpleSerializationContext : SerializationContext {

        public byte[] Data { get; set; } = new byte[] {};
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

    private void SendMessage(TRequest message) {
        try {
            if (message == null) {
                WriteMessage(false, null);
                // TODO(erd): correct to early return?
                return;
            }

            using var stream = new MemoryStream();
            var ctx = new SimpleSerializationContext();
            _method.RequestMarshaller.ContextualSerializer.Invoke(message, ctx);
            WriteMessage(false, ctx.Data.ToList());
        } catch (Exception ex) {
            // TODO(erd): make sure another send can't happen after close...
            // TODO(erd): why does this not propagate to user when this happens? need to set right fut?
            _baseStream.CloseWithRecvError(ex);
        }
    }

    private void WriteMessage(bool eos, List<Byte>? msgBytes) {
        if (msgBytes == null || msgBytes.Count == 0) {
            var packet = new PacketMessage { Eom = true };
            var requestMessage = new RequestMessage {
                    HasMessage = msgBytes != null,
                    PacketMessage = packet,
                    Eos = eos
            };
            _channel.WriteMessage(_baseStream._stream, requestMessage);
            return;
        }

        while (msgBytes.Count != 0) {
            var amountToSend = Math.Min(msgBytes.Count, MAX_REQUEST_MESSAGE_PACKET_DATA_SIZE);
            var packet = new PacketMessage {
                Data = Google.Protobuf.ByteString.CopyFrom(msgBytes.GetRange(0, amountToSend).ToArray()),
            };
            msgBytes = msgBytes.GetRange(amountToSend..^0);
            if (msgBytes.Count == 0) {
                packet.Eom = true;
            }
            var requestMessage = new RequestMessage {
                HasMessage = true,
                PacketMessage = packet,
                Eos = eos
            };
            _channel.WriteMessage(_baseStream._stream, requestMessage);
        }
    }

    public void OnResponse(Response resp) {
        switch (resp.TypeCase) {
            case Response.TypeOneofCase.Headers:
            if (_headersReceived) {
                    _baseStream.CloseWithRecvError(new Exception("headers already received"));
                    return;
                }
                if (_trailersReceived) {
                    _baseStream.CloseWithRecvError(new Exception("headers received after trailers"));
                    return;
                }
                ProcessHeaders(resp.Headers);
                break;
            case Response.TypeOneofCase.Message:
                if (!_headersReceived) {
                    _baseStream.CloseWithRecvError(new Exception("headers not yet received"));
                    return;
                }
                if (_trailersReceived) {
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
    private void ProcessHeaders(ResponseHeaders headers) {
        _headersReceived = true;
        var metadata = WebRTCClientChannel.ToGRPCMetadata(headers.Metadata);
        _responseListener.OnHeaders(metadata);
        // TODO(erd): need?
        // close(s.headersReceived)
    }

    class SimpleDeserializationContext : DeserializationContext {

        private readonly byte[] _data;

        public SimpleDeserializationContext(byte[] data) {
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
    private void ProcessMessage(ResponseMessage msg) {
        var result = _baseStream.ProcessPacketMessage(msg.PacketMessage);
        if (result == null) {
            return;
        }
        var ctx = new SimpleDeserializationContext(result.ToArray());
        var resp = _method.ResponseMarshaller.ContextualDeserializer(ctx);
        _responseListener.OnMessage(resp);
    }

    private void ProcessTrailers(ResponseTrailers trailers) {
        _trailersReceived = true;
        var status = (StatusCode) trailers.Status.Code;

        var metadata = WebRTCClientChannel.ToGRPCMetadata(trailers.Metadata);
        _responseListener.OnClose(new Status(status, trailers.Status.Message), metadata);

        if (status == StatusCode.OK) {
            _baseStream.CloseWithRecvError(null!);
            return;
        }
        _baseStream.CloseWithRecvError(new Exception(String.Format("Code=%d Message=%s", trailers.Status.Code, trailers.Status.Message)));
    }
}