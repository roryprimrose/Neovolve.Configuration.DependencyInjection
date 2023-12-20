namespace Neovolve.Configuration.DependencyInjection;

using System;
using Microsoft.Extensions.Options;

internal class MonitorProxy<TConcrete, TInterface> : IOptionsMonitor<TInterface>
    where TConcrete : class, TInterface
{
    private readonly IOptionsMonitor<TConcrete> _options;

    public MonitorProxy(IOptionsMonitor<TConcrete> options)
    {
        _options = options;
        options.OnChange((value, name) => ConfigChanged?.Invoke(value, name));
    }

    internal event Action<TInterface, string?>? ConfigChanged;

    public TInterface Get(string? name)
    {
        return _options.Get(name);
    }

    public IDisposable OnChange(Action<TInterface, string?> listener)
    {
        var disposable = new ConfigurationChangeTracker(this, listener);

        ConfigChanged += disposable.ConfigChanged;
        
        return disposable;
    }

    public TInterface CurrentValue => _options.CurrentValue;

    private sealed class ConfigurationChangeTracker : IDisposable
    {
        private readonly Action<TInterface, string?> _listener;
        private readonly MonitorProxy<TConcrete, TInterface> _monitor;

        public ConfigurationChangeTracker(MonitorProxy<TConcrete, TInterface> monitor,
            Action<TInterface, string?> listener)
        {
            _listener = listener;
            _monitor = monitor;
        }

        public void Dispose() => _monitor.ConfigChanged -= ConfigChanged;

        public void ConfigChanged(TInterface options, string? name) => _listener.Invoke(options, name);
    }
}