namespace Neovolve.Configuration.DependencyInjection.Generated
{
    using System;
    using Microsoft.Extensions.Logging;
    using Neovolve.Configuration.DependencyInjection.Comparison;

    /// <summary>
    ///     The <see cref="ConfigUpdateContext" /> class is the default <see cref="IConfigUpdateContext" /> used by
    ///     <see cref="DefaultConfigUpdater" /> to report configuration changes, read only properties and copy failures
    ///     while a generated <see cref="IConfigValueApplier{T}" /> applies updated values.
    /// </summary>
    internal sealed partial class ConfigUpdateContext : IConfigUpdateContext
    {
        private const string CopyValuesEventName =
            "Neovolve.Configuration.DependencyInjection.IConfigUpdater.UpdateConfig";

        private readonly ILogger? _logger;
        private readonly IConfigureWithOptions _options;
        private readonly Type _targetType;
        private readonly IValueProcessor _valueProcessor;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ConfigUpdateContext" /> class.
        /// </summary>
        /// <param name="targetType">The configuration type being updated.</param>
        /// <param name="logger">The optional logger used to record changes.</param>
        /// <param name="options">The options that control logging behaviour.</param>
        /// <param name="valueProcessor">The value processor used to identify changes for logging.</param>
        public ConfigUpdateContext(Type targetType, ILogger? logger, IConfigureWithOptions options,
            IValueProcessor valueProcessor)
        {
            _targetType = targetType;
            _logger = logger;
            _options = options;
            _valueProcessor = valueProcessor;
        }

        /// <inheritdoc />
        public bool Report(string propertyPath, object? previousValue, object? updatedValue)
        {
            if (_logger == null)
            {
                return false;
            }

            var changed = false;

            try
            {
                // Identify all the changes in the property value and log them.
                var changes = _valueProcessor.FindChanges(propertyPath, previousValue, updatedValue);

                foreach (var change in changes)
                {
                    changed = true;

                    var eventId = new EventId(5000, CopyValuesEventName + ":PropertyUpdated");

                    // A custom log format is required here, so the logging source generator is not used.
                    _logger.Log(_options.LogPropertyChangeLevel, eventId, change.MessageFormat, _targetType,
                        change.PropertyPath, change.FirstLogValue, change.SecondLogValue);
                }
            }
            catch (Exception ex)
            {
                // Record this failure with a reference to raising a GitHub issue.
                LogConfigChangeFailed(_logger, _targetType, propertyPath, ex);
            }

            return changed;
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
