namespace Neovolve.Configuration.DependencyInjection;

using System.Reflection;
using Microsoft.Extensions.Logging;
using Neovolve.Configuration.DependencyInjection.Comparison;

/// <summary>
///     The <see cref="DefaultConfigUpdater" />
///     class provides the default implementation for updating injected configuration values when configuration data
///     changes.
/// </summary>
public partial class DefaultConfigUpdater : IConfigUpdater
{
    private readonly IConfigureWithOptions _options;
    private readonly IValueProcessor _valueProcessor;

    /// <summary>
    ///     Initializes a new instance of the <see cref="DefaultConfigUpdater" /> class.
    /// </summary>
    /// <param name="valueProcessor">The value processor used to identify whether configuration values have changed.</param>
    /// <param name="options">The options for updating configuration values.</param>
    public DefaultConfigUpdater(IValueProcessor valueProcessor, IConfigureWithOptions options)
    {
        _valueProcessor = valueProcessor ?? throw new ArgumentNullException(nameof(valueProcessor));
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    /// <inheritdoc />
    public void UpdateConfig<T>(T? injectedConfig, T updatedConfig, string? name, ILogger? logger)
    {
        if (injectedConfig == null)
        {
            return;
        }

        if (updatedConfig == null)
        {
            return;
        }

        var targetType = typeof(T);
        var properties = targetType.GetBindableProperties(true);

        // The IOptionsMonitor<T>.OnChange event gets triggered twice on a change notification for file based configuration
        // To prevent noise in logging and updates to properties, we want to try to detect when an actual change has been made
        // We only want to set a property value if there is a change in the value, and we want to only log that changes have
        // occurred the first time a change is detected
        var changesFound = false;

        foreach (var property in properties)
        {
            var propertyUpdated = UpdateProperty(property, injectedConfig, updatedConfig, logger);

            if (propertyUpdated)
            {
                changesFound = true;
            }
        }

        // We have a change to the configuration
        if (changesFound)
        {
            if (logger != null)
            {
                LogConfigChanged(logger, targetType);
            }
        }
    }

    /// <summary>
    ///     Gets whether the property can be updated with new data.
    /// </summary>
    /// <param name="property">The property that has changed.</param>
    /// <returns><c>true</c> if the property can be updated; otherwise <c>false</c>.</returns>
    /// <remarks>
    ///     A runtime exception will be thrown if this method is overridden to return <c>true</c> when the property does not
    ///     support write operations.
    ///     The intended use of this virtual method is to indicate a writeable property as readonly as a way of preventing it
    ///     from being updated when configuration data changes.
    /// </remarks>
    protected virtual bool IsWritable(PropertyInfo property)
    {
        if (property.CanWrite == false)
        {
            return false;
        }

        if (property.SetMethod.IsPublic == false)
        {
            return false;
        }

        return true;
    }

    /// <summary>
    ///     Updates the property value on the injected configuration object with the value from the updated configuration
    ///     object.
    /// </summary>
    /// <typeparam name="T">The type of configuration object being updated.</typeparam>
    /// <param name="property">The property that has changed.</param>
    /// <param name="injectedConfig">The injected configuration to update.</param>
    /// <param name="updatedConfig">The updated configuration that contains the new data.</param>
    /// <param name="logger">The optional logging object.</param>
    /// <returns><c>true</c> if the property was updated; otherwise <c>false</c>.</returns>
    protected virtual bool UpdateProperty<T>(PropertyInfo property, T injectedConfig, T updatedConfig,
        ILogger? logger)
    {
        var targetType = typeof(T);

        if (IsWritable(property) == false)
        {
            ReportReadOnlyProperty(targetType, property, logger);

            return false;
        }

        object? updatedValue;
        object? previousValue;

        try
        {
            previousValue = property.GetValue(injectedConfig);
            updatedValue = property.GetValue(updatedConfig, null);

            // Set the property value on the target object regardless of whether a change has been detected
            // This ensures that the property is set if there is a bug in change detection to ensure correct operational state of the application
            property.SetValue(injectedConfig, updatedValue);
        }
        catch (Exception ex)
        {
            // We have failed to copy the value across to the injected config value
            // We don't want to fail the application, so we cannot allow the exception to throw up the stack
            // We can however attempt to obtain a logger so that we can report the failure
            if (logger != null)
            {
                LogConfigCopyFail(logger, targetType, property.Name, ex);
            }

            return false;
        }

        if (logger == null)
        {
            // There is no logger available, so we don't need to do any further processing
            return false;
        }

        if (logger.IsEnabled(_options.LogPropertyChangeLevel) == false)
        {
            // The logger that is available is not enabled for the level that we want to log at, so we don't need to do any further processing
            return false;
        }

        var propertyChanged = false;

        try
        {
            // Identify all the changes in the property value and log them

            var changes = _valueProcessor.FindChanges(property.Name, previousValue, updatedValue);

            foreach (var change in changes)
            {
                propertyChanged = true;

                var eventId = new EventId(5000, CopyValuesEventName + ":PropertyUpdated");

                // We are not using the logging source generator here because of the need to use a custom log format
                logger.Log(_options.LogPropertyChangeLevel, eventId, change.MessageFormat, targetType,
                    change.PropertyPath, change.FirstLogValue, change.SecondLogValue);
            }
        }
        catch (Exception ex)
        {
            // Record this failure with a reference to raising a GitHub issue
            LogConfigChangeFailed(logger, targetType, property.Name, ex);
        }

        return propertyChanged;
    }

    private static bool IsValueType(PropertyInfo property)
    {
        if (property.PropertyType.IsValueType)
        {
            return true;
        }

        if (property.PropertyType == typeof(string))
        {
            return true;
        }

        return false;
    }

    private void ReportReadOnlyProperty(Type targetType, PropertyInfo property, ILogger? logger)
    {
        if (logger == null)
        {
            return;
        }

        if (_options.LogReadOnlyPropertyType == LogReadOnlyPropertyType.None)
        {
            // Logging of readonly properties is disabled
            return;
        }

        if (_options.LogReadOnlyPropertyType == LogReadOnlyPropertyType.All)
        {
            LogConfigCopyDenied(logger, _options.LogReadOnlyPropertyLevel, targetType, property.Name);
        }

        if (IsValueType(property))
        {
            LogConfigCopyDenied(logger, _options.LogReadOnlyPropertyLevel, targetType, property.Name);
        }
    }
}