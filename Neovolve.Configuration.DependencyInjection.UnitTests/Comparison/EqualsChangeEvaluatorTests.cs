namespace Neovolve.Configuration.DependencyInjection.UnitTests.Comparison
{
    using System;
    using System.Globalization;
    using System.Linq;
    using Divergic.Logging.Xunit;
    using FluentAssertions;
    using Microsoft.Extensions.Logging;
    using Neovolve.Configuration.DependencyInjection.Comparison;
    using Xunit.Abstractions;

    public class EqualsChangeEvaluatorTests
    {
        private readonly ICacheLogger _logger;

        public EqualsChangeEvaluatorTests(ITestOutputHelper output)
        {
            _logger = output.BuildLogger();
        }

        [Fact]
        public void FindChangesReturnsChangeWhenNewValueIsNullAndOriginalValueIsNotNull()
        {
            var propertyPath = "MyProp";
            var originalValue = Guid.NewGuid().ToString();
            string? newValue = null;
            var next = new NextFindChanges((_, _, _) => throw new InvalidOperationException());

            var sut = new EqualsChangeEvaluator();

            var actual = sut.FindChanges(propertyPath, originalValue, newValue, next).ToList();

            actual.Should().HaveCount(1);

            var change = actual[0];

            _logger.LogInformation(change.MessageFormat, GetType(), propertyPath, change.FirstLogValue,
                change.SecondLogValue);

            change.PropertyPath.Should().Be(propertyPath);
            change.FirstLogValue.Should().Be(originalValue.ToString(CultureInfo.CurrentCulture));
            change.SecondLogValue.Should().Be("null");
        }

        [Fact]
        public void FindChangesReturnsChangeWhenOriginalValueIsNullAndNewValueIsNotNull()
        {
            var propertyPath = "MyProp";
            string? originalValue = null;
            var newValue = Guid.NewGuid().ToString();
            var next = new NextFindChanges((_, _, _) => throw new InvalidOperationException());

            var sut = new EqualsChangeEvaluator();

            var actual = sut.FindChanges(propertyPath, originalValue, newValue, next).ToList();

            actual.Should().HaveCount(1);

            var change = actual[0];

            _logger.LogInformation(change.MessageFormat, GetType(), propertyPath, change.FirstLogValue,
                change.SecondLogValue);

            change.PropertyPath.Should().Be(propertyPath);
            change.FirstLogValue.Should().Be("null");
            change.SecondLogValue.Should().Be(newValue.ToString(CultureInfo.CurrentCulture));
        }

        [Fact]
        public void FindChangesReturnsChangeWhenValuesAreNotEqual()
        {
            var propertyPath = "MyProp";
            var originalValue = Guid.NewGuid().ToString();
            var newValue = Guid.NewGuid().ToString();
            var next = new NextFindChanges((_, _, _) => throw new InvalidOperationException());

            var sut = new EqualsChangeEvaluator();

            var actual = sut.FindChanges(propertyPath, originalValue, newValue, next).ToList();

            actual.Should().HaveCount(1);

            var change = actual[0];

            _logger.LogInformation(change.MessageFormat, originalValue.GetType(), propertyPath, change.FirstLogValue,
                change.SecondLogValue);

            change.PropertyPath.Should().Be(propertyPath);
            change.FirstLogValue.Should().Be(originalValue.ToString(CultureInfo.CurrentCulture));
            change.SecondLogValue.Should().Be(newValue.ToString(CultureInfo.CurrentCulture));
        }

        [Fact]
        public void FindChangesReturnsEmptyWhenValuesAreEqual()
        {
            var propertyPath = "MyProp";
            var originalValue = Guid.NewGuid().ToString();
            var newValue = originalValue;
            var next = new NextFindChanges((_, _, _) => throw new InvalidOperationException());

            var sut = new EqualsChangeEvaluator();

            var actual = sut.FindChanges(propertyPath, originalValue, newValue, next);

            actual.Should().BeEmpty();
        }

        [Fact]
        public void OrderReturnsMaxValue()
        {
            var sut = new EqualsChangeEvaluator();

            sut.Order.Should().Be(int.MaxValue);
        }
    }
}