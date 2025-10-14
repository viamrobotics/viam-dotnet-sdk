using Viam.Common.V1;
using Viam.Core.Clients;

namespace Viam.ModularResources.Logging
{
    internal sealed class ViamLoggerSinkWorker(ViamLoggerProvider logProvider, ViamLoggerProviderOptions options, IMachineClientAccessor clientAccessor) : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var machineClient = await clientAccessor.GetAsync(stoppingToken).ConfigureAwait(false);
            var buffer = new List<LogEntry>(options.MaxBatchSize);
            //using var flushTimer = new PeriodicTimer(options.FlushPeriod);

            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    if (await logProvider.Reader.WaitToReadAsync(stoppingToken).ConfigureAwait(false) == false)
                        break;
                    var entry = await logProvider.Reader.ReadAsync(stoppingToken).ConfigureAwait(false);
                    buffer.Add(entry);

                    if (buffer.Count > 0)
                    {
                        await SafeSendAsync(machineClient, buffer, stoppingToken);
                        buffer.Clear();
                    }
                }
            }
            finally
            {
                // Final drain on shutdown
                while (logProvider.Reader.TryRead(out var item))
                    buffer.Add(item);
                if (buffer.Count > 0)
                    await SafeSendAsync(machineClient, buffer, CancellationToken.None);
            }
        }

        private static async Task SafeSendAsync(MachineClientBase machineClient, List<LogEntry> items, CancellationToken ct)
        {
            try
            {
                await machineClient.LogAsync(items, ct).ConfigureAwait(false);
            }
            catch(Exception ex)
            {
                // Swallow to avoid crashing the host. Consider a retry/backoff strategy and metrics.
                Console.WriteLine($"Failed to send logs: {ex}");
            }
        }
    }
}
