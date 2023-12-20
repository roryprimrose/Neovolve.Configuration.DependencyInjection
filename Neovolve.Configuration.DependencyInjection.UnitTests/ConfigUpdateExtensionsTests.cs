// ReSharper disable AccessToDisposedClosure

namespace Neovolve.Configuration.DependencyInjection.UnitTests
{
    using System.Diagnostics.CodeAnalysis;
    using FluentAssertions;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using ModelBuilder;
    using NSubstitute;
    using Xunit.Abstractions;

    public class ConfigUpdateExtensionsTests
    {
        private readonly ILoggerFactory _factory;

        public ConfigUpdateExtensionsTests(ITestOutputHelper output)
        {
            _factory = output.BuildLoggerFactory(LogLevel.Debug);
        }

        [Theory]
        [InlineData(null, "updated value")]
        [InlineData("injected value", null)]
        public void CopyValuesCanUpdateNullValues(string? injectedValue, string? updatedValue)
        {
            var injectedConfig = new SimpleType
            {
                Value = injectedValue
            };
            var updatedConfig = new SimpleType
            {
                Value = updatedValue
            };
            using var provider = BuildServiceProvider();

            provider.CopyValues(injectedConfig, updatedConfig);

            injectedConfig.Value.Should().Be(updatedValue);
            updatedConfig.Value.Should().Be(updatedValue);
        }

        [Theory]
        [InlineData(typeof(SimpleType))]
        [InlineData(typeof(InheritedType))]
        [InlineData(typeof(NestedType))]
        [InlineData(typeof(NestedRecords))]
        public void CopyValuesDoesNotLogWhenNotConfigured(Type targetType)
        {
            var injectedConfig = Model.Create(targetType);
            var updatedConfig = Model.Create(targetType);
            var provider = Substitute.For<IServiceProvider>();

            // We need to call CopyValues as the generic type signature so that the properties from the Type targetType parameter can be copied correctly
            var method = typeof(ConfigUpdateExtensions).GetMethod(nameof(ConfigUpdateExtensions.CopyValues));
            var typedMethod = method!.MakeGenericMethod(targetType);

            var action = () => typedMethod.Invoke(null, new[] { provider, injectedConfig, updatedConfig });

            action.Should().NotThrow();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("injected value")]
        public void CopyValuesDoesNotUpdatePropertyWhenBothValuesAreSame(string? value)
        {
            var injectedConfig = new SimpleType
            {
                Value = value
            };
            var updatedConfig = new SimpleType
            {
                Value = value
            };
            using var provider = BuildServiceProvider();

            provider.CopyValues(injectedConfig, updatedConfig);

            injectedConfig.Value.Should().Be(injectedConfig.Value);
            updatedConfig.Value.Should().Be(updatedConfig.Value);
        }

        [Fact]
        public void CopyValuesSkipsPrivateSetProperties()
        {
            var injectedConfig = Model.Create<PrivateSetType>();
            var updatedConfig = Model.Create<PrivateSetType>();
            using var provider = BuildServiceProvider();

            provider.CopyValues(injectedConfig, updatedConfig);

            injectedConfig.First.Should().Be(updatedConfig.First);
            injectedConfig.Second.Should().NotBe(updatedConfig.Second);
            injectedConfig.Third.Should().Be(updatedConfig.Third);
        }

        [Fact]
        public void CopyValuesSkipsPropertyWhenGetThrowsException()
        {
            var injectedConfig = Model.Ignoring<GetExceptionType>(x => x.Second).Create<GetExceptionType>();
            var updatedConfig = Model.Ignoring<GetExceptionType>(x => x.Second).Create<GetExceptionType>();
            using var provider = BuildServiceProvider();

            provider.CopyValues(injectedConfig, updatedConfig);

            injectedConfig.First.Should().Be(updatedConfig.First);
            injectedConfig.Third.Should().Be(updatedConfig.Third);
        }

        [Fact]
        public void CopyValuesSkipsPropertyWhenSetThrowsException()
        {
            var injectedConfig = Model.Ignoring<SetExceptionType>(x => x.Second).Create<SetExceptionType>();
            var updatedConfig = Model.Ignoring<SetExceptionType>(x => x.Second).Create<SetExceptionType>();
            using var provider = BuildServiceProvider();

            provider.CopyValues(injectedConfig, updatedConfig);

            injectedConfig.First.Should().Be(updatedConfig.First);
            injectedConfig.Second.Should().NotBe(updatedConfig.Second);
            injectedConfig.Third.Should().Be(updatedConfig.Third);
        }

