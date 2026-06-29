namespace Neovolve.Configuration.DependencyInjection.UnitTests;

using System.ComponentModel.DataAnnotations;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Neovolve.Configuration.DependencyInjection;
using Neovolve.Logging.Xunit;
using Xunit;

public class DefaultConfigValidatorTests
{
    private readonly ITestOutputHelper _output;

    public DefaultConfigValidatorTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void IsValidReturnsFalseWhenValidatorFails()
    {
        var services = new ServiceCollection();

        services.AddSingleton<IValidateOptions<Target>>(
            new TestValidateOptions(ValidateOptionsResult.Fail("Failure")));

        using var provider = services.BuildServiceProvider();

        var sut = new DefaultConfigValidator(provider);

        sut.IsValid(new Target(), null, _output.BuildLoggerFor<DefaultConfigValidator>()).Should().BeFalse();
    }

    [Fact]
    public void IsValidReturnsTrueWhenNoValidatorsRegistered()
    {
        var services = new ServiceCollection();

        using var provider = services.BuildServiceProvider();

        var sut = new DefaultConfigValidator(provider);

        sut.IsValid(new Target(), null, null).Should().BeTrue();
    }

    [Fact]
    public void IsValidReturnsTrueWhenValidatorSucceeds()
    {
        var services = new ServiceCollection();

        services.AddSingleton<IValidateOptions<Target>>(
            new TestValidateOptions(ValidateOptionsResult.Success));

        using var provider = services.BuildServiceProvider();

        var sut = new DefaultConfigValidator(provider);

        sut.IsValid(new Target(), null, null).Should().BeTrue();
    }

    [Theory]
    [InlineData(10, false)]
    [InlineData(600, true)]
    public void ThrowOnInvalidDataAnnotationsThrowsOnlyWhenInvalid(int value, bool shouldThrow)
    {
        var action = () => DefaultConfigValidator.ThrowOnInvalidDataAnnotations(new Target { Value = value });

        if (shouldThrow)
        {
            action.Should().Throw<OptionsValidationException>();
        }
        else
        {
            action.Should().NotThrow();
        }
    }

    [Fact]
    public void ThrowOnInvalidDataAnnotationsDoesNotThrowWhenValueIsNull()
    {
        var action = () => DefaultConfigValidator.ThrowOnInvalidDataAnnotations<Target>(null!);

        action.Should().NotThrow();
    }

    [Fact]
    public void ThrowsExceptionWithNullProvider()
    {
        var action = () => new DefaultConfigValidator(null!);

        action.Should().Throw<ArgumentNullException>();
    }

    private class Target
    {
        [Range(5, 120)]
        public int Value { get; set; } = 30;
    }

    private sealed class TestValidateOptions : IValidateOptions<Target>
    {
        private readonly ValidateOptionsResult _result;

        public TestValidateOptions(ValidateOptionsResult result)
        {
            _result = result;
        }

        public ValidateOptionsResult Validate(string? name, Target options)
        {
            return _result;
        }
    }
}
