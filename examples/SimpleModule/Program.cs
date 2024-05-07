using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Logging;
using Viam.App.V1;
using Viam.Core;
using Viam.Core.Resources;
using Viam.Core.Resources.Components;
using Viam.Core.Utils;
using Model = Viam.Core.Resources.Model;

Registry.RegisterResourceCreator(Sensor.SubType,
                                 ModularSensor.Model,
                                 new ResourceCreatorRegistration((logger, config, dependencies) => new ModularSensor(logger, config, dependencies), ModularSensor.ValidateConfig));
var module = Viam.ModularResources.Module.FromArgs(args);
module.AddModelFromRegistry(Sensor.SubType, new Model(new ModelFamily("viam", "sensor"), "mySensor"));
await module.Run();

public class ModularSensor : ISensor, IAsyncReconfigurable
{
    private readonly ILogger _logger;

    public ModularSensor(ILogger logger, ComponentConfig config, IDictionary<ViamResourceName, IResourceBase> dependencies)
    {
        _logger = logger;
        ResourceName = new ViamResourceName(SubType.FromString(config.Api), config.Name);
        Reconfigure(config, dependencies);
    }

    public static Model Model { get; } = new Model(new ModelFamily("viam", "sensor"), "mySensor");

    public ViamResourceName ResourceName { get; } 
    private ISensor? _temperatureSensor;

    public static string[] ValidateConfig(ComponentConfig config) => Array.Empty<string>();

    public ValueTask<IDictionary<string, object?>> DoCommand(
        IDictionary<string, object?> command,
        TimeSpan? timeout = null,
        CancellationToken cancellationToken = default)
    {
        //pmic_temp
        if (command.TryGetValue("command", out var cmd))
        {
            Console.WriteLine(cmd);
        }

        return new ValueTask<IDictionary<string, object?>>(new Dictionary<string, object?>());
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    public ValueTask StopResource() => throw new NotImplementedException();

    public ResourceStatus GetStatus() => throw new NotImplementedException();

    public async ValueTask<IDictionary<string, object?>> GetReadings(Struct? extra = null,
                                                               TimeSpan? timeout = null,
                                                               CancellationToken cancellationToken =
                                                                   default)
    {
        if (_temperatureSensor == null)
            throw new MissingDependencyException();
        var reading = await _temperatureSensor.GetReadings(cancellationToken: cancellationToken);
        var dict = new Dictionary<string, object?>() { { "Now", DateTime.UtcNow.ToString("O") } };
        if (reading.TryGetValue("pmic_temp", out var tempObj))
        {
            if (tempObj is double temp)
                dict.Add("temp", temp.ToString("f2"));
        }

        if (reading.TryGetValue("a", out var aObj))
        {
            if (aObj is int a)
                dict.Add("a", a);
        }

        return dict;
    }

    public ValueTask Reconfigure(ComponentConfig config, IDictionary<ViamResourceName, IResourceBase> dependencies)
    {
        _logger.LogDebug("Reconfiguring!");
        var cfg = config.Attributes.ToDictionary();
        var cfgName = cfg["sensor_name"] as string;
        var sensorName = Sensor.GetResourceName(cfgName);
        var sensor = dependencies[sensorName] as ISensor;
        _temperatureSensor = sensor ?? throw new MissingDependencyException(cfgName);
        return ValueTask.CompletedTask;
    }
}
