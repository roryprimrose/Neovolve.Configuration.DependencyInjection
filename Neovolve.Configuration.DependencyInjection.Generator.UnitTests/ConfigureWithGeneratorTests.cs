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

    private const string ClassificationGraphSource = @"
namespace Sample
{
    using System.Collections.ObjectModel;
    using Microsoft.Extensions.Hosting;

    public sealed class ChildItem
    {
        public string Value { get; set; } = string.Empty;
    }

    public sealed class ChildConfig
    {
        public string Setting { get; set; } = string.Empty;
    }

    public sealed class RootConfig
    {
        public int Count { get; set; }
        public Collection<string> Names { get; set; } = new();
        public Collection<ChildItem> Items { get; set; } = new();
        public ChildConfig Child { get; set; } = new();
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
    public void ClassifiesPropertiesIntoTypedChangeReports()
    {
        var harness = GeneratorTestHarness.Run(ClassificationGraphSource);

        harness.CompilationErrors.Should().BeEmpty();

        var generated = harness.GeneratedSources[0];

        // Scalar value -> ReportValue.
        generated.Should().Contain("context.ReportValue(\"Count\", previous, current)");

        // Collection of scalars -> ReportValues (count or per-element diffs).
        generated.Should().Contain("context.ReportValues(\"Names\", previous, current)");

        // Collection of complex elements -> ReportCount (count only, no per-element noise).
        generated.Should().Contain("context.ReportCount(\"Items\", previous, current)");

        // A report-only reporter is generated for the complex element type and used for per-element deep logging.
        generated.Should().Contain("internal static class global__Sample_ChildItem_Reporter");
        generated.Should().Contain("global__Sample_ChildItem_Reporter.Report(\"Items[\" + index + \"]\"");

        // Child configuration type -> assigned but not logged here in summary (logged by its own applier).
        generated.Should().Contain("injected.Child = current;");
        generated.Should().NotContain("context.ReportValue(\"Child\"");
        generated.Should().NotContain("context.ReportValues(\"Child\"");
        generated.Should().NotContain("context.ReportCount(\"Child\"");

        // In deep mode the child is walked via its reporter.
        generated.Should().Contain("global__Sample_ChildConfig_Reporter.Report(\"Child\"");
    }

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

    // The generator builds against Roslyn 4.8 so it loads on every supported consumer SDK (net472, net8.0,
    // net9.0, net10.0 and netstandard2.0). Bumping that floor silently drops support on older SDKs, so this
    // test fails if the generator ever references a newer Microsoft.CodeAnalysis than the documented floor.
    [Fact]
    public void GeneratorTargetsSupportedRoslynFloor()
    {
        var reference = typeof(ConfigureWithGenerator).Assembly
            .GetReferencedAssemblies()
            .Single(name => name.Name == "Microsoft.CodeAnalysis");

        reference.Version.Should().Be(new Version(4, 8, 0, 0));
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
    public void GeneratesForServiceCollectionCallSite()
    {
        const string source = @"
namespace Sample
{
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

    public sealed class RootConfig
    {
        public string RootValue { get; set; } = string.Empty;
    }

    public static class Caller
    {
        public static void Configure(IServiceCollection services, IConfiguration configuration)
        {
            services.ConfigureWith<RootConfig>(configuration);
        }
    }
}";

        var harness = GeneratorTestHarness.Run(source);

        harness.GeneratorDiagnostics.Should().BeEmpty();
        harness.CompilationErrors.Should().BeEmpty();
        harness.GeneratedSources.Should().ContainSingle();
        harness.GeneratedSources[0].Should().Contain("RegisterGraph(typeof(global::Sample.RootConfig)");
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
                && diagnostic.GetMessage().Contains("Sample.SettingsStruct")
                && diagnostic.GetMessage().Contains("Sample.RootConfig")
                && diagnostic.GetMessage().Contains("Settings"));

        // The root is not hot reloaded, so it is never reported as the not hot reloadable type.
        harness.GeneratorDiagnostics.Should()
            .NotContain(diagnostic => diagnostic.Id == "NCDI001"
                && diagnostic.GetMessage().Contains("'Sample.RootConfig' cannot be hot reloaded"));
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
                && diagnostic.GetMessage().Contains("no writable properties")
                && diagnostic.GetMessage().Contains("Sample.RootConfig")
                && diagnostic.GetMessage().Contains("Child"));
    }

