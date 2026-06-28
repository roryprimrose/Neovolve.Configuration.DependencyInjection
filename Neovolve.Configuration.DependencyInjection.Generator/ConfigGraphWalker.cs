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

    public static RootModel WalkRoot(INamedTypeSymbol root, Compilation compilation, LocationInfo? invocationLocation,
        CancellationToken cancellationToken)
    {
        var enumerableType = compilation.GetTypeByMetadataName("System.Collections.IEnumerable");
        var typeType = compilation.GetTypeByMetadataName("System.Type");
        var assemblyType = compilation.GetTypeByMetadataName("System.Reflection.Assembly");
        var streamType = compilation.GetTypeByMetadataName("System.IO.Stream");

        var skipTypes = new List<INamedTypeSymbol?> { enumerableType, typeType, assemblyType, streamType };

        var rootName = root.ToDisplayString(_fullyQualifiedFormat);
        var configTypes = new Dictionary<string, ConfigTypeModel>(StringComparer.Ordinal);
        var registrations = ImmutableArray.CreateBuilder<ChildRegistrationModel>();
        var deepTypes = new List<INamedTypeSymbol>();

        configTypes[rootName] = BuildConfigType(root, compilation, skipTypes, false, deepTypes);

        var rootInterfaces = GetAccessibleInterfaces(root, compilation);

        var ancestors = new HashSet<string>(StringComparer.Ordinal) { rootName };

        WalkChildren(root, string.Empty, compilation, skipTypes, configTypes, registrations, ancestors, deepTypes,
            cancellationToken);

        // Report-only types are the complex element types of indexable lists (and their reachable graph) that are not
        // registered configuration types. They get a Report function for deep nested logging but no applier.
        WalkReportOnlyTypes(compilation, skipTypes, configTypes, deepTypes, cancellationToken);

        return new RootModel(
            rootName,
            new EquatableArray<string>(rootInterfaces),
            new EquatableArray<ConfigTypeModel>(configTypes.Values.ToImmutableArray()),
            new EquatableArray<ChildRegistrationModel>(registrations.ToImmutable()),
            invocationLocation);
    }

    public static ImmutableArray<ConfigTypeModel> WalkAccessors(INamedTypeSymbol type, Compilation compilation,
        CancellationToken cancellationToken)
    {
        // Reuse the root walk and keep only the accessor models; attribute-driven generation produces accessors
        // for a type and its reachable graph but does not register a graph registrar.
        var model = WalkRoot(type, compilation, null, cancellationToken);

        return ImmutableArray.CreateRange(model.ConfigTypes);
    }

    private static ConfigTypeModel BuildConfigType(INamedTypeSymbol type, Compilation compilation,
        IReadOnlyCollection<INamedTypeSymbol?> skipTypes, bool isReportOnly, List<INamedTypeSymbol> deepTypes)
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
            var changeKind = ClassifyChange(property, type, compilation, skipTypes, out var elementTypeName,
                out var deepType);

            if (deepType != null)
            {
                deepTypes.Add(deepType);
            }

            propertyModels.Add(new ConfigPropertyModel(property.Name, propertyTypeName, canWrite, isValueType,
                changeKind, elementTypeName));
        }

        return new ConfigTypeModel(typeName, new EquatableArray<ConfigPropertyModel>(propertyModels.ToImmutable()),
            type.IsValueType, LocationInfo.CreateFrom(type), isReportOnly);
    }

    private static void WalkReportOnlyTypes(Compilation compilation,
        IReadOnlyCollection<INamedTypeSymbol?> skipTypes, Dictionary<string, ConfigTypeModel> configTypes,
        List<INamedTypeSymbol> seedTypes, CancellationToken cancellationToken)
    {
        var queue = new Queue<INamedTypeSymbol>(seedTypes);

        while (queue.Count > 0)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var type = queue.Dequeue();
            var name = type.ToDisplayString(_fullyQualifiedFormat);

            if (configTypes.ContainsKey(name))
            {
                // The type is already a registered configuration type or has already been captured for reporting.
                continue;
            }

            var discovered = new List<INamedTypeSymbol>();

            configTypes[name] = BuildConfigType(type, compilation, skipTypes, true, discovered);

            foreach (var child in discovered)
            {
                queue.Enqueue(child);
            }
        }
    }

    private static PropertyChangeKind ClassifyChange(IPropertySymbol property, INamedTypeSymbol owningType,
        Compilation compilation, IReadOnlyCollection<INamedTypeSymbol?> skipTypes, out string? elementTypeName,
        out INamedTypeSymbol? deepType)
    {
        elementTypeName = null;
        deepType = null;

        var type = property.Type;

        if (ShouldRecurse(type, owningType, skipTypes))
        {
            if (type.IsValueType)
            {
                // A configuration struct is registered as a one-time snapshot and is compared by value at the parent,
                // so it is treated as a scalar rather than walked for nested logging.
                return PropertyChangeKind.Scalar;
            }

            // The property is a child configuration type. It is assigned but logged independently in summary mode, and
            // walked for nested logging in deep mode.
            deepType = (INamedTypeSymbol)type;

            return PropertyChangeKind.ChildConfig;
        }

        var elementType = GetListElementType(type, compilation);

        if (elementType != null)
        {
            if (IsScalar(elementType))
            {
                elementTypeName = elementType.ToDisplayString(_fullyQualifiedFormat);

                return PropertyChangeKind.ScalarList;
            }

            if (elementType is INamedTypeSymbol namedElement
                && namedElement.TypeKind == TypeKind.Class
                && namedElement.SpecialType != SpecialType.System_String)
            {
                // An indexable list of complex elements can be walked per-element for nested logging in deep mode.
                elementTypeName = elementType.ToDisplayString(_fullyQualifiedFormat);
                deepType = namedElement;

                return PropertyChangeKind.ComplexList;
            }
        }

        if (ImplementsNonGenericICollection(type, compilation))
        {
            return PropertyChangeKind.Countable;
        }

        return PropertyChangeKind.Scalar;
    }

    private static ITypeSymbol? GetListElementType(ITypeSymbol type, Compilation compilation)
    {
        if (type is IArrayTypeSymbol array)
        {
            return array.ElementType;
        }

        var readOnlyListDef = compilation.GetTypeByMetadataName("System.Collections.Generic.IReadOnlyList`1");

        if (readOnlyListDef == null)
        {
            return null;
        }

        if (type is INamedTypeSymbol namedType
            && namedType.IsGenericType
            && SymbolEqualityComparer.Default.Equals(namedType.OriginalDefinition, readOnlyListDef))
        {
            return namedType.TypeArguments[0];
        }

        foreach (var implemented in type.AllInterfaces)
        {
            if (implemented.IsGenericType
                && SymbolEqualityComparer.Default.Equals(implemented.OriginalDefinition, readOnlyListDef))
            {
                return implemented.TypeArguments[0];
            }
        }

        return null;
    }

    private static bool ImplementsNonGenericICollection(ITypeSymbol type, Compilation compilation)
    {
        if (type is IArrayTypeSymbol)
        {
            return true;
        }

        var collectionType = compilation.GetTypeByMetadataName("System.Collections.ICollection");

        if (collectionType == null)
        {
            return false;
        }

        foreach (var implemented in type.AllInterfaces)
        {
            if (SymbolEqualityComparer.Default.Equals(implemented, collectionType))
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsScalar(ITypeSymbol type)
    {
        return type.IsValueType || type.SpecialType == SpecialType.System_String;
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

    private static bool IsConfigStruct(INamedTypeSymbol type)
    {
        // A struct is treated as a configuration type (rather than a scalar value such as Guid, TimeSpan,
        // DateTime or Nullable<T>) when it exposes at least one public settable property the binder would
        // populate from a configuration section.
        foreach (var property in GetBindableProperties(type))
        {
            if (property.SetMethod != null
                && property.SetMethod.DeclaredAccessibility == Accessibility.Public
                && property.SetMethod.IsInitOnly == false)
            {
                return true;
            }
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

        if (namedType.SpecialType == SpecialType.System_String)
        {
            return false;
        }

        var isClass = namedType.TypeKind == TypeKind.Class;
        var isConfigStruct = namedType.TypeKind == TypeKind.Struct && IsConfigStruct(namedType);

        if (isClass == false && isConfigStruct == false)
        {
            // Interfaces and derived types deeper in the graph require attribute hints, which are handled
            // separately. Scalar value types are leaf property values and are not recursed into.
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
        List<INamedTypeSymbol> deepTypes,
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
                new EquatableArray<string>(interfaces), childType.IsValueType));

            if (configTypes.ContainsKey(childName) == false)
            {
                configTypes[childName] = BuildConfigType(childType, compilation, skipTypes, false, deepTypes);
            }

            // Add the child to the current path to guard against cycles while still allowing the same type to
            // be registered at sibling paths.
            if (ancestors.Add(childName))
            {
                WalkChildren(childType, sectionPath, compilation, skipTypes, configTypes, registrations, ancestors,
                    deepTypes, cancellationToken);

                ancestors.Remove(childName);
            }
        }
    }
}
