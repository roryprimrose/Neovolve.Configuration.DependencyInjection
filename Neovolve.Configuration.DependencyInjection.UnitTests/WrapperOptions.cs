namespace Neovolve.Configuration.DependencyInjection.UnitTests;

using Microsoft.Extensions.Options;

public class WrapperOptions<T> : IOptions<T> where T : class
{
    public WrapperOptions(T config)
    {
        Value = config;
    }

    public T Value { get; }
}