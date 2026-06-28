namespace Neovolve.Configuration.DependencyInjection.Generated
{
    /// <summary>
    ///     The <see cref="GenerateConfigAccessorsAttribute" /> attribute requests that the source generator emit strongly
    ///     typed property accessors for one or more configuration types that are not otherwise reachable from a
    ///     <c>ConfigureWith&lt;T&gt;</c> root.
    /// </summary>
    /// <remarks>
    ///     Apply the attribute to a configuration class to generate accessors for that class and its reachable child types,
    ///     or apply it at the assembly level passing one or more types (including closed generic types) to generate accessors
    ///     for types declared elsewhere.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Assembly, AllowMultiple = true, Inherited = false)]
    public sealed class GenerateConfigAccessorsAttribute : Attribute
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
