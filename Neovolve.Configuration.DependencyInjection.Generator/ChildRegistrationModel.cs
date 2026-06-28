namespace Neovolve.Configuration.DependencyInjection.Generator;

/// <summary>
///     The <see cref="ChildRegistrationModel" /> struct describes the registration of a single child configuration type at
///     a configuration section, including the interfaces to redirect to it.
/// </summary>
internal readonly struct ChildRegistrationModel : IEquatable<ChildRegistrationModel>
{
    public ChildRegistrationModel(string typeFullyQualifiedName, string sectionPath,
        EquatableArray<string> interfaceFullyQualifiedNames, bool isValueType)
    {
        TypeFullyQualifiedName = typeFullyQualifiedName;
        SectionPath = sectionPath;
        InterfaceFullyQualifiedNames = interfaceFullyQualifiedNames;
        IsValueType = isValueType;
    }

    public bool Equals(ChildRegistrationModel other)
    {
        return TypeFullyQualifiedName == other.TypeFullyQualifiedName
            && SectionPath == other.SectionPath
            && InterfaceFullyQualifiedNames.Equals(other.InterfaceFullyQualifiedNames)
            && IsValueType == other.IsValueType;
    }

    public override bool Equals(object? obj)
    {
        return obj is ChildRegistrationModel other && Equals(other);
    }

    public override int GetHashCode()
    {
        var hash = 17;

        hash = (hash * 31) + TypeFullyQualifiedName.GetHashCode();
        hash = (hash * 31) + SectionPath.GetHashCode();
        hash = (hash * 31) + InterfaceFullyQualifiedNames.GetHashCode();
        hash = (hash * 31) + IsValueType.GetHashCode();

        return hash;
    }

    public EquatableArray<string> InterfaceFullyQualifiedNames { get; }

    public bool IsValueType { get; }

    public string SectionPath { get; }

    public string TypeFullyQualifiedName { get; }
}
