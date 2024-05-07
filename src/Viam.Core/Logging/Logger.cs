using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Viam.Core.Logging
{
    internal static class Logger
    {
        private static readonly ConcurrentDictionary<string, ILogger> Loggers = new ConcurrentDictionary<string, ILogger>();
        private static ILoggerFactory _loggerFactory = NullLoggerFactory.Instance;
        public static void SetLoggerFactory(ILoggerFactory loggerFactory) => _loggerFactory = loggerFactory;

        public static ILogger GetLogger<T>() => Loggers.GetOrAdd(typeof(T).Name, (s) => _loggerFactory.CreateLogger(s));
    }
}
