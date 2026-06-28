namespace Neovolve.Configuration.DependencyInjection.Generator;

/// <summary>
///     The <see cref="PropertyChangeKind" /> enum describes how a writable configuration property's change should be
///     detected and logged by the generated applier.
/// </summary>
internal enum PropertyChangeKind
{
    /// <summary>
    ///     A scalar or leaf value (value type, string, enum or other single value) compared with
    ///     <c>EqualityComparer&lt;T&gt;.Default</c> and logged as a single value change.
    /// </summary>
    Scalar = 0,

    /// <summary>
    ///     A list of scalar values logged as an entry count change or per-element value differences.
    /// </summary>
    ScalarList,

    /// <summary>
    ///     A collection whose elements are complex types, logged only as an entry count change.
    /// </summary>
    Countable,

    /// <summary>
    ///     A child configuration type that is registered and logged independently, so it is assigned but not logged at the
    ///     parent.
    /// </summary>
    ChildConfig
}
