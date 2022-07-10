using Proto.Api.Robot.V1;
using Viam.Net.Sdk.Core;
using Proto.Rpc.V1;

using (var dialer = new Dialer()) {
    var dialOpts = new DialOptions { 
        WebRTCOptions = new DialWebRTCOptions {
            SignalingInsecure = true,
            SignalingCredentials = new Credentials { Type = "api-key", Payload = "sosecret" }
        }
    };

    var logger = NLog.LogManager.GetCurrentClassLogger();
    using (var chan = await dialer.DialWebRTCAsync("http://localhost:8080", "something-unique", dialOpts)) {
        var robotClient = new RobotService.RobotServiceClient(chan);
        logger.Info(await robotClient.ResourceNamesAsync(new ResourceNamesRequest()));
    }
}
