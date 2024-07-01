namespace Neovolve.Configuration.DependencyInjection.UnitTests.ScenarioTests
{
    using System.Collections.ObjectModel;
    using FluentAssertions;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using ModelBuilder;
    using Neovolve.Configuration.DependencyInjection.Comparison;
    using Neovolve.Configuration.DependencyInjection.UnitTests.Models;
    using Xunit.Abstractions;

    public class ValueProcessorTests
    {
        private readonly ITestOutputHelper _output;

        public ValueProcessorTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void FindChangesReturnsChangeWhenCollectionAddsValue()
        {
            var propertyPath = Guid.NewGuid().ToString();
            var originalValue = Model.Create<Collection<Guid>>();
            var newValue = originalValue.ToList().Set(x => x.Add(Guid.NewGuid()));

            var changes = SUT.FindChanges(propertyPath, originalValue, newValue).ToList();

            changes.Should().HaveCount(1);

            var logger = _output.BuildLoggerFor<ValueProcessorTests>();

            foreach (var change in changes)
            {
                logger.LogInformation(change.MessageFormat, GetType(), change.PropertyPath, change.FirstLogValue,
                    change.SecondLogValue);
            }
        }

        [Fact]
        public void FindChangesReturnsChangeWhenCollectionRemovesValue()
        {
            var propertyPath = Guid.NewGuid().ToString();
            var originalValue = Model.Create<Collection<Guid>>();
            var newValue = originalValue.ToList().Set(x => x.RemoveAt(0));

            var changes = SUT.FindChanges(propertyPath, originalValue, newValue).ToList();

            changes.Should().HaveCount(1);

            var logger = _output.BuildLoggerFor<ValueProcessorTests>();

            foreach (var change in changes)
            {
                logger.LogInformation(change.MessageFormat, GetType(), change.PropertyPath, change.FirstLogValue,
                    change.SecondLogValue);
            }
        }

        [Fact]
        public void FindChangesReturnsChangeWhenCollectionValueChanges()
        {
            var propertyPath = Guid.NewGuid().ToString();
            var originalValue = Model.Create<Collection<Guid>>();
            var newValue = originalValue.ToList().Set(x => x[^1] = Guid.NewGuid());

            var changes = SUT.FindChanges(propertyPath, originalValue, newValue).ToList();

            changes.Should().HaveCount(1);

            var logger = _output.BuildLoggerFor<ValueProcessorTests>();

            foreach (var change in changes)
            {
                logger.LogInformation(change.MessageFormat, GetType(), change.PropertyPath, change.FirstLogValue,
                    change.SecondLogValue);
            }
        }

        [Fact]
        public void FindChangesReturnsChangeWhenDictionaryAddsValue()
        {
            var propertyPath = Guid.NewGuid().ToString();
            var originalValue = Model.Create<Dictionary<string, DateTimeOffset>>();
            var newValue = originalValue.ToDictionary(x => x.Key, x => x.Value)
                .Set(x => x.Add(Guid.NewGuid().ToString(), Model.Create<DateTimeOffset>()));

            var changes = SUT.FindChanges(propertyPath, originalValue, newValue).ToList();

            changes.Should().HaveCount(1);

            var logger = _output.BuildLoggerFor<ValueProcessorTests>();

            foreach (var change in changes)
            {
                logger.LogInformation(change.MessageFormat, GetType(), change.PropertyPath, change.FirstLogValue,
                    change.SecondLogValue);
            }
        }

        [Fact]
        public void FindChangesReturnsChangeWhenDictionaryRemovesValue()
        {
            var propertyPath = Guid.NewGuid().ToString();
            var originalValue = Model.Create<Dictionary<string, DateTimeOffset>>();
            var newValue = originalValue.ToDictionary(x => x.Key, x => x.Value).Set(x => x.Remove(x.Keys.Last()));

            var changes = SUT.FindChanges(propertyPath, originalValue, newValue).ToList();

            changes.Should().HaveCount(1);

            var logger = _output.BuildLoggerFor<ValueProcessorTests>();

            foreach (var change in changes)
            {
                logger.LogInformation(change.MessageFormat, GetType(), change.PropertyPath, change.FirstLogValue,
                    change.SecondLogValue);
            }
        }

        [Fact]
        public void FindChangesReturnsChangeWhenDictionaryValueChanges()
        {
            var propertyPath = Guid.NewGuid().ToString();
            var originalValue = Model.Create<Dictionary<string, DateTimeOffset>>();
            var newValue = originalValue.ToDictionary(x => x.Key, x => x.Value)
                .Set(x => x[x.Keys.Last()] = Model.Create<DateTimeOffset>());

            var changes = SUT.FindChanges(propertyPath, originalValue, newValue).ToList();

            changes.Should().HaveCount(1);

            var logger = _output.BuildLoggerFor<ValueProcessorTests>();

            foreach (var change in changes)
            {
                logger.LogInformation(change.MessageFormat, GetType(), change.PropertyPath, change.FirstLogValue,
                    change.SecondLogValue);
            }
        }

        [Theory]
        [InlineData(null, "test")]
        [InlineData("test", null)]
        [InlineData("test", "different")]
        [InlineData(123, 543)]
        [InlineData(true, false)]
        public void FindChangesReturnsChangeWhenDifferentValueTypeValuesProvided(object? originalValue,
            object? newValue)
        {
            var propertyPath = Guid.NewGuid().ToString();

            var changes = SUT.FindChanges(propertyPath, originalValue, newValue).ToList();

            changes.Should().HaveCount(1);

            var logger = _output.BuildLoggerFor<ValueProcessorTests>();

            foreach (var change in changes)
            {
                logger.LogInformation(change.MessageFormat, GetType(), change.PropertyPath, change.FirstLogValue,
                    change.SecondLogValue);
            }
        }

        [Fact]
        public void FindChangesReturnsChangeWhenListAddsValue()
        {
            var propertyPath = Guid.NewGuid().ToString();
            var originalValue = Model.Create<List<string>>();
            var newValue = originalValue.ToList().Set(x => x.Add(Guid.NewGuid().ToString()));

            var changes = SUT.FindChanges(propertyPath, originalValue, newValue).ToList();

            changes.Should().HaveCount(1);

            var logger = _output.BuildLoggerFor<ValueProcessorTests>();

            foreach (var change in changes)
            {
                logger.LogInformation(change.MessageFormat, GetType(), change.PropertyPath, change.FirstLogValue,
                    change.SecondLogValue);
            }
        }

        [Fact]
        public void FindChangesReturnsChangeWhenListRemovesValue()
        {
            var propertyPath = Guid.NewGuid().ToString();
            var originalValue = Model.Create<List<string>>();
            var newValue = originalValue.ToList().Set(x => x.RemoveAt(0));

            var changes = SUT.FindChanges(propertyPath, originalValue, newValue).ToList();

            changes.Should().HaveCount(1);

            var logger = _output.BuildLoggerFor<ValueProcessorTests>();

            foreach (var change in changes)
            {
                logger.LogInformation(change.MessageFormat, GetType(), change.PropertyPath, change.FirstLogValue,
                    change.SecondLogValue);
            }
        }

        [Fact]
        public void FindChangesReturnsChangeWhenListValueChanges()
        {
            var propertyPath = Guid.NewGuid().ToString();
            var originalValue = Model.Create<List<string>>();
            var newValue = originalValue.ToList().Set(x => x[^1] = Guid.NewGuid().ToString());

            var changes = SUT.FindChanges(propertyPath, originalValue, newValue).ToList();

            changes.Should().HaveCount(1);

            var logger = _output.BuildLoggerFor<ValueProcessorTests>();

            foreach (var change in changes)
            {
                logger.LogInformation(change.MessageFormat, GetType(), change.PropertyPath, change.FirstLogValue,
                    change.SecondLogValue);
            }
        }

        [Fact]
        public void FindChangesReturnsChangeWhenReadOnlyCollectionAddsValue()
        {
            var propertyPath = Guid.NewGuid().ToString();
            var originalValue = Model.Create<ReadOnlyCollection<DateTimeOffset>>();
            var newValue = originalValue.ToList().Set(x => x.Add(Model.Create<DateTimeOffset>()));

            var changes = SUT.FindChanges(propertyPath, originalValue, newValue).ToList();

            changes.Should().HaveCount(1);

            var logger = _output.BuildLoggerFor<ValueProcessorTests>();

            foreach (var change in changes)
            {
                logger.LogInformation(change.MessageFormat, GetType(), change.PropertyPath, change.FirstLogValue,
                    change.SecondLogValue);
            }
        }

        [Fact]
        public void FindChangesReturnsChangeWhenReadOnlyCollectionRemovesValue()
        {
            var propertyPath = Guid.NewGuid().ToString();
            var originalValue = Model.Create<ReadOnlyCollection<DateTimeOffset>>();
            var newValue = originalValue.ToList().Set(x => x.RemoveAt(0));

            var changes = SUT.FindChanges(propertyPath, originalValue, newValue).ToList();

            changes.Should().HaveCount(1);

            var logger = _output.BuildLoggerFor<ValueProcessorTests>();

            foreach (var change in changes)
            {
                logger.LogInformation(change.MessageFormat, GetType(), change.PropertyPath, change.FirstLogValue,
                    change.SecondLogValue);
            }
        }

        [Fact]
        public void FindChangesReturnsChangeWhenReadOnlyCollectionValueChanges()
        {
            var propertyPath = Guid.NewGuid().ToString();
            var originalValue = Model.Create<ReadOnlyCollection<DateTimeOffset>>();
            var newValue = originalValue.ToList().Set(x => x[^1] = Model.Create<DateTimeOffset>());

            var changes = SUT.FindChanges(propertyPath, originalValue, newValue).ToList();

            changes.Should().HaveCount(1);

            var logger = _output.BuildLoggerFor<ValueProcessorTests>();

            foreach (var change in changes)
            {
                logger.LogInformation(change.MessageFormat, GetType(), change.PropertyPath, change.FirstLogValue,
                    change.SecondLogValue);
            }
        }

        [Fact]
        public void FindChangesReturnsChangeWhenReadOnlyDictionaryAddsValue()
        {
            var propertyPath = Guid.NewGuid().ToString();
            var originalValue = Model.Create<Dictionary<Guid, int>>().AsReadOnly();
            var newValue = originalValue.ToDictionary(x => x.Key, x => x.Value)
                .Set(x => x.Add(Guid.NewGuid(), Model.Create<int>())).AsReadOnly();

            var changes = SUT.FindChanges(propertyPath, originalValue, newValue).ToList();

            changes.Should().HaveCount(1);

            var logger = _output.BuildLoggerFor<ValueProcessorTests>();

            foreach (var change in changes)
            {
                logger.LogInformation(change.MessageFormat, GetType(), change.PropertyPath, change.FirstLogValue,
                    change.SecondLogValue);
            }
        }

        [Fact]
        public void FindChangesReturnsChangeWhenReadOnlyDictionaryRemovesValue()
        {
            var propertyPath = Guid.NewGuid().ToString();
            var originalValue = Model.Create<Dictionary<Guid, int>>().AsReadOnly();
            var newValue = originalValue.ToDictionary(x => x.Key, x => x.Value).Set(x => x.Remove(x.Keys.Last()))
                .AsReadOnly();

            var changes = SUT.FindChanges(propertyPath, originalValue, newValue).ToList();

            changes.Should().HaveCount(1);

            var logger = _output.BuildLoggerFor<ValueProcessorTests>();

            foreach (var change in changes)
            {
                logger.LogInformation(change.MessageFormat, GetType(), change.PropertyPath, change.FirstLogValue,
                    change.SecondLogValue);
            }
        }

        [Fact]
        public void FindChangesReturnsChangeWhenReadOnlyDictionaryValueChanges()
        {
            var propertyPath = Guid.NewGuid().ToString();
            var originalValue = Model.Create<Dictionary<Guid, int>>().AsReadOnly();
            var newValue = originalValue.ToDictionary(x => x.Key, x => x.Value)
                .Set(x => x[x.Keys.Last()] = Model.Create<int>()).AsReadOnly();

            var changes = SUT.FindChanges(propertyPath, originalValue, newValue).ToList();

            changes.Should().HaveCount(1);

            var logger = _output.BuildLoggerFor<ValueProcessorTests>();

            foreach (var change in changes)
            {
                logger.LogInformation(change.MessageFormat, GetType(), change.PropertyPath, change.FirstLogValue,
                    change.SecondLogValue);
            }
        }

        [Fact]
        public void FindChangesReturnsEmptyWhenCollectionHasSameValues()
        {
            var propertyPath = Guid.NewGuid().ToString();
            var originalValue = Model.Create<Collection<Guid>>();
            var newValue = originalValue.ToList();

            var changes = SUT.FindChanges(propertyPath, originalValue, newValue);

            changes.Should().BeEmpty();
        }

        [Fact]
        public void FindChangesReturnsEmptyWhenDictionaryHasSameValues()
        {
            var propertyPath = Guid.NewGuid().ToString();
            var originalValue = Model.Create<Dictionary<string, DateTimeOffset>>();
            var newValue = originalValue.ToList();

            var changes = SUT.FindChanges(propertyPath, originalValue, newValue);

            changes.Should().BeEmpty();
        }

        [Fact]
        public void FindChangesReturnsEmptyWhenListHasSameValues()
        {
            var propertyPath = Guid.NewGuid().ToString();
            var originalValue = Model.Create<List<string>>();
            var newValue = originalValue.ToList();

            var changes = SUT.FindChanges(propertyPath, originalValue, newValue);

            changes.Should().BeEmpty();
        }

        [Fact]
        public void FindChangesReturnsEmptyWhenReadOnlyCollectionHasSameValues()
        {
            var propertyPath = Guid.NewGuid().ToString();
            var originalValue = Model.Create<ReadOnlyCollection<DateTimeOffset>>();
            var newValue = originalValue.ToList();

            var changes = SUT.FindChanges(propertyPath, originalValue, newValue);

            changes.Should().BeEmpty();
        }

        [Fact]
        public void FindChangesReturnsEmptyWhenReadOnlyDictionaryHasSameValues()
        {
            var propertyPath = Guid.NewGuid().ToString();
            var originalValue = Model.Create<Dictionary<Guid, DateTimeOffset>>();
            var newValue = originalValue.ToList();

            var changes = SUT.FindChanges(propertyPath, originalValue, newValue);

            changes.Should().BeEmpty();
        }

        [Fact]
        public void FindChangesReturnsEmptyWhenSameInstanceCompared()
        {
            var propertyPath = Guid.NewGuid().ToString();
            var value = Model.Create<SimpleType>();

            var changes = SUT.FindChanges(propertyPath, value, value);

            changes.Should().BeEmpty();
        }

        [Theory]
        [InlineData(null, null)]
        [InlineData("abc", "abc")]
        [InlineData(123, 123)]
        [InlineData(true, true)]
        public void FindChangesReturnsEmptyWhenSameValueTypeValueProvided(object? originalValue, object? newValue)
        {
            var propertyPath = Guid.NewGuid().ToString();

            var changes = SUT.FindChanges(propertyPath, originalValue, newValue);

            changes.Should().BeEmpty();
        }

        private static IValueProcessor GetProcessor()
        {
            var services = new ServiceCollection().AddChangeTracking();

            using var provider = services.BuildServiceProvider();

            return provider.GetRequiredService<IValueProcessor>();
        }

        private IValueProcessor SUT { get; } = GetProcessor();
    }
}