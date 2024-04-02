namespace Neovolve.Configuration.DependencyInjection;

using Microsoft.Extensions.DependencyInjection;

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
    ///     Gets when log warning messages are written as read only properties are encountered.
    /// </summary>
    ReadOnlyPropertyWarning LogReadOnlyPropertyWarning { get; }

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
}