using System.Buffers;
using System.Diagnostics;

using Google.Protobuf;

using Microsoft.Extensions.Logging;

using Proto.Rpc.Webrtc.V1;

using Viam.Core.Logging;

namespace Viam.Client.WebRTC
{
    internal record PacketMessageContents(ILogger logger, int? initialSize) : IDisposable
    {
        // Set the initial size to a sane 1KiB.
        private const int InitialSize = 1 << 10;
        public int Position { get; private set; }

        public byte[] Data { get; private set; } = ArrayPool<byte>.Shared.Rent(initialSize ?? InitialSize);

        public void AppendData(ByteString data)
        {
            var requiredSize = Position + data.Length;
            if (requiredSize > Data.Length)
            {
                logger.LogWebRtcPacketMessageContentsSizeExceeded(Data.Length, requiredSize);
                GrowArray(requiredSize);
            }
            logger.LogWebRtcPacketMessageCopyingData(data.Length, Position);
            data.CopyTo(Data, Position);
            Position += data.Length;
            logger.LogWebRtcPacketMessageDataCopyDone();
        }

        public void Dispose()
        {
            logger.LogWebRtcPacketMessageDispose();
            ArrayPool<byte>.Shared.Return(Data);
        }

        /// <summary>
        /// If the array is too small, increase it to the next power of two beyond the minimum length.
        /// </summary>
        /// <param name="minimumLength">The smallest size the array can be and still fit the new message parts</param>
        private void GrowArray(int minimumLength)
        {
            var newSize = RoundUpToPowerOfTwo(minimumLength);
            logger.LogWebRtcPacketMessageGrowArrayStart(newSize);

            var newData = ArrayPool<byte>.Shared.Rent(newSize);
            logger.LogWebRtcPacketMessageNewArraySize(newData.Length);

            Data.CopyTo(newData, 0);
            ArrayPool<byte>.Shared.Return(Data);
            Data = newData;
            logger.LogWebRtcPacketMessageGrowArrayEnd();
        }

        private static int RoundUpToPowerOfTwo(int minimumLength)
        {
            var v = minimumLength;
            v--;
            v |= v >> 1;
            v |= v >> 2;
            v |= v >> 4;
            v |= v >> 8;
            v |= v >> 16;
            v++;
            return v > WebRtcBaseStream.MaxMessageSize
                       ? WebRtcBaseStream.MaxMessageSize
                       : v;
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
            var data = msg.Data;
            if (_contents == null)
            {
                logger.LogWebRtcProcessNewPacketMessage();
                _contents = new PacketMessageContents(logger, data.Length);
            }
            else
            {
                logger.LogWebRtcProcessExistingPacketMessage();
                if (data.Length + _contents.Position > MaxMessageSize)
                {
                    try
                    {
                        logger.LogWebRtcMessageSizeExceeded(data.Length + _contents.Position, MaxMessageSize);
                        return null;
                    }
                    finally
                    {
                        _contents.Dispose();
                        _contents = null;
                    }
                }
            }

            _contents.AppendData(msg.Data);
            if (msg.Eom)
            {
                var contents = _contents;
                _contents = null;
                logger.LogWebRtcProcessPacketMessageEndOfMessage();
                return contents;
            }

            return null;
        }
    }
}