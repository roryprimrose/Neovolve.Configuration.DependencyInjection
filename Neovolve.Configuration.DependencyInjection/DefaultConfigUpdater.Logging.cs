// We want the extension methods to be exposed when hosting is available in the calling application
// ReSharper disable once CheckNamespace

namespace Neovolve.Configuration.DependencyInjection;

using Microsoft.Extensions.Logging;

internal partial class DefaultConfigUpdater
{
    private const string CopyValuesEventName = "Neovolve.Configuration.DependencyInjection.IConfigUpdater.UpdateConfig";

    [LoggerMessage(EventId = 5001, EventName = CopyValuesEventName + ":ConfigUpdated", Level = LogLevel.Information,
        Message = "Configuration updated on {TargetType}")]
    static partial void LogConfigChanged(ILogger logger, Type targetType);
}