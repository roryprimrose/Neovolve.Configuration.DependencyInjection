namespace Neovolve.Configuration.DependencyInjection.UnitTests;

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Neovolve.Configuration.DependencyInjection;
using Xunit;

public class ValidationTests
{
    [Fact]
    public void ConfigureWithBindsWhenNestedValueIsInRange()
    {
        var services = new ServiceCollection();

        services.ConfigureWith<ValidatedRootConfig>(BuildConfiguration(30));

        using var provider = services.BuildServiceProvider();

        var actual = provider.GetRequiredService<ValidatedChildConfig>();

        actual.TimeoutInSeconds.Should().Be(30);
    }

    [Fact]
    public void ConfigureWithThrowsWhenNestedValueIsOutOfRange()
    {
        var services = new ServiceCollection();

        services.ConfigureWith<ValidatedRootConfig>(BuildConfiguration(600));

        using var provider = services.BuildServiceProvider();

        var action = () => provider.GetRequiredService<ValidatedChildConfig>();

        action.Should().Throw<OptionsValidationException>();
    }

    private static IConfiguration BuildConfiguration(int timeoutInSeconds)
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    ["RootValue"] = "root",
                    ["Child:TimeoutInSeconds"] = timeoutInSeconds.ToString()
                })
            .Build();
    }

    public class ValidatedRootConfig
    {
        public ValidatedChildConfig Child { get; set; } = new();

        public string RootValue { get; set; } = string.Empty;
    }

    public class ValidatedChildConfig
    {
        [Range(5, 120)]
        public int TimeoutInSeconds { get; set; } = 30;
    }
}
