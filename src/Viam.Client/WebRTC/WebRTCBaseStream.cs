using System.Buffers;
using System.Diagnostics;
using Google.Protobuf;
using Microsoft.Extensions.Logging;
using Proto.Rpc.Webrtc.V1;

namespace Viam.Client.WebRTC
{
    internal record PacketMessageContents : IDisposable
    {
        public int Position { get; private set; }

        public readonly byte[] Data = ArrayPool<byte>.Shared.Rent(WebRtcBaseStream.MaxMessageSize);

        public void AppendData(ByteString data)
        {
            data.CopyTo(Data, Position);
            Position += data.Length;
        }

        public void Dispose()
        {
            ArrayPool<byte>.Shared.Return(Data);
        }
    }

    internal class WebRtcBaseStream(Proto.Rpc.Webrtc.V1.Stream stream, Action<ulong> onDone, ILogger logger)
    {
        private bool _closed;
        private PacketMessageContents? _contents;

        // MaxMessageSize is the maximum size a gRPC message can be.
        public static int MaxMessageSize = 1 << 25;

        public readonly Proto.Rpc.Webrtc.V1.Stream Stream = stream;

        public void CloseWithReceiveError(Exception ex)
        {
            if (_closed)
            {
                return;
            }
            _closed = true;
            onDone(Stream.Id);
        }

        public PacketMessageContents? ProcessPacketMessage(PacketMessage msg)
        {
            _contents ??= new PacketMessageContents();

            var data = msg.Data;
            if (data.Length + _contents.Position > MaxMessageSize)
            {
                try
                {
                    logger.LogWarning("message size larger than max {MaxMessageSize}; discarding", MaxMessageSize);
                    return null;
                }
                finally
                {
                    _contents.Dispose();
                    _contents = null;
                }
            }

            Debug.Assert(_contents != null);

            _contents.AppendData(msg.Data);
            if (msg.Eom)
            {
                var contents = _contents;
                _contents = null;
                return contents;
            }

            return null;
        }
    }
}