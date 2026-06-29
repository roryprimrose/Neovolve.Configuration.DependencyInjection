// We want the extension methods to be exposed when hosting is available in the calling application
// ReSharper disable once CheckNamespace

namespace Microsoft.Extensions.Hosting;

using Microsoft.Extensions.DependencyInjection;
using Neovolve.Configuration.DependencyInjection;

/// <summary>
///     The <see cref="HostBuilderExtensions" /> class provides methods for configuring dependency injection of
///     strong typed configuration types with support for hot reloading configuration changes.
/// </summary>
public static class HostBuilderExtensions
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

        // The host builder is only needed to reach the service collection and the configuration built by the
        // host. The registration is delegated to the IServiceCollection overload, which is the core of the
        // library.
        return builder.ConfigureServices((context, services) =>
            services.ConfigureWith<T>(context.Configuration, configure));
    }

    /// <summary>
    ///     The <see cref="ConfigureWith{T}(IHostApplicationBuilder)" /> method is used to configure the host application
    ///     builder with
    ///     configuration binding of type
    ///     <typeparamref name="T" />
    ///     and all its child types with hot reloading configuration changes onto injected raw configuration types.
    /// </summary>
    /// <typeparam name="T">The type of configuration to register.</typeparam>
    /// <param name="builder">The host application builder to configure.</param>
    /// <returns>The configured host application builder.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="builder" /> parameter is <c>null</c>.</exception>
    /// <remarks>
    ///     This method adds options of type <typeparamref name="T" /> to the service collection and registers the
    ///     configuration root of type <typeparamref name="T" /> and all child types found.
    ///     The injected raw types defined in the configuration type will support hot reloading of updated configuration.
    /// </remarks>
    public static IHostApplicationBuilder ConfigureWith<T>(this IHostApplicationBuilder builder) where T : class
    {
        _ = builder ?? throw new ArgumentNullException(nameof(builder));

        return ConfigureWith<T>(builder, _ => { });
    }

    /// <summary>
    ///     The <see cref="ConfigureWith{T}(IHostApplicationBuilder, bool)" /> method is used to configure the host
    ///     application builder with
    ///     configuration binding of type
    ///     <typeparamref name="T" />
    ///     and all its child types.
    /// </summary>
    /// <typeparam name="T">The type of configuration to register.</typeparam>
    /// <param name="builder">The host application builder to configure.</param>
    /// <param name="reloadInjectedRawTypes"><c>true</c> if hot reloading raw types is enabled; otherwise <c>false</c>.</param>
    /// <returns>The configured host application builder.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="builder" /> parameter is <c>null</c>.</exception>
    /// <remarks>
    ///     This method adds options of type <typeparamref name="T" /> to the service collection and registers the
    ///     configuration root of type <typeparamref name="T" />
    ///     and all child types found.
    ///     If <paramref name="reloadInjectedRawTypes" /> is <c>true</c>, the injected raw types defined in the configuration
    ///     type
    ///     will support hot reloading of updated configuration.
    /// </remarks>
    public static IHostApplicationBuilder ConfigureWith<T>(this IHostApplicationBuilder builder,
        bool reloadInjectedRawTypes) where T : class
    {
        _ = builder ?? throw new ArgumentNullException(nameof(builder));

        return ConfigureWith<T>(builder, x => { x.ReloadInjectedRawTypes = reloadInjectedRawTypes; });
    }

    /// <summary>
    ///     The <see cref="ConfigureWith{T}(IHostApplicationBuilder, Action&lt;ConfigureWithOptions&gt;)" /> method is used to
    ///     configure
    ///     the host application builder with
    ///     configuration binding of type
    ///     <typeparamref name="T" />
    ///     and all its child types.
    /// </summary>
    /// <typeparam name="T">The type of configuration to register.</typeparam>
    /// <param name="builder">The host application builder to configure.</param>
    /// <param name="configure">The configuration options action.</param>
    /// <returns>The configured host application builder.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="builder" /> parameter is <c>null</c>.</exception>
    public static IHostApplicationBuilder ConfigureWith<T>(this IHostApplicationBuilder builder,
        Action<ConfigureWithOptions> configure) where T : class
    {
        _ = builder ?? throw new ArgumentNullException(nameof(builder));

        // The host application builder exposes the service collection and configuration directly, so the
        // registration is delegated to the IServiceCollection overload, which is the core of the library.
        builder.Services.ConfigureWith<T>(builder.Configuration, configure);

        return builder;
    }
}