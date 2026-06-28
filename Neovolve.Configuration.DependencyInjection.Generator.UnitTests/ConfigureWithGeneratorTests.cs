namespace Neovolve.Configuration.DependencyInjection.Generator.UnitTests;

using FluentAssertions;
using Neovolve.Configuration.DependencyInjection.Generated;
using Xunit;

public class ConfigureWithGeneratorTests
{
    private const string StructGraphSource = @"
namespace Sample
{
    using Microsoft.Extensions.Hosting;

    public interface ISettings
    {
        string Name { get; }
    }

    public struct SettingsStruct : ISettings
    {
        public string Name { get; set; }
        public int Count { get; set; }
    }

    public sealed class RootConfig
    {
        public SettingsStruct Settings { get; set; }
        public string RootValue { get; set; } = string.Empty;
    }

    public static class Caller
    {
        public static void Configure(IHostBuilder builder)
        {
            builder.ConfigureWith<RootConfig>();
        }
    }
}";

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
    public void GeneratesReadOnlyReportForComputedProperty()
    {
        var harness = GeneratorTestHarness.Run(NestedGraphSource);

        var generated = harness.GeneratedSources[0];

        // The computed Timeout property is read only, so the applier reports it rather than copying it.
        generated.Should().Contain("context.ReportReadOnly(\"Timeout\", true);");
    }

    [Fact]
    public void ModuleInitializerRegistersApplierThatCopiesValues()
    {
        var harness = GeneratorTestHarness.Run(NestedGraphSource);

        harness.CompilationErrors.Should().BeEmpty();

        var assembly = harness.EmitAndLoad();

        var thirdConfigType = assembly.GetType("Sample.ThirdConfig", true)!;

        var applier = ResolveApplier(thirdConfigType);

        var injected = Activator.CreateInstance(thirdConfigType)!;
        var updated = Activator.CreateInstance(thirdConfigType)!;

        thirdConfigType.GetProperty("ThirdValue")!.SetValue(updated, "updated");

        InvokeApply(applier, injected, updated);

        thirdConfigType.GetProperty("ThirdValue")!.GetValue(injected).Should().Be("updated");
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

    [Fact]
    public void GeneratesAccessorsForAttributeMarkedTypeWithoutRegistrar()
    {
        const string source = @"
namespace Sample
{
    using Neovolve.Configuration.DependencyInjection.Generated;

    [GenerateConfigAccessors]
    public sealed class StandaloneConfig
    {
        public string Value { get; set; } = string.Empty;
    }
}";

        var harness = GeneratorTestHarness.Run(source);

        harness.CompilationErrors.Should().BeEmpty();

        var assembly = harness.EmitAndLoad();

        var standaloneType = assembly.GetType("Sample.StandaloneConfig", true)!;

        GeneratedConfigRegistry.TryGetApplier(standaloneType, out var applier).Should().BeTrue();
        applier.Should().NotBeNull();

        // A type that is only attribute-marked is not a ConfigureWith root, so no registrar is produced.
        GeneratedConfigRegistry.TryGetRegistrar(standaloneType, out _).Should().BeFalse();
    }

    [Fact]
    public void GeneratesAccessorsForAssemblyLevelClosedGenericType()
    {
        const string source = @"
using Neovolve.Configuration.DependencyInjection.Generated;

[assembly: GenerateConfigAccessors(typeof(Sample.ReadOnlyHolder<string>))]

namespace Sample
{
    public sealed class ReadOnlyHolder<T>
    {
        public ReadOnlyHolder(T value)
        {
            Value = value;
        }

        public T Value { get; }
    }
}";

        var harness = GeneratorTestHarness.Run(source);

        harness.CompilationErrors.Should().BeEmpty();

        var generated = harness.GeneratedSources[0];

        // The Value property is read only, so the applier reports it rather than copying it.
        generated.Should().Contain("context.ReportReadOnly(\"Value\", true);");

        var assembly = harness.EmitAndLoad();

        var closedType = assembly.GetType("Sample.ReadOnlyHolder`1", true)!.MakeGenericType(typeof(string));

        GeneratedConfigRegistry.TryGetApplier(closedType, out var applier).Should().BeTrue();
        applier.Should().NotBeNull();
    }

    [Fact]
    public void GeneratesStructRegistrationAndInterfaceForConfigStruct()
    {
        var harness = GeneratorTestHarness.Run(StructGraphSource);

        harness.CompilationErrors.Should().BeEmpty();

        var generated = harness.GeneratedSources[0];

        generated.Should().Contain("RegisterConfigStruct<global::Sample.SettingsStruct>");
        generated.Should().Contain("RegisterConfigStructInterface<global::Sample.ISettings>");
    }

    [Fact]
    public void ReportsNotHotReloadableForStructConfigType()
    {
        var harness = GeneratorTestHarness.Run(StructGraphSource);

        harness.GeneratorDiagnostics.Should()
            .Contain(diagnostic => diagnostic.Id == "NCDI001"
                && diagnostic.GetMessage().Contains("Sample.SettingsStruct"));

        // The root is not hot reloaded, so it is never reported.
        harness.GeneratorDiagnostics.Should()
            .NotContain(diagnostic => diagnostic.Id == "NCDI001"
                && diagnostic.GetMessage().Contains("Sample.RootConfig"));
    }

    [Fact]
    public void ReportsNotHotReloadableForRecordWithNoWritableProperties()
    {
        const string source = @"
namespace Sample
{
    using Microsoft.Extensions.Hosting;

    public sealed record ChildRecord(string Name, int Value);

    public sealed class RootConfig
    {
        public ChildRecord Child { get; set; } = new(string.Empty, 0);
        public string RootValue { get; set; } = string.Empty;
    }

    public static class Caller
    {
        public static void Configure(IHostBuilder builder) => builder.ConfigureWith<RootConfig>();
    }
}";

        var harness = GeneratorTestHarness.Run(source);

        harness.GeneratorDiagnostics.Should()
            .Contain(diagnostic => diagnostic.Id == "NCDI001"
                && diagnostic.GetMessage().Contains("Sample.ChildRecord")
                && diagnostic.GetMessage().Contains("no writable properties"));
    }

    [Fact]
    public void DoesNotReportNotHotReloadableForMutableClassGraph()
    {
        var harness = GeneratorTestHarness.Run(NestedGraphSource);

        harness.GeneratorDiagnostics.Should().NotContain(diagnostic => diagnostic.Id == "NCDI001");
    }

    private static void InvokeApply(object applier, object injected, object updated)
    {
        var applyMethod = applier.GetType().GetMethod("Apply")!;

        applyMethod.Invoke(applier, new[] { injected, updated, new StubConfigUpdateContext() });
    }

    private static object ResolveApplier(Type configType)
    {
        GeneratedConfigRegistry.TryGetApplier(configType, out var applier).Should().BeTrue();
        applier.Should().NotBeNull();

        return applier!;
    }

    private sealed class StubConfigUpdateContext : IConfigUpdateContext
    {
        public bool Report(string propertyPath, object? previousValue, object? updatedValue)
        {
            return false;
        }

        public void ReportCopyFailure(string propertyName, Exception exception)
        {
        }

        public void ReportReadOnly(string propertyPath, bool isValueType)
        {
        }

        public bool IsChangeLoggingEnabled => false;
    }
}
