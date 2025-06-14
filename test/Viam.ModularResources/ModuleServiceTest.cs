using Microsoft.Extensions.DependencyInjection;
using Viam.Core.Resources;
using Viam.Core.Resources.Components.Sensor;
using Viam.Module.V1;

namespace Viam.ModularResources.Test
{
    public class ModuleServiceTest
    {
        [Test]
        public async Task Test1()
        {
            var servicesBuilder = new ServiceCollection();
            servicesBuilder.AddTransient<SampleSensor>();
            servicesBuilder.AddTransient<ISensor, SampleSensor>();
            servicesBuilder.AddTransient<IResourceBase, SampleSensor>();
            servicesBuilder.BuildServiceProvider();
            servicesBuilder.AddLogging();
            servicesBuilder.AddSingleton<Viam.ModularResources.Services.ModuleService>();
            var serviceProvider = servicesBuilder.BuildServiceProvider();
            var service = serviceProvider.GetRequiredService<Viam.ModularResources.Services.ModuleService>();
            Assert.That(service, Is.Not.Null);
            await service.Ready(new ReadyRequest() { ParentAddress = "pete-orinnano-1-main.vwib729pdp.viam.cloud" },
                null);
            var res = await service.AddResource(
                new AddResourceRequest()
                {
                    Config = new App.V1.ComponentConfig()
                    { Api = "rdk:component:sensor", Model = "test:sensor:mySensor" }
                }, null);
        }
    }

    internal class SampleSensor : ISensor, IModularResource
    {
        public Model Model { get; } = new("test", "sensor", "mySensor");
        public ViamResourceName ResourceName { get; } = new ViamResourceName(SubType, "mySensor");
        public static SubType SubType { get; } = SensorClient.SubType;

        public ValueTask DisposeAsync()
        {
            return ValueTask.CompletedTask;
        }

        public ValueTask<Dictionary<string, object?>> DoCommand(IDictionary<string, object?> command,
            TimeSpan? timeout = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public ValueTask<Dictionary<string, object?>> GetReadings(IDictionary<string, object?>? extra = null,
            TimeSpan? timeout = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public ResourceStatus GetStatus()
        {
            throw new NotImplementedException();
        }

        public ValueTask StopResource()
        {
            throw new NotImplementedException();
        }

        public string[] ValidateConfig(App.V1.ComponentConfig config)
        {
            throw new NotImplementedException();
        }
    }
}