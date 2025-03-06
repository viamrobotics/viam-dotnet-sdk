using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Logging;
using System.Reflection.PortableExecutable;

using Viam.App.V1;
using Viam.Client.Clients;
using Viam.Client.Dialing;
using Viam.Core.Resources;
using Viam.Core.Resources.Components.Base;
using Viam.Core.Resources.Components.Camera;
using Viam.Core.Resources.Components.Sensor;
using Viam.ModularResources;
using Model = Viam.Core.Resources.Model;
var loggerFactory = LoggerFactory.Create(b =>
{
    b.AddJsonConsole(options =>
    {
        //options.UseUtcTimestamp = true;
        //options.TimestampFormat = "yyyy-MM-ddTHH:mm:ss.fff";
    }).SetMinimumLevel(LogLevel.Trace);
});
var robotClientOptions = DialOptions.FromAddress("pete-wsl-home-main.vwib729pdp.viam.cloud").WithLogging(loggerFactory).WithApiCredentials("5pjejrnwnmlxcerdkb5jfp2aavmauq6c", "6d5aa03e-bd75-4dd3-a981-4a1f0580704c");

var robotClient = await RobotClient.AtAddressAsync(robotClientOptions);
var my_dotnet_sensor = SensorClient.FromRobot(robotClient, "myDotnetSensor");
await my_dotnet_sensor.DoCommand(new Dictionary<string, object?>());
//var module = Viam.ModularResources.Module.FromArgs(args);
//module.AddModelFromRegistry(SensorClient.SubType, ModularSensor.Model);
//await module.Run();

//public sealed class ModularSensor : SimpleModularComponent, ISensor
//{
//    static ModularSensor()
//    {
//        Registry.RegisterResourceCreator(SensorClient.SubType,
//                                         Model,
//                                         new ResourceCreatorRegistration(
//                                             (logger, config, dependencies) =>
//                                                 new ModularSensor(logger, config, dependencies),
//                                             ValidateConfig));
//    }

//    public ModularSensor(ILogger logger, ComponentConfig config, IDictionary<ViamResourceName, IResourceBase> dependencies) : base(logger, config, dependencies)
//    {
//        Reconfigure(config, dependencies).ConfigureAwait(false).GetAwaiter().GetResult();
//    }

//    public static Model Model { get; } = new("viam", "sensor", "mySensor");

//    public static string[] ValidateConfig(ComponentConfig config) => Array.Empty<string>();

//    public ValueTask<IDictionary<string, object?>> GetReadings(Struct? extra = null,
//                                                               TimeSpan? timeout = null,
//                                                               CancellationToken cancellationToken =
//                                                                   default)
//    {
//        IDictionary<string, object?> dict = new Dictionary<string, object?>() { { "Now", DateTime.UtcNow.ToString("O") } };

//        return ValueTask.FromResult(dict);
//    }
//}
