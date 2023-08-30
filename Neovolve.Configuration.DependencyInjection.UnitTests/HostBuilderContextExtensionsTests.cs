namespace Neovolve.Configuration.DependencyInjection.UnitTests
{
    using FluentAssertions;

    public class HostBuilderContextExtensionsTests
    {
        [Fact]
        public void ConfigureWithThrowsExceptionWithNullBuilder()
        {
            Action action = () => HostBuilderContextExtensions.ConfigureWith<Config>(null!);

            action.Should().Throw<ArgumentNullException>();
        }
    }
}