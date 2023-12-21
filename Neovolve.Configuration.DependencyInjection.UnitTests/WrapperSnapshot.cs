namespace Neovolve.Configuration.DependencyInjection.UnitTests;

using Microsoft.Extensions.Options;

public class WrapperSnapshot<T> : IOptionsSnapshot<T> where T : class
{
    private Action<T, string?> _listener;

    public WrapperSnapshot(T config)
    {
        Value = config;
    }

    public T Get(string? name)
    {
        return Value;
    }

    public T Value { get; }
}