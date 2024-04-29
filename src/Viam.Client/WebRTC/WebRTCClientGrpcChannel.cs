using Grpc.Core;
using Microsoft.Extensions.Logging;
using Proto.Rpc.Webrtc.V1;
using SIPSorcery.Net;

using Viam.Core;

namespace Viam.Client.WebRTC
{
    public class WebRtcClientChannel : ViamChannel, IDisposable
    {
        // MaxStreamCount is the max number of streams a channel can have.
        public static int MaxStreamCount = 256;

        private readonly WebRtcBaseChannel _baseChannel;
        private readonly ILogger _logger;
        private long _streamIdCounter; // TODO(erd): lock
        private readonly WebRtcClientCallInvoker _callInvoker;

        public readonly Dictionary<ulong, IWebRtcClientStreamContainer> Streams = new();

        // TODO: Update this so we provide the right value to base()
        public WebRtcClientChannel(RTCPeerConnection peerConn, RTCDataChannel dataChannel, ILogger logger) : base("webrtc")
        {
            _baseChannel = new WebRtcBaseChannel(peerConn, dataChannel);
            dataChannel.onmessage += OnChannelMessage;
            _logger = logger;
            _callInvoker = new WebRtcClientCallInvoker(this, logger);
        }

        protected override CallInvoker GetCallInvoker() => _callInvoker;

        // TODO(erd): synchronized
        public void RemoveStreamById(ulong id)
        {
            Streams.Remove(id);
        }

        public static Proto.Rpc.Webrtc.V1.Metadata FromGrpcMetadata(global::Grpc.Core.Metadata? metadata)
        {
            var protoMd = new Proto.Rpc.Webrtc.V1.Metadata();
            if (metadata == null)
            {
                return protoMd;
            }

            foreach (var entry in metadata)
            {
                var strings = new Strings();
                strings.Values.Add(entry.Value);
                protoMd.Md.Add(entry.Key, strings);
            }

            return protoMd;
        }

        public static global::Grpc.Core.Metadata ToGrpcMetadata(Proto.Rpc.Webrtc.V1.Metadata? metadata)
        {
            var result = new global::Grpc.Core.Metadata();
            if (metadata == null)
            {
                return result;
            }

            foreach (KeyValuePair<string, Strings> entry in metadata.Md)
            {
                foreach (var value in entry.Value.Values)
                {
                    result.Add(entry.Key, value);
                }
            }

            return result;
        }

        public Proto.Rpc.Webrtc.V1.Stream NextStreamId() => new() { Id = (ulong)Interlocked.Increment(ref _streamIdCounter) };

        private void OnChannelMessage(RTCDataChannel dc, DataChannelPayloadProtocols protocols, byte[] data)
        {
            var resp = Response.Parser.ParseFrom(data);
            // TODO(erd): probably ned a catch on parse

            if (resp.Stream == null)
            {
                _logger.LogWarning("no stream id; discarding");
                return;
            }


            var stream = resp.Stream;
            var id = stream.Id;
            if (Streams.TryGetValue(id, out var activeStream))
            {
                activeStream.OnResponse(resp);
            }
            else
            {
                _logger.LogWarning("no stream for id; discarding: {id}", id);
            }
        }

        public Task<bool> Ready() => _baseChannel.Ready.Task;

        public void WriteHeaders(Proto.Rpc.Webrtc.V1.Stream stream, RequestHeaders headers)
        {
            _baseChannel.Write(new Request
            {
                Stream = stream,
                Headers = headers,
            });
        }

        public void WriteMessage(Proto.Rpc.Webrtc.V1.Stream stream, RequestMessage msg)
        {
            _baseChannel.Write(new Request
            {
                Stream = stream,
                Message = msg,
            });
        }

        public override void Dispose()
        {
            // TODO(erd): dispose streams
            _baseChannel.Dispose();
        }
    }
}