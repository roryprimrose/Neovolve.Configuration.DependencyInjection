namespace Neovolve.Configuration.DependencyInjection.UnitTests;

using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Neovolve.Configuration.DependencyInjection.UnitTests.Models;
using NSubstitute;

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
    [InlineData(true, LogLevel.Warning)]
    [InlineData(false, LogLevel.Debug)]
    public void ConfigureWithConfiguresDefaultReadOnlyPropertyLogLevelBasedOnEnvironment(bool isDevelopment,
        LogLevel expected)
    {
        var hostEnvironment = Substitute.For<IHostEnvironment>();

        hostEnvironment.EnvironmentName.Returns(isDevelopment ? "Development" : "Production");

        var builder = Host.CreateDefaultBuilder()
            .ConfigureServices(services => { services.AddSingleton(hostEnvironment); })
            .ConfigureWith<Config>();

        using var host = builder.Build();

        using var scope = host.Services.CreateScope();

        var actual = scope.ServiceProvider.GetRequiredService<IConfigureWithOptions>();

        actual.LogReadOnlyPropertyLevel.Should().Be(expected);
    }

    [Theory]
    [MemberData(nameof(ConfigTypesDataSet), true)]
    [MemberData(nameof(ConfigTypesDataSet), false)]
    public void ConfigureWithConfiguresInjectionTypes(Type expected, bool configureReload)
    {
        var builder = Host.CreateDefaultBuilder().ConfigureAppConfiguration((_, configuration) =>
        {
            configuration.AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    ["RootValue"] = "This is the root value",
                    ["First:FirstValue"] = "This is the first value",
                    ["First:Second:SecondValue"] = "This is the second value",
                    ["First:Second:MoreValues:First"] = "First",
                    ["First:Second:MoreValues:Second"] = "Second",
                    ["First:Second:MoreValues:Third"] = "Third",
                    ["First:Second:Third:ThirdValue"] = "This is the third value"
                });
        }).ConfigureWith<Config>(configureReload);

        using var host = builder.Build();

        using var scope = host.Services.CreateScope();

        var actual = scope.ServiceProvider.GetService(expected);

        actual.Should().NotBeNull();
        actual.Should().BeAssignableTo(expected);
    }

    [Fact]
    public void ConfigureWithDoesNotRegisterIgnoredType()
    {
        var data = new Dictionary<string, string>
        {
            ["Ignored:Id"] = Guid.NewGuid().ToString()
        };
        var builder = Host.CreateDefaultBuilder()
            .ConfigureAppConfiguration((_, configuration) => { configuration.AddInMemoryCollection(data); })
            .ConfigureWith<IgnoredTypesRoot>();

        using var host = builder.Build();

        var actual = host.Services.GetRequiredService<IgnoredTypesRoot>();

        actual.Ignored.Id.Should().Be(Guid.Parse(data["Ignored:Id"]));

        host.Services.GetService<Type>().Should().BeNull();
    }

    [Fact]
    public void ConfigureWithOptionsThrowsExceptionWithNullBuilder()
    {
        Action action = () => ConfigureWithExtensions.ConfigureWith<Config>(null!, _ => { });

        action.Should().Throw<ArgumentNullException>();
    }

    [Theory]
    [InlineData(typeof(ConfigureWithOptions))]
    [InlineData(typeof(IConfigureWithOptions))]
    public void ConfigureWithRegistersDefaultOptions(Type optionsType)
    {
        var expected = new ConfigureWithOptions();
        var builder = Host.CreateDefaultBuilder().ConfigureAppConfiguration((_, configuration) =>
        {
            configuration.AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    ["RootValue"] = "This is the root value",
                    ["First:FirstValue"] = "This is the first value",
                    ["First:Second:SecondValue"] = "This is the second value",
                    ["First:Second:MoreValues:First"] = "First",
                    ["First:Second:MoreValues:Second"] = "Second",
                    ["First:Second:MoreValues:Third"] = "Third",
                    ["First:Second:Third:ThirdValue"] = "This is the third value"
                });
        }).ConfigureWith<Config>();

        using var host = builder.Build();

        var actual = host.Services.GetRequiredService(optionsType);

        // Exclude log level because it is based on the current environment which is covered by different tests
        actual.Should().BeEquivalentTo(expected, opt => opt.Excluding(x => x.LogReadOnlyPropertyLevel));
    }

    [Theory]
    [InlineData(typeof(ConfigureWithOptions))]
    [InlineData(typeof(IConfigureWithOptions))]
    public void ConfigureWithRegistersProvidedOptions(Type optionsType)
    {
        var expected = new ConfigureWithOptions();
        var builder = Host.CreateDefaultBuilder()
            .ConfigureAppConfiguration((_, configuration) =>
            {
                configuration.AddInMemoryCollection(
                    new Dictionary<string, string?>
                    {
                        ["RootValue"] = "This is the root value",
                        ["First:FirstValue"] = "This is the first value",
                        ["First:Second:SecondValue"] = "This is the second value",
                        ["First:Second:MoreValues:First"] = "First",
                        ["First:Second:MoreValues:Second"] = "Second",
                        ["First:Second:MoreValues:Third"] = "Third",
                        ["First:Second:Third:ThirdValue"] = "This is the third value"
                    });
            })
            .ConfigureWith<Config>(x =>
            {
                x.ReloadInjectedRawTypes = expected.ReloadInjectedRawTypes;
                x.CustomLogCategory = expected.CustomLogCategory;
                x.LogCategoryType = expected.LogCategoryType;
                x.LogReadOnlyPropertyType = expected.LogReadOnlyPropertyType;
                x.LogReadOnlyPropertyLevel = expected.LogReadOnlyPropertyLevel;
            });

        using var host = builder.Build();

        var actual = host.Services.GetRequiredService(optionsType);

        actual.Should().BeEquivalentTo(expected);
    }

    [Theory]
    [InlineData(typeof(ConfigureWithOptions), true)]
    [InlineData(typeof(ConfigureWithOptions), false)]
    [InlineData(typeof(IConfigureWithOptions), true)]
    [InlineData(typeof(IConfigureWithOptions), false)]
    public void ConfigureWithRegistersReloadOptions(Type optionsType, bool configureReload)
    {
        var expected = new ConfigureWithOptions
        {
            ReloadInjectedRawTypes = configureReload
        };
        var builder = Host.CreateDefaultBuilder()
            .ConfigureAppConfiguration((_, configuration) =>
            {
                configuration.AddInMemoryCollection(
                    new Dictionary<string, string?>
                    {
                        ["RootValue"] = "This is the root value",
                        ["First:FirstValue"] = "This is the first value",
                        ["First:Second:SecondValue"] = "This is the second value",
                        ["First:Second:MoreValues:First"] = "First",
                        ["First:Second:MoreValues:Second"] = "Second",
                        ["First:Second:MoreValues:Third"] = "Third",
                        ["First:Second:Third:ThirdValue"] = "This is the third value"
                    });
            })
            .ConfigureWith<Config>(configureReload);

        using var host = builder.Build();

        var actual = host.Services.GetRequiredService(optionsType);

        actual.Should().BeEquivalentTo(expected, opt => opt.Excluding(x => x.LogReadOnlyPropertyLevel));
    }

    [Fact]
    public void ConfigureWithRegistersRootConfig()
    {
        var data = new Dictionary<string, string?>
        {
            ["RootValue"] = "This is the root value",
            ["First:FirstValue"] = "This is the first value",
            ["First:Second:SecondValue"] = "This is the second value",
            ["First:Second:MoreValues:First"] = "First",
            ["First:Second:MoreValues:Second"] = "Second",
            ["First:Second:MoreValues:Third"] = "Third",
            ["First:Second:Third:ThirdValue"] = "This is the third value"
        };
        var builder = Host.CreateDefaultBuilder()
            .ConfigureAppConfiguration((_, configuration) => { configuration.AddInMemoryCollection(data); })
            .ConfigureWith<Config>();

        using var host = builder.Build();

        var actual = host.Services.GetRequiredService<Config>();

        actual.RootValue.Should().Be(data["RootValue"]);
        actual.First.FirstValue.Should().Be(data["First:FirstValue"]);
        actual.First.Second.SecondValue.Should().Be(data["First:Second:SecondValue"]);
        actual.First.Second.MoreValues.Should().Contain(data["First:Second:MoreValues:First"]);
        actual.First.Second.MoreValues.Should().Contain(data["First:Second:MoreValues:Second"]);
        actual.First.Second.MoreValues.Should().Contain(data["First:Second:MoreValues:Third"]);
        actual.First.Second.Third.ThirdValue.Should().Be(data["First:Second:Third:ThirdValue"]);
    }

    [Fact]
    public void ConfigureWithRegistersSelfReferencingChildTypeOnlyOnce()
    {
        var data = new Dictionary<string, string>
        {
            ["Self:Id"] = Guid.NewGuid().ToString()
        };
        var builder = Host.CreateDefaultBuilder()
            .ConfigureAppConfiguration((_, configuration) => { configuration.AddInMemoryCollection(data); })
            .ConfigureWith<SelfReferenceRoot>();

        using var host = builder.Build();

        var actual = host.Services.GetRequiredService<SelfReferenceRoot>();

        actual.Self.Id.Should().Be(Guid.Parse(data["Self:Id"]));

        var references = host.Services.GetRequiredService<IEnumerable<SelfReference>>().ToList();

        references.Should().HaveCount(1);
        references[0].Should().BeEquivalentTo(actual.Self);
    }

    [Fact]
    public void ConfigureWithReloadThrowsExceptionWithNullBuilder()
    {
        Action action = () => ConfigureWithExtensions.ConfigureWith<Config>(null!, false);

        action.Should().Throw<ArgumentNullException>();
    }

    [Theory]
    [InlineData(typeof(IOptions<Config>))]
    [InlineData(typeof(IOptionsSnapshot<Config>))]
    [InlineData(typeof(IOptionsMonitor<Config>))]
    [InlineData(typeof(IOptions<IConfig>))]
    [InlineData(typeof(IOptionsSnapshot<IConfig>))]
    [InlineData(typeof(IOptionsMonitor<IConfig>))]
    public void ConfigureWithRemovesOptionsVariantsForRootConfigAndInterfaces(Type configType)
    {
        var data = new Dictionary<string, string?>
        {
            ["RootValue"] = "This is the root value",
            ["First:FirstValue"] = "This is the first value",
            ["First:Second:SecondValue"] = "This is the second value",
            ["First:Second:MoreValues:First"] = "First",
            ["First:Second:MoreValues:Second"] = "Second",
            ["First:Second:MoreValues:Third"] = "Third",
            ["First:Second:Third:ThirdValue"] = "This is the third value"
        };
        var builder = Host.CreateDefaultBuilder()
            .ConfigureAppConfiguration((_, configuration) => { configuration.AddInMemoryCollection(data); })
            .ConfigureWith<Config>();

        using var host = builder.Build();

        using var scope = host.Services.CreateScope();

        var provider = configType.Name.Contains("Snapshot") ? scope.ServiceProvider : host.Services;

        provider.GetService(configType).Should().BeNull();

        var action = new Action(() => provider.GetRequiredService(configType));

        action.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void ConfigureWithThrowsExceptionWithNullBuilder()
    {
        Action action = () => ConfigureWithExtensions.ConfigureWith<Config>(null!);

        action.Should().Throw<ArgumentNullException>();
    }
}