namespace Neovolve.Configuration.DependencyInjection.Generator;

using Microsoft.CodeAnalysis;

/// <summary>
///     The <see cref="ConfigureWithGenerator" /> class is a Roslyn incremental source generator that emits the strongly
///     typed configuration registration and update code for each <c>ConfigureWith&lt;T&gt;</c> root type so the library
///     can bind and hot-reload configuration without runtime reflection.
/// </summary>
[Generator(LanguageNames.CSharp)]
public sealed class ConfigureWithGenerator : IIncrementalGenerator
{
    /// <inheritdoc />
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // The generation pipeline is added in subsequent steps:
        //  1. discover ConfigureWith<T> invocations and resolve the root type symbol,
        //  2. walk the config type graph at compile time into an equatable model,
        //  3. emit the registrar, per-type property copy and module initializer.
    }
}
