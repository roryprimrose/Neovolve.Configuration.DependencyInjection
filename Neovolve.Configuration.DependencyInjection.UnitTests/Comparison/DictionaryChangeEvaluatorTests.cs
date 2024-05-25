namespace Neovolve.Configuration.DependencyInjection.UnitTests.Comparison
{
    using System.Collections;
    using System.ComponentModel;
    using System.Globalization;
    using FluentAssertions;
    using Microsoft.Extensions.Logging;
    using ModelBuilder;
    using Neovolve.Configuration.DependencyInjection.Comparison;
    using Neovolve.Logging.Xunit;
    using NSubstitute;
    using Xunit.Abstractions;

    public class DictionaryChangeEvaluatorTests
    {
        private readonly ICacheLogger _logger;

        public DictionaryChangeEvaluatorTests(ITestOutputHelper output)
        {
            _logger = output.BuildLogger();
        }

        [Fact]
        public void FindChangesReturnsChangeWhenKeyAddedAndRemovedAndCountMatches()
        {
            var propertyPath = "MyProp";
            var keyAdded = Guid.NewGuid().ToString();
            var originalValue = Model.Create<Dictionary<string, int>>();
            var keyRemoved = originalValue.Keys.Last();
            var newValue = new Dictionary<string, int>(originalValue);

            newValue.Remove(keyRemoved);
            newValue[keyAdded] = Environment.TickCount;

            var sut = new DictionaryChangeEvaluator();

            var actual = sut.FindChanges(propertyPath, originalValue, newValue,
                (_, _, _) => []).ToList();

            actual.Should().HaveCount(1);

            var change = actual[0];

            _logger.LogInformation(change.MessageFormat, originalValue.GetType(), propertyPath, change.FirstLogValue,
                change.SecondLogValue);

            actual[0].PropertyPath.Should().Be(propertyPath);
            actual[0].FirstLogValue.Should().Be(keyRemoved);
            actual[0].SecondLogValue.Should().Be(keyAdded);
        }

        [Fact]
        public void FindChangesReturnsChangeWhenMultipleKeysAddedAndRemovedAndCountMatches()
        {
            const int keyChangeCount = 3;
            var propertyPath = "MyProp";
            var originalValue = Model.Create<Dictionary<string, int>>();
            var originalKeys = originalValue.Keys.ToList();
            var newValue = new Dictionary<string, int>(originalValue);
            var keysAdded = new List<string>();
            var keysRemoved = new List<string>();
            var keysChanged = Math.Min(keyChangeCount, originalValue.Count);

            for (var index = 0; index < keysChanged; index++)
            {
                var keyToRemove = originalKeys[index];
                var keyToAdd = Guid.NewGuid().ToString();

                newValue.Remove(keyToRemove);
                keysRemoved.Add(keyToRemove);
                newValue[keyToAdd] = Environment.TickCount;
                keysAdded.Add(keyToAdd);
            }

            var sut = new DictionaryChangeEvaluator();

            var actual = sut.FindChanges(propertyPath, originalValue, newValue,
                (_, _, _) => []).ToList();

            actual.Should().HaveCount(1);

            var change = actual[0];

            _logger.LogInformation(change.MessageFormat, originalValue.GetType(), propertyPath, change.FirstLogValue,
                change.SecondLogValue);

            actual[0].PropertyPath.Should().Be(propertyPath);
            actual[0].FirstLogValue.Should().Be(string.Join(", ", keysRemoved));
            actual[0].SecondLogValue.Should().Be(string.Join(", ", keysAdded));
        }

        [Fact]
        public void FindChangesReturnsEmptyWhenDictionariesAreEmpty()
        {
            var propertyPath = "MyProp";
            var originalValue = new Dictionary<string, int>();
            var newValue = new Dictionary<string, int>();

            var sut = new DictionaryChangeEvaluator();

            var actual = sut.FindChanges(propertyPath, originalValue, newValue,
                (_, _, _) => []).ToList();

            actual.Should().BeEmpty();
        }

        [Fact]
        public void FindChangesReturnsEmptyWhenNextReturnsEmpty()
        {
            var propertyPath = "MyProp";
            var firstValue = Model.Create<Dictionary<string, int>>();
            var secondValue = new Dictionary<string, int>(firstValue);
            var nextCallCount = 0;

            var sut = new DictionaryChangeEvaluator();

            var actual = sut.FindChanges(propertyPath, firstValue, secondValue,
                (path, first, second) =>
                {
                    firstValue.Should().ContainValue((int)first!);
                    var key = firstValue.First(x => x.Value == (int)first!).Key;

                    secondValue[key].Should().Be((int)second!);
                    path.Should().Be(propertyPath + $"[{key}]");
                    nextCallCount++;

                    return [];
                }).ToList();

            nextCallCount.Should().Be(firstValue.Count);
            actual.Should().BeEmpty();
        }

        [Fact]
        public void FindChangesReturnsNextPerEntryWhenCountsAndKeysMatch()
        {
            var propertyPath = "MyProp";
            var firstValue = Model.Create<Dictionary<string, int>>();
            var secondValue = new Dictionary<string, int>(firstValue);
            var nextCallCount = 0;
            var results = new List<IdentifiedChange>(firstValue.Count);

            var sut = new DictionaryChangeEvaluator();

            var actual = sut.FindChanges(propertyPath, firstValue, secondValue,
                (path, first, second) =>
                {
                    firstValue.Should().ContainValue((int)first!);
                    var key = firstValue.First(x => x.Value == (int)first!).Key;

                    secondValue[key].Should().Be((int)second!);
                    path.Should().Be(propertyPath + $"[{key}]");
                    nextCallCount++;

                    var entry = Model.Create<IdentifiedChange>();

                    results.Add(entry);

                    return [entry];
                }).ToList();

            nextCallCount.Should().Be(firstValue.Count);
            actual.Should().BeEquivalentTo(results);
        }

        [Fact]
        public void FindChangesReturnsNextPerEntryWithConvertibleKeysWhenCountsAndKeysMatch()
        {
            var propertyPath = "MyProp";
            var firstValue = Model.Create<Dictionary<Guid, int>>();
            var secondValue = new Dictionary<Guid, int>(firstValue);
            var nextCallCount = 0;
            var results = new List<IdentifiedChange>(firstValue.Count);

            var sut = new DictionaryChangeEvaluator();

            var actual = sut.FindChanges(propertyPath, firstValue, secondValue,
                (path, first, second) =>
                {
                    firstValue.Should().ContainValue((int)first!);
                    var key = firstValue.First(x => x.Value == (int)first!).Key;

                    secondValue[key].Should().Be((int)second!);
                    path.Should().Be(propertyPath + $"[{key}]");
                    nextCallCount++;

                    var entry = Model.Create<IdentifiedChange>();

                    results.Add(entry);

                    return [entry];
                }).ToList();

            nextCallCount.Should().Be(firstValue.Count);
            actual.Should().BeEquivalentTo(results);
        }

        [Fact]
        public void FindChangesReturnsNextWhenCountMatchesButNewValueKeysNotCompatibleWithStrings()
        {
            var propertyPath = "MyProp";
            var results = Model.Create<List<IdentifiedChange>>();
            var data = Model.Create<InvalidType>();

            var originalValue = new Dictionary<string, int>
            {
                {
                    Guid.NewGuid().ToString(),
                    Environment.TickCount
                }
            };
            var newValue = new Dictionary<InvalidType, int>
            {
                {
                    data,
                    Environment.TickCount
                }
            };

            var sut = new DictionaryChangeEvaluator();

            var actual = sut.FindChanges(propertyPath, originalValue, newValue, (prop, original, updated) =>
            {
                prop.Should().Be(propertyPath);
                original.Should().Be(originalValue);
                updated.Should().Be(newValue);

                return results;
            }).ToList();

            actual.Should().BeEquivalentTo(results);
        }

        [Fact]
        public void FindChangesReturnsNextWhenCountMatchesButOriginalValueKeysNotCompatibleWithStrings()
        {
            var propertyPath = "MyProp";
            var results = Model.Create<List<IdentifiedChange>>();
            var data = Model.Create<InvalidType>();

            var originalValue = new Dictionary<InvalidType, int>
            {
                {
                    data,
                    Environment.TickCount
                }
            };
            var newValue = new Dictionary<string, int>
            {
                {
                    Guid.NewGuid().ToString(),
                    Environment.TickCount
                }
            };

            var sut = new DictionaryChangeEvaluator();

            var actual = sut.FindChanges(propertyPath, originalValue, newValue, (prop, original, updated) =>
            {
                prop.Should().Be(propertyPath);
                original.Should().Be(originalValue);
                updated.Should().Be(newValue);

                return results;
            }).ToList();

            actual.Should().BeEquivalentTo(results);
        }

        [Fact]
        public void FindChangesWhenOriginalValueHasDifferentCountReturnsIdentifiedChangeWithCountDifference()
        {
            var propertyPath = "MyProp";
            var originalValue = Substitute.For<IDictionary>();
            var newValue = Substitute.For<IDictionary>();
            originalValue.Count.Returns(5);
            newValue.Count.Returns(10);

            var sut = new DictionaryChangeEvaluator();

            var actual = sut.FindChanges(propertyPath, originalValue, newValue, (_, _, _) => []).ToList();

            actual.Should().HaveCount(1);

            var change = actual[0];

            _logger.LogInformation(change.MessageFormat, originalValue.GetType(), propertyPath, change.FirstLogValue,
                change.SecondLogValue);

            actual[0].PropertyPath.Should().Be(propertyPath);
            actual[0].FirstLogValue.Should().Be("5");
            actual[0].SecondLogValue.Should().Be("10");
        }

        [Fact]
        public void OrderReturnsGreaterThanReferenceChangeEvaluator()
        {
            var evaluator = new ReferenceChangeEvaluator();

            var sut = new DictionaryChangeEvaluator();

            sut.Order.Should().Be(evaluator.Order + 1);
        }
    }

    [TypeConverter(typeof(InvalidTypeConverter))]
    internal class InvalidType
    {
        public Guid Id { get; set; }
    }

    internal class InvalidTypeConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
        {
            // Disallow conversion from string
            if (sourceType == typeof(string))
            {
                return false;
            }

            return base.CanConvertFrom(context, sourceType);
        }

        public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType)
        {
            // Disallow conversion to string
            if (destinationType == typeof(string))
            {
                return false;
            }

            return base.CanConvertTo(context, destinationType);
        }

        public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
        {
            if (value is string)
            {
                throw new NotSupportedException("Conversion from string is not supported.");
            }

            return base.ConvertFrom(context, culture, value);
        }

        public override object? ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value,
            Type destinationType)
        {
            if (destinationType == typeof(string))
            {
                throw new NotSupportedException("Conversion to string is not supported.");
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}