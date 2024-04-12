using Viam.Robot.V1;
using Viam.Net.Sdk.Core;
using Proto.Rpc.V1;
using Proto.Rpc.Examples.Echo.V1;
using Grpc.Core;

var logger = NLog.LogManager.GetCurrentClassLogger();
using (var dialer = new Dialer(logger))
{
    var dialOpts = new DialOptions
    {
        WebRTCOptions = new DialWebRTCOptions
        {
            SignalingInsecure = true,
            SignalingCredentials = new Credentials { Type = "api-key", Payload = "sosecret" }
        }
    };

    using (var chan = await dialer.DialWebRTCAsync("http://localhost:8080", "something-unique", dialOpts))
    {
        var robotClient = new RobotService.RobotServiceClient(chan);
        var echoClient = new EchoService.EchoServiceClient(chan);

        var echoStream = echoClient.EchoBiDi();
        await echoStream.RequestStream.WriteAsync(new EchoBiDiRequest() { Message = "one" });

        for (var i = 0; i < 3; i++)
        {
            await echoStream.ResponseStream.MoveNext();
            logger.Info(echoStream.ResponseStream.Current);
        }

        await echoStream.RequestStream.WriteAsync(new EchoBiDiRequest() { Message = "two" });

        for (var i = 0; i < 3; i++)
        {
            await echoStream.ResponseStream.MoveNext();
            logger.Info(echoStream.ResponseStream.Current);
        }

        await echoStream.RequestStream.CompleteAsync();

        var statusRespStream = robotClient.StreamStatus(new StreamStatusRequest
        {
            ResourceNames = {
            new Viam.Common.V1.ResourceName() { Namespace = "rdk", Type = "component", Subtype = "arm", Name = "arm1" } }
        });
        await foreach (var resp in statusRespStream.ResponseStream.ReadAllAsync().ConfigureAwait(false))
        {
            logger.Info(resp);
        }

        logger.Info(await robotClient.ResourceNamesAsync(new ResourceNamesRequest()));

        await Task.Run(() =>
        {
            logger.Info(robotClient.ResourceNames(new ResourceNamesRequest()));
        });
    }
}
