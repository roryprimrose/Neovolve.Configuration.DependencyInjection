namespace Neovolve.Configuration.DependencyInjection.Generated
{
    using System;

    /// <summary>
    ///     The <see cref="SkipConfigPropertyAttribute" /> attribute marks a property so the source generator excludes it
    ///     from the configuration graph.
    /// </summary>
    /// <remarks>
    ///     A skipped property is not registered as a child configuration type and is not copied or logged when
    ///     configuration is hot reloaded. Apply it to a property the configuration binder populates but that should not
    ///     participate in hot reload, for example a property whose value is expensive to compare or should never change at
    ///     runtime.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public sealed class SkipConfigPropertyAttribute : Attribute
    {
    }
}
