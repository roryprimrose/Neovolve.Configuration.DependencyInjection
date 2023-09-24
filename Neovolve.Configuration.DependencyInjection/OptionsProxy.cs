namespace Neovolve.Configuration.DependencyInjection;

using Microsoft.Extensions.Options;

internal class OptionsProxy<TConcrete, TInterface> : IOptions<TInterface>
    where TConcrete : class, TInterface where TInterface : class
{
    private readonly IOptions<TConcrete> _options;

    public OptionsProxy(IOptions<TConcrete> options)
    {
        _options = options;
    }

    public TInterface Value => _options.Value;
}