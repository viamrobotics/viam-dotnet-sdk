using Microsoft.Extensions.Logging;
using Viam.Client.Clients;
using Viam.Client.Dialing;

if (args.Length < 1)
{
    throw new ArgumentException("must supply grpc address");
}

var grpcAddress = args[0];
var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
var logger = loggerFactory.CreateLogger<Program>();
var dialOpts = DialOptions.FromAddress(grpcAddress)
    .WithLogging(loggerFactory)
    .SetInsecure();

var robotClient = await ViamMachineClient.CreateFromDialOptions(dialOpts);
var resourceNames = await robotClient.ResourceNamesAsync();
logger.LogInformation("Resource Names: {ResourceName}", string.Join(",", resourceNames.Select(x => x.Name)));