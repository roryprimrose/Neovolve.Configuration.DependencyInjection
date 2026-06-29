namespace Neovolve.Configuration.DependencyInjection;

using Microsoft.Extensions.Logging;
using Neovolve.Configuration.DependencyInjection.Generated;

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

        if (GeneratedConfigRegistry.TryGetApplier<T>(out var applier) == false)
        {
            // The source generator emits an applier for every configuration type reachable from a ConfigureWith<T>
            // root and for any type marked with [GenerateConfigAccessors]. A type without a registered applier has
            // no bindable properties to copy.
            return;
        }

        var targetType = typeof(T);

        // The context carries the logging policy for this single update. The generated applier copies each writable
        // property directly and reports changes, read only properties and copy failures back through the context.
        var context = new ConfigUpdateContext(targetType, logger, _options);

        // The IOptionsMonitor<T>.OnChange event gets triggered twice on a change notification for file based
        // configuration. The applier only reports a change the first time a value actually differs, so logging stays
        // quiet on the duplicate notification while the values are still copied across.
        var changesFound = applier!.Apply(injectedConfig, updatedConfig, context);

        if (changesFound
            && logger != null)
        {
            LogConfigChanged(logger, targetType);
        }
    }
}