    [Fact]
    public void ReportsNotHotReloadableWithNestedConfigurationPath()
    {
        const string source = @"
namespace Sample
{
    using Microsoft.Extensions.Hosting;

    public sealed record EndpointRecord(string Host, int Port);

    public sealed class ServiceConfig
    {
        public EndpointRecord Endpoint { get; set; } = new(string.Empty, 0);
        public string ServiceValue { get; set; } = string.Empty;
    }

    public sealed class RootConfig
    {
        public ServiceConfig Service { get; set; } = new();
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
                && diagnostic.GetMessage().Contains("Sample.EndpointRecord")
                && diagnostic.GetMessage().Contains("Service:Endpoint")
                && diagnostic.GetMessage().Contains("Sample.RootConfig"));
    }

    [Fact]
    public void DoesNotReportNotHotReloadableForMutableClassGraph()
    {
        var harness = GeneratorTestHarness.Run(NestedGraphSource);

        harness.GeneratorDiagnostics.Should().NotContain(diagnostic => diagnostic.Id == "NCDI001");
    }

    [Fact]
    public void ExcludesPropertyAndTypeMarkedToSkip()
    {
        const string source = @"
namespace Sample
{
    using Microsoft.Extensions.Hosting;
    using Neovolve.Configuration.DependencyInjection.Generated;

    [SkipConfigType]
    public sealed class IgnoredType
    {
        public string Value { get; set; } = string.Empty;
    }

    public sealed class RootConfig
    {
        public string Kept { get; set; } = string.Empty;

        [SkipConfigProperty]
        public string Skipped { get; set; } = string.Empty;

        public IgnoredType Ignored { get; set; } = new();
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

        harness.CompilationErrors.Should().BeEmpty();

        var generated = harness.GeneratedSources[0];

        // The kept property is still applied.
        generated.Should().Contain("context.ReportValue(\"Kept\", previous, current)");

        // The [SkipConfigProperty] property is excluded entirely.
        generated.Should().NotContain("\"Skipped\"");

        // The [SkipConfigType] property and its type are excluded entirely.
        generated.Should().NotContain("\"Ignored\"");
        generated.Should().NotContain("Sample.IgnoredType");
    }

    [Fact]
    public void ExcludesTypeMarkedAtAssemblyLevelWithSkipConfigType()
    {
        const string source = @"
using Neovolve.Configuration.DependencyInjection.Generated;

[assembly: SkipConfigType(typeof(Sample.Excluded))]

namespace Sample
{
    using Microsoft.Extensions.Hosting;

    public sealed class Excluded
    {
        public string Value { get; set; } = string.Empty;
    }

    public sealed class RootConfig
    {
        public string Kept { get; set; } = string.Empty;

        public Excluded Item { get; set; } = new();
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

        harness.CompilationErrors.Should().BeEmpty();

        var generated = harness.GeneratedSources[0];

        generated.Should().Contain("context.ReportValue(\"Kept\", previous, current)");
        generated.Should().NotContain("\"Item\"");
        generated.Should().NotContain("Sample.Excluded");
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
        public bool ReportValue<TValue>(string propertyPath, TValue previousValue, TValue updatedValue)
        {
            return false;
        }

        public bool ReportValues<TItem>(string propertyPath, System.Collections.Generic.IReadOnlyList<TItem>? previousValues,
            System.Collections.Generic.IReadOnlyList<TItem>? updatedValues)
        {
            return false;
        }

        public bool ReportCount(string propertyPath, System.Collections.ICollection? previousValue,
            System.Collections.ICollection? updatedValue)
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

        public bool LogNestedChanges => false;
    }
}
