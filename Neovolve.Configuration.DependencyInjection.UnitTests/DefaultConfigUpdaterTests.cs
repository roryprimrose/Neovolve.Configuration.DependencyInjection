﻿// ReSharper disable AccessToDisposedClosure

namespace Neovolve.Configuration.DependencyInjection.UnitTests
{
    using Divergic.Logging.Xunit;
    using FluentAssertions;
    using Microsoft.Extensions.Logging;
    using ModelBuilder;
    using NSubstitute;
    using Xunit.Abstractions;

    public sealed class DefaultConfigUpdaterTests : TestsInternal
    {
        public DefaultConfigUpdaterTests(ITestOutputHelper output) : base(
            output.BuildLoggerFor<DefaultConfigUpdater>(LogLevel.Trace))
        {
        }

        [Theory]
        [InlineData(null, "updated value")]
        [InlineData("injected value", null)]
        public void UpdateConfigCanUpdateNullValues(string? injectedValue, string? updatedValue)
        {
            var injectedConfig = new SimpleType
            {
                First = injectedValue
            };
            var updatedConfig = new SimpleType
            {
                First = updatedValue
            };
            var name = Guid.NewGuid().ToString();

            SUT.UpdateConfig(injectedConfig, updatedConfig, name, Service<ILogger>());

            injectedConfig.First.Should().Be(updatedValue);
            updatedConfig.First.Should().Be(updatedValue);
        }

        [Theory]
        [InlineData(typeof(SimpleType))]
        [InlineData(typeof(InheritedType))]
        [InlineData(typeof(NestedType))]
        [InlineData(typeof(NestedRecords))]
        public void UpdateConfigDoesNotLogWhenNotConfigured(Type targetType)
        {
            var injectedConfig = Model.Create(targetType);
            var updatedConfig = Model.Create(targetType);
            var name = Guid.NewGuid().ToString();

            var action = () =>
                SUT.UpdateConfig(injectedConfig, updatedConfig, name, null);

            action.Should().NotThrow();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("injected value")]
        public void UpdateConfigDoesNotUpdatePropertyWhenBothValuesAreSame(string? value)
        {
            var injectedConfig = new SimpleType
            {
                First = value
            };
            var updatedConfig = new SimpleType
            {
                First = value
            };
            var name = Guid.NewGuid().ToString();

            SUT.UpdateConfig(injectedConfig, updatedConfig, name, Service<ILogger>());

            injectedConfig.First.Should().Be(injectedConfig.First);
            updatedConfig.First.Should().Be(updatedConfig.First);
        }

        [Theory]
        [InlineData(LogReadOnlyPropertyType.All, true)]
        [InlineData(LogReadOnlyPropertyType.ValueTypesOnly, false)]
        [InlineData(LogReadOnlyPropertyType.None, false)]
        public void UpdateConfigLogsChangesToReadOnlyReferenceTypeWhenEnabled(LogReadOnlyPropertyType option, bool logWritten)
        {
            var injectedConfig = Model.Create<ReadOnlyType<SimpleType>>();
            var updatedConfig = Model.Create<ReadOnlyType<SimpleType>>();
            var name = Guid.NewGuid().ToString();

            Use(new ConfigureWithOptions
            {
                LogReadOnlyPropertyType = option
            });

            var logger = Service<ICacheLogger<DefaultConfigUpdater>>();

            SUT.UpdateConfig(injectedConfig, updatedConfig, name, logger);

            if (logWritten)
            {
                logger.Entries.Should().Contain(x => x.EventId.Id == 5003);
            }
            else
            {
                logger.Entries.Should().NotContain(x => x.EventId.Id == 5003);
            }
        }

        [Theory]
        [InlineData(LogLevel.Critical)]
        [InlineData(LogLevel.Debug)]
        [InlineData(LogLevel.Error)]
        [InlineData(LogLevel.Information)]
        [InlineData(LogLevel.Trace)]
        [InlineData(LogLevel.Warning)]
        public void UpdateConfigLogsChangesToReadOnlyPropertyWithLevelOption(LogLevel logLevel)
        {
            var injectedConfig = Model.Create<ReadOnlyType<SimpleType>>();
            var updatedConfig = Model.Create<ReadOnlyType<SimpleType>>();
            var name = Guid.NewGuid().ToString();

            Use(new ConfigureWithOptions
            {
                LogReadOnlyPropertyType = LogReadOnlyPropertyType.All,
                LogReadOnlyPropertyLevel = logLevel
            });

            var logger = Service<ICacheLogger<DefaultConfigUpdater>>();

            SUT.UpdateConfig(injectedConfig, updatedConfig, name, logger);

            logger.Entries.Should().Contain(x => x.EventId.Id == 5003 && x.LogLevel == logLevel);
        }

        [Theory]
        [InlineData(LogReadOnlyPropertyType.All, true)]
        [InlineData(LogReadOnlyPropertyType.ValueTypesOnly, true)]
        [InlineData(LogReadOnlyPropertyType.None, false)]
        public void UpdateConfigLogsChangesToReadOnlyStringWhenEnabled(LogReadOnlyPropertyType option, bool logWritten)
        {
            var injectedConfig = Model.Create<ReadOnlyType<string>>();
            var updatedConfig = Model.Create<ReadOnlyType<string>>();
            var name = Guid.NewGuid().ToString();

            Use(new ConfigureWithOptions
            {
                LogReadOnlyPropertyType = option
            });

            var logger = Service<ICacheLogger<DefaultConfigUpdater>>();

            SUT.UpdateConfig(injectedConfig, updatedConfig, name, logger);

            if (logWritten)
            {
                logger.Entries.Should().Contain(x => x.EventId.Id == 5003);
            }
            else
            {
                logger.Entries.Should().NotContain(x => x.EventId.Id == 5003);
            }
        }

