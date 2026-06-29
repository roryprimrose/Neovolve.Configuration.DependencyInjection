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

            actual.CustomLogCategory.Should().Be("Neovolve.Configuration.DependencyInjection.ConfigureWith");
            actual.LogCategoryType.Should().Be(LogCategoryType.TargetType);
            actual.LogPropertyChangeLevel.Should().Be(LogLevel.Information);
            actual.LogReadOnlyPropertyType.Should().Be(LogReadOnlyPropertyType.ValueTypesOnly);
            actual.LogReadOnlyPropertyLevel.Should().Be(LogLevel.None);
            actual.NestedChangeLogging.Should().Be(NestedChangeLogging.Summary);
            actual.ReloadInjectedRawTypes.Should().BeTrue();
        }
    }
}