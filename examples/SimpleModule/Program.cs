using Microsoft.Extensions.Logging;
using System.Text.Json.Serialization;
using Viam.App.V1;
using Viam.Core.Resources;
using Viam.Core.Resources.Components.Sensor;
using Viam.ModularResources;
using Viam.Serialization;
using Model = Viam.Core.Resources.Model;

var builder = ModuleBuilder.FromArgs(args);
builder.RegisterComponent<ISensor, ModularSensor>();
await builder.Build().Run();

public sealed class ModularSensor(ILogger<ModularSensor> logger, ViamResourceName resourceName)
    : ModularResource<ModularSensor>(logger, resourceName), ISensor, IModularResourceService
{
    public static Model Model { get; } = new("viam", "sensor", "mySensor");
    public static Service ServiceName => Service.SensorService;
    public static SubType SubType { get; } = SubType.Sensor;

    private Config? _config;

    public async ValueTask<Dictionary<string, object?>> GetReadings(IDictionary<string, object?>? extra = null,
        TimeSpan? timeout = null,
        CancellationToken cancellationToken = default)
    {
        await Task.Yield();
        Command command = new CreateCommand
        {
            Foo = "bar",
            Bar = 1,
            A = new A(),
            RequiredFooNum = MyEnum.Bar,
            NullableRequiredFooNum = MyEnum.Bar,
            As = [],
            ADict = new Dictionary<string, A>(),
            ADictNullableA = new Dictionary<string, A?>(),
            NullableADict = new Dictionary<string, A>(),
            NullableADictNullableA = new Dictionary<string, A?>(),
        };
        return command;
    }

    public override ValueTask Reconfigure(ComponentConfig config,
        IDictionary<ViamResourceName, IResourceBase> dependencies)
    {
        _config = Config.FromStruct(config.Attributes);
        return ValueTask.CompletedTask;
    }
}

[StructMappable]
public partial class Config
{
    [JsonPropertyName("bob")] 
    public string Name { get; set; } = string.Empty;
    public CfgStruct? NullableStruct { get; set; }
    public CfgStruct Struct { get; set; } = new CfgStruct();
}

[StructMappable]
public partial struct CfgStruct
{
    public short Short { get; set; }
    public short? NullableShort { get; set; }
}

[GenerateDictionaryMapper]
public partial class MyReadings
{
    public string? Now { get; set; }
    public string? Name { get; set; }
    public string? Foo { get; set; }
}


[GenerateDictionaryMapper]
public abstract partial class Command
{
}

[GenerateDictionaryMapper]
public partial class CreateCommand : Command
{
    public required string Foo { get; init; }
    public required short? Bar { get; init; }
    public required MyEnum RequiredFooNum { get; init; } = MyEnum.Bar;
    public required MyEnum? NullableRequiredFooNum { get; init; } = MyEnum.Bar;
    public MyEnum FooNum { get; init; } = MyEnum.Bar;
    public MyEnum? NullableFooNum { get; init; } = MyEnum.Bar;
    public required A[] As { get; init; } = [];
    public required A A { get; init; }
    public required IDictionary<string, A> ADict { get; init; } = new Dictionary<string, A>();
    public required IDictionary<string, A?> ADictNullableA { get; init; } = new Dictionary<string, A?>();
    public required IDictionary<string, A?>? NullableADictNullableA { get; init; } = new Dictionary<string, A?>();
    public required IDictionary<string, A>? NullableADict { get; init; } = new Dictionary<string, A>();
}

[GenerateDictionaryMapper]
public partial class DeleteCommand : Command
{
    public required string Bar { get; init; }
}

public enum MyEnum
{
    Foo,
    Bar
}

[GenerateDictionaryMapper]
public partial class A
{
    public string? Name { get; set; }
}