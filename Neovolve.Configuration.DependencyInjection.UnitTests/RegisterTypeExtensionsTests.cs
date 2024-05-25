namespace Neovolve.Configuration.DependencyInjection.UnitTests
{
    using FluentAssertions;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Options;
    using ModelBuilder;
    using NSubstitute;

    public class RegisterTypeExtensionsTests
    {
        [Fact]
        public void RegisterConfigInterfaceTypeRegistersInterface()
        {
            var injectedConfig = Model.Create<SimpleType>();
            var services = new ServiceCollection();

            services.AddSingleton(injectedConfig);
            services.RegisterConfigInterfaceType<SimpleType, ISimpleType>();

            using var provider = services.BuildServiceProvider();

            var actual = provider.GetService<ISimpleType>();

            actual.Should().NotBeNull();
            actual!.Should().BeSameAs(injectedConfig);
        }

        [Fact]
        public void RegisterConfigInterfaceTypeRegistersMonitorForInterface()
        {
            var injectedConfig = Model.Create<SimpleType>();
            var monitor = new WrapperMonitor<SimpleType>(injectedConfig);
            var services = new ServiceCollection();

            services.AddSingleton<IOptionsMonitor<SimpleType>>(monitor);
            services.RegisterConfigInterfaceType<SimpleType, ISimpleType>();

            using var provider = services.BuildServiceProvider();

            var actual = provider.GetService<IOptionsMonitor<ISimpleType>>();

            actual.Should().NotBeNull();
            actual!.CurrentValue.Should().BeSameAs(injectedConfig);
        }

        [Fact]
        public void RegisterConfigInterfaceTypeRegistersOptionsForInterface()
        {
            var injectedConfig = Model.Create<SimpleType>();
            var options = new WrapperOptions<SimpleType>(injectedConfig);
            var services = new ServiceCollection();

            services.AddSingleton<IOptions<SimpleType>>(options);
            services.RegisterConfigInterfaceType<SimpleType, ISimpleType>();

            using var provider = services.BuildServiceProvider();

            var actual = provider.GetService<IOptions<ISimpleType>>();

            actual.Should().NotBeNull();
            actual!.Value.Should().BeSameAs(injectedConfig);
        }

        [Fact]
        public void RegisterConfigInterfaceTypeRegistersSnapshotForInterface()
        {
            var injectedConfig = Model.Create<SimpleType>();
            var snapshot = new WrapperSnapshot<SimpleType>(injectedConfig);
            var services = new ServiceCollection();

            services.AddSingleton<IOptionsSnapshot<SimpleType>>(snapshot);
            services.RegisterConfigInterfaceType<SimpleType, ISimpleType>();

            using var provider = services.BuildServiceProvider();

            var actual = provider.GetService<IOptionsSnapshot<ISimpleType>>();

            actual.Should().NotBeNull();
            actual!.Value.Should().BeSameAs(injectedConfig);
        }

        [Fact]
        public void RegisterConfigTypeDoesNotUpdateExistingConfigurationObjectWhenReloadIsFalse()
        {
            var options = new ConfigureWithOptions
            {
                ReloadInjectedRawTypes = false
            };
            var injectedConfig = Model.Create<SimpleType>();
            var updatedConfig = Model.Create<SimpleType>();
            var monitor = new WrapperMonitor<SimpleType>(injectedConfig);
            var services = new ServiceCollection();
            var name = Guid.NewGuid().ToString();

            var section = Substitute.For<IConfigurationSection>();

            services.AddSingleton<IConfigureWithOptions>(options);
            services.AddSingleton<IOptionsMonitor<SimpleType>>(monitor);
            services.RegisterConfigType<SimpleType>(section);

            using var provider = services.BuildServiceProvider();

            var actual = provider.GetService<SimpleType>();

            monitor.UpdateConfig(updatedConfig, name);

            injectedConfig.Should().NotBeEquivalentTo(updatedConfig);
            actual.Should().BeSameAs(injectedConfig);
        }

        [Fact]
        public void RegisterConfigTypeReturnsSingletonTypeFromMonitor()
        {
            var options = new ConfigureWithOptions
            {
                ReloadInjectedRawTypes = true
            };
            var injectedConfig = Model.Create<SimpleType>();
            var monitor = new WrapperMonitor<SimpleType>(injectedConfig);
            var services = new ServiceCollection();

            var section = Substitute.For<IConfigurationSection>();

            services.AddSingleton<IConfigureWithOptions>(options);
            services.AddSingleton<IOptionsMonitor<SimpleType>>(monitor);
            services.RegisterConfigType<SimpleType>(section);

            using var provider = services.BuildServiceProvider();

            var firstActual = provider.GetService<SimpleType>();

            firstActual.Should().BeEquivalentTo(injectedConfig);

            var secondActual = provider.GetService<SimpleType>();

            secondActual.Should().BeSameAs(firstActual);
        }

        [Fact]
        public void RegisterConfigTypeUpdatesExistingConfigurationObjectWhenReloadIsTrue()
        {
            var options = new ConfigureWithOptions
            {
                ReloadInjectedRawTypes = true
            };
            var injectedConfig = Model.Create<SimpleType>();
            var updatedConfig = Model.Create<SimpleType>();
            var monitor = new WrapperMonitor<SimpleType>(injectedConfig);
            var services = new ServiceCollection();
            var name = Guid.NewGuid().ToString();

            var section = Substitute.For<IConfigurationSection>();

            services.AddSingleton<IConfigureWithOptions>(options);
            services.AddSingleton<IOptionsMonitor<SimpleType>>(monitor);
            services.AddTransient<IConfigUpdater, DefaultConfigUpdater>();
            services.RegisterConfigType<SimpleType>(section);

            using var provider = services.BuildServiceProvider();

            var actual = provider.GetService<SimpleType>();

            monitor.UpdateConfig(updatedConfig, name);

            injectedConfig.Should().BeEquivalentTo(updatedConfig);
            actual.Should().BeSameAs(injectedConfig);
        }
    }
}