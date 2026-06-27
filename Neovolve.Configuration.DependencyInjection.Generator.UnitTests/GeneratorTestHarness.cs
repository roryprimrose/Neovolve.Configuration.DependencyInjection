namespace Neovolve.Configuration.DependencyInjection.Generator.UnitTests;

using System.Collections.Immutable;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

/// <summary>
///     The <see cref="GeneratorTestHarness" /> class hosts the <see cref="ConfigureWithGenerator" /> in an in-memory
///     Roslyn compilation so tests can run the generator against source text and assert on the generated output and
///     diagnostics.
/// </summary>
internal sealed class GeneratorTestHarness
{
    private static readonly ImmutableArray<MetadataReference> _references = BuildReferences();

    private GeneratorTestHarness(
        ImmutableArray<Diagnostic> generatorDiagnostics,
        ImmutableArray<string> generatedSources,
        ImmutableArray<Diagnostic> compilationDiagnostics,
        CSharpCompilation outputCompilation)
    {
        GeneratorDiagnostics = generatorDiagnostics;
        GeneratedSources = generatedSources;
        CompilationDiagnostics = compilationDiagnostics;
        OutputCompilation = outputCompilation;
    }

    public static GeneratorTestHarness Run(string source)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.Latest));

        var compilation = CSharpCompilation.Create(
            "GeneratorTestAssembly",
            new[] { syntaxTree },
            _references,
            new CSharpCompilationOptions(
                OutputKind.DynamicallyLinkedLibrary,
                nullableContextOptions: NullableContextOptions.Enable));

        var driver = CSharpGeneratorDriver.Create(new ConfigureWithGenerator());

        driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var generatorDiagnostics);

        var generatedSources = outputCompilation.SyntaxTrees
            .Where(tree => ReferenceEquals(tree, syntaxTree) == false)
            .Select(tree => tree.ToString())
            .ToImmutableArray();

        return new GeneratorTestHarness(
            generatorDiagnostics,
            generatedSources,
            outputCompilation.GetDiagnostics(),
            (CSharpCompilation)outputCompilation);
    }

    private static ImmutableArray<MetadataReference> BuildReferences()
    {
        var references = AppDomain.CurrentDomain.GetAssemblies()
            .Where(assembly => assembly.IsDynamic == false && string.IsNullOrEmpty(assembly.Location) == false)
            .Select(assembly => (MetadataReference)MetadataReference.CreateFromFile(assembly.Location))
            .ToList();

        return references.ToImmutableArray();
    }

    public ImmutableArray<Diagnostic> CompilationDiagnostics { get; }

    public ImmutableArray<Diagnostic> CompilationErrors =>
        CompilationDiagnostics.Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error).ToImmutableArray();

    public ImmutableArray<string> GeneratedSources { get; }

    public ImmutableArray<Diagnostic> GeneratorDiagnostics { get; }

    public CSharpCompilation OutputCompilation { get; }
}
