namespace Neovolve.Configuration.DependencyInjection.Generator.UnitTests;

using FluentAssertions;
using Xunit;

public class ConfigureWithGeneratorTests
{
    [Fact]
    public void GeneratorRunsWithoutDiagnostics()
    {
        const string source = @"
namespace Sample
{
    public sealed class RootConfig
    {
        public string RootValue { get; set; } = string.Empty;
    }

    public static class Caller
    {
        public static void Configure(Microsoft.Extensions.Hosting.IHostBuilder builder)
        {
            builder.ConfigureWith<RootConfig>();
        }
    }
}";

        var harness = GeneratorTestHarness.Run(source);

        harness.GeneratorDiagnostics.Should().BeEmpty();
    }
}
