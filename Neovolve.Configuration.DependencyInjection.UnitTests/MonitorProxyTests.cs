namespace Neovolve.Configuration.DependencyInjection.UnitTests
{
    using FluentAssertions;
    using Microsoft.Extensions.Options;
    using ModelBuilder;

    public class MonitorProxyTests
    {
        [Fact]
        public void CurrentValueReturnsFromSource()
        {
            var name = Guid.NewGuid().ToString();
            var expected = Model.Create<SimpleType>();

            var source = new WrapperMonitor<SimpleType>(expected);

            var sut = new MonitorProxy<SimpleType, ISimpleType>(source);

            var actual = sut.CurrentValue;

            actual.Should().Be(expected);
        }

        [Fact]
        public void GetReturnsValueFromSourceValue()
        {
            var name = Guid.NewGuid().ToString();
            var expected = Model.Create<SimpleType>();

            var source = new WrapperMonitor<SimpleType>(expected);

            var sut = new MonitorProxy<SimpleType, ISimpleType>(source);

            var actual = sut.Get(name);

            actual.Should().Be(expected);
        }

        [Fact]
        public void OnChangeRegistersListenerForConfigChangedEvent()
        {
            var name = Guid.NewGuid().ToString();
            var expected = Model.Create<SimpleType>();
            var updatedConfig = Model.Create<SimpleType>();
            var listenerTriggered = false;

            var source = new WrapperMonitor<SimpleType>(expected);

            var sut = new MonitorProxy<SimpleType, ISimpleType>(source);

            using var actual = sut.OnChange((config, updatedName) =>
            {
                config.Should().Be(updatedConfig);
                updatedName.Should().Be(name);
                listenerTriggered = true;
            });

            source.UpdateConfig(updatedConfig, name);

            listenerTriggered.Should().BeTrue();
        }

        private class WrapperMonitor<T> : IOptionsMonitor<T>
        {
            private Action<T, string?> _listener;

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
    }
}