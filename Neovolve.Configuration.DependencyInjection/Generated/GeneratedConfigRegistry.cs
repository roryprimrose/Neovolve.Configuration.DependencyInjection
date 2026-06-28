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
        private static readonly ConcurrentDictionary<Type, object> _appliers = new();
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
        ///     Registers the strongly typed value applier for a configuration type.
        /// </summary>
        /// <param name="configType">The configuration type the applier updates.</param>
        /// <param name="applier">The generated <see cref="IConfigValueApplier{T}" /> for the configuration type.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="configType" /> parameter is <c>null</c>.</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="applier" /> parameter is <c>null</c>.</exception>
        public static void RegisterApplier(Type configType, object applier)
        {
            _ = configType ?? throw new ArgumentNullException(nameof(configType));
            _ = applier ?? throw new ArgumentNullException(nameof(applier));

            _appliers[configType] = applier;
        }

        /// <summary>
        ///     Attempts to get the strongly typed value applier registered for a configuration type.
        /// </summary>
        /// <typeparam name="T">The configuration type to look up.</typeparam>
        /// <param name="applier">When this method returns, contains the registered applier if found; otherwise <c>null</c>.</param>
        /// <returns><c>true</c> if an applier was registered for <typeparamref name="T" />; otherwise <c>false</c>.</returns>
        public static bool TryGetApplier<T>(out IConfigValueApplier<T>? applier)
        {
            if (_appliers.TryGetValue(typeof(T), out var registered))
            {
                applier = (IConfigValueApplier<T>)registered;

                return true;
            }

            applier = null;

            return false;
        }

        /// <summary>
        ///     Attempts to get the value applier registered for a configuration type.
        /// </summary>
        /// <param name="configType">The configuration type to look up.</param>
        /// <param name="applier">When this method returns, contains the registered applier if found; otherwise <c>null</c>.</param>
        /// <returns><c>true</c> if an applier was registered for <paramref name="configType" />; otherwise <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="configType" /> parameter is <c>null</c>.</exception>
        public static bool TryGetApplier(Type configType, out object? applier)
        {
            _ = configType ?? throw new ArgumentNullException(nameof(configType));

            return _appliers.TryGetValue(configType, out applier);
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
