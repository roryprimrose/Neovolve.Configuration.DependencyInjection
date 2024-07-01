namespace Neovolve.Configuration.DependencyInjection.UnitTests
{
    using FluentAssertions;
    using ModelBuilder;
    using Neovolve.Configuration.DependencyInjection.UnitTests.Models;

    public class SnapshotProxyTests
    {
        [Fact]
        public void GetReturnsValueFromSourceValue()
        {
            var name = Guid.NewGuid().ToString();
            var expected = Model.Create<SimpleType>();

            var source = new WrapperSnapshot<SimpleType>(expected);

            var sut = new SnapshotProxy<SimpleType, ISimpleType>(source);

            var actual = sut.Get(name);

            actual.Should().Be(expected);
        }

        [Fact]
        public void ValueReturnsFromSource()
        {
            var name = Guid.NewGuid().ToString();
            var expected = Model.Create<SimpleType>();

            var source = new WrapperSnapshot<SimpleType>(expected);

            var sut = new SnapshotProxy<SimpleType, ISimpleType>(source);

            var actual = sut.Value;

            actual.Should().Be(expected);
        }
    }
}