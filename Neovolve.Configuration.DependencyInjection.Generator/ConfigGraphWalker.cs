namespace Neovolve.Configuration.DependencyInjection.Generator;

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using Microsoft.CodeAnalysis;

/// <summary>
///     The <see cref="ConfigGraphWalker" /> class walks the configuration type graph from a <c>ConfigureWith&lt;T&gt;</c>
///     root, mirroring the runtime type discovery so the generator can emit strongly typed accessors and registration code
///     for every reachable configuration type.
/// </summary>
internal static class ConfigGraphWalker
{
    private static readonly SymbolDisplayFormat _fullyQualifiedFormat = SymbolDisplayFormat.FullyQualifiedFormat;

    public static RootModel WalkRoot(INamedTypeSymbol root, Compilation compilation, CancellationToken cancellationToken)
    {
        var enumerableType = compilation.GetTypeByMetadataName("System.Collections.IEnumerable");
        var typeType = compilation.GetTypeByMetadataName("System.Type");
        var assemblyType = compilation.GetTypeByMetadataName("System.Reflection.Assembly");
        var streamType = compilation.GetTypeByMetadataName("System.IO.Stream");

        var skipTypes = new List<INamedTypeSymbol?> { enumerableType, typeType, assemblyType, streamType };

        var rootName = root.ToDisplayString(_fullyQualifiedFormat);
        var configTypes = new Dictionary<string, ConfigTypeModel>(StringComparer.Ordinal);
        var registrations = ImmutableArray.CreateBuilder<ChildRegistrationModel>();

        configTypes[rootName] = BuildConfigType(root);

        var rootInterfaces = GetAccessibleInterfaces(root, compilation);

        var ancestors = new HashSet<string>(StringComparer.Ordinal) { rootName };

        WalkChildren(root, string.Empty, compilation, skipTypes, configTypes, registrations, ancestors,
            cancellationToken);

        return new RootModel(
            rootName,
            new EquatableArray<string>(rootInterfaces),
            new EquatableArray<ConfigTypeModel>(configTypes.Values.ToImmutableArray()),
            new EquatableArray<ChildRegistrationModel>(registrations.ToImmutable()));
    }

    private static ConfigTypeModel BuildConfigType(INamedTypeSymbol type)
    {
        var typeName = type.ToDisplayString(_fullyQualifiedFormat);
        var propertyModels = ImmutableArray.CreateBuilder<ConfigPropertyModel>();

        foreach (var property in GetBindableProperties(type))
        {
            var propertyTypeName = property.Type.ToDisplayString(_fullyQualifiedFormat);
            var canWrite = property.SetMethod != null
                && property.SetMethod.DeclaredAccessibility == Accessibility.Public
                && property.SetMethod.IsInitOnly == false;
            var isValueType = property.Type.IsValueType || property.Type.SpecialType == SpecialType.System_String;

            propertyModels.Add(new ConfigPropertyModel(property.Name, propertyTypeName, canWrite, isValueType));
        }

        return new ConfigTypeModel(typeName, new EquatableArray<ConfigPropertyModel>(propertyModels.ToImmutable()));
    }

    private static ImmutableArray<string> GetAccessibleInterfaces(INamedTypeSymbol type, Compilation compilation)
    {
        var interfaces = ImmutableArray.CreateBuilder<string>();

        foreach (var implemented in type.AllInterfaces)
        {
            if (compilation.IsSymbolAccessibleWithin(implemented, compilation.Assembly) == false)
            {
                continue;
            }

            interfaces.Add(implemented.ToDisplayString(_fullyQualifiedFormat));
        }

        return interfaces.ToImmutable();
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

    private static void WalkChildren(
        INamedTypeSymbol owningType,
        string sectionPrefix,
        Compilation compilation,
        IReadOnlyCollection<INamedTypeSymbol?> skipTypes,
        Dictionary<string, ConfigTypeModel> configTypes,
        ImmutableArray<ChildRegistrationModel>.Builder registrations,
        HashSet<string> ancestors,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var prefix = sectionPrefix.Length == 0 ? string.Empty : sectionPrefix + ":";

        foreach (var property in GetBindableProperties(owningType))
        {
            if (ShouldRecurse(property.Type, owningType, skipTypes) == false)
            {
                continue;
            }

            var childType = (INamedTypeSymbol)property.Type;
            var childName = childType.ToDisplayString(_fullyQualifiedFormat);
            var sectionPath = prefix + property.Name;
            var interfaces = GetAccessibleInterfaces(childType, compilation);

            registrations.Add(new ChildRegistrationModel(childName, sectionPath,
                new EquatableArray<string>(interfaces)));

            if (configTypes.ContainsKey(childName) == false)
            {
                configTypes[childName] = BuildConfigType(childType);
            }

            // Add the child to the current path to guard against cycles while still allowing the same type to
            // be registered at sibling paths.
            if (ancestors.Add(childName))
            {
                WalkChildren(childType, sectionPath, compilation, skipTypes, configTypes, registrations, ancestors,
                    cancellationToken);

                ancestors.Remove(childName);
            }
        }
    }
}
