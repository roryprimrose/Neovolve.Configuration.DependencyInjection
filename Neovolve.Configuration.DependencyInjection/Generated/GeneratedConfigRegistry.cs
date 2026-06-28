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
        private static readonly ConcurrentDictionary<Type, IConfigGraphRegistrar> _registrars = new();

        /// <summary>
        ///     Registers the generated graph registrar for a <c>ConfigureWith&lt;T&gt;</c> root type.
        /// </summary>
        /// <param name="rootType">The root configuration type the registrar registers the graph for.</param>
        /// <param name="registrar">The generated registrar for the root type.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="rootType" /> parameter is <c>null</c>.</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="registrar" /> parameter is <c>null</c>.</exception>
        public static void RegisterGraph(Type rootType, IConfigGraphRegistrar registrar)
        {
            _ = rootType ?? throw new ArgumentNullException(nameof(rootType));
            _ = registrar ?? throw new ArgumentNullException(nameof(registrar));

            _registrars[rootType] = registrar;
        }

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

        /// <summary>
        ///     Attempts to get the generated graph registrar registered for a <c>ConfigureWith&lt;T&gt;</c> root type.
        /// </summary>
        /// <param name="rootType">The root configuration type to look up.</param>
        /// <param name="registrar">When this method returns, contains the registered registrar if found; otherwise <c>null</c>.</param>
        /// <returns><c>true</c> if a registrar was registered for <paramref name="rootType" />; otherwise <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="rootType" /> parameter is <c>null</c>.</exception>
        public static bool TryGetRegistrar(Type rootType, out IConfigGraphRegistrar? registrar)
        {
            _ = rootType ?? throw new ArgumentNullException(nameof(rootType));

            return _registrars.TryGetValue(rootType, out registrar);
        }
    }
}
