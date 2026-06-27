namespace Neovolve.Configuration.DependencyInjection.Generated
{
    using System.Collections.Concurrent;
    using System.ComponentModel;

    /// <summary>
    ///     The <see cref="GeneratedConfigRegistry" /> class holds the strongly typed configuration metadata produced by the
    ///     source generator so the library can bind and update configuration types without runtime reflection.
    /// </summary>
    /// <remarks>
    ///     Generated code populates this registry from a module initializer that runs once when the consuming assembly is
    ///     loaded. This type is infrastructure for generated code and is not intended to be used directly from application
    ///     code.
    /// </remarks>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class GeneratedConfigRegistry
    {
        private static readonly ConcurrentDictionary<Type, ConfigPropertyAccessor[]> _properties = new();

        /// <summary>
        ///     Registers the strongly typed property accessors for a configuration type.
        /// </summary>
        /// <param name="configType">The configuration type the accessors describe.</param>
        /// <param name="accessors">The property accessors for the configuration type.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="configType" /> parameter is <c>null</c>.</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="accessors" /> parameter is <c>null</c>.</exception>
        public static void RegisterProperties(Type configType, ConfigPropertyAccessor[] accessors)
        {
            _ = configType ?? throw new ArgumentNullException(nameof(configType));
            _ = accessors ?? throw new ArgumentNullException(nameof(accessors));

            _properties[configType] = accessors;
        }

        /// <summary>
        ///     Attempts to get the strongly typed property accessors registered for a configuration type.
        /// </summary>
        /// <param name="configType">The configuration type to look up.</param>
        /// <param name="accessors">When this method returns, contains the registered accessors if found; otherwise <c>null</c>.</param>
        /// <returns><c>true</c> if accessors were registered for <paramref name="configType" />; otherwise <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="configType" /> parameter is <c>null</c>.</exception>
        public static bool TryGetProperties(Type configType, out ConfigPropertyAccessor[]? accessors)
        {
            _ = configType ?? throw new ArgumentNullException(nameof(configType));

            return _properties.TryGetValue(configType, out accessors);
        }
    }
}
