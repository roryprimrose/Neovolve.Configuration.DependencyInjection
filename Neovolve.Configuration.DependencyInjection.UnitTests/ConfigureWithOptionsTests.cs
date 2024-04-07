namespace Neovolve.Configuration.DependencyInjection.UnitTests
{
    using FluentAssertions;
    using Microsoft.Extensions.Logging;

    public class ConfigureWithOptionsTests
    {
        [Fact]
        public void CreatesWithDefaultValues()
        {
            var actual = new ConfigureWithOptions();

            actual.CustomLogCategory.Should().BeEmpty();
            actual.LogCategoryType.Should().Be(LogCategoryType.TargetType);
            actual.LogReadOnlyPropertyType.Should().Be(LogReadOnlyPropertyType.ValueTypesOnly);
            actual.LogReadOnlyPropertyLevel.Should().Be(LogLevel.Warning);
            actual.ReloadInjectedRawTypes.Should().BeTrue();
        }
    }
}