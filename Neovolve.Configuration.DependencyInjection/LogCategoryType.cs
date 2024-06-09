namespace Neovolve.Configuration.DependencyInjection;

/// <summary>
///     The <see cref="LogCategoryType" />
///     enum defines how the log category is determined when logging information about configuration binding.
/// </summary>
public enum LogCategoryType
{
    /// <summary>
    ///     Log messages are written to a category of the configuration type being bound.
    /// </summary>
    /// <remarks>Using this option means that log messages can be disabled for specific configuration type.</remarks>
    TargetType,

    /// <summary>
    ///     Log messages are written to a category defined by <see cref="ConfigureWithOptions.CustomLogCategory" />.
    /// </summary>
    Custom
}