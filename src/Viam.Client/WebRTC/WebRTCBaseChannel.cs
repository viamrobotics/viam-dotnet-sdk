using System.Buffers;
using Google.Protobuf;
using Microsoft.Extensions.Logging;
using SIPSorcery.Net;

namespace Viam.Client.WebRTC
{
    internal class WebRtcBaseChannel : IDisposable
    {
        private readonly RTCPeerConnection _peerConn;
        private readonly RTCDataChannel _dataChannel;
        private readonly ILogger _logger;

        public readonly TaskCompletionSource<bool> Ready;

        private readonly SemaphoreSlim _semaphore = new(1);
        private bool _closed;
        private string _closedReason = "";
        private readonly Action? _onPeerDone;
        private bool _peerDoneOnce;

        public WebRtcBaseChannel(ILogger logger,
                                 RTCPeerConnection peerConn,
                                 RTCDataChannel dataChannel,
                                 Action? onPeerDone = null)
        {
            _logger = logger;
            _onPeerDone = onPeerDone;
            _peerConn = peerConn;
            _dataChannel = dataChannel;
            Ready = new TaskCompletionSource<bool>();

            dataChannel.onopen += OnChannelOpen;
            dataChannel.onclose += OnChannelClose;
            dataChannel.onerror += OnChannelError;

            peerConn.oniceconnectionstatechange += ConnStateChanged;
            ConnStateChanged(peerConn.iceConnectionState);
        }

        private void DoPeerDone()
        {
            _logger.LogTrace("Peer done, waiting for lock");
            _semaphore.Wait();
            try
            {
                _logger.LogTrace("Peer done, got lock");
                if (_peerDoneOnce)
                    return;

                _logger.LogTrace("Peer not done once, doing peer done");
                _peerDoneOnce = true;
                _onPeerDone?.Invoke();
            }
            finally
            {
                _logger.LogTrace("Releasing Peer done lock");
                _semaphore.Release();
            }
        }

        private void ConnStateChanged(RTCIceConnectionState connectionState)
        {
            _logger.LogTrace("WebRTCBaseChannel ConnStateChanged, waiting for lock");
            _semaphore.Wait();
            try
            {
                _logger.LogTrace("WebRTCBaseChannel ConnStateChanged, got lock");
                if (_closed)
                {
                    _logger.LogTrace("WebRTCBaseChannel ConnStateChanged, closed");
                    DoPeerDone();
                    return;
                }
            }
            finally
            {
                _semaphore.Release();
            }

            _logger.LogTrace("WebRTCBaseChannel Handling connection state change {ConnectionState}", connectionState);
            switch (connectionState)
            {
                case RTCIceConnectionState.failed:
                    _logger.LogError("WebRTC connection failed");
                    var ex = new Exception("WebRTC connection failed");
                    Ready.SetException(ex);
                    DoPeerDone();
                    break;
                case RTCIceConnectionState.disconnected:
                case RTCIceConnectionState.closed:
                    DoPeerDone();
                    break;
                case RTCIceConnectionState.@new:
                    break;
                case RTCIceConnectionState.checking:
                    break;
                case RTCIceConnectionState.connected:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(connectionState), connectionState, null);
            }
        }

        private void OnChannelOpen()
        {
            _logger.LogTrace("WebRTCBaseChannel open");
            Ready.SetResult(true);
        }

        private void OnChannelClose()
        {
            _logger.LogTrace("WebRTCBaseChannel closed");
            CloseWithReason("data channel closed");
        }

        private void OnChannelError(string err)
        {
            _logger.LogTrace("WebRTCBaseChannel error: {err}", err);
            CloseWithReason(err);
        }

        private void CloseWithReason(string reason)
        {
            if (_closed) return;

            // TODO: There might be a race on the lock here
            _semaphore.Wait();
            try
            {
                _closed = true;
                _closedReason = reason;
                // TODO(erd): cancelFunc
                _peerConn.Close(reason);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public void Dispose()
        {
            _logger.LogTrace("WebRTCBaseChannel dispose");
            _peerConn.close();
            _peerConn.Dispose();
            _dataChannel.close();
        }

        public (bool, string) Closed()
        {
            _semaphore.Wait();
            try
            {
                return (_closed, _closedReason);
            }
            finally
            {
                _semaphore.Release();
            }
        }



        public void Write(IMessage msg)
        {
            var messageSize = msg.CalculateSize();
            var b = ArrayPool<byte>.Shared.Rent(messageSize);
            try
            {
                var bytesToSend = b[..messageSize];
                msg.WriteTo(bytesToSend);
                _dataChannel.send(bytesToSend);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(b);
            }
        }
    }
}