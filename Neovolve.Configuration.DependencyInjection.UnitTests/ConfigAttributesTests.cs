namespace Neovolve.Configuration.DependencyInjection.UnitTests;

using FluentAssertions;
using Neovolve.Configuration.DependencyInjection.Generated;
using Xunit;

public class ConfigAttributesTests
{
    [Fact]
    public void GenerateConfigAccessorsDefaultsToEmptyTypes()
    {
        var sut = new GenerateConfigAccessorsAttribute();

        sut.Types.Should().BeEmpty();
    }

    [Fact]
    public void GenerateConfigAccessorsStoresSuppliedTypes()
    {
        var sut = new GenerateConfigAccessorsAttribute(typeof(string), typeof(int));

        sut.Types.Should().Equal(typeof(string), typeof(int));
    }

    [Fact]
    public void SkipConfigTypeDefaultsToEmptyTypes()
    {
        var sut = new SkipConfigTypeAttribute();

        sut.Types.Should().BeEmpty();
    }

    [Fact]
    public void SkipConfigTypeReplacesNullTypesWithEmpty()
    {
        var sut = new SkipConfigTypeAttribute(null!);

        sut.Types.Should().BeEmpty();
    }

    [Fact]
    public void SkipConfigTypeStoresSuppliedTypes()
    {
        var sut = new SkipConfigTypeAttribute(typeof(string));

        sut.Types.Should().Equal(typeof(string));
    }

    [Fact]
    public void SkipConfigPropertyCanBeConstructed()
    {
        var sut = new SkipConfigPropertyAttribute();

        sut.Should().NotBeNull();
    }
}
