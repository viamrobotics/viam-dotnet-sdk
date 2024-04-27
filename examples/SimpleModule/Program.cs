using Google.Protobuf.WellKnownTypes;

using Viam.App.V1;
using Viam.Core.Resources;
using Viam.Core.Resources.Components;

using Model = Viam.Core.Resources.Model;

Registry.RegisterResourceCreator(Sensor.SubType,
                                 ModularSensor.Model,
                                 new ResourceCreatorRegistration((config, dependencies) => new ModularSensor(config, dependencies), ModularSensor.ValidateConfig));
var module = Viam.ModularResources.Module.FromArgs(args);
module.AddModelFromRegistry(Sensor.SubType, new Model(new ModelFamily("viam", "sensor"), "mySensor"));
await module.Run();

public class ModularSensor(ComponentConfig config, string[] dependencies) : ISensor, IAsyncReconfigurable
{
    public static Model Model { get; } = new Model(new ModelFamily("viam", "sensor"), "mySensor");

    public ViamResourceName ResourceName { get; } = new ViamResourceName(SubType.FromString(config.Api), config.Name);

    public static string[] ValidateConfig(ComponentConfig config) => Array.Empty<string>();

    public ValueTask<IDictionary<string, object?>> DoCommand(
        IDictionary<string, object?> command,
        TimeSpan? timeout = null,
        CancellationToken cancellationToken = default)
    {
        if (command.TryGetValue("command", out var cmd))
        {
            Console.WriteLine(cmd);
        }

        return new ValueTask<IDictionary<string, object?>>(new Dictionary<string, object?>());
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    public ValueTask StopResource() => throw new NotImplementedException();

    public ResourceStatus GetStatus() => throw new NotImplementedException();

    public ValueTask<IDictionary<string, object?>> GetReadings(Struct? extra = null,
                                                               TimeSpan? timeout = null,
                                                               CancellationToken cancellationToken =
                                                                   default)
    {
        var dict = new Dictionary<string, object?>() { { "Hello", "World" }, { "Now", DateTime.UtcNow.ToString("O") } };
        return new ValueTask<IDictionary<string, object?>>(dict);
    }

    public ValueTask Reconfigure(ComponentConfig config, IDictionary<ViamResourceName, IResourceBase> dependencies)
    {
        Console.WriteLine("Reconfiguring!");
        return ValueTask.CompletedTask;
    }
}
