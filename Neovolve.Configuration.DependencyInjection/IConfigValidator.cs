namespace Neovolve.Configuration.DependencyInjection
{
    using Microsoft.Extensions.Logging;

    /// <summary>
    ///     The <see cref="IConfigValidator" /> interface defines validation of reloaded configuration values so invalid
    ///     reloads can be rejected before they are applied.
    /// </summary>
    public interface IConfigValidator
    {
        /// <summary>
        ///     Determines whether a reloaded configuration value is valid, logging any validation failures.
        /// </summary>
        /// <typeparam name="T">The configuration type being validated.</typeparam>
        /// <param name="value">The configuration value to validate.</param>
        /// <param name="name">The named options instance, or <c>null</c> for the default instance.</param>
        /// <param name="logger">The logger used to report validation failures.</param>
        /// <returns><c>true</c> if the value is valid; otherwise <c>false</c>.</returns>
        bool IsValid<T>(T value, string? name, ILogger? logger) where T : class;
    }
}
