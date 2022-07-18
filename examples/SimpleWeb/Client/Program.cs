using Proto.Api.Robot.V1;
using Viam.Net.Sdk.Core;
using Grpc.Net.Client;
using Grpc.Net.Client.Web;

if (args.Length < 1)
{
    throw new ArgumentException("must supply grpc address");
}
var grpcAddress = args[0];

var logger = NLog.LogManager.GetCurrentClassLogger();
using (var dialer = new Dialer(logger))
{
    var dialOpts = new DialOptions
    {
        Insecure = true,
        ChannelOptions = new GrpcChannelOptions
        {
            HttpHandler = new GrpcWebHandler(new HttpClientHandler())
        }
    };

    using (var chan = await dialer.DialDirectGRPCAsync(grpcAddress, dialOpts))
    {
        var robotClient = new RobotService.RobotServiceClient(chan);
        logger.Info(await robotClient.ResourceNamesAsync(new ResourceNamesRequest()));
    }
}
