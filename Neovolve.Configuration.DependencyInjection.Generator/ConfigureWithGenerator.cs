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

    private const string GenerateAccessorsAttributeName =
        "Neovolve.Configuration.DependencyInjection.Generated.GenerateConfigAccessorsAttribute";

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

        // [GenerateConfigAccessors] requests accessors for types that are not reachable from a ConfigureWith<T>
        // root, including closed generic types named at the assembly level.
        var perAttribute = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                GenerateAccessorsAttributeName,
                static (_, _) => true,
                static (attributeContext, token) => TransformAttribute(attributeContext, token))
            .Where(static types => types.Count > 0);

        var hasModuleInitializer = context.CompilationProvider.Select(
            static (compilation, _) => HasModuleInitializer(compilation));

        var collected = perInvocation.Collect()
            .Combine(perAttribute.Collect())
            .Combine(hasModuleInitializer);

        context.RegisterSourceOutput(collected, static (productionContext, input) =>
            Execute(productionContext, input.Left.Left, input.Left.Right, input.Right));
    }

    private static void Execute(
        SourceProductionContext context,
        ImmutableArray<RootModel> roots,
        ImmutableArray<EquatableArray<ConfigTypeModel>> attributeTypes,
        bool hasModuleInitializer)
    {
        if (roots.IsDefaultOrEmpty && attributeTypes.IsDefaultOrEmpty)
        {
            return;
        }

        var distinctRoots = new Dictionary<string, RootModel>(StringComparer.Ordinal);

        foreach (var root in roots)
        {
            distinctRoots[root.RootTypeFullyQualifiedName] = root;
        }

        var extraTypes = new Dictionary<string, ConfigTypeModel>(StringComparer.Ordinal);

        foreach (var set in attributeTypes)
        {
            foreach (var configType in set)
            {
                extraTypes[configType.FullyQualifiedName] = configType;
            }
        }

        if (distinctRoots.Count == 0 && extraTypes.Count == 0)
        {
            return;
        }

        var source = ConfigSourceEmitter.Emit(
            distinctRoots.Values.ToImmutableArray(),
            extraTypes.Values.ToImmutableArray(),
            hasModuleInitializer);

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

    private static EquatableArray<ConfigTypeModel> TransformAttribute(GeneratorAttributeSyntaxContext context,
        CancellationToken token)
    {
        var compilation = context.SemanticModel.Compilation;
        var collected = new Dictionary<string, ConfigTypeModel>(StringComparer.Ordinal);

        var explicitTypes = new List<INamedTypeSymbol>();

        foreach (var attribute in context.Attributes)
        {
            explicitTypes.AddRange(ExtractTypes(attribute));
        }

        if (explicitTypes.Count == 0 && context.TargetSymbol is INamedTypeSymbol targetType)
        {
            // Applied to a class with no explicit types: generate accessors for that class and its graph.
            explicitTypes.Add(targetType);
        }

        foreach (var type in explicitTypes)
        {
            foreach (var model in ConfigGraphWalker.WalkAccessors(type, compilation, token))
            {
                collected[model.FullyQualifiedName] = model;
            }
        }

        return new EquatableArray<ConfigTypeModel>(collected.Values.ToImmutableArray());
    }

    private static IEnumerable<INamedTypeSymbol> ExtractTypes(AttributeData attribute)
    {
        foreach (var argument in attribute.ConstructorArguments)
        {
            if (argument.Kind == TypedConstantKind.Array)
            {
                foreach (var element in argument.Values)
                {
                    if (element.Value is INamedTypeSymbol named)
                    {
                        yield return named;
                    }
                }
            }
            else if (argument.Value is INamedTypeSymbol single)
            {
                yield return single;
            }
        }
    }
}
