namespace Neovolve.Configuration.DependencyInjection.UnitTests
{
    using FluentAssertions;

    public class ConfigureWithOptionsTests
    {
        [Fact]
        public void CreatesWithDefaultValues()
        {
            var actual = new ConfigureWithOptions();

            actual.CustomLogCategory.Should().BeEmpty();
            actual.LogCategoryType.Should().Be(LogCategoryType.TargetType);
            actual.LogReadOnlyPropertyType.Should().Be(LogReadOnlyPropertyType.None);
            actual.ReloadInjectedRawTypes.Should().BeTrue();
        }
    }
}