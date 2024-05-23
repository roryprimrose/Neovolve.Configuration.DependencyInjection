// We want the extension methods to be exposed when hosting is available in the calling application
// ReSharper disable once CheckNamespace

namespace Microsoft.Extensions.Hosting;

using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Neovolve.Configuration.DependencyInjection;

/// <summary>
///     The <see cref="ConfigureWithExtensions" /> class provides methods for configuring dependency injection of
///     strong typed configuration types with support for hot reloading configuration changes.
/// </summary>
public static class ConfigureWithExtensions
{
    /// <summary>
    ///     The <see cref="ConfigureWith{T}(IHostBuilder)" /> method is used to configure the host builder with
    ///     configuration binding of type
    ///     <typeparamref name="T" />
    ///     and all its child types with hot reloading configuration changes onto injected raw configuration types.
    /// </summary>
    /// <typeparam name="T">The type of configuration to register.</typeparam>
    /// <param name="builder">The host builder to configure.</param>
    /// <returns>The configured host builder.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="builder" /> parameter is <c>null</c>.</exception>
    /// <remarks>
    ///     This method adds options of type <typeparamref name="T" /> to the service collection and registers the
    ///     configuration root of type <typeparamref name="T" /> and all child types found.
    ///     The injected raw types defined in the configuration type will support hot reloading of updated configuration.
    /// </remarks>
    public static IHostBuilder ConfigureWith<T>(this IHostBuilder builder) where T : class
    {
        _ = builder ?? throw new ArgumentNullException(nameof(builder));

        return ConfigureWith<T>(builder, _ => { });
    }

    /// <summary>
    ///     The <see cref="ConfigureWith{T}(IHostBuilder, bool)" /> method is used to configure the host builder with
    ///     configuration binding of type
    ///     <typeparamref name="T" />
    ///     and all its child types.
    /// </summary>
    /// <typeparam name="T">The type of configuration to register.</typeparam>
    /// <param name="builder">The host builder to configure.</param>
    /// <param name="reloadInjectedRawTypes"><c>true</c> if hot reloading raw types is enabled; otherwise <c>false</c>.</param>
    /// <returns>The configured host builder.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="builder" /> parameter is <c>null</c>.</exception>
    /// <remarks>
    ///     This method adds options of type <typeparamref name="T" /> to the service collection and registers the
    ///     configuration root of type <typeparamref name="T" />
    ///     and all child types found.
    ///     If <paramref name="reloadInjectedRawTypes" /> is <c>true</c>, the injected raw types defined in the configuration
    ///     type
    ///     will support hot reloading of updated configuration.
    /// </remarks>
    public static IHostBuilder ConfigureWith<T>(this IHostBuilder builder, bool reloadInjectedRawTypes) where T : class
    {
        _ = builder ?? throw new ArgumentNullException(nameof(builder));

        return ConfigureWith<T>(builder, x => { x.ReloadInjectedRawTypes = reloadInjectedRawTypes; });
    }

    /// <summary>
    ///     The <see cref="ConfigureWith{T}(IHostBuilder, Action&lt;ConfigureWithOptions&gt;)" /> method is used to configure
    ///     the host builder with
    ///     configuration binding of type
    ///     <typeparamref name="T" />
    ///     and all its child types.
    /// </summary>
    /// <typeparam name="T">The type of configuration to register.</typeparam>
    /// <param name="builder">The host builder to configure.</param>
    /// <param name="configure">The configuration options action.</param>
    /// <returns>The configured host builder.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="builder" /> parameter is <c>null</c>.</exception>
    public static IHostBuilder ConfigureWith<T>(this IHostBuilder builder, Action<ConfigureWithOptions> configure)
        where T : class
    {
        _ = builder ?? throw new ArgumentNullException(nameof(builder));

        // Get the initial options which we need to do the recursion through configuration types
        // NOTE: This configuration could be different to the singleton registered below which is used at the point of processing configuration updates
        // The change in the singleton registration is with respect to LogReadOnlyPropertyLevel which is not used for the property recursion so this should be safe
        var initialOptions = new ConfigureWithOptions();
        
        configure(initialOptions);

        return builder.ConfigureServices((_, services) =>
            {
                // Add the configuration registration
                services.AddSingleton(c =>
                {
                    var config = new ConfigureWithOptions();

                    var hostEnvironment = c.GetService<IHostEnvironment>();

                    if (hostEnvironment != null 
                        && hostEnvironment.IsDevelopment())
                    {
                        config.LogReadOnlyPropertyLevel = LogLevel.Warning;
                    }
                    else
                    {
                        config.LogReadOnlyPropertyLevel = LogLevel.Debug;
                    }

                    configure(config);

                    return config;
                });

                // Add a redirect from the configuration type to its interface
                services.AddSingleton<IConfigureWithOptions>(provider =>
                    provider.GetRequiredService<ConfigureWithOptions>());

                // Add the default configuration updater if one is not already registered
                services.TryAddTransient<IConfigUpdater, DefaultConfigUpdater>();
            })
            .RegisterConfigurationRoot<T>(initialOptions);
    }
}