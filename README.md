# Introduction

The Neovolve.Configuration.DependencyInjection NuGet package provides `IHostBuilder` extension methods for adding dependency injection support for nested configuration types with hot reload support.

[![GitHub license](https://img.shields.io/badge/License-MIT-blue.svg)](https://github.com/roryprimrose/Neovolve.Configuration.DependencyInjection/blob/master/LICENSE)&nbsp;&nbsp;&nbsp;[![Nuget](https://img.shields.io/nuget/v/Neovolve.Configuration.DependencyInjection.svg)&nbsp;&nbsp;&nbsp;![Nuget](https://img.shields.io/nuget/dt/Neovolve.Configuration.DependencyInjection.svg)](https://www.nuget.org/packages/Neovolve.Configuration.DependencyInjection)

[![Actions Status](https://github.com/roryprimrose/Neovolve.Configuration.DependencyInjection/workflows/CI/badge.svg)](https://github.com/roryprimrose/Neovolve.Configuration.DependencyInjection/actions)

# Installation

The package can be installed from NuGet using ```Install-Package Neovolve.Configuration.DependencyInjection```.

# Usage

# Limitations

This extension uses type information to recurse through all the available properties and their types defined against the root configuration type. If any of the property types are defined as interfaces then the extension will register configuration types according to those interface definitions rather than any concrete type that the property may hold at runtime. This reason for this is that the extesion does not bind the configuration objects to recurse through the runtime configuration data but instead relies purely on the type information.

For example consider the following type structure:

```csharp

public interface IConfig
{
    string RootValue { get; }
}

public class Config : IConfig
{
    public IFirstConfig First { get; } = new FirstConfig();

    public string RootValue { get; set; }
}

public interface IFirstConfig
{
    string FirstValue { get; }
}

public class FirstConfig : IFirstConfig
{
    public string FirstValue { get; set; }

    public SecondConfig Second { get; } = new();
}

public interface ISecondConfig
{
    string SecondValue { get; }
}

public class SecondConfig : ISecondConfig
{
    string SecondValue { get; set; }
}

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Host.ConfigureWith<Config>();
```

Using this extension on `Config` would result in the following service registrations:

```
Config
IConfig
IOptions<Config>
IOptions<IConfig>
ISnapshotOptions<Config>
ISnapshotOptions<IConfig>
IMonitorOptions<Config>
IMonitorOptions<IConfig>
IOptions<FirstConfig>
IOptions<IFirstConfig>
ISnapshotOptions<FirstConfig>
ISnapshotOptions<IFirstConfig>
MonitorOptions<FirstConfig>
IMonitorOptions<IFirstConfig>
```

There would be no registrations for the following types:

```
IOptions<SecondConfig>
IOptions<ISecondConfig>
ISnapshotOptions<SecondConfig>
ISnapshotOptions<ISecondConfig>
MonitorOptions<SecondConfig>
IMonitorOptions<ISecondConfig>
```

You will need to define the properties on configuration types that provides the greatest reach of type information possible. The additional types above would be registered if the config class defined the `First` property type as `FirstConfig` rather than `IFirstConfig`.

```csharp
public class Config : IConfig
{
    public FirstConfig First { get; } = new FirstConfig();

    public string RootValue { get; set; }
}
```

When `Config.First` is `FirstConfig` rather than `IFirstConfig` then the type searching logic can find the `Second` property on `FirstConfig` which is not defined on the interface.