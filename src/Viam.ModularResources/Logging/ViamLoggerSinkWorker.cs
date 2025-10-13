using Viam.Common.V1;
using Viam.ModularResources.Services;

namespace Viam.ModularResources.Logging
{
    internal sealed class ViamLoggerSinkWorker(ViamLoggerProvider logProvider, ViamLoggerProviderOptions options, ModuleService moduleService) : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var buffer = new List<LogEntry>(options.MaxBatchSize);
            var flushTimer = new PeriodicTimer(options.FlushPeriod);

            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    // Wait for at least one item or a timer tick
                    var gotItem = false;

                    // Prefer data if available
                    while (buffer.Count < options.MaxBatchSize &&
                           logProvider.Reader.TryRead(out var item))
                    {
                        buffer.Add(item);
                        gotItem = true;
                    }

                    if (!gotItem)
                    {
                        var waitTask = logProvider.Reader.WaitToReadAsync(stoppingToken).AsTask();
                        var tickTask = flushTimer.WaitForNextTickAsync(stoppingToken).AsTask();

                        var completed = await Task.WhenAny(waitTask, tickTask);
                        if (completed == waitTask && await waitTask && logProvider.Reader.TryRead(out var first))
                        {
                            buffer.Add(first);
                            // try grab more without awaiting
                            while (buffer.Count < options.MaxBatchSize &&
                                   logProvider.Reader.TryRead(out var more))
                                buffer.Add(more);
                        }
                    }

                    if (buffer.Count > 0)
                    {
                        await SafeSendAsync(buffer, stoppingToken);
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
                    await SafeSendAsync(buffer, CancellationToken.None);
            }
        }

        private async Task SafeSendAsync(List<LogEntry> items, CancellationToken ct)
        {
            try
            {
                await moduleService.LogAsync(items, ct).ConfigureAwait(false);
            }
            catch
            {
                // Swallow to avoid crashing the host. Consider a retry/backoff strategy and metrics.
            }
        }
    }
}
