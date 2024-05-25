﻿// We want the extension methods to be exposed when hosting is available in the calling application
// ReSharper disable once CheckNamespace

namespace Neovolve.Configuration.DependencyInjection;

using Microsoft.Extensions.Logging;

public partial class DefaultConfigUpdater
{
    private const string CopyValuesEventName = "Neovolve.Configuration.DependencyInjection.IConfigUpdater.UpdateConfig";

    [LoggerMessage(EventId = 5000, EventName = CopyValuesEventName + ":ConfigUpdated", Level = LogLevel.Information,
        Message = "Configuration updated on {targetType}")]
    static partial void LogConfigChanged(ILogger logger, Type targetType);

    [LoggerMessage(EventId = 5003, EventName = CopyValuesEventName + ":UpdateDenied",
        Message =
            "Unable to update property {targetType}.{property} for config change because the property is read only")]
    static partial void LogConfigCopyDenied(ILogger logger, LogLevel logLevel, Type targetType, string property);

    [LoggerMessage(EventId = 5002, EventName = CopyValuesEventName + ":UpdateFailed", Level = LogLevel.Error,
        Message = "Failed to copy hot reload config value for property {targetType}.{property}")]
    static partial void LogConfigCopyFail(ILogger logger, Type targetType, string property, Exception ex);

    [LoggerMessage(EventId = 5001, EventName = CopyValuesEventName + ":PropertyUpdated", Level = LogLevel.Debug,
        Message =
            "Configuration updated on property {targetType}.{propertyPath} from '{oldValue}' to '{newValue}'")]
    static partial void LogConfigPropertyChanged(ILogger logger, Type targetType, string propertyPath, object? oldValue,
        object? newValue);
}