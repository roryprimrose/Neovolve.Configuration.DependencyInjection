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
            actual.LogCategory.Should().Be(LogCategory.TargetType);
            actual.LogReadOnlyPropertyWarning.Should().Be(ReadOnlyPropertyWarning.None);
            actual.ReloadInjectedRawTypes.Should().BeTrue();
        }
    }
}