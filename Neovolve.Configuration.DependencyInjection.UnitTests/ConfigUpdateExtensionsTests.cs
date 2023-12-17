namespace Neovolve.Configuration.DependencyInjection.UnitTests
{
    using FluentAssertions;
    using NSubstitute;

    public class ConfigUpdateExtensionsTests
    {
        [Fact]
        public void CopyValuesTakesNoActionWhenClassHasNoProperties()
        {
            var injectedConfig = new EmptyClass();
            var updatedConfig = new EmptyClass();
            var provider = Substitute.For<IServiceProvider>();

            var action = () => provider.CopyValues(injectedConfig, updatedConfig);

            action.Should().NotThrow();
        }

        [Fact]
        public void CopyValuesTakesNoActionWhenInjectedConfigIsNull()
        {
            FirstConfig? injectedConfig = null;
            var updatedConfig = new FirstConfig();
            var provider = Substitute.For<IServiceProvider>();

            var action = () => provider.CopyValues(injectedConfig, updatedConfig);

            action.Should().NotThrow();
        }

        [Fact]
        public void CopyValuesTakesNoActionWhenUpdatedConfigIsNull()
        {
            var injectedConfig = new FirstConfig();
            FirstConfig? updatedConfig = null;
            var provider = Substitute.For<IServiceProvider>();

            var action = () => provider.CopyValues(injectedConfig, updatedConfig);

            action.Should().NotThrow();
        }

        private class EmptyClass
        {
        }
    }
}