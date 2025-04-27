using Microsoft.Extensions.Logging;

using Viam.App.V1;
using Viam.Core.Resources;
using Viam.Core.Resources.Components.Sensor;
using Viam.ModularResources;
using Model = Viam.Core.Resources.Model;
var loggerFactory = LoggerFactory.Create(b =>
{
    b.AddJsonConsole(options =>
    {
        options.TimestampFormat = "yyyy-MM-ddTHH:mm:ss.fff";
    }).SetMinimumLevel(LogLevel.Trace);
});

var module = Viam.ModularResources.Module.FromArgs(args);
module.AddModelFromRegistry(SensorClient.SubType, ModularSensor.Model);
await module.Run();

public sealed class ModularSensor : SimpleModularComponent, ISensor
{
    static ModularSensor()
    {
        Registry.RegisterResourceCreator(SensorClient.SubType,
                                         Model,
                                         new ResourceCreatorRegistration(
                                             (logger, config, dependencies) =>
                                                 new ModularSensor(logger, config, dependencies),
                                             ValidateConfig));
    }

    public ModularSensor(ILogger logger, ComponentConfig config, IDictionary<ViamResourceName, IResourceBase> dependencies) : base(logger, config, dependencies)
    {
        Reconfigure(config, dependencies).ConfigureAwait(false).GetAwaiter().GetResult();
    }

    public static Model Model { get; } = new("viam", "sensor", "mySensor");

    public static string[] ValidateConfig(ComponentConfig config) => Array.Empty<string>();

    public ValueTask<IDictionary<string, object?>> GetReadings(IDictionary<string, object?>? extra = null,
                                                               TimeSpan? timeout = null,
                                                               CancellationToken cancellationToken =
                                                                   default)
    {
        IDictionary<string, object?> dict = new Dictionary<string, object?>() { { "Now", DateTime.UtcNow.ToString("O") } };

        return ValueTask.FromResult(dict);
    }
}
