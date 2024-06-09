namespace Neovolve.Configuration.DependencyInjection;

using System.Collections.ObjectModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

/// <summary>
///     The <see cref="IConfigureWithOptions" /> interface defines the options used to configure how the configuration
///     update and logging behaves.
/// </summary>
public interface IConfigureWithOptions
{
    /// <summary>
    ///     Gets the custom log category used when <see cref="LogCategoryType" /> is
    ///     <see cref="LogCategoryType.Custom" />.
    /// </summary>
    string CustomLogCategory { get; }

    /// <summary>
    ///     Gets the log category to use when logging messages for configuration binding.
    /// </summary>
    LogCategoryType LogCategoryType { get; }

    /// <summary>
    ///     Gets the log level to use when logging property change notifications when <see cref="ReloadInjectedRawTypes" /> is
    ///     <c>true</c>.
    /// </summary>
    LogLevel LogPropertyChangeLevel { get; }

    /// <summary>
    ///     Gets the log level to use when logging that updates are detected for read only properties when <see cref="ReloadInjectedRawTypes" /> is
    ///     <c>true</c>.
    /// </summary>
    LogLevel LogReadOnlyPropertyLevel { get; }

    /// <summary>
    ///     Gets when log warning messages are written as read only properties are encountered.
    /// </summary>
    LogReadOnlyPropertyType LogReadOnlyPropertyType { get; }

    /// <summary>
    ///     Gets whether injected raw types are updated with reloaded configuration.
    /// </summary>
    /// <remarks>
    ///     When set to <c>true</c>, the <see cref="IServiceCollection" /> will contain registrations of all configuration
    ///     types and their interfaces using a proxy configuration type that will automatically respond
    ///     to configuration types. This will cause the injected raw configuration types in services to have their
    ///     configuration properties updated as underlying configuration data sources change.
    /// </remarks>
    bool ReloadInjectedRawTypes { get; }

    /// <summary>
    ///     Gets the collection of types that should be skipped when attempting register types for configuration sections.
    /// </summary>
    Collection<Type> SkipPropertyTypes { get; }
}