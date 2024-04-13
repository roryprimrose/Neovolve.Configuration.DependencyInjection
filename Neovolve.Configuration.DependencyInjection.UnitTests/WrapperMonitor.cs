namespace Neovolve.Configuration.DependencyInjection.UnitTests;

using Microsoft.Extensions.Options;

public class WrapperMonitor<T> : IOptionsMonitor<T>
{
    private Action<T, string?>? _listener;

    public WrapperMonitor(T config)
    {
        CurrentValue = config;
    }

    public T Get(string? name)
    {
        return CurrentValue;
    }

    public IDisposable? OnChange(Action<T, string?> listener)
    {
        _listener = listener;

        return null;
    }

    public void UpdateConfig(T newConfig, string name)
    {
        _listener?.Invoke(newConfig, name);
        CurrentValue = newConfig;
    }

    public T CurrentValue { get; private set; }
}