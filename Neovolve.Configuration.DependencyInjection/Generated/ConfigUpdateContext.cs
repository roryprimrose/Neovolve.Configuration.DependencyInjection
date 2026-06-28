namespace Neovolve.Configuration.DependencyInjection.Generated
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using Microsoft.Extensions.Logging;

    /// <summary>
    ///     The <see cref="ConfigUpdateContext" /> class is the default <see cref="IConfigUpdateContext" /> used by
    ///     <see cref="DefaultConfigUpdater" /> to detect and log configuration changes while a generated
    ///     <see cref="IConfigValueApplier{T}" /> applies updated values.
    /// </summary>
    internal sealed partial class ConfigUpdateContext : IConfigUpdateContext
    {
        private const string CopyValuesEventName =
            "Neovolve.Configuration.DependencyInjection.IConfigUpdater.UpdateConfig";

        private const string ChangeMessagePrefix =
            "Configuration updated on property {TargetType}.{PropertyPath} ";

        private const string ValueChangeFormat = ChangeMessagePrefix + "from '{OldValue}' to '{NewValue}'";

        private const string CountChangeFormat = ChangeMessagePrefix + "from {OldValue} entries to {NewValue} entries";

        private readonly ILogger? _logger;
        private readonly IConfigureWithOptions _options;
        private readonly Type _targetType;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ConfigUpdateContext" /> class.
        /// </summary>
        /// <param name="targetType">The configuration type being updated.</param>
        /// <param name="logger">The optional logger used to record changes.</param>
        /// <param name="options">The options that control logging behaviour.</param>
        public ConfigUpdateContext(Type targetType, ILogger? logger, IConfigureWithOptions options)
        {
            _targetType = targetType;
            _logger = logger;
            _options = options;
        }

        /// <inheritdoc />
        public bool ReportValue<TValue>(string propertyPath, TValue previousValue, TValue updatedValue)
        {
            if (_logger == null)
            {
                return false;
            }

            try
            {
                if (EqualityComparer<TValue>.Default.Equals(previousValue, updatedValue))
                {
                    return false;
                }

                LogValueChange(propertyPath, Format(previousValue), Format(updatedValue));

                return true;
            }
            catch (Exception ex)
            {
                LogConfigChangeFailed(_logger, _targetType, propertyPath, ex);

                return false;
            }
        }

        /// <inheritdoc />
        public bool ReportValues<TItem>(string propertyPath, IReadOnlyList<TItem>? previousValues,
            IReadOnlyList<TItem>? updatedValues)
        {
            if (_logger == null)
            {
                return false;
            }

            try
            {
                var previousCount = previousValues?.Count ?? 0;
                var updatedCount = updatedValues?.Count ?? 0;

                if (previousCount != updatedCount)
                {
                    LogCountChange(propertyPath, previousCount, updatedCount);

                    return true;
                }

                if (previousValues == null
                    || updatedValues == null)
                {
                    return false;
                }

                var changed = false;

                for (var index = 0; index < previousCount; index++)
                {
                    if (EqualityComparer<TItem>.Default.Equals(previousValues[index], updatedValues[index]))
                    {
                        continue;
                    }

                    LogValueChange($"{propertyPath}[{index}]", Format(previousValues[index]),
                        Format(updatedValues[index]));

                    changed = true;
                }

                return changed;
            }
            catch (Exception ex)
            {
                LogConfigChangeFailed(_logger, _targetType, propertyPath, ex);

                return false;
            }
        }

        /// <inheritdoc />
        public bool ReportCount(string propertyPath, ICollection? previousValue, ICollection? updatedValue)
        {
            if (_logger == null)
            {
                return false;
            }

            try
            {
                var previousCount = previousValue?.Count ?? 0;
                var updatedCount = updatedValue?.Count ?? 0;

                if (previousCount == updatedCount)
                {
                    return false;
                }

                LogCountChange(propertyPath, previousCount, updatedCount);

                return true;
            }
            catch (Exception ex)
            {
                LogConfigChangeFailed(_logger, _targetType, propertyPath, ex);

                return false;
            }
        }

        /// <inheritdoc />
        public void ReportCopyFailure(string propertyName, Exception exception)
        {
            if (_logger == null)
            {
                return;
            }

            LogConfigCopyFail(_logger, _targetType, propertyName, exception);
        }

        /// <inheritdoc />
        public void ReportReadOnly(string propertyPath, bool isValueType)
        {
            if (_logger == null)
            {
                return;
            }

            if (_options.LogReadOnlyPropertyType == LogReadOnlyPropertyType.None)
            {
                // Logging of read only properties is disabled.
                return;
            }

            if (_options.LogReadOnlyPropertyType == LogReadOnlyPropertyType.All)
            {
                LogConfigCopyDenied(_logger, _options.LogReadOnlyPropertyLevel, _targetType, propertyPath);
            }

            if (isValueType)
            {
                LogConfigCopyDenied(_logger, _options.LogReadOnlyPropertyLevel, _targetType, propertyPath);
            }
        }

        private static string Format<T>(T value)
        {
            if (typeof(T).IsValueType)
            {
                // A value type is never null and calling ToString directly is a constrained virtual call, so the
                // value is not boxed.
                return value!.ToString() ?? string.Empty;
            }

            // A reference is cast to object without boxing; the runtime treats the box of a reference type as a no-op.
            var reference = (object?)value;

            return reference == null ? "null" : reference.ToString() ?? "null";
        }

        private void LogCountChange(string propertyPath, int previousCount, int updatedCount)
        {
            var eventId = new EventId(5000, CopyValuesEventName + ":PropertyUpdated");

            _logger!.Log(_options.LogPropertyChangeLevel, eventId, CountChangeFormat, _targetType, propertyPath,
                previousCount, updatedCount);
        }

        private void LogValueChange(string propertyPath, string previousValue, string updatedValue)
        {
            var eventId = new EventId(5000, CopyValuesEventName + ":PropertyUpdated");

            // A custom log format is required here, so the logging source generator is not used.
            _logger!.Log(_options.LogPropertyChangeLevel, eventId, ValueChangeFormat, _targetType, propertyPath,
                previousValue, updatedValue);
        }

        /// <inheritdoc />
        public bool IsChangeLoggingEnabled
        {
            get
            {
                if (_logger == null)
                {
                    return false;
                }

                return _logger.IsEnabled(_options.LogPropertyChangeLevel);
            }
        }
    }
}
