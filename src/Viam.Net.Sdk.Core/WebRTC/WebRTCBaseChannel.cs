using System;
using System.Buffers;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf;
using SIPSorcery.Net;

namespace Viam.Net.Sdk.Core.WebRTC
{
    internal class WebRTCBaseChannel : IDisposable
    {
        private readonly RTCPeerConnection _peerConn;
        private readonly RTCDataChannel _dataChannel;

        public readonly TaskCompletionSource<bool> Ready;

        private readonly SemaphoreSlim _semaphore = new(1);
        private bool _closed = false;
        private string _closedReason = "";
        private readonly Action? _onPeerDone;
        private bool _peerDoneOnce;

        public WebRTCBaseChannel(RTCPeerConnection peerConn,
                                 RTCDataChannel dataChannel,
                                 Action? onPeerDone = null)
        {
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
            _semaphore.Wait();
            try
            {
                if (_peerDoneOnce)
                    return;

                _peerDoneOnce = true;
                _onPeerDone?.Invoke();
            }
            finally
            {
                _semaphore.Release();
            }
        }

        private async void ConnStateChanged(RTCIceConnectionState connectionState)
        {
            await _semaphore.WaitAsync();
            try
            {
                if (_closed)
                {
                    DoPeerDone();
                    return;
                }
            }
            finally
            {
                _semaphore.Release();
            }

            switch (connectionState)
            {
                case RTCIceConnectionState.disconnected:
                case RTCIceConnectionState.failed:
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
            Ready.SetResult(true);
        }

        private void OnChannelClose()
        {
            CloseWithReason("data channel closed");
        }

        private void OnChannelError(string err)
        {
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
            CloseWithReason("");
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