        [Fact]
        public void CopyValuesSkipsReadOnlyProperties()
        {
            var injectedConfig = Model.Create<ReadOnlyType>();
            var updatedConfig = Model.Create<ReadOnlyType>();
            using var provider = BuildServiceProvider();

            provider.CopyValues(injectedConfig, updatedConfig);

            injectedConfig.First.Should().Be(updatedConfig.First);
            injectedConfig.Second.Should().NotBe(updatedConfig.Second);
            injectedConfig.Third.Should().Be(updatedConfig.Third);
        }

        [Fact]
        public void CopyValuesTakesNoActionWhenClassHasNoProperties()
        {
            var injectedConfig = new EmptyClass();
            var updatedConfig = new EmptyClass();
            using var provider = BuildServiceProvider();

            var action = () => provider.CopyValues(injectedConfig, updatedConfig);

            action.Should().NotThrow();
        }

        [Fact]
        public void CopyValuesTakesNoActionWhenInjectedConfigIsNull()
        {
            FirstConfig? injectedConfig = null;
            var updatedConfig = new FirstConfig();
            using var provider = BuildServiceProvider();

            var action = () => provider.CopyValues(injectedConfig, updatedConfig);

            action.Should().NotThrow();
        }

        [Fact]
        public void CopyValuesTakesNoActionWhenUpdatedConfigIsNull()
        {
            var injectedConfig = new FirstConfig();
            FirstConfig? updatedConfig = null;
            using var provider = BuildServiceProvider();

            var action = () => provider.CopyValues(injectedConfig, updatedConfig);

            action.Should().NotThrow();
        }

        [Theory]
        [InlineData(typeof(SimpleType))]
        [InlineData(typeof(InheritedType))]
        [InlineData(typeof(NestedType))]
        [InlineData(typeof(NestedRecords))]
        public void CopyValuesUpdatesPropertiesOnTargetType(Type targetType)
        {
            var injectedConfig = Model.Create(targetType);
            var updatedConfig = Model.Create(targetType);
            using var provider = BuildServiceProvider();

            // We need to call CopyValues as the generic type signature so that the properties from the Type targetType parameter can be copied correctly
            var method = typeof(ConfigUpdateExtensions).GetMethod(nameof(ConfigUpdateExtensions.CopyValues));
            var typedMethod = method!.MakeGenericMethod(targetType);

            typedMethod.Invoke(null, new[] { provider, injectedConfig, updatedConfig });

            injectedConfig.Should().BeEquivalentTo(updatedConfig);
        }

        private ServiceProvider BuildServiceProvider()
        {
            var services = new ServiceCollection();

            services.AddSingleton(_factory);

            return services.BuildServiceProvider();
        }

        private class EmptyClass
        {
        }

        [SuppressMessage("ReSharper", "AutoPropertyCanBeMadeGetOnly.Local")]
        private class GetExceptionType
        {
            private string _second = Guid.NewGuid().ToString();
            public string First { get; set; } = Guid.NewGuid().ToString();

            public string Second { get => throw new InvalidOperationException(); set => _second = value; }

            public string Third { get; set; } = Guid.NewGuid().ToString();
        }

        private class InheritedType : SimpleType
        {
            public Guid Other { get; set; }
        }

        private class NestedRecords
        {
            public ChildRecord First { get; set; }
            public ChildRecord Second { get; set; }
        }

        private class NestedType
        {
            public InheritedType Child { get; set; }
            public int Something { get; set; }
        }

        [SuppressMessage("ReSharper", "AutoPropertyCanBeMadeGetOnly.Local")]
        private class PrivateSetType
        {
            public string First { get; set; } = Guid.NewGuid().ToString();
            public string Second { get; private set; } = Guid.NewGuid().ToString();
            public string Third { get; set; } = Guid.NewGuid().ToString();
        }

        [SuppressMessage("ReSharper", "AutoPropertyCanBeMadeGetOnly.Local")]
        private class ReadOnlyType
        {
            public string First { get; set; } = Guid.NewGuid().ToString();
            public string Second { get; } = Guid.NewGuid().ToString();
            public string Third { get; set; } = Guid.NewGuid().ToString();
        }

        [SuppressMessage("ReSharper", "AutoPropertyCanBeMadeGetOnly.Local")]
        private class SetExceptionType
        {
            private readonly string _second = Guid.NewGuid().ToString();
            public string First { get; set; } = Guid.NewGuid().ToString();

            public string Second { get => _second; set => throw new InvalidOperationException(); }

            public string Third { get; set; } = Guid.NewGuid().ToString();
        }

        private class SimpleType
        {
            public string? Value { get; set; }
        }

        private record ChildRecord(string Name, int Start, bool End);
    }
}