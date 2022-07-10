using Proto.Api.Robot.V1;
using Viam.Net.Sdk.Core;
using Proto.Rpc.V1;

if (args.Length < 1) {
    throw new ArgumentException("must supply grpc address");
}
var grpcAddress = args[0];

using (var dialer = new Dialer()) {
    var dialOpts = new DialOptions { 
        Insecure = true,
        AuthEntity = "something-unique",
        Credentials = new Credentials { Type = "api-key", Payload = "sosecret" }
    };

    var logger = NLog.LogManager.GetCurrentClassLogger();
    using (var chan = await dialer.DialDirectGRPCAsync(grpcAddress, dialOpts)) {
        var robotClient = new RobotService.RobotServiceClient(chan);
        logger.Info(await robotClient.ResourceNamesAsync(new ResourceNamesRequest()));
    }
}
