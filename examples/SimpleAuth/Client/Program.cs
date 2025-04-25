using Microsoft.Extensions.Logging;
using Viam.Client.Clients;
using Viam.Client.Dialing;

if (args.Length < 3)
{
    throw new ArgumentException("must supply machine address, api-key, and api-key-id");
}
var grpcAddress = args[0];
var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Trace));
var logger = loggerFactory.CreateLogger<Program>();
var dialOpts = DialOptions.FromAddress(grpcAddress)
                          .WithLogging(loggerFactory)
                          .WithApiCredentials(args[1], args[2]);

var robotClient = await RobotClient.AtAddressAsync(dialOpts);
var resourceNames = await robotClient.ResourceNamesAsync();
logger.LogInformation("Resource Names: {ResourceName}", string.Join(",", resourceNames.Select(x => x.Name)));
