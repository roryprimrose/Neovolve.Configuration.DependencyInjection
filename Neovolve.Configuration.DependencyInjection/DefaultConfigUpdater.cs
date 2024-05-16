namespace Neovolve.Configuration.DependencyInjection;

using System;
using System.Collections;
using System.Reflection;
using Microsoft.Extensions.Logging;

/// <summary>
///     The <see cref="DefaultConfigUpdater" />
///     class provides the default implementation for updating injected configuration values when configuration data
///     changes.
/// </summary>
public partial class DefaultConfigUpdater : IConfigUpdater
{
    private readonly IConfigureWithOptions _options;

    /// <summary>
    ///     Initializes a new instance of the <see cref="DefaultConfigUpdater" /> class.
    /// </summary>
    /// <param name="options">The options for updating configuration values.</param>
    public DefaultConfigUpdater(IConfigureWithOptions options)
    {
        _options = options;
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
    ///     Gets whether the old value and the updated value are the same.
    /// </summary>
    /// <param name="oldValue">The old configuration value.</param>
    /// <param name="updatedValue">The new configuration value.</param>
    /// <returns><c>true</c> if the configuration value has changed; otherwise <c>false</c>.</returns>
    /// <remarks>
    ///     The result of this method determines whether the property on the injected object will be updated with the new value
    ///     and optionally whether changes will be logged.
    /// </remarks>
    protected virtual bool IsMatchingValue(object? oldValue, object? updatedValue)
    {
        if (BothValuesNull(oldValue, updatedValue))
        {
            // There is no change
            return true;
        }

        if (oldValue == null)
        {
            return false;
        }

        if (updatedValue == null)
        {
            return false;
        }

        // If the type is IDictionary then we can check if the count has changed or if any of the items have changed
        // This needs to be checked before ICollection below because IDictionary implements ICollection but is more specific
        if (oldValue is IDictionary oldDictionary
            && updatedValue is IDictionary updatedDictionary)
        {
            return IsMatchingDictionary(oldDictionary, updatedDictionary);
        }

        // If the type is ICollection then we can check if the count has changed or if any of the items have changed
        if (oldValue is ICollection oldCollection
            && updatedValue is ICollection updatedCollection)
        {
            return IsMatchingCollection(oldCollection, updatedCollection);
        }

        // Any other scenario we can only rely on Equals
        if (oldValue.Equals(updatedValue))
        {
            return true;
        }

        return false;
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

        try
        {
            var oldValue = property.GetValue(injectedConfig);
            var updatedValue = property.GetValue(updatedConfig, null);

            if (IsMatchingValue(oldValue, updatedValue))
            {
                // Both values exist but they are the same
                return false;
            }

            property.SetValue(injectedConfig, updatedValue);

            if (logger != null)
            {
                LogPropertyChanged<T>(property, logger, targetType, oldValue, updatedValue);
            }

            return true;
        }
        catch (Exception ex)
        {
            // We have failed to copy the value across to the injected config value
            // We don't want to fail the application so we cannot allow the exception to throw up the stack
            // We can however attempt to obtain a logger so that we can report the failure
            if (logger != null)
            {
                LogConfigCopyFail(logger, targetType, property.Name, ex);
            }

            return false;
        }
    }

    private static bool BothValuesNull(object? oldValue, object? updatedValue)
    {
        if (oldValue != null)
        {
            return false;
        }

        if (updatedValue != null)
        {
            return false;
        }

        return true;
    }

    private static void LogPropertyChanged<T>(PropertyInfo property, ILogger logger, Type targetType, object oldValue,
        object updatedValue)
    {
        // Attempt to identify whether there is a friendly log message to be written
        // Most types (like value types) will work fine with ToString
        // Types like anything ICollection will not however
        // In these cases we can do a simple log like entries have been added or removed
        LogConfigPropertyChanged(logger, targetType, property.Name, oldValue, updatedValue);
    }

    private bool IsMatchingCollection(ICollection oldCollection, ICollection updatedCollection)
    {
        if (oldCollection.Count != updatedCollection.Count)
        {
            return false;
        }

        // We have the same number of items in the collection
        // We need to check if any of the items have changed
        var oldEnumerator = oldCollection.GetEnumerator();
        var updatedEnumerator = updatedCollection.GetEnumerator();

        while (oldEnumerator.MoveNext()
               && updatedEnumerator.MoveNext())
        {
            if (IsMatchingValue(oldEnumerator.Current, updatedEnumerator.Current) == false)
            {
                return false;
            }
        }

        if (oldEnumerator is IDisposable oldDisposable)
        {
            oldDisposable.Dispose();
        }

        if (updatedEnumerator is IDisposable updatedDisposable)
        {
            updatedDisposable.Dispose();
        }

        return true;
    }

    private bool IsMatchingDictionary(IDictionary oldDictionary, IDictionary updatedDictionary)
    {
        if (oldDictionary.Count != updatedDictionary.Count)
        {
            return false;
        }

        // We have the same number of items in the dictionary
        // We need to check if any of the items have changed
        foreach (DictionaryEntry oldEntry in oldDictionary)
        {
            if (updatedDictionary.Contains(oldEntry.Key) == false)
            {
                return false;
            }

            var updatedEntry = updatedDictionary[oldEntry.Key];

            if (IsMatchingValue(oldEntry.Value, updatedEntry) == false)
            {
                return false;
            }
        }

        return true;
    }

    private bool IsValueType(PropertyInfo property)
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