using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;

using Viam.Client.Clients;
using Viam.Client.Dialing;

if (args.Length < 3)
{
    throw new ArgumentException("must supply machine address, api-key, and api-key-id");
}
var grpcAddress = args[0];
var loggerFactory = LoggerFactory.Create(builder => builder.AddSimpleConsole(options =>
{
    options.SingleLine = true;
    options.ColorBehavior = LoggerColorBehavior.Enabled;
    options.TimestampFormat = "yyyy-MM-ddTHH:mm:ss.fffZ ";
}).SetMinimumLevel(LogLevel.Debug));
var logger = loggerFactory.CreateLogger<Program>();
var dialOpts = DialOptions.FromAddress(grpcAddress)
                          .WithLogging(loggerFactory)
                          .WithApiCredentials(args[1], args[2]);

var robotClient = await RobotClient.AtAddressAsync(dialOpts);
var resourceNames = await robotClient.ResourceNamesAsync();
logger.LogInformation("Resource Names: {ResourceName}", string.Join(",", resourceNames.Select(x => x.Name)));
