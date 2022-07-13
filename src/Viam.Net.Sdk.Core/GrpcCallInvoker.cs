namespace Viam.Net.Sdk.Core;

using Grpc.Core;
using Proto.Rpc.Webrtc.V1;

class TempCallInvoker : CallInvoker {
    // see golang/client_stream.go
    private const int MAX_REQUEST_MESSAGE_PACKET_DATA_SIZE = 16373;

    private readonly WebRTCClientChannel _channel;
    private readonly Stream _stream;
    private readonly Action<ulong> _onDone;
    private readonly NLog.Logger _logger;

    public TempCallInvoker(
        Stream stream,
        WebRTCClientChannel channel,
        Action<ulong> onDone,
        NLog.Logger logger
    ) {
        _stream = stream;
        _channel = channel;
        _onDone = onDone;
        _logger = logger;
    }

    // TODO(erd): synchronized on client chan
    private WebRTCClientStream<TRequest, TResponse> makeStream<TRequest, TResponse>(Method<TRequest, TResponse> method) {
        if (_channel.Streams.ContainsKey(_stream.Id)) {
            throw new Exception("channel already exists with id");
        }
        if (_channel.Streams.Count > WebRTCClientChannel.MaxStreamCount) {
            throw new Exception("stream limit hit");
        }
        var activeStream = new WebRTCClientStream<TRequest, TResponse>(method, _stream, _channel, _onDone, _logger);
        _channel.Streams.Add(_stream.Id, activeStream);
        return activeStream;
    }

    public override AsyncClientStreamingCall<TRequest, TResponse> AsyncClientStreamingCall<TRequest, TResponse>(Method<TRequest, TResponse> method, string? host, CallOptions options)
        where TRequest : class
        where TResponse : class {
            throw new NotImplementedException("not yet");
    }

    public override AsyncDuplexStreamingCall<TRequest, TResponse> AsyncDuplexStreamingCall<TRequest, TResponse>(Method<TRequest, TResponse> method, string? host, CallOptions options)
        where TRequest : class
        where TResponse : class {
            throw new NotImplementedException("not yet");
    }

    public override AsyncServerStreamingCall<TResponse> AsyncServerStreamingCall<TRequest, TResponse>(Method<TRequest, TResponse> method, string? host, CallOptions options, TRequest request)
        where TRequest : class
        where TResponse : class {   
            throw new NotImplementedException("not yet");
    }

    public override AsyncUnaryCall<TResponse> AsyncUnaryCall<TRequest, TResponse>(Method<TRequest, TResponse> method, string? host, CallOptions options, TRequest request)
        where TRequest : class
        where TResponse : class {
            var clientStream = makeStream(method);
            return clientStream.UnaryCall(options, request);
    }

    public override TResponse BlockingUnaryCall<TRequest, TResponse>(Method<TRequest, TResponse> method, string? host, CallOptions options, TRequest request)
        where TRequest : class
        where TResponse : class {
            var clientStream = makeStream(method);
            return clientStream.UnaryCall(options, request).GetAwaiter().GetResult();
    }
}