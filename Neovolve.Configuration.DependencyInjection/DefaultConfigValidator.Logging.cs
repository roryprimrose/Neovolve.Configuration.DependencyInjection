// We want the extension methods to be exposed when hosting is available in the calling application
// ReSharper disable once CheckNamespace

namespace Neovolve.Configuration.DependencyInjection;

using System;
using Microsoft.Extensions.Logging;

internal partial class DefaultConfigValidator
{
    [LoggerMessage(EventId = 5002,
        EventName = "Neovolve.Configuration.DependencyInjection.IConfigValidator:ValidationFailed",
        Level = LogLevel.Error,
        Message = "Configuration reload for {TargetType} was rejected because validation failed: {Failures}")]
    static partial void LogValidationFailed(ILogger logger, Type targetType, string failures);
}
