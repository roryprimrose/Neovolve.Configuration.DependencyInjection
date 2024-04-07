# Introduction

The Neovolve.Configuration.DependencyInjection NuGet package provides `IHostBuilder` extension methods for adding dependency injection support for nested configuration types with hot reload support.

[![GitHub license](https://img.shields.io/badge/License-MIT-blue.svg)](https://github.com/roryprimrose/Neovolve.Configuration.DependencyInjection/blob/master/LICENSE)&nbsp;&nbsp;&nbsp;[![Nuget](https://img.shields.io/nuget/v/Neovolve.Configuration.DependencyInjection.svg)&nbsp;&nbsp;&nbsp;![Nuget](https://img.shields.io/nuget/dt/Neovolve.Configuration.DependencyInjection.svg)](https://www.nuget.org/packages/Neovolve.Configuration.DependencyInjection)

[![Actions Status](https://github.com/roryprimrose/Neovolve.Configuration.DependencyInjection/workflows/CI/badge.svg)](https://github.com/roryprimrose/Neovolve.Configuration.DependencyInjection/actions)

# Installation

The package can be installed from NuGet using ```Install-Package Neovolve.Configuration.DependencyInjection```.

# Usage
This package requires that the application bootstrapping provide a root configuration type that defines the configuration structure that the application uses. 

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
    .ConfigureWith<RootConfig>()
```

Given the above example, the following services would be registered with the host application:

| Type | IOptions&lt;T&gt; | IOptionsSnapshot&lt;T&gt; | IOptionsMonitor&lt;T&gt; | Supports hot reload |
|-|-|-|-|
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
The options binding system in .NET Core supports hot reload of configuration data. This is typically done by watching the configuration source for changes and then reloading the configuration data. This is useful for scenarios where configuration data is stored in a file and the application needs to react to changes in the file without needing to restart the application. This support is provided by the `IOptionsSnapshot<>` and `IOptionsMonitor<>` services.

One of the benefits of this package is that it supports hot reloading of injected raw configuration services by default. A raw type is a configuration class and its defined interfaces that are found under the root configuration type. In this definition, a raw type is anything other than `IOption<>`, `IOptionsSnapshot<>` or `IOptionsMonitor<>`. 

In the above configuration example, the raw types are:
- IRootConfig
- RootConfig
- IFirstConfig
- FirstConfig
- ISecondConfig
- SecondConfig
- IThirdConfig
- ThirdConfig

This package detects when a configuration change has occurred by watching `IOptionsMonitor<>.OnChange` on all configuration services registered under the root configuration type. The package then updates the existing raw type in memory which works because the raw types are registered as singleton services. This allows the application class to receive updated configuration data at runtime without needing to use `IOptionsMonitor<T>` but injecting the raw configuration type instead. Logging is provided as the mechanism for recording that an injected raw type has been updated.

The main reason to use `IOptionsMonitor<>` instead of the raw type is when the application class wants to hook into the `IOptionsMonitor.OnChange` method itself to run some custom code when the configuration changes.

The hot reload support for raw configuration types can be disabled by setting the `ReloadInjectedRawTypes` option to `false` in the `ConfigureWith<T>` overload.

# Options
The following are the default options that `ConfigureWith<T>` uses.

| Option | Type | Default | Description |
|-|-|-|-|
| CustomLogCategory | `string` | `string.Empty` | The custom log category used when `LogCategoryType` is `LogCategoryType.Custom` |
| LogCategoryType | `LogCategoryType` | `LogCategoryType.TargetType` | The log category to use when logging messages for configuration updates on raw types. Supported values are `TargetType`, `LibraryType` and `Custom`. |
| LogReadOnlyPropertyLevel | `LogLevel` | `LogLevel.Warning` in Development; otherwise `Debug` | The log level to use when logging that updates are detected for read-only properties |
| LogReadOnlyPropertyType | `LogReadOnlyPropertyType` | `LogReadOnlyPropertyType.ValueTypesOnly` | The types of read-only properties to log when they are updated. Supported values are `All`, `ValueTypesOnly` and `None.` |
| ReloadInjectedRawTypes | `bool` | `true` | Determines if raw types that are injected into the configuration system should be reloaded when the configuration changes |

These options can be set in the `ConfigureWith<T>` overload.

```csharp
var builder = Host.CreateDefaultBuilder()
    .ConfigureWith<RootConfig>(x => {
        x.CustomLogCategory = "MyCustomCategory";
        x.LogCategoryType = LogCategoryType.Custom;
        x.LogReadOnlyPropertyLevel = LogLevel.Information;
        x.LogReadOnlyPropertyType = LogReadOnlyPropertyType.All;
        x.ReloadInjectedRawTypes = false;
    });
```

# Recommendations

## Use read-only interface definitions for configuration types
Configuration class definitions require that properties are mutable to allow the configuration system to set the values. There is a risk of an application class mutating the configuration data after it is injected into a class constructor. The way to prevent unintended mutation of configuration data at runtime is to define read-only interfaces for the configuration types. This will allow the configuration system to set the values but the application code will not be able to change the values.

The `ConfigureWith<T>` extension method supports this by registering any configuration interfaces found under the root configuration type.

## Properties for child configuration types should be classes
Assuming that any configuration interfaces hide unnecessary child configuration types, all properties that represent child configuration types should be defined as their classes rather than interfaces. The `ConfigureWith<T>` extension method uses reflection to walk the type hierarchy from the root configuration type by finding and recursing through all the properties.

For example, if the `First` property on `RootConfig` above was defined as `IFirstConfig` rather than `FirstConfig` then the `Second` property on `FirstConfig` would not be found and registered as a service. This is because the `IFirstConfig` does not define the `Second` property but `FirstConfig` does.

## Avoid resolving the root config service

The root configuration type provided to `ConfigureWith<T>` is registered as a service of `T` however using this service would typically break [Law of Demeter](https://en.wikipedia.org/wiki/Law_of_Demeter) in the application. Additionally, `ConfigureWith<T>` explicitly removes the service registrations of the root config for the `IOptions<T>`, `IOptionsSnapshot<T>` or `IOptionsMonitor<T>` services as they do not support hot reload. If you really do need to resolve root level configuration then use an interface like `IRootConfig` in the example above. In this case, hot reloaded data will still not be available on these services.
