namespace Viam.ModularResources.Logging
{
    public sealed class ViamLoggerProviderOptions
    {
        public int ChannelCapacity { get; set; } = 10_000;
        public int MaxBatchSize { get; set; } = 100;
        public TimeSpan FlushPeriod { get; set; } = TimeSpan.FromSeconds(2);
        public bool IncludeScopes { get; set; } = true;
    }
}
