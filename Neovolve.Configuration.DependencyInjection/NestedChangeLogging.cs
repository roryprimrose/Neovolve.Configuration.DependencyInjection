namespace Neovolve.Configuration.DependencyInjection;

/// <summary>
///     The <see cref="NestedChangeLogging" />
///     enum defines how much detail is logged when a configuration property whose type is a class or a collection of
///     classes changes on configuration reload.
/// </summary>
public enum NestedChangeLogging
{
    /// <summary>
    ///     Logs a summary of each change. Class properties are not logged at the parent because they are registered and
    ///     logged independently, and collections of classes are logged only as an entry count change.
    /// </summary>
    /// <remarks>
    ///     This is the default and avoids repeating the type name of unchanged nested values in the log.
    /// </remarks>
    Summary = 0,

    /// <summary>
    ///     Logs the individual nested property changes using a full property path, for example
    ///     <c>FilterRules[0].Port</c>.
    /// </summary>
    /// <remarks>
    ///     This walks class properties and the elements of collections of classes, which costs more on deeper
    ///     configuration graphs and can repeat changes that are also logged independently by a registered child type.
    /// </remarks>
    Deep
}
