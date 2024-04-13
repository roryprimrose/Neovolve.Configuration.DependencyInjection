namespace Neovolve.Configuration.DependencyInjection.UnitTests
{
    using FluentAssertions;
    using ModelBuilder;

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