namespace Viam.Net.Sdk.Core;

using Google.Protobuf;
using Microsoft.MixedReality.WebRTC;

class WebRTCBaseChannel : IDisposable {

    private readonly PeerConnection PeerConn;
    private readonly DataChannel DataChannel;

    public readonly TaskCompletionSource<bool> Ready;

    private object _closedMu = new Object();
    private bool _closed = false;
    private string _closedReason = "";

    public WebRTCBaseChannel(
        PeerConnection peerConn,
        DataChannel dataChannel,
        Action? onPeerDone = null
    ) {
        PeerConn = peerConn;
        DataChannel = dataChannel;
        Ready = new TaskCompletionSource<bool>();

        dataChannel.StateChanged += OnChannelStateChanged;

        var peerDoneOnce = false;
        var doPeerDone = () => {
            if (!peerDoneOnce && onPeerDone != null) {
                peerDoneOnce = true;
                onPeerDone();
            }
        };
        var connStateChanged = (IceConnectionState connectionState) => {
            lock(_closedMu) {
                if (_closed) {
                    doPeerDone();
                    return;
                }
            }

            Task.Run(() => {
                lock(_closedMu) {
                    if (_closed) {
                        doPeerDone();
                        return;
                    }
                }

                switch (connectionState) {
                    case IceConnectionState.Disconnected:
                    case IceConnectionState.Failed:
                    case IceConnectionState.Closed:
                        doPeerDone();
                        break;
                };
            });
        };

        peerConn.IceStateChanged += new PeerConnection.IceStateChangedDelegate(connStateChanged);
    }

    private void OnChannelStateChanged() {
        switch (DataChannel.State) {
            case DataChannel.ChannelState.Open:
                Ready.SetResult(true);
                break;
            case DataChannel.ChannelState.Closing:
            case DataChannel.ChannelState.Closed:
                CloseWithReason("data channel closed");
                break;
        }
    }

    private void CloseWithReason(string reason) {
        lock(_closedMu) {
            if (_closed) {
                return;
            }
            _closed = true;
            _closedReason = reason;
            // TODO(erd): cancelFunc
            PeerConn.Dispose();
        }
    }

    public void Dispose() {
        CloseWithReason("");
    }

    public (bool, string) Closed() {
        lock (_closedMu) {
            return (_closed, _closedReason);
        }
    }

    const int maxDataChannelSize = 16384;

    public void Write(IMessage msg) {
        using var stream = new MemoryStream();
        Console.WriteLine("sending stuff");
        DataChannel.SendMessage(stream.ToArray());
    }
}
