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

    /// <summary>
    ///     Emits the generated compilation to an in-memory assembly, loads it and runs its module initializers so the
    ///     generated registrations are applied.
    /// </summary>
    public Assembly EmitAndLoad()
    {
        using var stream = new MemoryStream();

        var result = OutputCompilation.Emit(stream);

        if (result.Success == false)
        {
            var errors = string.Join(
                Environment.NewLine,
                result.Diagnostics.Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error));

            throw new InvalidOperationException("Generated code failed to compile:" + Environment.NewLine + errors);
        }

        var assembly = Assembly.Load(stream.ToArray());

        // Run the [ModuleInitializer] so the generated registrations are applied before the test inspects them.
        System.Runtime.CompilerServices.RuntimeHelpers.RunModuleConstructor(assembly.ManifestModule.ModuleHandle);

        return assembly;
    }

    private static ImmutableArray<MetadataReference> BuildReferences()
    {
        var seenPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var references = new List<MetadataReference>();

        // The trusted platform assemblies list contains the full framework reference set plus the
        // application's own dependencies (the library, hosting and DI abstractions), so generated code
        // that references them compiles inside the test compilation.
        if (AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES") is string trustedAssemblies)
        {
            foreach (var path in trustedAssemblies.Split(Path.PathSeparator))
            {
                AddReference(references, seenPaths, path);
            }
        }

        // Force the assemblies the generated code and test sources depend on to be referenced even if they
        // were not already part of the trusted platform set.
        AddReference(references, seenPaths, typeof(object).Assembly.Location);
        AddReference(references, seenPaths, typeof(Microsoft.Extensions.Hosting.IHostBuilder).Assembly.Location);
        AddReference(references, seenPaths,
            typeof(Microsoft.Extensions.DependencyInjection.IServiceCollection).Assembly.Location);
        AddReference(references, seenPaths,
            typeof(Configuration.DependencyInjection.Generated.GeneratedConfigRegistry).Assembly.Location);

        return references.ToImmutableArray();
    }

    private static void AddReference(List<MetadataReference> references, HashSet<string> seenPaths, string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            return;
        }

        if (File.Exists(path) == false)
        {
            return;
        }

        if (seenPaths.Add(path) == false)
        {
            return;
        }

        references.Add(MetadataReference.CreateFromFile(path));
    }

    public ImmutableArray<Diagnostic> CompilationDiagnostics { get; }

    public ImmutableArray<Diagnostic> CompilationErrors =>
        CompilationDiagnostics.Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error).ToImmutableArray();

    public ImmutableArray<string> GeneratedSources { get; }

    public ImmutableArray<Diagnostic> GeneratorDiagnostics { get; }

    public CSharpCompilation OutputCompilation { get; }
}
