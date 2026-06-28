namespace Neovolve.Configuration.DependencyInjection.UnitTests;

using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Neovolve.Configuration.DependencyInjection;
using Neovolve.Configuration.DependencyInjection.UnitTests.Models;
using Xunit;

public class ConfigureWithServiceCollectionExtensionsTests
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void ConfigureWithAppliesReloadInjectedRawTypesOption(bool reload)
    {
        var configuration = BuildConfiguration();
        var services = new ServiceCollection();

        services.ConfigureWith<Config>(configuration, reload);

        using var provider = services.BuildServiceProvider();

        provider.GetRequiredService<IConfigureWithOptions>().ReloadInjectedRawTypes.Should().Be(reload);
    }

    [Fact]
    public void ConfigureWithBindsRootAndNestedConfiguration()
    {
        var configuration = BuildConfiguration();
        var services = new ServiceCollection();

        services.ConfigureWith<Config>(configuration);

        using var provider = services.BuildServiceProvider();

        var actual = provider.GetRequiredService<Config>();

        actual.RootValue.Should().Be("This is the root value");
        actual.First.FirstValue.Should().Be("This is the first value");
        actual.First.Second.SecondValue.Should().Be("This is the second value");
        actual.First.Second.Third.ThirdValue.Should().Be("This is the third value");
    }

    [Fact]
    public void ConfigureWithRegistersChangeTrackingServices()
    {
        var configuration = BuildConfiguration();
        var services = new ServiceCollection();

        services.ConfigureWith<Config>(configuration);

        using var provider = services.BuildServiceProvider();

        provider.GetService<IConfigUpdater>().Should().NotBeNull();
        provider.GetService<IConfigureWithOptions>().Should().NotBeNull();
    }

    [Fact]
    public void ConfigureWithRegistersConfigurationInterfaces()
    {
        var configuration = BuildConfiguration();
        var services = new ServiceCollection();

        services.ConfigureWith<Config>(configuration);

        using var provider = services.BuildServiceProvider();

        provider.GetRequiredService<IConfig>().RootValue.Should().Be("This is the root value");
        provider.GetRequiredService<IFirstConfig>().FirstValue.Should().Be("This is the first value");
    }

    [Fact]
    public void ConfigureWithThrowsExceptionWithNullConfiguration()
    {
        var services = new ServiceCollection();

        var action = () => services.ConfigureWith<Config>(null!);

        action.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void ConfigureWithThrowsExceptionWithNullServices()
    {
        var configuration = BuildConfiguration();

        var action = () => ConfigureWithServiceCollectionExtensions.ConfigureWith<Config>(null!, configuration);

        action.Should().Throw<ArgumentNullException>();
    }

    private static IConfiguration BuildConfiguration()
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    ["RootValue"] = "This is the root value",
                    ["First:FirstValue"] = "This is the first value",
                    ["First:Second:SecondValue"] = "This is the second value",
                    ["First:Second:Third:ThirdValue"] = "This is the third value"
                })
            .Build();
    }
}
