// ReSharper disable AccessToDisposedClosure

namespace Neovolve.Configuration.DependencyInjection.UnitTests
{
    using FluentAssertions;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using ModelBuilder;
    using NSubstitute;
    using Xunit.Abstractions;

    public sealed class ConfigUpdateExtensionsTests : IDisposable
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
                First = injectedValue
            };
            var updatedConfig = new SimpleType
            {
                First = updatedValue
            };
            using var provider = BuildServiceProvider();

            provider.CopyValues(injectedConfig, updatedConfig);

            injectedConfig.First.Should().Be(updatedValue);
            updatedConfig.First.Should().Be(updatedValue);
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
                First = value
            };
            var updatedConfig = new SimpleType
            {
                First = value
            };
            using var provider = BuildServiceProvider();

            provider.CopyValues(injectedConfig, updatedConfig);

            injectedConfig.First.Should().Be(injectedConfig.First);
            updatedConfig.First.Should().Be(updatedConfig.First);
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

        public void Dispose()
        {
            _factory.Dispose();
        }
    }
}