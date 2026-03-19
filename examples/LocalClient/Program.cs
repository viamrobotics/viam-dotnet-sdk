using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;

using Viam.Client.Clients;
using Viam.Client.Dialing;
using Viam.Core.Resources.Components.Camera;
using Viam.Core.Resources.Components.Sensor;
using Viam.Core.Resources.Services.VisionService;

var loggerFactory = LoggerFactory.Create(builder => builder.AddSimpleConsole(options =>
{
    options.SingleLine = true;
    options.ColorBehavior = LoggerColorBehavior.Enabled;
    options.TimestampFormat = "yyyy-MM-ddTHH:mm:ss.fffZ ";
}).SetMinimumLevel(LogLevel.Trace));
var logger = loggerFactory.CreateLogger<Program>();

var dialOpts = DialOptions.FromAddress("rozza-v1-1-main.unb49wp316.viam.cloud")
    .WithLogging(loggerFactory)
    .WithApiCredentials(
        "ek52tafaoinnxvambr6pbo20y6jublsh",
        "df6db839-47a5-41a2-b2b3-c3ee8208492e");

var machine = await MachineClient.CreateFromDialOptions(dialOpts);

try
{
    var resourceNames = await machine.ResourceNamesAsync();
    logger.LogInformation("Resources:");
    logger.LogInformation("{Resources}", string.Join(", ", resourceNames.Select(x => x.ToString())));

    // Cameras - GetProperties
    string[] cameraNames =
    [
        "camera-dough-storage",
        "camera-cheese-pepperoni",
        "camera-cheese-storage",
        "camera-oven",
        "camera-cutter",
        "camera-lockers",
        "camera-prep",
        "camera-finishing"
    ];

    foreach (var cameraName in cameraNames)
    {
        try
        {
            await using var camera = await CameraClient.FromMachine(machine, cameraName);
            var props = await camera.GetProperties();
            logger.LogInformation("{CameraName} Properties return value: {Props}", cameraName, props);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "{CameraName}", cameraName);
            return;
        }
    }

    // Sensors - GetReadings
    string[] sensorNames =
    [
        "app8-api",
        "quality-check-pre-bake",
        "quality-check-finishing",
        "cpu_monitor",
        "temperature_monitor",
        "gpu_monitor",
        "throttling_monitor",
        "memory_monitor"
    ];

    foreach (var sensorName in sensorNames)
    {
        try
        {
            await using var sensor = await SensorClient.FromMachine(machine, sensorName);
            var readings = await sensor.GetReadings();
            logger.LogInformation("{SensorName} Readings return value: {Readings}",
                sensorName,
                string.Join(", ", readings.Select(x => $"{x.Key}: {x.Value}")));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "{SensorName}", sensorName);
            return;
        }
    }

    // Vision Services - GetProperties
    string[] visionServiceNames =
    [
        "pizza-detector",
        "pepperoni-quality-classifier",
        "dough-quality-classifier",
        "sauce-quality-classifier",
        "cheese-quality-classifier"
    ];

    foreach (var visionName in visionServiceNames)
    {
        try
        {
            var visionService = await VisionServiceClient.FromMachine(machine, visionName);
            var props = await visionService.GetProperties();
            logger.LogInformation("{VisionName} GetProperties return value: ClassificationsSupported={CS}, DetectionsSupported={DS}, ObjectPointCloudsSupported={OPC}",
                visionName, props.ClassificationsSupported, props.DetectionsSupported, props.ObjectPointCloudsSupported);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "{VisionName}", visionName);
            return;
        }
    }
}
finally
{
    await machine.DisposeAsync();
}
