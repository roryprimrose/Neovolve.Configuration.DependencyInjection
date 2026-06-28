namespace Neovolve.Configuration.DependencyInjection.Generated
{
    /// <summary>
    ///     The <see cref="GenerateConfigAccessorsAttribute" /> attribute requests that the source generator emit strongly
    ///     typed property accessors for one or more configuration types that are not otherwise reachable from a
    ///     <c>ConfigureWith&lt;T&gt;</c> root.
    /// </summary>
    /// <remarks>
    ///     This is an internal test hook used to exercise applier generation for types that are updated through
    ///     <c>IConfigUpdater</c> directly rather than through <c>ConfigureWith&lt;T&gt;</c>. It is not part of the public
    ///     API.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Assembly, AllowMultiple = true, Inherited = false)]
    internal sealed class GenerateConfigAccessorsAttribute : Attribute
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="GenerateConfigAccessorsAttribute" /> class for the type the
        ///     attribute is applied to.
        /// </summary>
        public GenerateConfigAccessorsAttribute()
        {
            Types = Array.Empty<Type>();
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="GenerateConfigAccessorsAttribute" /> class for the supplied types.
        /// </summary>
        /// <param name="types">The configuration types to generate accessors for.</param>
        public GenerateConfigAccessorsAttribute(params Type[] types)
        {
            Types = types ?? Array.Empty<Type>();
        }

        /// <summary>
        ///     Gets the configuration types to generate accessors for when the attribute is applied at the assembly level.
        /// </summary>
        public Type[] Types { get; }
    }
}
