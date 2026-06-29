namespace Neovolve.Configuration.DependencyInjection.Generated
{
    using System.ComponentModel;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    ///     The <see cref="IConfigGraphRegistrar" /> interface is implemented by generated code to register the configuration
    ///     type graph for a <c>ConfigureWith&lt;T&gt;</c> root without runtime reflection.
    /// </summary>
    /// <remarks>
    ///     This interface is infrastructure for generated code and is not intended to be implemented by application code.
    /// </remarks>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public interface IConfigGraphRegistrar
    {
        /// <summary>
        ///     Registers the root configuration type and all reachable child configuration types and their interfaces.
        /// </summary>
        /// <param name="services">The service collection to register configuration services into.</param>
        /// <param name="configuration">The configuration used to bind the registered types.</param>
        void Register(IServiceCollection services, IConfiguration configuration);
    }
}
