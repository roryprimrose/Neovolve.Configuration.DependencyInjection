namespace Neovolve.Configuration.DependencyInjection.UnitTests.Comparison
{
    using System;
    using System.Linq;
    using Divergic.Logging.Xunit;
    using FluentAssertions;
    using Microsoft.Extensions.Logging;
    using Neovolve.Configuration.DependencyInjection.Comparison;
    using Xunit.Abstractions;

    public class ComparableChangeEvaluatorTests
    {
        private readonly ICacheLogger _logger;

        public ComparableChangeEvaluatorTests(ITestOutputHelper output)
        {
            _logger = output.BuildLogger();
        }

        [Fact]
        public void FindChangesReturnsChangeWhenValuesDoNotMatch()
        {
            var propertyPath = Guid.NewGuid().ToString();
            var originalValue = 123;
            var newValue = 124;
            var nextCalled = false;

            var sut = new ComparableChangeEvaluator();

            var actual = sut.FindChanges(propertyPath, originalValue, newValue, (_, _, _) =>
            {
                nextCalled = true;

                return [];
            }).ToList();
            
            actual.Should().HaveCount(1);

            var change = actual[0];

            _logger.LogInformation(change.MessageFormat, originalValue.GetType(), propertyPath, change.FirstLogValue,
                change.SecondLogValue);

            change.PropertyPath.Should().Be(propertyPath);
            change.FirstLogValue.Should().Be(originalValue.ToString());
            change.SecondLogValue.Should().Be(newValue.ToString());
            nextCalled.Should().BeFalse();
        }

        [Fact]
        public void FindChangesReturnsEmptyWhenValuesMatch()
        {
            var originalValue = 123;
            var newValue = 123;
            var nextCalled = false;

            var sut = new ComparableChangeEvaluator();

            var actual = sut.FindChanges("Test", originalValue, newValue, (_, _, _) =>
            {
                nextCalled = true;

                return [];
            });

            actual.Should().BeEmpty();
            nextCalled.Should().BeFalse();
        }

        [Fact]
        public void OrderReturnsGreaterThanEquatableChangeEvaluator()
        {
            var otherEvaluator = new EquatableChangeEvaluator();

            var sut = new ComparableChangeEvaluator();

            sut.Order.Should().BeGreaterThan(otherEvaluator.Order);
        }

        [Fact]
        public void OrderReturnsLessThanEqualsChangeEvaluator()
        {
            var otherEvaluator = new EqualsChangeEvaluator();

            var sut = new ComparableChangeEvaluator();

            sut.Order.Should().BeLessThan(otherEvaluator.Order);
        }
    }
}