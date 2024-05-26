namespace Neovolve.Configuration.DependencyInjection.UnitTests.Comparison
{
    using FluentAssertions;
    using ModelBuilder;
    using Neovolve.Configuration.DependencyInjection.Comparison;

    public class NullChangeEvaluatorTests
    {
        [Fact]
        public void FindChangesReturnsChangeWhenOriginalValueIsNotNullAndNewValueIsNull()
        {
            var propertyPath = Guid.NewGuid().ToString();
            var originalValue = new object();
            var newValue = default(object);

            var sut = new NullChangeEvaluator();

            var actual = sut.FindChanges(propertyPath, originalValue, newValue,
                (_, _, _) => throw new NotImplementedException()).ToList();

            actual.Should().HaveCount(1);
            actual[0].PropertyPath.Should().Be(propertyPath);
            actual[0].FirstLogValue.Should().Be("not null");
            actual[0].SecondLogValue.Should().Be("null");
        }

        [Fact]
        public void FindChangesReturnsChangeWhenOriginalValueIsNullAndNewValueIsNotNull()
        {
            var propertyPath = Guid.NewGuid().ToString();
            var originalValue = default(object);
            var newValue = new object();

            var sut = new NullChangeEvaluator();

            var actual = sut.FindChanges(propertyPath, originalValue, newValue,
                (_, _, _) => throw new NotImplementedException()).ToList();

            actual.Should().HaveCount(1);
            actual[0].PropertyPath.Should().Be(propertyPath);
            actual[0].FirstLogValue.Should().Be("null");
            actual[0].SecondLogValue.Should().Be("not null");
        }

        [Fact]
        public void FindChangesReturnsEmptyWhenOriginalValueAndNewValueAreNull()
        {
            var propertyPath = Guid.NewGuid().ToString();
            var originalValue = default(object);
            var newValue = default(object);

            var sut = new NullChangeEvaluator();

            var actual = sut.FindChanges(propertyPath, originalValue, newValue,
                (_, _, _) => throw new NotImplementedException());

            actual.Should().BeEmpty();
        }

        [Fact]
        public void FindChangesReturnsNextWhenBothValuesAreNotNull()
        {
            var propertyPath = Guid.NewGuid().ToString();
            var originalValue = new object();
            var newValue = new object();
            var expected = Model.Create<List<IdentifiedChange>>();

            var sut = new NullChangeEvaluator();

            var actual = sut.FindChanges(propertyPath, originalValue, newValue,
                (propPath, original, updated) =>
                {
                    propPath.Should().Be(propertyPath);
                    original.Should().Be(originalValue);
                    updated.Should().Be(newValue);

                    return expected;
                }).ToList();

            actual.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void OrderReturnsZero()
        {
            var sut = new NullChangeEvaluator();

            sut.Order.Should().Be(0);
        }
    }
}