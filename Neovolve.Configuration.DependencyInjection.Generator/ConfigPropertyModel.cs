namespace Neovolve.Configuration.DependencyInjection.Generator;

/// <summary>
///     The <see cref="ConfigPropertyModel" /> struct describes a single bindable property on a configuration type using
///     only the data required to emit a strongly typed accessor.
/// </summary>
internal readonly struct ConfigPropertyModel : IEquatable<ConfigPropertyModel>
{
    public ConfigPropertyModel(string name, string typeFullyQualifiedName, bool canWrite, bool isValueType,
        PropertyChangeKind changeKind, string? elementTypeFullyQualifiedName)
    {
        Name = name;
        TypeFullyQualifiedName = typeFullyQualifiedName;
        CanWrite = canWrite;
        IsValueType = isValueType;
        ChangeKind = changeKind;
        ElementTypeFullyQualifiedName = elementTypeFullyQualifiedName;
    }

    public bool Equals(ConfigPropertyModel other)
    {
        return Name == other.Name
            && TypeFullyQualifiedName == other.TypeFullyQualifiedName
            && CanWrite == other.CanWrite
            && IsValueType == other.IsValueType
            && ChangeKind == other.ChangeKind
            && ElementTypeFullyQualifiedName == other.ElementTypeFullyQualifiedName;
    }

    public override bool Equals(object? obj)
    {
        return obj is ConfigPropertyModel other && Equals(other);
    }

    public override int GetHashCode()
    {
        var hash = 17;

        hash = (hash * 31) + Name.GetHashCode();
        hash = (hash * 31) + TypeFullyQualifiedName.GetHashCode();
        hash = (hash * 31) + CanWrite.GetHashCode();
        hash = (hash * 31) + IsValueType.GetHashCode();
        hash = (hash * 31) + ChangeKind.GetHashCode();
        hash = (hash * 31) + (ElementTypeFullyQualifiedName?.GetHashCode() ?? 0);

        return hash;
    }

    public bool CanWrite { get; }

    public PropertyChangeKind ChangeKind { get; }

    public string? ElementTypeFullyQualifiedName { get; }

    public bool IsValueType { get; }

    public string Name { get; }

    public string TypeFullyQualifiedName { get; }
}
