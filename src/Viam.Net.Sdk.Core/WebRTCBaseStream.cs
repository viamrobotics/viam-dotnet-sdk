namespace Viam.Net.Sdk.Core;

using Proto.Rpc.Webrtc.V1;

class WebRTCBaseStream
{
    // MaxMessageSize is the maximum size a gRPC message can be.
    public static long MaxMessageSize = 1 << 25;

    public readonly Stream _stream;
    private readonly Action<ulong> _onDone;

    private bool _closed = false;
    private readonly List<List<Byte>> packetBuf = new List<List<byte>>();
    private int packetBufSize = 0;
    private Exception _ex = null!;
    private readonly NLog.Logger _logger;

    public WebRTCBaseStream(Stream stream, Action<ulong> onDone, NLog.Logger logger)
    {
        _stream = stream;
        _onDone = onDone;
        _logger = logger;
    }

    public void CloseWithRecvError(Exception ex)
    {
        if (_closed)
        {
            return;
        }
        _closed = true;
        _ex = ex;
        _onDone(_stream.Id);
    }

    public List<Byte>? ProcessPacketMessage(PacketMessage msg)
    {
        var data = msg.Data;
        if (data.Length + this.packetBufSize > MaxMessageSize)
        {
            this.packetBuf.Clear();
            this.packetBufSize = 0;
            _logger.Warn("message size larger than max " + MaxMessageSize + "; discarding");
            return null;
        }
        this.packetBuf.Add(data.ToList());
        this.packetBufSize += data.Length;
        if (msg.Eom)
        {
            var allData = new List<Byte>(this.packetBufSize);
            int position = 0;
            foreach (var partialData in this.packetBuf)
            {
                allData.InsertRange(position, partialData);
                position += partialData.Count;
            }
            this.packetBuf.Clear();
            this.packetBufSize = 0;
            return allData;
        }
        return null;
    }
}