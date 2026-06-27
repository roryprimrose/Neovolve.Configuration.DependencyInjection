namespace Neovolve.Configuration.DependencyInjection.Generator;

/// <summary>
///     The <see cref="ConfigTypeModel" /> struct describes a configuration type and its bindable properties so the
///     generator can emit strongly typed accessors for it.
/// </summary>
internal readonly struct ConfigTypeModel : IEquatable<ConfigTypeModel>
{
    public ConfigTypeModel(string fullyQualifiedName, EquatableArray<ConfigPropertyModel> properties)
    {
        FullyQualifiedName = fullyQualifiedName;
        Properties = properties;
    }

    public bool Equals(ConfigTypeModel other)
    {
        return FullyQualifiedName == other.FullyQualifiedName
            && Properties.Equals(other.Properties);
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

        return hash;
    }

    public string FullyQualifiedName { get; }

    public EquatableArray<ConfigPropertyModel> Properties { get; }
}
