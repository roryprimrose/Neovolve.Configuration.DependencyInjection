namespace Neovolve.Configuration.DependencyInjection.UnitTests.Comparison
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using Divergic.Logging.Xunit;
    using FluentAssertions;
    using Microsoft.Extensions.Logging;
    using ModelBuilder;
    using Neovolve.Configuration.DependencyInjection.Comparison;
    using Xunit.Abstractions;

    public class CollectionChangeEvaluatorTests
    {
        private readonly ICacheLogger _logger;

        public CollectionChangeEvaluatorTests(ITestOutputHelper output)
        {
            _logger = output.BuildLogger();
        }

        [Fact]
        public void FindChangesReturnsChangeWhenCountsDiffer()
        {
            var propertyPath = "MyProp";
            var firstValue = Model.Create<List<string>>();
            var secondValue = new List<string>(firstValue).Set(x => x.Add(Guid.NewGuid().ToString()));

            var sut = new CollectionChangeEvaluator();

            var actual = sut.FindChanges(propertyPath, firstValue, secondValue,
                (path, first, second) => throw new NotImplementedException()).ToList();

            actual.Should().HaveCount(1);

            var change = actual[0];

            _logger.LogInformation(change.MessageFormat, firstValue.GetType(), propertyPath, change.FirstLogValue,
                change.SecondLogValue);

            change.PropertyPath.Should().Be(propertyPath);
            change.FirstLogValue.Should().Be(firstValue.Count.ToString(CultureInfo.CurrentCulture));
            change.SecondLogValue.Should().Be(secondValue.Count.ToString(CultureInfo.CurrentCulture));
        }

        [Fact]
        public void FindChangesReturnsEmptyWhenCollectionsAreEmpty()
        {
            var propertyPath = "MyProp";
            var firstValue = Array.Empty<string>();
            var secondValue = Array.Empty<string>();

            var sut = new CollectionChangeEvaluator();

            var actual = sut.FindChanges(propertyPath, firstValue, secondValue,
                (_, _, _) => []).ToList();

            actual.Should().BeEmpty();
        }

        [Fact]
        public void FindChangesReturnsEmptyWhenNextReturnsEmpty()
        {
            var propertyPath = "MyProp";
            var firstValue = Model.Create<List<string>>();
            var secondValue = new List<string>(firstValue);
            var nextCallCount = 0;

            var sut = new CollectionChangeEvaluator();

            var actual = sut.FindChanges(propertyPath, firstValue, secondValue,
                (path, first, second) =>
                {
                    first.Should().Be(firstValue[nextCallCount]);
                    second.Should().Be(secondValue[nextCallCount]);
                    path.Should().Be(propertyPath + $"[{nextCallCount}]");
                    nextCallCount++;

                    return [];
                }).ToList();

            nextCallCount.Should().Be(firstValue.Count);
            actual.Should().BeEmpty();
        }

        [Fact]
        public void FindChangesReturnsNextWhenCountsMatch()
        {
            var propertyPath = "MyProp";
            var firstValue = Model.Create<List<string>>();
            var secondValue = new List<string>(firstValue);
            var nextCallCount = 0;
            var results = new List<IdentifiedChange>(firstValue.Count);

            var sut = new CollectionChangeEvaluator();

            var actual = sut.FindChanges(propertyPath, firstValue, secondValue,
                (path, first, second) =>
                {
                    first.Should().Be(firstValue[nextCallCount]);
                    second.Should().Be(secondValue[nextCallCount]);
                    path.Should().Be(propertyPath + $"[{nextCallCount}]");
                    nextCallCount++;

                    var entry = Model.Create<IdentifiedChange>();

                    results.Add(entry);

                    return [entry];
                }).ToList();

            nextCallCount.Should().Be(firstValue.Count);
            actual.Should().BeEquivalentTo(results);
        }

        [Fact]
        public void OrderReturnsGreaterThanDictionaryChangeEvaluator()
        {
            var evaluator = new DictionaryChangeEvaluator();

            var sut = new CollectionChangeEvaluator();

            sut.Order.Should().Be(evaluator.Order + 1);
        }
    }
}