namespace Neovolve.Configuration.DependencyInjection.Generated
{
    using System;

    /// <summary>
    ///     The <see cref="SkipConfigTypeAttribute" /> attribute marks a type so the source generator excludes every
    ///     property of that type from the configuration graph.
    /// </summary>
    /// <remarks>
    ///     Apply the attribute to a configuration type to exclude it wherever it appears as a property, or apply it at the
    ///     assembly level passing one or more types to exclude types declared elsewhere. A skipped type is not registered as
    ///     a child configuration type and properties of that type are not copied or logged when configuration is hot
    ///     reloaded.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Assembly, AllowMultiple = true,
        Inherited = false)]
    public sealed class SkipConfigTypeAttribute : Attribute
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="SkipConfigTypeAttribute" /> class for the type the attribute is
        ///     applied to.
        /// </summary>
        public SkipConfigTypeAttribute()
        {
            Types = Array.Empty<Type>();
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="SkipConfigTypeAttribute" /> class for the supplied types.
        /// </summary>
        /// <param name="types">The configuration types to exclude from the configuration graph.</param>
        public SkipConfigTypeAttribute(params Type[] types)
        {
            Types = types ?? Array.Empty<Type>();
        }

        /// <summary>
        ///     Gets the types to exclude from the configuration graph when the attribute is applied at the assembly level.
        /// </summary>
        public Type[] Types { get; }
    }
}
