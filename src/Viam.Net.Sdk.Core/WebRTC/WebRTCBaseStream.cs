using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Proto.Rpc.Webrtc.V1;

namespace Viam.Core.WebRTC
{
    internal class WebRTCBaseStream
    {
        private readonly ILogger _logger;
        private readonly Action<ulong> _onDone;
        private bool _closed = false;
        private readonly List<List<byte>> packetBuf = new List<List<byte>>();
        private int _packetBufSize = 0;

        // MaxMessageSize is the maximum size a gRPC message can be.
        public static long MaxMessageSize = 1 << 25;

        public readonly Stream Stream;

        public WebRTCBaseStream(Stream stream, Action<ulong> onDone, ILogger logger)
        {
            Stream = stream;
            _onDone = onDone;
            _logger = logger;
        }

        public void CloseWithReceiveError(Exception ex)
        {
            if (_closed)
            {
                return;
            }
            _closed = true;
            _onDone(Stream.Id);
        }

        public List<byte>? ProcessPacketMessage(PacketMessage msg)
        {
            var data = msg.Data;
            if (data.Length + _packetBufSize > MaxMessageSize)
            {
                packetBuf.Clear();
                _packetBufSize = 0;
                _logger.LogWarning("message size larger than max {MaxMessageSize}; discarding", MaxMessageSize);
                return null;
            }

            packetBuf.Add(data.ToList());
            _packetBufSize += data.Length;
            if (msg.Eom)
            {
                var allData = new List<byte>(_packetBufSize);
                var position = 0;
                foreach (var partialData in packetBuf)
                {
                    allData.InsertRange(position, partialData);
                    position += partialData.Count;
                }

                packetBuf.Clear();
                _packetBufSize = 0;
                return allData;
            }

            return null;
        }
    }
}