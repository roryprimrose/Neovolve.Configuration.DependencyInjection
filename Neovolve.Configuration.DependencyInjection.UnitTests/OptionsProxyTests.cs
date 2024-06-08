namespace Neovolve.Configuration.DependencyInjection.UnitTests
{
    using FluentAssertions;
    using ModelBuilder;
    using Neovolve.Configuration.DependencyInjection.UnitTests.Models;

    public class OptionsProxyTests
    {
        [Fact]
        public void ValueReturnsValueFromSourceOptions()
        {
            var config = Model.Create<SimpleType>();

            var source = new WrapperOptions<SimpleType>(config);

            var sut = new OptionsProxy<SimpleType, ISimpleType>(source);

            sut.Value.Should().BeEquivalentTo(config);
        }
    }
}