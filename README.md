# Introduction

The **Neovolve.Configuration.DependencyInjection** NuGet package provides `IHostBuilder` extension methods for registering strong typed configuration bindings as services. It supports registration of nested configuration types and hot reload support.

[![GitHub license](https://img.shields.io/badge/License-MIT-blue.svg)](https://github.com/roryprimrose/Neovolve.Configuration.DependencyInjection/blob/master/LICENSE)&nbsp;&nbsp;&nbsp;[![Nuget](https://img.shields.io/nuget/v/Neovolve.Configuration.DependencyInjection.svg)&nbsp;&nbsp;&nbsp;![Nuget](https://img.shields.io/nuget/dt/Neovolve.Configuration.DependencyInjection.svg)](https://www.nuget.org/packages/Neovolve.Configuration.DependencyInjection)

[![Actions Status](https://github.com/roryprimrose/Neovolve.Configuration.DependencyInjection/workflows/CI/badge.svg)](https://github.com/roryprimrose/Neovolve.Configuration.DependencyInjection/actions)

# Installation

The package can be installed from NuGet using 

```powershell
Install-Package Neovolve.Configuration.DependencyInjection
```

# Usage
This package requires that the application bootstrapping provide a root configuration class that matches the configuration structure that the application uses. 

The `ConfigureWith<T>` extension method registers the configuration type, all nested configuration types and all interfaces found as services in the host application. It will also ensure that `IOptions<>`, `IOptionsSnapshot<>` and `IOptionsMonitor<>` types are registered with the class types found under the root config type as well as all their interfaces.

For example consider the following nested configuration type structure:

```csharp

public interface IRootConfig
{
    string RootValue { get; }
}

public class RootConfig : IRootConfig
{
    public FirstConfig First { get; set; } = new();
    public string RootValue { get; set; } = string.Empty;
}

public interface IFirstConfig
{
    string FirstValue { get; }
}

public class FirstConfig : IFirstConfig
{
    public string FirstValue { get; set; } = string.Empty;
    public SecondConfig Second { get; set; } = new();
}

public interface ISecondConfig
{
    string SecondValue { get; }
}

public class SecondConfig : ISecondConfig
{
    public string SecondValue { get; set; } = string.Empty;
    public ThirdConfig Third { get; set; } = new();
}

public interface IThirdConfig
{
    string ThirdValue { get; }
    TimeSpan Timeout { get; }
}

public class ThirdConfig : IThirdConfig
{
    public int TimeoutInSeconds { get; set; } = 123;

    public TimeSpan Timeout => TimeSpan.FromSeconds(TimeoutInSeconds);

    public string ThirdValue { get; set; } = string.Empty;
}
```

The json configuration source for this data could be something like the following.

```json
{
  "RootValue": "This is the root value",
  "First": {
    "Second": {
      "Third": {
        "ThirdValue": "This is the third value",
        "TimeoutInSeconds":  123
      },
      "SecondValue": "This is the second value"
    },
    "FirstValue": "This is the first value"
  }
}
```

For an ASP.Net system, this would be registered like the following:

```csharp
var builder = WebApplication.CreateBuilder(args);

// Register all configuration types
builder.Host.ConfigureWith<RootConfig>();
```

For other platforms, such as console applications, this would be registered like the following:

```csharp
var builder = Host.CreateDefaultBuilder()
    .ConfigureWith<RootConfig>();
```

Using `IHostBuilder` is the recommended way to use this library. For scenarios that work directly with an `IServiceCollection` and an `IConfiguration`, an equivalent overload is available. The `IHostBuilder` extension methods are thin wrappers over these.

```csharp
builder.Services.ConfigureWith<RootConfig>(builder.Configuration);
```

Given the above example, the following services would be registered with the host application:

| Type | IOptions&lt;T&gt; | IOptionsSnapshot&lt;T&gt; | IOptionsMonitor&lt;T&gt; | Supports hot reload |
|-|-|-|-|-|
| RootConfig | No | No | No | No |
| IRootConfig | No | No | No | No |
| FirstConfig | Yes | Yes | Yes | Yes, except for IOptions&lt;FirstConfig&gt; |
| IFirstConfig | Yes | Yes | Yes | Yes, except for IOptions&lt;IFirstConfig&gt; |
| SecondConfig | Yes | Yes | Yes | Yes, except for IOptions&lt;SecondConfig&gt; |
| ISecondConfig | Yes | Yes | Yes | Yes, except for IOptions&lt;ISecondConfig&gt; |
| ThirdConfig | Yes | Yes | Yes | Yes, except for IOptions&lt;ThirdConfig&gt; |
| IThirdConfig | Yes | Yes | Yes | Yes, except for IOptions&lt;IThirdConfig&gt; |

See [Options pattern in .NET -> Options interfaces](https://learn.microsoft.com/en-us/dotnet/core/extensions/options#options-interfaces) for more information on the `IOptions<>`, `IOptionsSnapshot<>` and `IOptionsMonitor<>` types.

# Hot reload support
The options binding system in .NET Core supports hot reload of configuration data which is implemented by some configuration providers like the providers for json and ini files. This is typically done by watching the configuration source for changes and then reloading the configuration data. This is useful for scenarios where configuration data is stored in a file and the application needs to react to changes in the file without needing to restart the application. This support is provided by the `IOptionsSnapshot<>` and `IOptionsMonitor<>` services.

One of the benefits of this package is that it supports hot reloading of injected raw configuration services by default. A raw type is a configuration class and its defined interfaces that are found under the root configuration type. In this definition, a raw type is anything other than `IOptions<>`, `IOptionsSnapshot<>` or `IOptionsMonitor<>`. 

In the above configuration example, the raw types that support hot reloading are:
- IFirstConfig
- FirstConfig
- ISecondConfig
- SecondConfig
- IThirdConfig
- ThirdConfig

This package detects when a configuration change has occurred by watching `IOptionsMonitor<>.OnChange` on all configuration services registered under the root configuration type. The package then updates the existing raw type in memory which works because the raw types are registered as singleton services. This allows the application class to receive updated configuration data at runtime by injecting a `T` configuration class/interface without needing to use `IOptionsMonitor<T>` or `IOptionsSnapshot<T>`. Logging is provided as the mechanism for recording that an injected raw type has been updated.

The reason to use `IOptionsMonitor<>` instead of the raw type is when the application class wants to hook into the `IOptionsMonitor.OnChange` method itself to run some custom code when the configuration changes.

The hot reload support for raw configuration types can be disabled by setting the `ReloadInjectedRawTypes` option to `false` in the `ConfigureWith<T>` overload.

## Configuration types must be mutable classes to hot reload
Hot reload works by updating the existing singleton instance of a configuration type in place when the configuration changes. That requires the type to be a reference type with writable properties. Two kinds of configuration type cannot be hot reloaded:

- **Value types** (`struct` and `record struct`). A struct is copied when it is injected, so updating the registered instance would not reach the copies already handed to application classes. Struct configuration types are bound and registered once as a snapshot of the configuration at startup, and they do not receive later updates.
- **Reference types with no writable properties** (for example a positional `record` whose properties are all `init`-only). There is nowhere to write the updated values, so the injected instance keeps its startup values.

The source generator reports these cases at compile time as warning **NCDI001** so the limitation is visible where the type is declared rather than failing silently at runtime. The accompanying code fix converts a `struct` (or `record struct`) configuration type into a class so it can hot reload. For a record with no writable properties, add settable properties or convert it to a mutable class.

If a one-time snapshot is the intended behaviour for a given type, suppress the warning for that type:

```csharp
[System.Diagnostics.CodeAnalysis.SuppressMessage("Neovolve.Configuration", "NCDI001", Justification = "Snapshot binding is intended for this type.")]
public struct StartupOnlySettings
{
    public string Name { get; set; }
}
```

The warning can also be disabled project-wide with `<NoWarn>$(NoWarn);NCDI001</NoWarn>`.

# Options
The following are the default options that `ConfigureWith<T>` uses.

| Option | Type | Default | Description |
|-|-|-|-|
| CustomLogCategory | `string` | `Neovolve.Configuration.DependencyInjection.ConfigureWith` | The custom log category used when `LogCategoryType` is `LogCategoryType.Custom` |
| LogCategoryType | `LogCategoryType` | `LogCategoryType.TargetType` | The log category to use when logging messages for configuration updates on raw types. Supported values are `TargetType` or `Custom`. |
| LogPropertyChangeLevel | `LogLevel` | `LogLevel.Information` | The log level to use when logging that a property on an injected raw type has been updated when `ReloadInjectedRawTypes` is `true`. |
| LogReadOnlyPropertyLevel | `LogLevel` | `LogLevel.Warning` in Development; otherwise `Debug` | The log level to use when logging that updates are detected for read-only properties on an injected raw type has been updated when `ReloadInjectedRawTypes` is `true`. |
| LogReadOnlyPropertyType | `LogReadOnlyPropertyType` | `LogReadOnlyPropertyType.ValueTypesOnly` | The types of read-only properties to log when they are updated. Supported values are `All`, `ValueTypesOnly` and `None.` |
| NestedChangeLogging | `NestedChangeLogging` | `NestedChangeLogging.Summary` | How much detail is logged when a class property or a collection of classes changes. `Summary` logs class properties independently and collections of classes as an entry count only. `Deep` also walks class properties and the elements of collections of classes, logging individual nested changes with a full property path (for example `FilterRules[0].Port`). |
| ReloadInjectedRawTypes | `bool` | `true` | Determines if raw types that are injected into the configuration system should be reloaded when the configuration changes |

These options can be set in the `ConfigureWith<T>` overload.

```csharp
var builder = Host.CreateDefaultBuilder()
    .ConfigureWith<RootConfig>(x => {
        x.CustomLogCategory = "MyCustomCategory";
        x.LogCategoryType = LogCategoryType.Custom;
        x.LogReadOnlyPropertyLevel = LogLevel.Information;
        x.LogReadOnlyPropertyType = LogReadOnlyPropertyType.All;
        x.NestedChangeLogging = NestedChangeLogging.Deep;
        x.ReloadInjectedRawTypes = false;
    });
```

To exclude a property or type from the configuration graph, see [Excluding properties and types](#excluding-properties-and-types).

# Source generator
This package includes a Roslyn source generator that runs in the project that calls `ConfigureWith<T>`. At compile time it walks the configuration type graph from each `ConfigureWith<T>` root and emits the registration code plus a strongly typed value applier for each configuration type. Each applier assigns the updated property values directly onto the injected instance, so the library binds and hot reloads configuration without runtime reflection and without boxing value type properties on the update path. The generator ships with the package and requires no configuration.

Because the generator knows each property's type at compile time, it also decides how a property change is logged (when change logging is enabled), without any runtime type inspection:

- Scalar values (numbers, strings, enums, and so on) are logged as `from 'old' to 'new'`.
- Collections of scalar values are logged as an entry count change, or as the individual element values that changed.
- Collections of complex types are logged only as an entry count change, because logging each element would just repeat the element type name.
- Child configuration types are assigned but not logged at the parent, because each child type is registered and logs its own changes independently.

For example, given a `ServerConfig` with a `Name` string, a `Tags` collection of strings and an `Endpoints` collection of `Endpoint` objects, a reload that renames the server and adds a tag logs the following in the default `Summary` mode:

```text
Configuration updated on property ServerConfig.Name from 'web-01' to 'web-02'
Configuration updated on property ServerConfig.Tags from 2 entries to 3 entries
```

A change to an endpoint's port is not logged in `Summary` mode when the endpoint count is unchanged, because the elements are complex types. Setting the `NestedChangeLogging` option to `Deep` also logs the individual nested changes inside class properties and the elements of collections of classes, using a full property path:

```text
Configuration updated on property ServerConfig.Endpoints[0].Port from '80' to '443'
```

Deep logging costs more on deeper graphs and is off by default. It can also repeat a change that a registered child type already logs on its own, because both the parent (using a nested path) and the child type report it.

## Excluding properties and types
The generator always treats `IEnumerable`, `Type`, `Assembly` and `Stream` as leaf values rather than nested configuration sections. To exclude additional properties or types from the configuration graph (so they are not registered, copied or logged on hot reload), use the exclusion attributes.

Mark an individual property with `[SkipConfigProperty]`:

```csharp
using Neovolve.Configuration.DependencyInjection.Generated;

public class ServerConfig
{
    public string Name { get; set; } = string.Empty;

    [SkipConfigProperty]
    public string ComputedToken { get; set; } = string.Empty;
}
```

Mark a type with `[SkipConfigType]` to exclude it wherever it appears as a property, or list types at the assembly level:

```csharp
using Neovolve.Configuration.DependencyInjection.Generated;

[SkipConfigType]
public class DiagnosticState
{
    public string Value { get; set; } = string.Empty;
}

// or, for types declared elsewhere:
[assembly: SkipConfigType(typeof(SomeThirdPartyType))]
```

The generator also reports diagnostic **NCDI001** when a configuration type in the graph cannot be hot reloaded (see [Configuration types must be mutable classes to hot reload](#configuration-types-must-be-mutable-classes-to-hot-reload)). A code fix is included to convert a `struct` configuration type into a class.

# Recommendations

## Use read-only interface definitions for configuration types
Configuration class definitions require that properties are mutable to allow the configuration binding system to set the values. There is a risk of an application class mutating the configuration data after it is injected into a class constructor. The way to prevent unintended mutation of configuration data at runtime is to define a read-only interface for the configuration class. This will allow the configuration system to set the values but the application code will not be able to change the values.

The `ConfigureWith<T>` extension method supports this by registering any configuration interfaces found under the root configuration class.

## Properties for child configuration types should be classes
Assuming that any configuration interfaces hide unnecessary child configuration types, all properties that represent child configuration types should be defined as their classes rather than interfaces on the parent configuration class. The `ConfigureWith<T>` extension method walks the type hierarchy from the root configuration type at compile time using a source generator, finding and recursing through all the properties.

For example, if the `First` property on `RootConfig` above was defined as `IFirstConfig` rather than `FirstConfig` then the `Second` property on `FirstConfig` would not be found and registered as a service. This is because the `IFirstConfig` does not define the `Second` property but `FirstConfig` does.

## Avoid resolving the root config service

The root configuration type provided to `ConfigureWith<T>` is registered as a service of `T` however using this service would typically break [Law of Demeter](https://en.wikipedia.org/wiki/Law_of_Demeter) in the application. Additionally, `ConfigureWith<T>` explicitly removes the service registrations of the root config for the `IOptions<T>`, `IOptionsSnapshot<T>` or `IOptionsMonitor<T>` services as they do not support hot reload. If you really do need to resolve root level configuration then use an interface like `IRootConfig` in the example above. In this case, hot reloaded data will still not be available on these services.
