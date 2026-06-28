namespace Neovolve.Configuration.DependencyInjection.Generator.UnitTests;

using FluentAssertions;
using Neovolve.Configuration.DependencyInjection.Generated;
using Xunit;

public class ConfigureWithGeneratorTests
{
    private const string NestedGraphSource = @"
namespace Sample
{
    using Microsoft.Extensions.Hosting;

    public sealed class ThirdConfig
    {
        public int TimeoutInSeconds { get; set; } = 123;
        public System.TimeSpan Timeout => System.TimeSpan.FromSeconds(TimeoutInSeconds);
        public string ThirdValue { get; set; } = string.Empty;
    }

    public sealed class SecondConfig
    {
        public string SecondValue { get; set; } = string.Empty;
        public ThirdConfig Third { get; set; } = new();
    }

    public sealed class FirstConfig
    {
        public string FirstValue { get; set; } = string.Empty;
        public SecondConfig Second { get; set; } = new();
    }

    public sealed class RootConfig
    {
        public FirstConfig First { get; set; } = new();
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

    [Fact]
    public void GeneratesAccessorsForRootAndNestedTypesThatCompile()
    {
        var harness = GeneratorTestHarness.Run(NestedGraphSource);

        harness.GeneratorDiagnostics.Should().BeEmpty();
        harness.CompilationErrors.Should().BeEmpty();
        harness.GeneratedSources.Should().ContainSingle();

        var generated = harness.GeneratedSources[0];

        generated.Should().Contain("typeof(global::Sample.RootConfig)");
        generated.Should().Contain("typeof(global::Sample.FirstConfig)");
        generated.Should().Contain("typeof(global::Sample.SecondConfig)");
        generated.Should().Contain("typeof(global::Sample.ThirdConfig)");
        generated.Should().Contain("ModuleInitializer");
    }

    [Fact]
    public void GeneratesReadOnlyAccessorWithoutSetter()
    {
        var harness = GeneratorTestHarness.Run(NestedGraphSource);

        var generated = harness.GeneratedSources[0];

        // The computed Timeout property is read only, so its accessor is registered with a null setter.
        generated.Should().Contain("\"Timeout\", false, true,");
    }

    [Fact]
    public void ModuleInitializerRegistersAccessorsThatCopyValues()
    {
        var harness = GeneratorTestHarness.Run(NestedGraphSource);

        harness.CompilationErrors.Should().BeEmpty();

        var assembly = harness.EmitAndLoad();

        var thirdConfigType = assembly.GetType("Sample.ThirdConfig", true)!;

        GeneratedConfigRegistry.TryGetProperties(thirdConfigType, out var accessors).Should().BeTrue();
        accessors.Should().NotBeNull();

        var thirdValue = accessors!.Single(accessor => accessor.Name == "ThirdValue");
        thirdValue.CanWrite.Should().BeTrue();

        var source = Activator.CreateInstance(thirdConfigType)!;
        var target = Activator.CreateInstance(thirdConfigType)!;

        thirdValue.SetValue!(source, "updated");
        thirdValue.SetValue!(target, thirdValue.GetValue(source));

        thirdValue.GetValue(target).Should().Be("updated");
    }

    [Fact]
    public void DoesNotGenerateWhenNoConfigureWithInvocation()
    {
        const string source = @"
namespace Sample
{
    public sealed class RootConfig
    {
        public string RootValue { get; set; } = string.Empty;
    }
}";

        var harness = GeneratorTestHarness.Run(source);

        harness.GeneratedSources.Should().BeEmpty();
    }

    [Fact]
    public void GeneratesRegistrarWithNestedSectionPaths()
    {
        var harness = GeneratorTestHarness.Run(NestedGraphSource);

        harness.CompilationErrors.Should().BeEmpty();

        var generated = harness.GeneratedSources[0];

        generated.Should().Contain("RegisterRoot<global::Sample.RootConfig>");
        generated.Should().Contain(
            "RegisterConfigType<global::Sample.FirstConfig>(services, configuration.GetSection(\"First\"))");
        generated.Should().Contain(
            "RegisterConfigType<global::Sample.SecondConfig>(services, configuration.GetSection(\"First:Second\"))");
        generated.Should().Contain(
            "RegisterConfigType<global::Sample.ThirdConfig>(services, configuration.GetSection(\"First:Second:Third\"))");
        generated.Should().Contain("RegisterGraph(typeof(global::Sample.RootConfig)");
    }

    [Fact]
    public void ModuleInitializerRegistersGraphRegistrar()
    {
        var harness = GeneratorTestHarness.Run(NestedGraphSource);

        harness.CompilationErrors.Should().BeEmpty();

        var assembly = harness.EmitAndLoad();

        var rootConfigType = assembly.GetType("Sample.RootConfig", true)!;

        GeneratedConfigRegistry.TryGetRegistrar(rootConfigType, out var registrar).Should().BeTrue();
        registrar.Should().NotBeNull();
    }
}
