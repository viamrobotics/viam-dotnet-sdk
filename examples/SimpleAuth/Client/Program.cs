using Proto.Api.Robot.V1;
using Viam.Net.Sdk.Core;
using Proto.Rpc.V1;

if (args.Length < 1) {
    throw new ArgumentException("must supply grpc address");
}
var grpcAddress = args[0];

var logger = NLog.LogManager.GetCurrentClassLogger();
using (var dialer = new Dialer(logger)) {
    var dialOpts = new DialOptions { 
        Insecure = true,
        AuthEntity = "something-unique",
        Credentials = new Credentials { Type = "api-key", Payload = "sosecret" }
    };

    using (var chan = await dialer.DialDirectGRPCAsync(grpcAddress, dialOpts)) {
        var robotClient = new RobotService.RobotServiceClient(chan);
        logger.Info(await robotClient.ResourceNamesAsync(new ResourceNamesRequest()));
    }
}
