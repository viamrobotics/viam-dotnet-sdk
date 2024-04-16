using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using Proto.Rpc.Webrtc.V1;
using SIPSorcery.Net;

namespace Viam.Net.Sdk.Core.WebRTC
{
    public class WebRTCClientChannel : ViamChannel, IDisposable
    {
        // MaxStreamCount is the max number of streams a channel can have.
        public static int MaxStreamCount = 256;

        private readonly WebRTCBaseChannel _baseChannel;
        private readonly ILogger _logger;
        private long _streamIdCounter = 0; // TODO(erd): lock
        private readonly WebRTCClientCallInvoker _callInvoker;

        public readonly Dictionary<ulong, IWebRTCClientStreamContainer> Streams = new();

        public WebRTCClientChannel(RTCPeerConnection peerConn, RTCDataChannel dataChannel, ILogger logger)
        {
            _baseChannel = new WebRTCBaseChannel(peerConn, dataChannel);
            dataChannel.onmessage += OnChannelMessage;
            _logger = logger;
            _callInvoker = new WebRTCClientCallInvoker(this, logger);
        }

        protected override CallInvoker GetCallInvoker() => _callInvoker;

        // TODO(erd): synchronized
        public void RemoveStreamById(ulong id)
        {
            Streams.Remove(id);
        }

        public static Proto.Rpc.Webrtc.V1.Metadata FromGRPCMetadata(global::Grpc.Core.Metadata? metadata)
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

        public static global::Grpc.Core.Metadata ToGRPCMetadata(Proto.Rpc.Webrtc.V1.Metadata? metadata)
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

        public Stream NextStreamId() => new() { Id = (ulong)Interlocked.Increment(ref _streamIdCounter) };

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
            var activeStream = Streams[id];
            if (activeStream == null)
            {
                _logger.LogWarning("no stream for id; discarding: {id}", id);
                return;
            }

            activeStream.OnResponse(resp);
        }

        public Task<bool> Ready() => _baseChannel.Ready.Task;

        public void WriteHeaders(Stream stream, RequestHeaders headers)
        {
            _baseChannel.Write(new Request
            {
                Stream = stream,
                Headers = headers,
            });
        }

        public void WriteMessage(Stream stream, RequestMessage msg)
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