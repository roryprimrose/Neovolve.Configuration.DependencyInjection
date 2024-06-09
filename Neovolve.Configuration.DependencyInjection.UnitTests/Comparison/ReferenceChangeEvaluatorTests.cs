namespace Neovolve.Configuration.DependencyInjection.UnitTests.Comparison
{
    using FluentAssertions;
    using ModelBuilder;
    using Neovolve.Configuration.DependencyInjection.Comparison;

    public class ReferenceChangeEvaluatorTests
    {
        [Fact]
        public void FindChangesReturnsEmptyWhenOriginalValueIsNewValue()
        {
            var propertyPath = Guid.NewGuid().ToString();
            var originalValue = new object();
            var newValue = originalValue;

            var sut = new ReferenceChangeEvaluator();

            var actual = sut.FindChanges(propertyPath, originalValue, newValue,
                (_, _, _) => throw new NotImplementedException());

            actual.Should().BeEmpty();
        }

        [Fact]
        public void FindChangesReturnsWhenWhenValuesAreNotSame()
        {
            var propertyPath = Guid.NewGuid().ToString();
            var originalValue = new object();
            var newValue = new object();
            var expected = Model.Create<List<IdentifiedChange>>();

            var sut = new ReferenceChangeEvaluator();

            var actual = sut.FindChanges(propertyPath, originalValue, newValue, (path, original, updated) =>
            {
                path.Should().Be(propertyPath);
                original.Should().Be(originalValue);
                updated.Should().Be(newValue);

                return expected;
            });

            actual.Should().BeEquivalentTo(expected);
        }
    }
}