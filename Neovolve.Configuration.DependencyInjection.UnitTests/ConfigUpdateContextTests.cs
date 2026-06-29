namespace Neovolve.Configuration.DependencyInjection.UnitTests;

using System;
using System.Collections.ObjectModel;
using FluentAssertions;
using Neovolve.Configuration.DependencyInjection;
using Neovolve.Configuration.DependencyInjection.Generated;
using Neovolve.Logging.Xunit;
using Xunit;

public class ConfigUpdateContextTests
{
    private readonly ITestOutputHelper _output;

    public ConfigUpdateContextTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void IsChangeLoggingEnabledReturnsFalseWhenNoLogger()
    {
        var sut = new ConfigUpdateContext(typeof(string), null, new ConfigureWithOptions());

        sut.IsChangeLoggingEnabled.Should().BeFalse();
    }

    [Fact]
    public void IsChangeLoggingEnabledReturnsTrueWithLogger()
    {
        var sut = new ConfigUpdateContext(typeof(string), Logger(), new ConfigureWithOptions());

        sut.IsChangeLoggingEnabled.Should().BeTrue();
    }

    [Theory]
    [InlineData(NestedChangeLogging.Deep, true)]
    [InlineData(NestedChangeLogging.Summary, false)]
    public void LogNestedChangesReflectsOption(NestedChangeLogging mode, bool expected)
    {
        var options = new ConfigureWithOptions { NestedChangeLogging = mode };

        var sut = new ConfigUpdateContext(typeof(string), Logger(), options);

        sut.LogNestedChanges.Should().Be(expected);
    }

    [Fact]
    public void ReportCountReturnsTrueWhenCountChanges()
    {
        var sut = new ConfigUpdateContext(typeof(string), Logger(), new ConfigureWithOptions());

        var previous = new Collection<string> { "a" };
        var updated = new Collection<string> { "a", "b" };

        sut.ReportCount("Items", previous, updated).Should().BeTrue();
    }

    [Theory]
    [InlineData(1, 1, false)]
    [InlineData(1, 2, true)]
    public void ReportValueReturnsWhetherValueChanged(int previous, int updated, bool expected)
    {
        var sut = new ConfigUpdateContext(typeof(string), Logger(), new ConfigureWithOptions());

        sut.ReportValue("Name", previous, updated).Should().Be(expected);
    }

    [Fact]
    public void ReportValuesReturnsTrueWhenElementChanges()
    {
        var sut = new ConfigUpdateContext(typeof(string), Logger(), new ConfigureWithOptions());

        sut.ReportValues("Items", new[] { "a" }, new[] { "b" }).Should().BeTrue();
    }

    [Fact]
    public void ReportReadOnlyDoesNotThrowWhenNoneConfigured()
    {
        var options = new ConfigureWithOptions { LogReadOnlyPropertyType = LogReadOnlyPropertyType.None };

        var sut = new ConfigUpdateContext(typeof(string), Logger(), options);

        var action = () => sut.ReportReadOnly("Name", true);

        action.Should().NotThrow();
    }

    [Fact]
    public void ReportReadOnlyDoesNotThrowWhenAllConfigured()
    {
        var options = new ConfigureWithOptions { LogReadOnlyPropertyType = LogReadOnlyPropertyType.All };

        var sut = new ConfigUpdateContext(typeof(string), Logger(), options);

        var action = () => sut.ReportReadOnly("Name", false);

        action.Should().NotThrow();
    }

    [Fact]
    public void ReportCopyFailureDoesNotThrow()
    {
        var sut = new ConfigUpdateContext(typeof(string), Logger(), new ConfigureWithOptions());

        var action = () => sut.ReportCopyFailure("Name", new InvalidOperationException());

        action.Should().NotThrow();
    }

    [Fact]
    public void ReportCountReturnsFalseWhenUnchanged()
    {
        var sut = new ConfigUpdateContext(typeof(string), Logger(), new ConfigureWithOptions());

        var previous = new Collection<string> { "a" };
        var updated = new Collection<string> { "b" };

        sut.ReportCount("Items", previous, updated).Should().BeFalse();
    }

    private ICacheLogger Logger()
    {
        return _output.BuildLogger();
    }
}
