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
        options.OnChange((options, name) => _onChange?.Invoke(options, name));
    }

    internal event Action<TInterface, string>? _onChange;

    public TInterface Get(string name)
    {
        return _options.Get(name);
    }

    public IDisposable OnChange(Action<TInterface, string> listener)
    {
        var disposable = new ConfigurationChangeTracker(this, listener);

        _onChange += disposable.OnChange;

        return disposable;
    }

    public TInterface CurrentValue => _options.CurrentValue;

    internal sealed class ConfigurationChangeTracker : IDisposable
    {
        private readonly Action<TInterface, string> _listener;
        private readonly MonitorProxy<TConcrete, TInterface> _monitor;

        public ConfigurationChangeTracker(MonitorProxy<TConcrete, TInterface> monitor,
            Action<TInterface, string> listener)
        {
            _listener = listener;
            _monitor = monitor;
        }

        public void Dispose() => _monitor._onChange -= OnChange;

        public void OnChange(TInterface options, string name) => _listener.Invoke(options, name);
    }
}