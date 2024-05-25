namespace Neovolve.Configuration.DependencyInjection.UnitTests.Comparison
{
    using FluentAssertions;
    using Neovolve.Configuration.DependencyInjection.Comparison;

    public class EquatableChangeEvaluatorTests
    {
        [Fact]
        public void OrderReturnsLessThanComparableChangeEvaluator()
        {
            var otherEvaluator = new ComparableChangeEvaluator();

            var sut = new EquatableChangeEvaluator();

            sut.Order.Should().BeLessThan(otherEvaluator.Order);
        }
    }
}