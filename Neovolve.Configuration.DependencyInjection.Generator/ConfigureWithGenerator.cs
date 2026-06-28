namespace Neovolve.Configuration.DependencyInjection.Generator;

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

/// <summary>
///     The <see cref="ConfigureWithGenerator" /> class is a Roslyn incremental source generator that emits the strongly
///     typed configuration property accessors for each <c>ConfigureWith&lt;T&gt;</c> root type so the library can update
///     hot-reloaded configuration without runtime reflection.
/// </summary>
[Generator(LanguageNames.CSharp)]
public sealed class ConfigureWithGenerator : IIncrementalGenerator
{
    private const string ConfigureWithMethodName = "ConfigureWith";

    private const string ConfigureWithTypeName = "ConfigureWithExtensions";

    /// <inheritdoc />
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Each ConfigureWith<T> call site is resolved and its configuration type graph is walked into an
        // equatable model in the transform. Because the model is value-equatable, the source output only
        // re-runs when the shape of a configuration type actually changes, not on every keystroke.
        var perInvocation = context.SyntaxProvider
            .CreateSyntaxProvider(
                static (node, _) => IsCandidateInvocation(node),
                static (syntaxContext, token) => Transform(syntaxContext, token))
            .Where(static root => string.IsNullOrEmpty(root.RootTypeFullyQualifiedName) == false);

        var hasModuleInitializer = context.CompilationProvider.Select(
            static (compilation, _) => HasModuleInitializer(compilation));

        var collected = perInvocation.Collect().Combine(hasModuleInitializer);

        context.RegisterSourceOutput(collected, static (productionContext, input) =>
            Execute(productionContext, input.Left, input.Right));
    }

    private static void Execute(
        SourceProductionContext context,
        ImmutableArray<RootModel> roots,
        bool hasModuleInitializer)
    {
        if (roots.IsDefaultOrEmpty)
        {
            return;
        }

        var distinct = new Dictionary<string, RootModel>(StringComparer.Ordinal);

        foreach (var root in roots)
        {
            distinct[root.RootTypeFullyQualifiedName] = root;
        }

        if (distinct.Count == 0)
        {
            return;
        }

        var source = ConfigSourceEmitter.Emit(distinct.Values.ToImmutableArray(), hasModuleInitializer);

        context.AddSource("NeovolveConfigurationBinders.g.cs", source);
    }

    private static bool HasModuleInitializer(Compilation compilation)
    {
        var symbol = compilation.GetTypeByMetadataName("System.Runtime.CompilerServices.ModuleInitializerAttribute");

        return symbol != null && compilation.IsSymbolAccessibleWithin(symbol, compilation.Assembly);
    }

    private static bool IsCandidateInvocation(SyntaxNode node)
    {
        return node is InvocationExpressionSyntax
        {
            Expression: MemberAccessExpressionSyntax
            {
                Name: SimpleNameSyntax { Identifier.ValueText: ConfigureWithMethodName }
            }
        };
    }

    private static RootModel Transform(GeneratorSyntaxContext context, CancellationToken token)
    {
        var invocation = (InvocationExpressionSyntax)context.Node;

        if (context.SemanticModel.GetSymbolInfo(invocation, token).Symbol is not IMethodSymbol method)
        {
            return default;
        }

        if (method.Name != ConfigureWithMethodName
            || method.ContainingType?.Name != ConfigureWithTypeName
            || method.TypeArguments.Length != 1)
        {
            return default;
        }

        if (method.TypeArguments[0] is not INamedTypeSymbol rootType
            || rootType.TypeKind != TypeKind.Class)
        {
            return default;
        }

        return ConfigGraphWalker.WalkRoot(rootType, context.SemanticModel.Compilation, token);
    }
}
