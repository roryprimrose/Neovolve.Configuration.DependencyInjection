namespace Neovolve.Configuration.DependencyInjection.Generator;

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using Microsoft.CodeAnalysis;

/// <summary>
///     The <see cref="ConfigGraphWalker" /> class walks the configuration type graph from a set of root types, mirroring
///     the runtime type discovery so the generator can emit strongly typed accessors for every reachable configuration
///     type.
/// </summary>
internal static class ConfigGraphWalker
{
    private static readonly SymbolDisplayFormat _fullyQualifiedFormat = SymbolDisplayFormat.FullyQualifiedFormat;

    public static ImmutableArray<ConfigTypeModel> Walk(
        IReadOnlyCollection<INamedTypeSymbol> roots,
        Compilation compilation,
        CancellationToken cancellationToken)
    {
        var enumerableType = compilation.GetTypeByMetadataName("System.Collections.IEnumerable");
        var typeType = compilation.GetTypeByMetadataName("System.Type");
        var assemblyType = compilation.GetTypeByMetadataName("System.Reflection.Assembly");
        var streamType = compilation.GetTypeByMetadataName("System.IO.Stream");

        var skipTypes = new List<INamedTypeSymbol?> { enumerableType, typeType, assemblyType, streamType };

        var models = ImmutableArray.CreateBuilder<ConfigTypeModel>();
        var visited = new HashSet<string>(StringComparer.Ordinal);
        var queue = new Queue<INamedTypeSymbol>();

        foreach (var root in roots)
        {
            queue.Enqueue(root);
        }

        while (queue.Count > 0)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var type = queue.Dequeue();
            var typeName = type.ToDisplayString(_fullyQualifiedFormat);

            if (visited.Add(typeName) == false)
            {
                continue;
            }

            var properties = GetBindableProperties(type);
            var propertyModels = ImmutableArray.CreateBuilder<ConfigPropertyModel>();

            foreach (var property in properties)
            {
                var propertyTypeName = property.Type.ToDisplayString(_fullyQualifiedFormat);
                var canWrite = property.SetMethod != null
                    && property.SetMethod.DeclaredAccessibility == Accessibility.Public
                    && property.SetMethod.IsInitOnly == false;
                var isValueType = property.Type.IsValueType || property.Type.SpecialType == SpecialType.System_String;

                propertyModels.Add(new ConfigPropertyModel(property.Name, propertyTypeName, canWrite, isValueType));

                if (ShouldRecurse(property.Type, type, skipTypes))
                {
                    queue.Enqueue((INamedTypeSymbol)property.Type);
                }
            }

            models.Add(new ConfigTypeModel(typeName, new EquatableArray<ConfigPropertyModel>(propertyModels.ToImmutable())));
        }

        return models.ToImmutable();
    }

    private static IEnumerable<IPropertySymbol> GetBindableProperties(INamedTypeSymbol type)
    {
        var seen = new HashSet<string>(StringComparer.Ordinal);
        var current = type;

        while (current != null && current.SpecialType != SpecialType.System_Object)
        {
            foreach (var member in current.GetMembers())
            {
                if (member is not IPropertySymbol property)
                {
                    continue;
                }

                if (property.IsStatic)
                {
                    continue;
                }

                if (property.IsIndexer || property.Parameters.IsEmpty == false)
                {
                    continue;
                }

                if (property.GetMethod == null || property.GetMethod.DeclaredAccessibility != Accessibility.Public)
                {
                    continue;
                }

                // Properties declared on a derived type hide those of the same name on a base type.
                if (seen.Add(property.Name) == false)
                {
                    continue;
                }

                yield return property;
            }

            current = current.BaseType;
        }
    }

    private static bool ShouldRecurse(ITypeSymbol propertyType, INamedTypeSymbol owningType,
        IReadOnlyCollection<INamedTypeSymbol?> skipTypes)
    {
        if (propertyType is not INamedTypeSymbol namedType)
        {
            return false;
        }

        if (namedType.TypeKind != TypeKind.Class)
        {
            // Interfaces and derived types deeper in the graph require attribute hints, which are handled
            // separately. Value types and strings are never recursed into.
            return false;
        }

        if (namedType.SpecialType == SpecialType.System_String)
        {
            return false;
        }

        if (SymbolEqualityComparer.Default.Equals(namedType, owningType))
        {
            return false;
        }

        foreach (var skipType in skipTypes)
        {
            if (skipType != null && IsAssignableTo(namedType, skipType))
            {
                return false;
            }
        }

        return true;
    }

    private static bool IsAssignableTo(INamedTypeSymbol type, INamedTypeSymbol target)
    {
        if (target.TypeKind == TypeKind.Interface)
        {
            if (SymbolEqualityComparer.Default.Equals(type, target))
            {
                return true;
            }

            foreach (var implemented in type.AllInterfaces)
            {
                if (SymbolEqualityComparer.Default.Equals(implemented, target))
                {
                    return true;
                }
            }

            return false;
        }

        INamedTypeSymbol? current = type;

        while (current != null)
        {
            if (SymbolEqualityComparer.Default.Equals(current, target))
            {
                return true;
            }

            current = current.BaseType;
        }

        return false;
    }
}
