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

    /// <summary>
    ///     A configuration property is a read only mutable collection. It binds at startup but cannot be hot reloaded in
    ///     place safely, so the author is advised to expose a settable property on the class and a read only property on
    ///     any interface.
    /// </summary>
    public static readonly DiagnosticDescriptor ReadOnlyCollectionNotHotReloadable = new(
        "NCDI002",
        "Read-only collection configuration property cannot be hot reloaded",
        "Configuration property '{0}' is a read-only collection; the injected instance binds it at startup but will not "
        + "receive hot reload updates for it. Declare the property with a public getter and setter on the class so the "
        + "reloaded collection is assigned atomically, and keep a get-only declaration on any interface so callers "
        + "cannot replace it.",
        Category,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        helpLinkUri: HelpLink);
}
