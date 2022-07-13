namespace Viam.Net.Sdk.Core;

using Grpc.Core;
using Proto.Rpc.Webrtc.V1;
using Microsoft.MixedReality.WebRTC;

class WebRTCClientChannel : GrpcChannel, IDisposable{

    // MaxStreamCount is the max number of streams a channel can have.
    public static int MaxStreamCount = 256;

    private readonly WebRTCBaseChannel _baseChannel;
    private readonly NLog.Logger _logger;
    private ulong streamIDCounter = 0; // TODO(erd): lock

    public readonly Dictionary<ulong, WebRTCClientStreamContainer> Streams = new Dictionary<ulong, WebRTCClientStreamContainer>();

    public WebRTCClientChannel(PeerConnection peerConn, DataChannel dataChannel, NLog.Logger logger) : base("doesnotmatter") {
        _baseChannel = new WebRTCBaseChannel(peerConn, dataChannel);
        dataChannel.MessageReceived += OnChannelMessage;
        _logger = logger;
    }

// TODO(erd): synchronized
    public override CallInvoker CreateCallInvoker() {
        var stream = nextStreamID();
        return new TempCallInvoker(stream, this, (id) => RemoveStreamByID(id), _logger);
    }

// TODO(erd): synchronized
    private void RemoveStreamByID(ulong id) {
        Streams.Remove(id);
    }

    public static Proto.Rpc.Webrtc.V1.Metadata FromGRPCMetadata(Grpc.Core.Metadata? metadata) {
        var protoMd = new Proto.Rpc.Webrtc.V1.Metadata();
        if (metadata == null) {
            return protoMd;
        }
        foreach (Grpc.Core.Metadata.Entry entry in metadata) {
            var strings = new Strings();
            strings.Values.Add(entry.Value);
            protoMd.Md.Add(entry.Key, strings);
        }
        return protoMd;
    }

    public static Grpc.Core.Metadata ToGRPCMetadata(Proto.Rpc.Webrtc.V1.Metadata metadata) {
        var result = new Grpc.Core.Metadata();
        if (metadata == null) {
            return result;
        }
        foreach (KeyValuePair<string, Strings> entry in metadata.Md) {
            foreach (string value in entry.Value.Values) {
                result.Add(entry.Key, value);
            }
        }
        return result;
    }

    private Stream nextStreamID() {
        return new Stream { Id = streamIDCounter++ };
    }

    private void OnChannelMessage(byte[] data) {
        var resp = Response.Parser.ParseFrom(data);
        // TODO(erd): probably ned a catch on parse

        if (resp.Stream == null) {
            _logger.Warn("no stream id; discarding");
            return;
        }

        var stream = resp.Stream;
        var id = stream.Id;
        var activeStream = Streams[id];
        if (activeStream == null) {
            _logger.Warn("no stream for id; discarding: id=" + id);
            return;
        }

        activeStream.OnResponse(resp);
    }

    public Task<bool> Ready() {
        return _baseChannel.Ready.Task;
    }

    public void WriteHeaders(Stream stream, RequestHeaders headers) {
        _baseChannel.Write(new Request {
            Stream = stream,
            Headers = headers,
        });
    } 

    public void WriteMessage(Stream stream, RequestMessage msg) {
        _baseChannel.Write(new Request {
            Stream = stream,
            Message = msg,
        });
    } 

    public override void Dispose() {
        // TODO(erd): dispose streams
        _baseChannel.Dispose();
    }
}

public static class ListExtensions
{
    public static List<T> GetRange<T>(this List<T> list, Range range)
    {
        var (start, length) = range.GetOffsetAndLength(list.Count);
        return list.GetRange(start, length);
    }
}
