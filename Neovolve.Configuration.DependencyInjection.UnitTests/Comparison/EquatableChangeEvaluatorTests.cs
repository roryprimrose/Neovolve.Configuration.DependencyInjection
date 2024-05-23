using Neovolve.Configuration.DependencyInjection.Comparison;

namespace Neovolve.Configuration.DependencyInjection.UnitTests.Comparison
{
    using FluentAssertions;

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