namespace Neovolve.Configuration.DependencyInjection.UnitTests;

using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

public class ConfigureWithExtensionsTests
{
    public static IEnumerable<object[]> ConfigTypesDataSet(bool configureReload)
    {
        yield return new object[] { typeof(Config), configureReload };
        yield return new object[] { typeof(IConfig), configureReload };
        yield return new object[] { typeof(FirstConfig), configureReload };
        yield return new object[] { typeof(IOptions<FirstConfig>), configureReload };
        yield return new object[] { typeof(IOptionsSnapshot<FirstConfig>), configureReload };
        yield return new object[] { typeof(IOptionsMonitor<FirstConfig>), configureReload };
        yield return new object[] { typeof(IFirstConfig), configureReload };
        yield return new object[] { typeof(IOptions<IFirstConfig>), configureReload };
        yield return new object[] { typeof(IOptionsSnapshot<IFirstConfig>), configureReload };
        yield return new object[] { typeof(IOptionsMonitor<IFirstConfig>), configureReload };
        yield return new object[] { typeof(SecondConfig), configureReload };
        yield return new object[] { typeof(IOptions<SecondConfig>), configureReload };
        yield return new object[] { typeof(IOptionsSnapshot<SecondConfig>), configureReload };
        yield return new object[] { typeof(IOptionsMonitor<SecondConfig>), configureReload };
        yield return new object[] { typeof(ISecondConfig), configureReload };
        yield return new object[] { typeof(IOptions<ISecondConfig>), configureReload };
        yield return new object[] { typeof(IOptionsSnapshot<ISecondConfig>), configureReload };
        yield return new object[] { typeof(IOptionsMonitor<ISecondConfig>), configureReload };
        yield return new object[] { typeof(ThirdConfig), configureReload };
        yield return new object[] { typeof(IOptions<ThirdConfig>), configureReload };
        yield return new object[] { typeof(IOptionsSnapshot<ThirdConfig>), configureReload };
        yield return new object[] { typeof(IOptionsMonitor<ThirdConfig>), configureReload };
        yield return new object[] { typeof(IThirdConfig), configureReload };
        yield return new object[] { typeof(IOptions<IThirdConfig>), configureReload };
        yield return new object[] { typeof(IOptionsSnapshot<IThirdConfig>), configureReload };
        yield return new object[] { typeof(IOptionsMonitor<IThirdConfig>), configureReload };
    }

    [Theory]
    [MemberData(nameof(ConfigTypesDataSet), true)]
    [MemberData(nameof(ConfigTypesDataSet), false)]
    public void ConfigureWithConfiguresInjectionTypes(Type expected, bool configureReload)
    {
        var builder = Host.CreateDefaultBuilder();

        builder.ConfigureAppConfiguration((_, configuration) =>
        {
            configuration.AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    ["RootValue"] = "This is the root value",
                    ["First:FirstValue"] = "This is the first value",
                    ["First:Second:SecondValue"] = "This is the second value",
                    ["First:Second:Third:ThirdValue"] = "This is the third value"
                });
        });

        builder.ConfigureWith<Config>(configureReload);

        using var host = builder.Build();

        using var scope = host.Services.CreateScope();

        var actual = scope.ServiceProvider.GetService(expected);

        actual.Should().NotBeNull();
        actual.Should().BeAssignableTo(expected);
    }

    [Theory]
    [InlineData(typeof(ConfigureWithOptions))]
    [InlineData(typeof(IConfigureWithOptions))]
    public void ConfigureWithRegistersDefaultOptions(Type optionsType)
    {
        var expected = new ConfigureWithOptions();
        var builder = Host.CreateDefaultBuilder();

        builder.ConfigureAppConfiguration((_, configuration) =>
        {
            configuration.AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    ["RootValue"] = "This is the root value",
                    ["First:FirstValue"] = "This is the first value",
                    ["First:Second:SecondValue"] = "This is the second value",
                    ["First:Second:Third:ThirdValue"] = "This is the third value"
                });
        });

        builder.ConfigureWith<Config>();

        using var host = builder.Build();

        var actual = host.Services.GetRequiredService(optionsType);

        actual.Should().BeEquivalentTo(expected);
    }
    
    [Theory]
    [InlineData(typeof(ConfigureWithOptions))]
    [InlineData(typeof(IConfigureWithOptions))]
    public void ConfigureWithRegistersProvidedOptions(Type optionsType)
    {
        var expected = new ConfigureWithOptions();
        var builder = Host.CreateDefaultBuilder();

        builder.ConfigureAppConfiguration((_, configuration) =>
        {
            configuration.AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    ["RootValue"] = "This is the root value",
                    ["First:FirstValue"] = "This is the first value",
                    ["First:Second:SecondValue"] = "This is the second value",
                    ["First:Second:Third:ThirdValue"] = "This is the third value"
                });
        });

        builder.ConfigureWith<Config>(x =>
        {
            x.ReloadInjectedRawTypes = expected.ReloadInjectedRawTypes;
            x.CustomLogCategory = expected.CustomLogCategory;
            x.LogCategory = expected.LogCategory;
            x.LogReadOnlyPropertyWarning = expected.LogReadOnlyPropertyWarning;
        });

        using var host = builder.Build();
        
        var actual = host.Services.GetRequiredService(optionsType);

        actual.Should().BeEquivalentTo(expected);
    }

    [Theory]
    [InlineData(typeof(ConfigureWithOptions), true)]
    [InlineData(typeof(IConfigureWithOptions), false)]
    public void ConfigureWithRegistersReloadOptions(Type optionsType, bool configureReload)
    {
        var expected = new ConfigureWithOptions
        {
            ReloadInjectedRawTypes = configureReload
        };
        var builder = Host.CreateDefaultBuilder();

        builder.ConfigureAppConfiguration((_, configuration) =>
        {
            configuration.AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    ["RootValue"] = "This is the root value",
                    ["First:FirstValue"] = "This is the first value",
                    ["First:Second:SecondValue"] = "This is the second value",
                    ["First:Second:Third:ThirdValue"] = "This is the third value"
                });
        });

        builder.ConfigureWith<Config>(configureReload);

        using var host = builder.Build();
        
        var actual = host.Services.GetRequiredService(optionsType);

        actual.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void ConfigureWithReloadThrowsExceptionWithNullBuilder()
    {
        Action action = () => ConfigureWithExtensions.ConfigureWith<Config>(null!, false);

        action.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void ConfigureWithThrowsExceptionWithNullBuilder()
    {
        Action action = () => ConfigureWithExtensions.ConfigureWith<Config>(null!);

        action.Should().Throw<ArgumentNullException>();
    }
}