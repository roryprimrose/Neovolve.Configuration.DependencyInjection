namespace Neovolve.Configuration.DependencyInjection.Generator;

/// <summary>
///     The <see cref="ConfigTypeModel" /> struct describes a configuration type and its bindable properties so the
///     generator can emit strongly typed accessors for it.
/// </summary>
internal readonly struct ConfigTypeModel : IEquatable<ConfigTypeModel>
{
    public ConfigTypeModel(string fullyQualifiedName, EquatableArray<ConfigPropertyModel> properties, bool isValueType,
        LocationInfo? declarationLocation, bool isReportOnly)
    {
        FullyQualifiedName = fullyQualifiedName;
        Properties = properties;
        IsValueType = isValueType;
        DeclarationLocation = declarationLocation;
        IsReportOnly = isReportOnly;
    }

    public bool Equals(ConfigTypeModel other)
    {
        return FullyQualifiedName == other.FullyQualifiedName
            && Properties.Equals(other.Properties)
            && IsValueType == other.IsValueType
            && Nullable.Equals(DeclarationLocation, other.DeclarationLocation)
            && IsReportOnly == other.IsReportOnly;
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
        hash = (hash * 31) + IsReportOnly.GetHashCode();

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

    public bool IsReportOnly { get; }

    public LocationInfo? DeclarationLocation { get; }

    public EquatableArray<ConfigPropertyModel> Properties { get; }
}
