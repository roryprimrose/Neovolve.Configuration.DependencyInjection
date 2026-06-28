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
        public bool Report<TValue>(string propertyPath, TValue previousValue, TValue updatedValue)
        {
            if (_logger == null)
            {
                return false;
            }

            if (typeof(TValue).IsValueType)
            {
                // Value types cannot be routed through the object based evaluator pipeline without boxing, and they are
                // never collections, so the change is formatted and logged directly from the strongly typed values. The
                // generated applier only calls this method once the values have already been found to differ.
                try
                {
                    // Calling ToString directly (rather than via the null-conditional operator) keeps the call as a
                    // constrained virtual call so the value type is not boxed.
                    var valueChange = new IdentifiedChange(propertyPath, previousValue!.ToString() ?? string.Empty,
                        updatedValue!.ToString() ?? string.Empty);

                    LogChange(_logger, valueChange);
                }
                catch (Exception ex)
                {
                    // Record this failure with a reference to raising a GitHub issue.
                    LogConfigChangeFailed(_logger, _targetType, propertyPath, ex);
                }

                return true;
            }

            var changed = false;

            try
            {
                // A reference does not box when passed as object, so reference type values keep the full change
                // evaluator pipeline (collection, dictionary and custom evaluators with nested property paths).
                var changes = _valueProcessor.FindChanges(propertyPath, previousValue, updatedValue);

                foreach (var change in changes)
                {
                    changed = true;

                    LogChange(_logger, change);
                }
            }
            catch (Exception ex)
            {
                // Record this failure with a reference to raising a GitHub issue.
                LogConfigChangeFailed(_logger, _targetType, propertyPath, ex);
            }

            return changed;
        }

        private void LogChange(ILogger logger, IdentifiedChange change)
        {
            var eventId = new EventId(5000, CopyValuesEventName + ":PropertyUpdated");

            // A custom log format is required here, so the logging source generator is not used.
            logger.Log(_options.LogPropertyChangeLevel, eventId, change.MessageFormat, _targetType,
                change.PropertyPath, change.FirstLogValue, change.SecondLogValue);
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
