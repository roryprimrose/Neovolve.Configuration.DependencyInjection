// IServiceCollection extension methods are exposed in the dependency injection namespace by convention so
// they surface on a service collection without importing the hosting namespace.
// ReSharper disable once CheckNamespace

namespace Microsoft.Extensions.DependencyInjection;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Neovolve.Configuration.DependencyInjection;
using Neovolve.Configuration.DependencyInjection.Generated;

/// <summary>
///     The <see cref="ServiceCollectionExtensions" /> class provides <see cref="IServiceCollection" />
///     methods for configuring dependency injection of strong typed configuration types with support for hot reloading
///     configuration changes.
/// </summary>
/// <remarks>
///     These overloads are an alternative to the <c>IHostBuilder</c> extension methods for scenarios that work directly
///     with an <see cref="IServiceCollection" /> and an <see cref="IConfiguration" /> (for example minimal hosting).
/// </remarks>
public static class ServiceCollectionExtensions
{
    /// <summary>
    ///     The <see cref="ConfigureWith{T}(IServiceCollection, IConfiguration)" /> method registers configuration binding of
    ///     type <typeparamref name="T" /> and all its child types with hot reloading configuration changes onto injected raw
    ///     configuration types.
    /// </summary>
    /// <typeparam name="T">The type of configuration to register.</typeparam>
    /// <param name="services">The service collection to configure.</param>
    /// <param name="configuration">The configuration used to bind the registered types.</param>
    /// <returns>The configured service collection.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="services" /> parameter is <c>null</c>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="configuration" /> parameter is <c>null</c>.</exception>
    public static IServiceCollection ConfigureWith<T>(this IServiceCollection services, IConfiguration configuration)
        where T : class
    {
        return ConfigureWith<T>(services, configuration, _ => { });
    }

    /// <summary>
    ///     The <see cref="ConfigureWith{T}(IServiceCollection, IConfiguration, bool)" /> method registers configuration
    ///     binding of type <typeparamref name="T" /> and all its child types.
    /// </summary>
    /// <typeparam name="T">The type of configuration to register.</typeparam>
    /// <param name="services">The service collection to configure.</param>
    /// <param name="configuration">The configuration used to bind the registered types.</param>
    /// <param name="reloadInjectedRawTypes"><c>true</c> if hot reloading raw types is enabled; otherwise <c>false</c>.</param>
    /// <returns>The configured service collection.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="services" /> parameter is <c>null</c>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="configuration" /> parameter is <c>null</c>.</exception>
    public static IServiceCollection ConfigureWith<T>(this IServiceCollection services, IConfiguration configuration,
        bool reloadInjectedRawTypes) where T : class
    {
        return ConfigureWith<T>(services, configuration, x => { x.ReloadInjectedRawTypes = reloadInjectedRawTypes; });
    }

    /// <summary>
    ///     The <see cref="ConfigureWith{T}(IServiceCollection, IConfiguration, Action&lt;ConfigureWithOptions&gt;)" /> method
    ///     registers configuration binding of type <typeparamref name="T" /> and all its child types.
    /// </summary>
    /// <typeparam name="T">The type of configuration to register.</typeparam>
    /// <param name="services">The service collection to configure.</param>
    /// <param name="configuration">The configuration used to bind the registered types.</param>
    /// <param name="configure">The configuration options action.</param>
    /// <returns>The configured service collection.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="services" /> parameter is <c>null</c>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="configuration" /> parameter is <c>null</c>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="configure" /> parameter is <c>null</c>.</exception>
    public static IServiceCollection ConfigureWith<T>(this IServiceCollection services, IConfiguration configuration,
        Action<ConfigureWithOptions> configure) where T : class
    {
        _ = services ?? throw new ArgumentNullException(nameof(services));
        _ = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _ = configure ?? throw new ArgumentNullException(nameof(configure));

        // Add the options registration
        services.AddSingleton(c =>
        {
            var config = new ConfigureWithOptions();

            configure(config);

            return config;
        });

        // Add a redirect from the options type to its interface
        services.AddSingleton<IConfigureWithOptions>(provider =>
            provider.GetRequiredService<ConfigureWithOptions>());

        // Add the default configuration updater if one is not already registered
        services.TryAddTransient<IConfigUpdater, DefaultConfigUpdater>();

        // Add the default configuration validator if one is not already registered
        services.TryAddSingleton<IConfigValidator, DefaultConfigValidator>();

        if (GeneratedConfigRegistry.TryGetRegistrar(typeof(T), out var registrar) == false)
        {
            throw new InvalidOperationException(
                $"No generated configuration registrar was found for '{typeof(T)}'. Ensure the "
                + "Neovolve.Configuration.DependencyInjection source generator runs in the project that calls "
                + "ConfigureWith.");
        }

        // The source generator emits a strongly typed registrar for this root type, so the configuration graph
        // is registered without runtime reflection.
        registrar!.Register(services, configuration);

        return services;
    }
}
