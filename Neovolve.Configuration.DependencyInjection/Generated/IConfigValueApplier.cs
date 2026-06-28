namespace Neovolve.Configuration.DependencyInjection.Generated
{
    using System.ComponentModel;

    /// <summary>
    ///     The <see cref="IConfigValueApplier{T}" /> interface defines a strongly typed applier that copies updated
    ///     configuration values onto an injected configuration instance when configuration changes.
    /// </summary>
    /// <typeparam name="T">The configuration type the applier updates.</typeparam>
    /// <remarks>
    ///     Implementations are produced by the source generator for each configuration type reachable from a
    ///     <c>ConfigureWith&lt;T&gt;</c> root and for any type marked with <c>[GenerateConfigAccessors]</c>. The generated
    ///     code assigns each writable property directly, avoiding the boxing and reflection of a delegate based property
    ///     table. This type is infrastructure for generated code and is not intended to be used directly from application
    ///     code.
    /// </remarks>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public interface IConfigValueApplier<in T>
    {
        /// <summary>
        ///     Applies the values from <paramref name="updated" /> onto <paramref name="injected" />.
        /// </summary>
        /// <param name="injected">The configuration instance that has been injected into other services.</param>
        /// <param name="updated">The configuration instance holding the updated values.</param>
        /// <param name="context">The context used to report changes, read only properties and copy failures.</param>
        /// <returns><c>true</c> if any value change was logged; otherwise <c>false</c>.</returns>
        bool Apply(T injected, T updated, IConfigUpdateContext context);
    }
}
