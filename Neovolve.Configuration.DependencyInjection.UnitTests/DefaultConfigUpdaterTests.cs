// ReSharper disable AccessToDisposedClosure

namespace Neovolve.Configuration.DependencyInjection.UnitTests
{
    using FluentAssertions;
    using Microsoft.Extensions.Logging;
    using ModelBuilder;
    using Neovolve.Configuration.DependencyInjection.UnitTests.Models;
    using Neovolve.Logging.Xunit;
    using NSubstitute;

    public sealed class DefaultConfigUpdaterTests : TestsInternal
    {
        public DefaultConfigUpdaterTests(ITestOutputHelper output) : base(
            output.BuildLoggerFor<DefaultConfigUpdater>(LogLevel.Trace))
        {
        }

        [Fact]
        public void ThrowsExceptionWithNullOptions()
        {
            var action = () => new DefaultConfigUpdater(null!);

            action.Should().Throw<ArgumentNullException>();
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
        [InlineData(LogLevel.Critical)]
        [InlineData(LogLevel.Debug)]
        [InlineData(LogLevel.Error)]
        [InlineData(LogLevel.Information)]
        [InlineData(LogLevel.Trace)]
        [InlineData(LogLevel.Warning)]
        public void UpdateConfigDoesNotLogChangesToPropertyWithLevelOptionWhenDisabled(LogLevel logLevel)
        {
            var injectedConfig = Model.Create<SimpleType>();
            var updatedConfig = Model.Create<SimpleType>();
            var name = Guid.NewGuid().ToString();

            Use(new ConfigureWithOptions
            {
                LogPropertyChangeLevel = LogLevel.None
            });

            var logger = Service<ICacheLogger<DefaultConfigUpdater>>();

            SUT.UpdateConfig(injectedConfig, updatedConfig, name, logger);

            logger.Entries.Should().NotContain(x => x.EventId.Id == 5000 && x.LogLevel == logLevel);
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
        [InlineData(LogLevel.Critical)]
        [InlineData(LogLevel.Debug)]
        [InlineData(LogLevel.Error)]
        [InlineData(LogLevel.Information)]
        [InlineData(LogLevel.Trace)]
        [InlineData(LogLevel.Warning)]
        public void UpdateConfigIgnoresLogForReadOnlyPropertyWhenNoLoggerProvided(LogLevel logLevel)
        {
            var injectedConfig = Model.Create<ReadOnlyType<SimpleType>>();
            var updatedConfig = Model.Create<ReadOnlyType<SimpleType>>();
            var name = Guid.NewGuid().ToString();

            Use(new ConfigureWithOptions
            {
                LogReadOnlyPropertyType = LogReadOnlyPropertyType.All,
                LogReadOnlyPropertyLevel = logLevel
            });

            var action = () => SUT.UpdateConfig(injectedConfig, updatedConfig, name, null);

            action.Should().NotThrow();
        }

        [Theory]
        [InlineData(LogLevel.Critical)]
        [InlineData(LogLevel.Debug)]
        [InlineData(LogLevel.Error)]
        [InlineData(LogLevel.Information)]
        [InlineData(LogLevel.Trace)]
        [InlineData(LogLevel.Warning)]
        public void UpdateConfigLogsChangesToPropertyWithLevelOptionWhenEnabled(LogLevel logLevel)
        {
            var injectedConfig = Model.Create<SimpleType>();
            var updatedConfig = Model.Create<SimpleType>();
            var name = Guid.NewGuid().ToString();

            Use(new ConfigureWithOptions
            {
                LogPropertyChangeLevel = logLevel
            });

            var logger = Service<ICacheLogger<DefaultConfigUpdater>>();

            SUT.UpdateConfig(injectedConfig, updatedConfig, name, logger);

            logger.Entries.Should().Contain(x => x.EventId.Id == 5000 && x.LogLevel == logLevel);
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
        [InlineData(LogReadOnlyPropertyType.ValueTypesOnly, false)]
        [InlineData(LogReadOnlyPropertyType.None, false)]
        public void UpdateConfigLogsChangesToReadOnlyReferenceTypeWhenEnabled(LogReadOnlyPropertyType option,
            bool logWritten)
        {
            var injectedConfig = Model.Create<ReadOnlyType<SimpleType>>();
            var updatedConfig = Model.Create<ReadOnlyType<SimpleType>>();
            var name = Guid.NewGuid().ToString();

            Use(new ConfigureWithOptions
            {
                LogReadOnlyPropertyType = option,
                LogReadOnlyPropertyLevel = LogLevel.Warning
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
        public void UpdateConfigLogsChangesToReadOnlyStringWhenEnabled(LogReadOnlyPropertyType option, bool logWritten)
        {
            var injectedConfig = Model.Create<ReadOnlyType<string>>();
            var updatedConfig = Model.Create<ReadOnlyType<string>>();
            var name = Guid.NewGuid().ToString();

            Use(new ConfigureWithOptions
            {
                LogReadOnlyPropertyType = option,
                LogReadOnlyPropertyLevel = LogLevel.Warning
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
        public void UpdateConfigLogsChangesToReadOnlyValueTypeWhenEnabled(LogReadOnlyPropertyType option,
            bool logWritten)
        {
            var injectedConfig = Model.Create<ReadOnlyType<int>>();
            var updatedConfig = Model.Create<ReadOnlyType<int>>();
            var name = Guid.NewGuid().ToString();

            Use(new ConfigureWithOptions
            {
                LogReadOnlyPropertyType = option,
                LogReadOnlyPropertyLevel = LogLevel.Warning
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
        [InlineData(null)]
        [InlineData("injected value")]
        public void UpdateConfigRetainsPropertyValueWhenBothValuesAreSame(string? value)
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

        [Fact]
        public void UpdateConfigLogsValueTypeChange()
        {
            var injectedConfig = new ValueOnlyType
            {
                Number = 1
            };
            var updatedConfig = new ValueOnlyType
            {
                Number = 2
            };
            var name = Guid.NewGuid().ToString();

            Use(new ConfigureWithOptions
            {
                LogPropertyChangeLevel = LogLevel.Information
            });

            var logger = Service<ICacheLogger<DefaultConfigUpdater>>();

            SUT.UpdateConfig(injectedConfig, updatedConfig, name, logger);

            // The value was copied and the change was logged.
            injectedConfig.Number.Should().Be(2);
            logger.Entries.Should().Contain(x => x.EventId.Id == 5000);
        }

        [Fact]
        public void UpdateConfigLogsNestedCollectionElementChangesWhenDeep()
        {
            var injectedConfig = new DeepRoot();
            injectedConfig.Items.Add(new DeepItem { Value = "a" });
            var updatedConfig = new DeepRoot();
            updatedConfig.Items.Add(new DeepItem { Value = "b" });
            var name = Guid.NewGuid().ToString();

            Use(new ConfigureWithOptions
            {
                LogPropertyChangeLevel = LogLevel.Information,
                NestedChangeLogging = NestedChangeLogging.Deep
            });

            var logger = Service<ICacheLogger<DefaultConfigUpdater>>();

            SUT.UpdateConfig(injectedConfig, updatedConfig, name, logger);

            // The element field change is logged with a nested property path.
            logger.Entries.Should().Contain(x => x.EventId.Id == 5000 && x.Message.Contains("Items[0].Value"));
        }

        [Fact]
        public void UpdateConfigDoesNotLogNestedCollectionElementChangesWhenSummary()
        {
            var injectedConfig = new DeepRoot();
            injectedConfig.Items.Add(new DeepItem { Value = "a" });
            var updatedConfig = new DeepRoot();
            updatedConfig.Items.Add(new DeepItem { Value = "b" });
            var name = Guid.NewGuid().ToString();

            Use(new ConfigureWithOptions
            {
                LogPropertyChangeLevel = LogLevel.Information,
                NestedChangeLogging = NestedChangeLogging.Summary
            });

            var logger = Service<ICacheLogger<DefaultConfigUpdater>>();

            SUT.UpdateConfig(injectedConfig, updatedConfig, name, logger);

            // In summary mode the unchanged entry count means nothing about the collection is logged.
            logger.Entries.Should().NotContain(x => x.Message.Contains("Items"));
        }

        [Fact]
        public void UpdateConfigDoesNotCopySkippedProperty()
        {
            var injectedConfig = new SkipPropertyType
            {
                Kept = "kept-old",
                Ignored = "ignored-old"
            };
            var updatedConfig = new SkipPropertyType
            {
                Kept = "kept-new",
                Ignored = "ignored-new"
            };
            var name = Guid.NewGuid().ToString();

            SUT.UpdateConfig(injectedConfig, updatedConfig, name, Service<ILogger>());

            // The kept property is copied; the [SkipConfigProperty] property is left unchanged.
            injectedConfig.Kept.Should().Be("kept-new");
            injectedConfig.Ignored.Should().Be("ignored-old");
        }

        private DefaultConfigUpdater SUT => GetSUT<DefaultConfigUpdater>();
    }
}