namespace Neovolve.Configuration.DependencyInjection;

/// <summary>
///     The <see cref="LogReadOnlyPropertyType" />
///     enum defines the circumstances in which warnings are logged when read only properties cannot be updated on
///     configuration reload.
/// </summary>
public enum LogReadOnlyPropertyType
{
    /// <summary>
    ///     Logs a warning for all read only properties.
    /// </summary>
    All,

    /// <summary>
    ///     Logs a warning for read only properties of value types and strings.
    /// </summary>
    /// <remarks>
    ///     This will log warnings for read only properties only if the property type is a value type or is a
    ///     <see cref="string" />.
    /// </remarks>
    ValueTypesOnly,

    /// <summary>
    ///     Disables warning logs for read only properties.
    /// </summary>
    None
}