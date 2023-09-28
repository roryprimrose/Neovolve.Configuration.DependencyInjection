// We want the extension methods to be exposed when hosting is available in the calling application
// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.Hosting;

using System;
using Microsoft.Extensions.Logging;

public static partial class HostBuilderContextExtensions
{
    [LoggerMessage(EventId = 5000, EventName = CopyValuesEventName, Level = LogLevel.Information,
        Message = "Configuration updated on {targetType}")]
    static partial void LogConfigChanged(ILogger logger, Type targetType);

    [LoggerMessage(EventId = 5001, EventName = CopyValuesEventName, Level = LogLevel.Debug,
        Message =
            "Configuration updated on property {targetType}.{property} from '{oldValue}' to '{newValue}'")]
    static partial void LogConfigChanged(ILogger logger, Type targetType, string property, object? oldValue,
        object? newValue);

    [LoggerMessage(EventId = 5003, EventName = CopyValuesEventName, Level = LogLevel.Warning,
        Message =
            "Unable to update property {targetType}.{property} for config change because the property is read only")]
    static partial void LogConfigCopyDenied(ILogger logger, Type targetType, string property);

    [LoggerMessage(EventId = 5002, EventName = CopyValuesEventName, Level = LogLevel.Error,
        Message = "Failed to copy hot reload config value for property {targetType}.{property}")]
    static partial void LogConfigCopyFail(ILogger logger, Exception ex, Type targetType, string property);
}