        [Theory]
        [InlineData(LogReadOnlyPropertyType.All, true)]
        [InlineData(LogReadOnlyPropertyType.ValueTypesOnly, true)]
        [InlineData(LogReadOnlyPropertyType.None, false)]
        public void UpdateConfigLogsChangesToReadOnlyValueTypeWhenEnabled(LogReadOnlyPropertyType option, bool logWritten)
        {
            var injectedConfig = Model.Create<ReadOnlyType<int>>();
            var updatedConfig = Model.Create<ReadOnlyType<int>>();
            var name = Guid.NewGuid().ToString();

            Use(new ConfigureWithOptions
            {
                LogReadOnlyPropertyType = option
            });

            var logger = Service<ICacheLogger<DefaultConfigUpdater>>();

            SUT.UpdateConfig(injectedConfig, updatedConfig, name, logger);

            if (logWritten)
            {
                logger.Entries.Should().Contain(x => x.EventId.Id == 5003);
            }
            else
            {
                logger.Entries.Should().NotContain(x => x.EventId.Id == 5003);
            }
        }

        [Fact]
        public void UpdateConfigSkipsPrivateSetProperties()
        {
            var injectedConfig = Model.Create<PrivateSetType>();
            var updatedConfig = Model.Create<PrivateSetType>();
            var name = Guid.NewGuid().ToString();

            SUT.UpdateConfig(injectedConfig, updatedConfig, name, Service<ILogger>());

            injectedConfig.First.Should().Be(updatedConfig.First);
            injectedConfig.Second.Should().NotBe(updatedConfig.Second);
            injectedConfig.Third.Should().Be(updatedConfig.Third);
        }

        [Fact]
        public void UpdateConfigSkipsPropertyWhenGetThrowsException()
        {
            var injectedConfig = Model.Ignoring<GetExceptionType>(x => x.Second).Create<GetExceptionType>();
            var updatedConfig = Model.Ignoring<GetExceptionType>(x => x.Second).Create<GetExceptionType>();
            var name = Guid.NewGuid().ToString();

            SUT.UpdateConfig(injectedConfig, updatedConfig, name, Service<ILogger>());

            injectedConfig.First.Should().Be(updatedConfig.First);
            injectedConfig.Third.Should().Be(updatedConfig.Third);
        }

        [Fact]
        public void UpdateConfigSkipsPropertyWhenSetThrowsException()
        {
            var injectedConfig = Model.Ignoring<SetExceptionType>(x => x.Second).Create<SetExceptionType>();
            var updatedConfig = Model.Ignoring<SetExceptionType>(x => x.Second).Create<SetExceptionType>();
            var name = Guid.NewGuid().ToString();

            SUT.UpdateConfig(injectedConfig, updatedConfig, name, Service<ILogger>());

            injectedConfig.First.Should().Be(updatedConfig.First);
            injectedConfig.Second.Should().NotBe(updatedConfig.Second);
            injectedConfig.Third.Should().Be(updatedConfig.Third);
        }

        [Fact]
        public void UpdateConfigSkipsReadOnlyProperties()
        {
            var injectedConfig = Model.Create<ReadOnlyType>();
            var updatedConfig = Model.Create<ReadOnlyType>();
            var name = Guid.NewGuid().ToString();

            SUT.UpdateConfig(injectedConfig, updatedConfig, name, Service<ILogger>());

            injectedConfig.First.Should().Be(updatedConfig.First);
            injectedConfig.Second.Should().NotBe(updatedConfig.Second);
            injectedConfig.Third.Should().Be(updatedConfig.Third);
        }

        [Fact]
        public void UpdateConfigTakesNoActionWhenClassHasNoProperties()
        {
            var injectedConfig = new EmptyClass();
            var updatedConfig = new EmptyClass();
            var name = Guid.NewGuid().ToString();

            var action = () => SUT.UpdateConfig(injectedConfig, updatedConfig, name, Service<ILogger>());

            action.Should().NotThrow();
        }

        [Fact]
        public void UpdateConfigTakesNoActionWhenInjectedConfigIsNull()
        {
            FirstConfig? injectedConfig = null;
            var updatedConfig = new FirstConfig();
            var name = Guid.NewGuid().ToString();

            var action = () => SUT.UpdateConfig(injectedConfig, updatedConfig, name, Service<ILogger>());

            action.Should().NotThrow();
        }

        [Fact]
        public void UpdateConfigTakesNoActionWhenUpdatedConfigIsNull()
        {
            var injectedConfig = new FirstConfig();
            FirstConfig? updatedConfig = null;
            var name = Guid.NewGuid().ToString();

            var action = () => SUT.UpdateConfig(injectedConfig, updatedConfig, name, Service<ILogger>());

            action.Should().NotThrow();
        }

        private DefaultConfigUpdater SUT => GetSUT<DefaultConfigUpdater>();
    }
}