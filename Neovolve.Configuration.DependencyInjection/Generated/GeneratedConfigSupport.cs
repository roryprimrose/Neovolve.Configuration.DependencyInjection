// The registration helpers are invoked from generated code, so this facade lives in the Generated namespace
// and wraps the internal registration extensions to keep the real registration logic internal.

namespace Neovolve.Configuration.DependencyInjection.Generated
{
    using System.ComponentModel;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Options;

    /// <summary>
    ///     The <see cref="GeneratedConfigSupport" /> class exposes the configuration registration primitives that generated
    ///     graph registrars call to register configuration types without runtime reflection.
    /// </summary>
    /// <remarks>
    ///     This type is infrastructure for generated code and is not intended to be used directly from application code.
    /// </remarks>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class GeneratedConfigSupport
    {
        /// <summary>
        ///     Registers the root configuration value and removes the options service variants that do not support hot reload.
        /// </summary>
        /// <typeparam name="TRoot">The root configuration type.</typeparam>
        /// <param name="services">The service collection to register into.</param>
        /// <param name="configuration">The configuration used to bind the root value.</param>
        /// <returns>The bound root configuration value.</returns>
        public static TRoot RegisterRoot<TRoot>(IServiceCollection services, IConfiguration configuration)
            where TRoot : class
        {
            var value = configuration.Get<TRoot>()!;

            services.AddSingleton(value);

            // The default configuration support registers IOptions<T> and related interfaces that just return a new
            // instance of T. Null those registrations out for the root so resolving them behaves as if unregistered.
            services.AddSingleton<IOptions<TRoot>>(_ => null!);
            services.AddScoped<IOptionsSnapshot<TRoot>>(_ => null!);
            services.AddSingleton<IOptionsMonitor<TRoot>>(_ => null!);

            return value;
        }

        /// <summary>
        ///     Registers a root configuration interface and removes the options service variants that do not support hot
        ///     reload.
        /// </summary>
        /// <typeparam name="TRoot">The root configuration type.</typeparam>
        /// <typeparam name="TInterface">The interface implemented by the root configuration type.</typeparam>
        /// <param name="services">The service collection to register into.</param>
        /// <param name="value">The bound root configuration value.</param>
        public static void RegisterRootInterface<TRoot, TInterface>(IServiceCollection services, TRoot value)
            where TRoot : class, TInterface
            where TInterface : class
        {
            services.AddSingleton<TInterface>(value);

            services.AddSingleton<IOptions<TInterface>>(_ => null!);
            services.AddScoped<IOptionsSnapshot<TInterface>>(_ => null!);
            services.AddSingleton<IOptionsMonitor<TInterface>>(_ => null!);
        }

        /// <summary>
        ///     Registers a child configuration type and its options services for the supplied configuration section.
        /// </summary>
        /// <typeparam name="T">The child configuration type.</typeparam>
        /// <param name="services">The service collection to register into.</param>
        /// <param name="section">The configuration section bound to the type.</param>
        public static void RegisterConfigType<T>(IServiceCollection services, IConfigurationSection section)
            where T : class
        {
            services.RegisterConfigType<T>(section);
        }

        /// <summary>
        ///     Registers the redirection from a child configuration type to one of its interfaces.
        /// </summary>
        /// <typeparam name="TConcrete">The concrete child configuration type.</typeparam>
        /// <typeparam name="TInterface">The interface implemented by the concrete type.</typeparam>
        /// <param name="services">The service collection to register into.</param>
        public static void RegisterConfigInterfaceType<TConcrete, TInterface>(IServiceCollection services)
            where TConcrete : class, TInterface
            where TInterface : class
        {
            services.RegisterConfigInterfaceType<TConcrete, TInterface>();
        }
    }
}
