using Google.Protobuf;
using SIPSorcery.Net;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Viam.Net.Sdk.Core
{
    class WebRTCBaseChannel : IDisposable
    {

        private readonly RTCPeerConnection PeerConn;
        private readonly RTCDataChannel DataChannel;

        public readonly TaskCompletionSource<bool> Ready;

        private object _closedMu = new Object();
        private bool _closed = false;
        private string _closedReason = "";

        public WebRTCBaseChannel(
            RTCPeerConnection peerConn,
            RTCDataChannel dataChannel,
            Action? onPeerDone = null
        )
        {
            PeerConn = peerConn;
            DataChannel = dataChannel;
            Ready = new TaskCompletionSource<bool>();

            dataChannel.onopen += OnChannelOpen;
            dataChannel.onclose += OnChannelClose;
            dataChannel.onerror += OnChannelError;

            var peerDoneOnce = false;
            Action doPeerDone = () =>
            {
                if (!peerDoneOnce && onPeerDone != null)
                {
                    peerDoneOnce = true;
                    onPeerDone();
                }
            };
            Action<RTCIceConnectionState> connStateChanged = (RTCIceConnectionState connectionState) =>
            {
                lock (_closedMu)
                {
                    if (_closed)
                    {
                        doPeerDone();
                        return;
                    }
                }

                Task.Run(() =>
                {
                    lock (_closedMu)
                    {
                        if (_closed)
                        {
                            doPeerDone();
                            return;
                        }
                    }

                    switch (connectionState)
                    {
                        case RTCIceConnectionState.disconnected:
                        case RTCIceConnectionState.failed:
                        case RTCIceConnectionState.closed:
                            doPeerDone();
                            break;
                    };
                });
            };

            peerConn.oniceconnectionstatechange += connStateChanged;
            connStateChanged(peerConn.iceConnectionState);
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
            lock (_closedMu)
            {
                if (_closed)
                {
                    return;
                }
                _closed = true;
                _closedReason = reason;
                // TODO(erd): cancelFunc
                PeerConn.Close(reason);
            }
        }

        public void Dispose()
        {
            CloseWithReason("");
        }

        public (bool, string) Closed()
        {
            lock (_closedMu)
            {
                return (_closed, _closedReason);
            }
        }

        const int maxDataChannelSize = 16384;

        public void Write(IMessage msg)
        {
            using var stream = new MemoryStream();
            msg.WriteTo(stream);
            stream.Flush();
            DataChannel.send(stream.ToArray());
        }
    }
}