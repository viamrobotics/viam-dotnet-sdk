using Microsoft.Extensions.Logging;

using System.Text.Json.Serialization;

using Viam.App.V1;
using Viam.Core.Resources;
using Viam.Core.Resources.Components.Sensor;
using Viam.ModularResources;
using Viam.Serialization;

using Model = Viam.Core.Resources.Model;

var builder = Viam.ModularResources.ModuleBuilder.FromArgs(args);
builder.RegisterComponent<ISensor, ModularSensor>();
await builder.Build().Run();

public sealed class ModularSensor(ILogger<ModularSensor> logger, ViamResourceName resourceName) : ModularResource<ModularSensor>(logger, resourceName), ISensor, IModularResourceService
{
    public static Model Model { get; } = new("viam", "sensor", "mySensor");
    public static Service ServiceName => Service.SensorService;
    public static SubType SubType { get; } = SubType.Sensor;

    private Config? _config;

    public ValueTask<IDictionary<string, object?>> GetReadings(IDictionary<string, object?>? extra = null,
                                                               TimeSpan? timeout = null,
                                                               CancellationToken cancellationToken = default)
    {
        return ValueTask.FromResult(new MyReadings() { Now = DateTime.UtcNow.ToString("O"), Name = _config?.Name, Foo = "bar" }.ToDictionary());
    }

    public override ValueTask Reconfigure(ComponentConfig config, IDictionary<ViamResourceName, IResourceBase> dependencies)
    {
        _config = Config.FromStruct(config.Attributes);
        return ValueTask.CompletedTask;
    }
}

[DictionaryMapper]
public partial class MyReadings : MySubReadings
{
    [JsonPropertyName("now")]
    public required string Now { get; init; }

    [JsonPropertyName("name")]
    public string? Name { get; init; }
}

[DictionaryMapper]
public partial class MySubReadings
{
    [JsonPropertyName("foo")]
    public required string Foo { get; init; }
}

[StructMappable]
public partial class Config
{
    [JsonPropertyName("bob")]
    public string Name { get; set; } = string.Empty;
}