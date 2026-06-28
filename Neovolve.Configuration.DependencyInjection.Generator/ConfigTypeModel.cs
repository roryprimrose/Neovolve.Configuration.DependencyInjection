namespace Neovolve.Configuration.DependencyInjection.Generator;

/// <summary>
///     The <see cref="ConfigTypeModel" /> struct describes a configuration type and its bindable properties so the
///     generator can emit strongly typed accessors for it.
/// </summary>
internal readonly struct ConfigTypeModel : IEquatable<ConfigTypeModel>
{
    public ConfigTypeModel(string fullyQualifiedName, EquatableArray<ConfigPropertyModel> properties, bool isValueType,
        LocationInfo? declarationLocation)
    {
        FullyQualifiedName = fullyQualifiedName;
        Properties = properties;
        IsValueType = isValueType;
        DeclarationLocation = declarationLocation;
    }

    public bool Equals(ConfigTypeModel other)
    {
        return FullyQualifiedName == other.FullyQualifiedName
            && Properties.Equals(other.Properties)
            && IsValueType == other.IsValueType
            && Nullable.Equals(DeclarationLocation, other.DeclarationLocation);
    }

    public override bool Equals(object? obj)
    {
        return obj is ConfigTypeModel other && Equals(other);
    }

    public override int GetHashCode()
    {
        var hash = 17;

        hash = (hash * 31) + FullyQualifiedName.GetHashCode();
        hash = (hash * 31) + Properties.GetHashCode();
        hash = (hash * 31) + IsValueType.GetHashCode();
        hash = (hash * 31) + (DeclarationLocation?.GetHashCode() ?? 0);

        return hash;
    }

    public bool HasWritableProperty
    {
        get
        {
            foreach (var property in Properties)
            {
                if (property.CanWrite)
                {
                    return true;
                }
            }

            return false;
        }
    }

    public string FullyQualifiedName { get; }

    public bool IsValueType { get; }

    public LocationInfo? DeclarationLocation { get; }

    public EquatableArray<ConfigPropertyModel> Properties { get; }
}
