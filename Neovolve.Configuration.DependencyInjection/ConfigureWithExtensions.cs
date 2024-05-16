// We want the extension methods to be exposed when hosting is available in the calling application
// ReSharper disable once CheckNamespace

namespace Microsoft.Extensions.Hosting;

using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Neovolve.Configuration.DependencyInjection;
using Neovolve.Configuration.DependencyInjection.Comparison;

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

        return builder.ConfigureServices((_, services) =>
            {
                // Add the options registration
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

                // Add a redirect from the options type to its interface
                services.AddSingleton<IConfigureWithOptions>(provider =>
                    provider.GetRequiredService<ConfigureWithOptions>());

                // Register the value evaluators
                services.TryAddEnumerable(new ServiceDescriptor(typeof(IChangeEvaluator), typeof(NullChangeEvaluator),
                    ServiceLifetime.Singleton));
                services.TryAddEnumerable(new ServiceDescriptor(typeof(IChangeEvaluator),
                    typeof(ReferenceChangeEvaluator), ServiceLifetime.Singleton));
                services.TryAddEnumerable(new ServiceDescriptor(typeof(IChangeEvaluator),
                    typeof(DictionaryChangeEvaluator), ServiceLifetime.Singleton));
                services.TryAddEnumerable(new ServiceDescriptor(typeof(IChangeEvaluator),
                    typeof(CollectionChangeEvaluator), ServiceLifetime.Singleton));
                services.TryAddEnumerable(new ServiceDescriptor(typeof(IChangeEvaluator), typeof(EqualsChangeEvaluator),
                    ServiceLifetime.Singleton));

                // Register the evaluator processor that uses all the evaluators
                services.AddSingleton<IValueProcessor, ValueProcessor>();

                // Add the default configuration updater if one is not already registered
                services.TryAddTransient<IConfigUpdater, DefaultConfigUpdater>();
            })
            .RegisterConfigurationRoot<T>();
    }
}