namespace Neovolve.Configuration.DependencyInjection
{
    using Microsoft.Extensions.Logging;

    /// <summary>
    ///     The <see cref="IConfigUpdater" />
    ///     interface defines the members for updating injected configuration objects when the underlying configuration system
    ///     detects changes to the configuration data.
    /// </summary>
    public interface IConfigUpdater
    {
        /// <summary>
        ///     Updates the configuration on <paramref name="injectedConfig" /> with the values from
        ///     <paramref name="updatedConfig" />.
        /// </summary>
        /// <typeparam name="T">The type of configuration being updated.</typeparam>
        /// <param name="injectedConfig">The configuration object that has been injected into other services.</param>
        /// <param name="updatedConfig">The configuration object that holds the updated data.</param>
        /// <param name="name">The name of the TOptions object.</param>
        /// <param name="logger">The logger to use for logging changes.</param>
        void UpdateConfig<T>(T? injectedConfig, T updatedConfig, string? name, ILogger? logger);
    }
}