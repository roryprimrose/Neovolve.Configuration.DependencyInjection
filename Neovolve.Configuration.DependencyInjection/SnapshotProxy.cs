namespace Neovolve.Configuration.DependencyInjection;

using Microsoft.Extensions.Options;

internal class SnapshotProxy<TConcrete, TInterface> : IOptionsSnapshot<TInterface>
    where TConcrete : class, TInterface where TInterface : class
{
    private readonly IOptionsSnapshot<TConcrete> _options;

    public SnapshotProxy(IOptionsSnapshot<TConcrete> options)
    {
        _options = options;
    }

    public TInterface Get(string? name)
    {
        return _options.Get(name);
    }

    public TInterface Value => _options.Value;
}