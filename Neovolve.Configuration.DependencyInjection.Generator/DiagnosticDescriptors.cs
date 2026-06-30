namespace Neovolve.Configuration.DependencyInjection.Generator;

using Microsoft.CodeAnalysis;

/// <summary>
///     The <see cref="DiagnosticDescriptors" /> class declares the diagnostics reported by the configuration source
///     generator.
/// </summary>
internal static class DiagnosticDescriptors
{
    private const string Category = "Neovolve.Configuration";

    private const string HelpLink = "https://github.com/roryprimrose/Neovolve.Configuration.DependencyInjection";

    /// <summary>
    ///     A configuration type in a <c>ConfigureWith&lt;T&gt;</c> graph cannot be hot reloaded because it is a value type
    ///     or has no writable properties.
    /// </summary>
    public static readonly DiagnosticDescriptor NotHotReloadable = new(
        "NCDI001",
        "Configuration type cannot be hot reloaded",
        "Configuration type '{0}' cannot be hot reloaded because it {1}; injected instances will not receive "
        + "configuration updates - {2}{3}",
        Category,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        helpLinkUri: HelpLink);
}
