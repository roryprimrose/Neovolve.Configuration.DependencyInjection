namespace Neovolve.Configuration.DependencyInjection.Generated
{
    using System;
    using Microsoft.Extensions.Logging;

    internal sealed partial class ConfigUpdateContext
    {
        [LoggerMessage(EventId = 5004, EventName = CopyValuesEventName + ":LogFailure", Level = LogLevel.Warning,
            Message =
                "Failed to log change of value for property {TargetType}.{Property}. Please report exception to https://github.com/roryprimrose/Neovolve.Configuration.DependencyInjection/issues")]
        static partial void LogConfigChangeFailed(ILogger logger, Type targetType, string property, Exception ex);

        [LoggerMessage(EventId = 5003, EventName = CopyValuesEventName + ":UpdateDenied",
            Message =
                "Unable to update property {TargetType}.{Property} for config change because the property is read only")]
        static partial void LogConfigCopyDenied(ILogger logger, LogLevel logLevel, Type targetType, string property);

        [LoggerMessage(EventId = 5002, EventName = CopyValuesEventName + ":UpdateFailed", Level = LogLevel.Error,
            Message = "Failed to copy hot reload config value for property {TargetType}.{Property}")]
        static partial void LogConfigCopyFail(ILogger logger, Type targetType, string property, Exception ex);
    }
}
