using System.Threading.Channels;
using Google.Protobuf.WellKnownTypes;
using Viam.Common.V1;

namespace Viam.ModularResources.Logging
{
    internal sealed class ViamLoggerProvider(ViamLoggerProviderOptions options) : ILoggerProvider, ISupportExternalScope
    {
        private IExternalScopeProvider _scopeProvider = NullExternalScopeProvider.Instance;

        private readonly Channel<LogEntry> _channel = Channel.CreateBounded<LogEntry>(new BoundedChannelOptions(options.ChannelCapacity)
        {
            SingleReader = true,
            SingleWriter = false,
            FullMode = BoundedChannelFullMode.Wait
        });

        public ChannelReader<LogEntry> Reader => _channel.Reader;
        public ChannelWriter<LogEntry> Writer => _channel.Writer;

        public ILogger CreateLogger(string categoryName) =>
            new ViamLogger(categoryName, options, _scopeProvider, Writer);

        public void SetScopeProvider(IExternalScopeProvider scopeProvider) =>
            _scopeProvider = scopeProvider ?? NullExternalScopeProvider.Instance;

        private sealed class ViamLogger(
            string category,
            ViamLoggerProviderOptions opts,
            IExternalScopeProvider scopes,
            ChannelWriter<LogEntry> writer)
            : ILogger
        {
            public IDisposable BeginScope<TState>(TState state) where TState : notnull => scopes.Push(state);

            public bool IsEnabled(LogLevel logLevel) => logLevel >= opts.MinimumLevel;

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state,
                Exception? exception, Func<TState, Exception?, string> formatter)
            {
                if (!IsEnabled(logLevel)) return;

                // Capture state and scope as dictionaries (keep allocations modest)
                IReadOnlyDictionary<string, object?>? stateDict = state as IReadOnlyDictionary<string, object?>;
                stateDict ??= state is IEnumerable<KeyValuePair<string, object?>> kvps
                    ? new Dictionary<string, object?>(kvps)
                    : null;

                IReadOnlyDictionary<string, object?>? scopeDict = null;
                if (opts.IncludeScopes)
                {
                    var flat = new Dictionary<string, object?>();
                    scopes.ForEachScope((scopeObj, acc) =>
                    {
                        if (scopeObj is IEnumerable<KeyValuePair<string, object?>> s)
                            foreach (var kv in s) acc[kv.Key] = kv.Value;
                        else
                            acc[$"scope:{scopeObj?.GetType().Name}"] = scopeObj;
                    }, flat);
                    if (flat.Count > 0) scopeDict = flat;
                }

                var item = new LogEntry()
                {
                    LoggerName = category,
                    Time = Timestamp.FromDateTimeOffset(DateTimeOffset.UtcNow),
                    Host = Environment.MachineName,
                    Level = "",
                    Message = formatter(state, exception),
                    Caller = null,
                    Fields = { },
                    Stack = ""
                };

                _ = writer.TryWrite(item); // non-blocking; drop if full (configurable)
            }
        }

        internal sealed class NullExternalScopeProvider : IExternalScopeProvider
        {
            public static readonly IExternalScopeProvider Instance = new NullExternalScopeProvider();

            private NullExternalScopeProvider() { }

            public void ForEachScope<TState>(Action<object, TState> callback, TState state)
            {
                // no scopes → do nothing
            }

            public IDisposable Push(object? state) => NullScope.Scope;

            private sealed class NullScope : IDisposable
            {
                public static readonly IDisposable Scope = new NullScope();
                public void Dispose() { }
            }
        }

        public void Dispose()
        {
            // TODO release managed resources here
        }
    }
}
