namespace Neovolve.Configuration.DependencyInjection.Generator;

/// <summary>
///     The <see cref="RootModel" /> struct describes a single <c>ConfigureWith&lt;T&gt;</c> root: the configuration types
///     to emit accessors for, the root interfaces and the ordered child registrations.
/// </summary>
internal readonly struct RootModel : IEquatable<RootModel>
{
    public RootModel(
        string rootTypeFullyQualifiedName,
        EquatableArray<string> rootInterfaceFullyQualifiedNames,
        EquatableArray<ConfigTypeModel> configTypes,
        EquatableArray<ChildRegistrationModel> registrations,
        LocationInfo? invocationLocation)
    {
        RootTypeFullyQualifiedName = rootTypeFullyQualifiedName;
        RootInterfaceFullyQualifiedNames = rootInterfaceFullyQualifiedNames;
        ConfigTypes = configTypes;
        Registrations = registrations;
        InvocationLocation = invocationLocation;
    }

    public bool Equals(RootModel other)
    {
        return RootTypeFullyQualifiedName == other.RootTypeFullyQualifiedName
            && RootInterfaceFullyQualifiedNames.Equals(other.RootInterfaceFullyQualifiedNames)
            && ConfigTypes.Equals(other.ConfigTypes)
            && Registrations.Equals(other.Registrations)
            && Nullable.Equals(InvocationLocation, other.InvocationLocation);
    }

    public override bool Equals(object? obj)
    {
        return obj is RootModel other && Equals(other);
    }

    public override int GetHashCode()
    {
        var hash = 17;

        hash = (hash * 31) + RootTypeFullyQualifiedName.GetHashCode();
        hash = (hash * 31) + RootInterfaceFullyQualifiedNames.GetHashCode();
        hash = (hash * 31) + ConfigTypes.GetHashCode();
        hash = (hash * 31) + Registrations.GetHashCode();
        hash = (hash * 31) + (InvocationLocation?.GetHashCode() ?? 0);

        return hash;
    }

    public EquatableArray<ConfigTypeModel> ConfigTypes { get; }

    public LocationInfo? InvocationLocation { get; }

    public EquatableArray<ChildRegistrationModel> Registrations { get; }

    public EquatableArray<string> RootInterfaceFullyQualifiedNames { get; }

    public string RootTypeFullyQualifiedName { get; }
}
