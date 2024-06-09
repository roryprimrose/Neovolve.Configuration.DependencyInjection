namespace Neovolve.Configuration.DependencyInjection.UnitTests.Comparison
{
    using FluentAssertions;
    using Microsoft.Extensions.Logging;
    using ModelBuilder;
    using Neovolve.Configuration.DependencyInjection.Comparison;
    using Neovolve.Configuration.DependencyInjection.UnitTests.Models;
    using Neovolve.Logging.Xunit;
    using Xunit.Abstractions;

    public class EquatableChangeEvaluatorTests
    {
        private readonly ICacheLogger _logger;

        public EquatableChangeEvaluatorTests(ITestOutputHelper output)
        {
            _logger = output.BuildLogger();
        }

        [Fact]
        public void FindChangesCanInvokeMultipleTimesForDifferentTypes()
        {
            var propertyPath = "Test";
            var firstValue = Guid.NewGuid().ToString();
            var firstValueClone = (string)firstValue.Clone();
            var secondValue = Guid.NewGuid();
            var secondValueClone = Guid.Parse(secondValue.ToString());

            var sut = new EquatableChangeEvaluator();

            var firstActual = sut.FindChanges(propertyPath, firstValue, firstValueClone,
                (path, original, value) => throw new NotImplementedException());

            firstActual.Should().BeEmpty();

            var secondActual = sut.FindChanges(propertyPath, secondValue, secondValueClone,
                (path, original, value) => throw new NotImplementedException());

            secondActual.Should().BeEmpty();
        }

        [Fact]
        public void FindChangesCanInvokeMultipleTimesForSameType()
        {
            var propertyPath = "Test";
            var originalValue = Guid.NewGuid().ToString();
            var newValue = (string)originalValue.Clone();

            var sut = new EquatableChangeEvaluator();

            var firstActual = sut.FindChanges(propertyPath, originalValue, newValue,
                (path, original, value) => throw new NotImplementedException());

            firstActual.Should().BeEmpty();

            var secondActual = sut.FindChanges(propertyPath, originalValue, newValue,
                (path, original, value) => throw new NotImplementedException());

            secondActual.Should().BeEmpty();
        }

        [Fact]
        public void FindChangesReturnsEmptyWhenValuesAreEqual()
        {
            var propertyPath = "Test";
            var originalValue = Guid.NewGuid().ToString();
            var newValue = (string)originalValue.Clone();

            var sut = new EquatableChangeEvaluator();

            var actual = sut.FindChanges(propertyPath, originalValue, newValue,
                (path, original, value) => throw new NotImplementedException());

            actual.Should().BeEmpty();
        }

        [Fact]
        public void FindChangesReturnsNextWhenOriginalValueNotEquatable()
        {
            var propertyPath = "Test";
            var originalValue = new FirstConfig();
            var newValue = Guid.NewGuid().ToString();
            var results = Model.Create<List<IdentifiedChange>>();

            var sut = new EquatableChangeEvaluator();

            var actual = sut.FindChanges(propertyPath, originalValue, newValue,
                (path, original, updated) =>
                {
                    path.Should().Be(propertyPath);
                    original.Should().Be(originalValue);
                    updated.Should().Be(newValue);

                    return results;
                });

            actual.Should().BeEquivalentTo(results);
        }

        [Fact]
        public void FindChangesReturnsNextWhenUpdatedValueNotEquatable()
        {
            var propertyPath = "Test";
            var originalValue = Guid.NewGuid().ToString();
            var newValue = new FirstConfig();
            var results = Model.Create<List<IdentifiedChange>>();

            var sut = new EquatableChangeEvaluator();

            var actual = sut.FindChanges(propertyPath, originalValue, newValue,
                (path, original, updated) =>
                {
                    path.Should().Be(propertyPath);
                    original.Should().Be(originalValue);
                    updated.Should().Be(newValue);

                    return results;
                });

            actual.Should().BeEquivalentTo(results);
        }

        [Fact]
        public void FindChangesReturnsResultWhenValuesAreNotEqual()
        {
            var propertyPath = "Test";
            var originalValue = Guid.NewGuid().ToString();
            var newValue = Guid.NewGuid().ToString();

            var sut = new EquatableChangeEvaluator();

            var actual = sut.FindChanges(propertyPath, originalValue, newValue,
                (path, original, value) => throw new NotImplementedException()).ToList();

            actual.Should().NotBeEmpty();

            var change = actual[0];

            _logger.LogInformation(change.MessageFormat, originalValue.GetType(), propertyPath, change.FirstLogValue,
                change.SecondLogValue);

            change.FirstLogValue.Should().Contain(originalValue);
            change.SecondLogValue.Should().Contain(newValue);
            change.PropertyPath.Should().Be(propertyPath);
        }

        [Fact]
        public void IsFinalEvaluatorReturnsFalse()
        {
            var sut = new EquatableChangeEvaluator();

            sut.IsFinalEvaluator.Should().BeFalse();
        }

        [Fact]
        public void OrderReturnsLessThanComparableChangeEvaluator()
        {
            var otherEvaluator = new ComparableChangeEvaluator();

            var sut = new EquatableChangeEvaluator();

            sut.Order.Should().BeLessThan(otherEvaluator.Order);
        }
    }
}