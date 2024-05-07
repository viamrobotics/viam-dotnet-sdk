using Viam.Core.Resources;
using Viam.Core.Resources.Services;

namespace Viam.ModularResources
{
    internal static partial class LogMessages
    {
        [LoggerMessage(EventId = 3000, Message = "Unable to find service for {SubType}. Available Services {AvailableServices}", Level = LogLevel.Information)]
        internal static partial void LogModularServiceMissingService(this ILogger logger, SubType subType, IServiceBase[] availableServices);

        [LoggerMessage(EventId = 3001, Message = "Loading resource creator for {SubType} {Model}", Level = LogLevel.Debug)]
        internal static partial void LogModularServiceLookingForService(this ILogger logger, SubType subType, Model model);
    }
}
