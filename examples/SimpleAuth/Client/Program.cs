using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Viam.Client.Clients;
using Viam.Client.Dialing;
using Viam.Core.Resources.Components.Sensor;

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
}).SetMinimumLevel(LogLevel.Trace));
var logger = loggerFactory.CreateLogger<Program>();
var dialOpts = DialOptions.FromAddress(grpcAddress)
    .WithLogging(loggerFactory)
    .WithApiCredentials(args[1], args[2]);

var robotClient = await ViamMachineClient.CreateFromDialOptions(dialOpts);
var resourceNames = await robotClient.ResourceNamesAsync();
logger.LogInformation("Resource Names: {ResourceName}", string.Join(",", resourceNames.Select(x => x.Name)));

await using var client = SensorClient.FromRobot(robotClient, "mySensor");
var readings = await client.GetReadings();
logger.LogInformation("Readings: {Readings}", string.Join(",", readings.Select(x => $"{x.Key}: {x.Value}")));