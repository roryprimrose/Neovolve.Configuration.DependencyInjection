﻿namespace Neovolve.Configuration.DependencyInjection
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;

    public static partial class HostBuilderContextExtensions
    {
        private const string CopyValuesEventName = "Neovolve.Configuration.DependencyInjection.CopyValues";
        private static readonly Type _extensionType = typeof(HostBuilderContextExtensions);

        private static readonly MethodInfo _registerConfigTypeMember =
            _extensionType.GetMethod(nameof(RegisterConfigType), BindingFlags.Static | BindingFlags.NonPublic, null,
                new[] { typeof(IServiceCollection), typeof(IConfigurationSection), typeof(bool) }, null) ??
            throw new InvalidOperationException(
                $"Unable to find {_extensionType}.{nameof(RegisterConfigType)}<T> method");

        private static readonly MethodInfo _registerConfigInterfaceTypeMember =
            _extensionType.GetMethod(nameof(RegisterConfigInterfaceType), BindingFlags.Static | BindingFlags.NonPublic,
                null, new[] { typeof(IServiceCollection) }, null) ??
            throw new InvalidOperationException(
                $"Unable to find {_extensionType}.{nameof(RegisterConfigInterfaceType)}<TConcrete, TInterface> method");

        private static readonly ConcurrentDictionary<Type, ILogger?> _loggerCache = new();

        private static readonly Dictionary<Type, List<PropertyInfo>> _propertyCache = new();
        
        public static IHostBuilder ConfigureWith<T>(this IHostBuilder builder) where T : class
        {
            return ConfigureWith<T>(builder, true);
        }

        public static IHostBuilder ConfigureWith<T>(this IHostBuilder builder, bool reloadInjectedTypes) where T : class
        {
            _ = builder ?? throw new ArgumentNullException(nameof(builder));

            return builder.RegisterConfigurationRoot<T>()
                .ConfigureServices((context, services) =>
                {
                    var configuration = context.Configuration;

                    var owningType = typeof(T);
                    var sectionPrefix = string.Empty;

                    RegisterChildTypes(configuration, services, owningType, sectionPrefix, reloadInjectedTypes);
                });
        }

        private static void CopyValues<T>(IServiceProvider serviceProvider, T injectedConfig, T updatedConfig)
        {
            var targetType = typeof(T);
            var properties = GetProperties(targetType, true);
            var logger = GetLogger<T>(serviceProvider);

            // The IOptionsMonitor<T>.OnChange event gets triggered twice on a change notification for file based configuration
            // To prevent noise in logging and updates to properties, we want to try to detect when an actual change has been made
            // We only want to set a property value if there is a change in the value and we want to only log that changes have
            // occurred the first time a change is detected
            var changesFound = false;

            foreach (var property in properties)
            {
                try
                {
                    var oldValue = property.GetValue(injectedConfig);
                    var updatedValue = property.GetValue(updatedConfig, null);

                    if (oldValue == null &&
                        updatedValue == null)
                    {
                        // There is no change
                        continue;
                    }

                    if (oldValue != null &&
                        updatedValue != null &&
                        oldValue.Equals(updatedValue))
                    {
                        // Both values exist but they are the same
                        continue;
                    }

                    // We have a change to the configuration
                    if (changesFound == false)
                    {
                        if (logger != null)
                        {
                            LogConfigChanged(logger, targetType);
                        }

                        changesFound = true;
                    }

                    property.SetValue(injectedConfig, updatedValue);

                    if (logger != null)
                    {
                        LogConfigChanged(logger, targetType, property.Name, oldValue, updatedValue);
                    }
                }
                catch (Exception ex)
                {
                    // We have failed to copy the value across to the injected config value
                    // We don't want to fail the application so we cannot allow the exception to throw up the stack
                    // We can however attempt to obtain a logger so that we can report the failure
                    if (logger != null)
                    {
                        LogConfigCopyFail(logger, ex, targetType, property.Name);
                    }
                }
            }
        }

        private static ILogger? GetLogger<T>(IServiceProvider serviceProvider)
        {
            return _loggerCache.GetOrAdd(typeof(T), _ =>
            {
                var factory = serviceProvider.GetService<ILoggerFactory>();
                var logger = factory?.CreateLogger<T>();

                return logger;
            });
        }

        private static List<PropertyInfo> GetProperties(Type targetType, bool reloadInjectedTypes)
        {
            if (_propertyCache.TryGetValue(targetType, out var cachedProperties))
            {
                return cachedProperties;
            }

            // Either we are not auto-reloading injected types or the properties do not exist in cache yet
            var properties = (from x in targetType.GetProperties()
                              where x.CanRead && x.CanWrite && x.GetMethod.IsPublic && x.SetMethod.IsPublic
                              select x).ToList();

            // We are only going to cache the properties of the target type if auto-reloading injected types is enabled
            // because the property values will continue to be used beyond the initial bootstrapping of the application
            if (reloadInjectedTypes)
            {
                _propertyCache[targetType] = properties;
            }

            return properties;
        }

        [LoggerMessage(EventId = 5000, EventName = CopyValuesEventName, Level = LogLevel.Information,
            Message = "Configuration updated on {targetType}")]
        static partial void LogConfigChanged(ILogger logger, Type targetType);

        [LoggerMessage(EventId = 5001, EventName = CopyValuesEventName, Level = LogLevel.Debug,
            Message =
                "Configuration updated on {targetType}.{property} from '{oldValue}' to '{newValue}'")]
        static partial void LogConfigChanged(ILogger logger, Type targetType, string property, object? oldValue,
            object? newValue);

        [LoggerMessage(EventId = 5002, EventName = CopyValuesEventName, Level = LogLevel.Error,
            Message = "Failed to copy hot reload config value for {targetType}.{property}")]
        static partial void LogConfigCopyFail(ILogger logger, Exception ex, Type targetType, string property);

        private static void RegisterChildTypes(IConfiguration configuration, IServiceCollection services, Type owningType,
            string sectionPrefix, bool reloadInjectedTypes)
        {
            // Get the reference to the RegisterConfigType method

            if (string.IsNullOrWhiteSpace(sectionPrefix) == false)
            {
                // Add the required spacer between the section values
                sectionPrefix += ":";
            }

            var properties = GetProperties(owningType, reloadInjectedTypes);

            foreach (var propertyInfo in properties)
            {
                var configType = propertyInfo.PropertyType;

                if (configType.IsValueType)
                {
                    // We don't support value types
                    // Configuration types must be classes
                    continue;
                }

                if (configType == typeof(string))
                {
                    // Edge case, we can't support strings which are classes
                    continue;
                }

                var sectionPath = sectionPrefix + propertyInfo.Name;
                var section = configuration.GetSection(sectionPath);

                var registerConfigType = _registerConfigTypeMember.MakeGenericMethod(configType);

                // Call RegisterConfigType<PropertyType>(services, section)
                registerConfigType.Invoke(null, new object[] { services, section, reloadInjectedTypes });

                var interfaces = propertyInfo.PropertyType.GetInterfaces();

                foreach (var interfaceType in interfaces)
                {
                    var registerConfigInterfaceType =
                        _registerConfigInterfaceTypeMember.MakeGenericMethod(configType, interfaceType);

                    // Call RegisterConfigInterfaceType<PropertyType, InterfaceType>(services, section)
                    registerConfigInterfaceType.Invoke(null, new object[] { services });
                }

                RegisterChildTypes(configuration, services, propertyInfo.PropertyType, sectionPath, reloadInjectedTypes);
            }
        }

        private static IServiceCollection RegisterConfigInterfaceType<TConcrete, TInterface>(
            IServiceCollection services) where TConcrete : class, TInterface
            where TInterface : class
        {
            // Add registration to redirect IOptionsMonitor<TConcrete> to IOptionsMonitor<TInterface>
            services.AddSingleton<IOptionsMonitor<TInterface>>(x =>
            {
                var concreteMonitor = x.GetRequiredService<IOptionsMonitor<TConcrete>>();

                var interfaceMonitor = new MonitorProxy<TConcrete, TInterface>(concreteMonitor);

                return interfaceMonitor;
            });

            // Add registration to redirect IOptionsSnapshot<TConcrete> to IOptionsSnapshot<TInterface>
            services.AddScoped<IOptionsSnapshot<TInterface>>(x =>
            {
                var concreteSnapshot = x.GetRequiredService<IOptionsSnapshot<TConcrete>>();

                return new SnapshotProxy<TConcrete, TInterface>(concreteSnapshot);
            });

            // We want this to be a singleton so that we have a single instance that we can update when configuration changes
            services.AddSingleton<TInterface>(x =>
            {
                var options = x.GetRequiredService<TConcrete>();

                return options;
            });

            return services;
        }

        private static IServiceCollection RegisterConfigType<T>(IServiceCollection services,
            IConfigurationSection section, bool reloadInjectedTypes) where T : class
        {
            // Configure this type so that we can get access to IOptions<T>, IOptionsSnapshot<T> and IOptionsMonitor<T>
            services.Configure<T>(section);

            // Configure the injection of T as a single instance
            // We want this to be a singleton so that we have a single instance that we can update when configuration changes
            services.AddSingleton(x =>
            {
                var options = x.GetRequiredService<IOptionsMonitor<T>>();

                var injectedValue = options.CurrentValue;

                // If we are auto-reloading injected types then set up the event so that we can copy across the config values
                if (reloadInjectedTypes)
                {
                    // Respond to config changes and copy across config changes to the original injected value
                    // This will work because the classes are reference types
                    options.OnChange((config, _) => { CopyValues(x, injectedValue, config); });
                }

                return injectedValue;
            });

            return services;
        }

        private static IHostBuilder RegisterConfigurationRoot<T>(this IHostBuilder builder) where T : class
        {
            return builder.ConfigureServices((context, services) => RegisterConfigurationRoot<T>(services, context.Configuration));
        }

        private static void RegisterConfigurationRoot<T>(IServiceCollection services, IConfiguration configuration) where T : class
        {
            // This registers static values
            services.AddOptions<T>();

            var value = configuration.Get<T>();

            if (value == null)
            {
                throw new InvalidOperationException($"Failed to bind application configuration to {typeof(T)}.");
            }

            services.AddSingleton(value);

            var interfaces = typeof(T).GetInterfaces();

            foreach (var interfaceType in interfaces)
            {
                services.AddSingleton(interfaceType, value);
            }
        }
    }
}