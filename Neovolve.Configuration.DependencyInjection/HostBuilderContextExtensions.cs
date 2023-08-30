namespace Neovolve.Configuration.DependencyInjection
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;

    public static class HostBuilderContextExtensions
    {
        private static readonly Type _extensionType = typeof(HostBuilderContextExtensions);

        private static readonly MethodInfo _registerConfigTypeMember =
            _extensionType.GetMethod(nameof(RegisterConfigType), BindingFlags.Static | BindingFlags.NonPublic, null,
                new[] { typeof(IServiceCollection), typeof(IConfigurationSection), typeof(bool) }, null) ??
            throw new InvalidOperationException(
                $"Unable to find {_extensionType}.{nameof(RegisterConfigType)}<T> method");

        private static readonly MethodInfo _registerConfigInterfaceTypeMember =
            _extensionType.GetMethod(nameof(RegisterConfigInterfaceType), BindingFlags.Static | BindingFlags.NonPublic,
                null, new[] { typeof(IServiceCollection), typeof(bool) }, null) ??
            throw new InvalidOperationException(
                $"Unable to find {_extensionType}.{nameof(RegisterConfigInterfaceType)}<TConcrete, TInterface> method");

        private static readonly Dictionary<Type, List<PropertyInfo>> _propertyCache = new();


        public static IHostBuilder ConfigureWith<T>(this IHostBuilder builder) where T : class
        {
            return ConfigureWith<T>(builder, true);
        }

        public static IHostBuilder ConfigureWith<T>(this IHostBuilder builder, bool reloadInjectedTypes) where T : class
        {
            return builder.RegisterConfigurationRoot<T>()
                .ConfigureServices((context, services) =>
                {
                    var owningType = typeof(T);
                    var sectionPrefix = string.Empty;

                    RegisterChildTypes(context, services, owningType, sectionPrefix, reloadInjectedTypes);
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

        private static void RegisterChildTypes(HostBuilderContext context, IServiceCollection services, Type owningType,
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
                var section = context.Configuration.GetSection(sectionPath);

                var registerConfigType = _registerConfigTypeMember.MakeGenericMethod(configType);

                // Call RegisterConfigType<PropertyType>(services, section)
                registerConfigType.Invoke(null, new object[] { services, section, reloadInjectedTypes });

                var interfaces = propertyInfo.PropertyType.GetInterfaces();

                foreach (var interfaceType in interfaces)
                {
                    var registerConfigInterfaceType =
                        _registerConfigInterfaceTypeMember.MakeGenericMethod(configType, interfaceType);

                    // Call RegisterConfigInterfaceType<PropertyType, InterfaceType>(services, section)
                    registerConfigInterfaceType.Invoke(null, new object[] { services, reloadInjectedTypes });
                }

                RegisterChildTypes(context, services, propertyInfo.PropertyType, sectionPath, reloadInjectedTypes);
            }
        }

        private static IServiceCollection RegisterConfigInterfaceType<TConcrete, TInterface>(
            IServiceCollection services, bool reloadInjectedTypes) where TConcrete : class, TInterface
            where TInterface : class
        {
            // Add registration to redirect IOptionsMonitor<TConcrete> to IOptionsMonitor<TInterface>
            services.AddSingleton<IOptionsMonitor<TInterface>>(x =>
            {
                var concreteMonitor = x.GetRequiredService<IOptionsMonitor<TConcrete>>();

                var interfaceMonitor = new MonitorRedirect<TConcrete, TInterface>(concreteMonitor);

                return interfaceMonitor;
            });

            // Add registration to redirect IOptionsSnapshot<TConcrete> to IOptionsSnapshot<TInterface>
            services.AddScoped<IOptionsSnapshot<TInterface>>(x =>
            {
                var concreteSnapshot = x.GetRequiredService<IOptionsSnapshot<TConcrete>>();

                return new SnapshotRedirect<TConcrete, TInterface>(concreteSnapshot);
            });

            // We want this to be a singleton so that we have a single instance that we can update when configuration changes
            services.AddSingleton(x =>
            {
                var options = x.GetRequiredService<IOptionsMonitor<TConcrete>>();

                var injectedValue = options.CurrentValue;
                
                // If we are auto-reloading injected types then set up the event so that we can copy across the config values
                if (reloadInjectedTypes)
                {
                    // Respond to config changes and copy across config changes to the original injected value
                    // This will work because the classes are reference types
                    options.OnChange((config, name) => { CopyValues(x, injectedValue, config); });
                }

                return (TInterface)injectedValue;
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
                    options.OnChange((config, name) => { CopyValues(x, injectedValue, config); });
                }

                return injectedValue;
            });

            return services;
        }

        private static void CopyValues<T>(IServiceProvider serviceProvider, T injectedConfig, T updatedConfig)
        {
            var targetType = typeof(T);
            var properties = GetProperties(targetType, true);

            foreach (var property in properties)
            {
                try
                {
                    var updatedValue = property.GetValue(updatedConfig, null);

                    property.SetValue(injectedConfig, updatedValue);
                }
                catch (Exception ex)
                {
                    // We have failed to copy the value across to the injected config value
                    // We don't want to fail the application so we cannot allow the exception to throw up the stack
                    // We can however attempt to obtain a logger so that we can report the failure
                    var factory = serviceProvider.GetService<ILoggerFactory>();
                    var logger = factory?.CreateLogger<T>();

                    logger?.LogError(ex, "Failed to copy hot reload config value for {targetType}.{property}", targetType, property);
                }
            }
        }

        private static IHostBuilder RegisterConfigurationRoot<T>(this IHostBuilder builder) where T : class
        {
            // This registers static values
            builder.ConfigureServices((context, services) =>
            {
                services.AddOptions<T>();

                var value = context.Configuration.Get<T>();

                services.AddSingleton(value);

                var interfaces = typeof(T).GetInterfaces();

                foreach (var interfaceType in interfaces)
                {
                    services.AddSingleton(interfaceType, value);
                }
            });

            return builder;
        }
    }

    internal class MonitorRedirect<TConcrete, TInterface> : IOptionsMonitor<TInterface>
        where TConcrete : class, TInterface
    {
        private readonly IOptionsMonitor<TConcrete> _options;

        public MonitorRedirect(IOptionsMonitor<TConcrete> options)
        {
            _options = options;
            options.OnChange((options, name) => _onChange?.Invoke(options, name));
        }

        internal event Action<TInterface, string>? _onChange;

        public TInterface Get(string name)
        {
            return _options.Get(name);
        }

        public IDisposable OnChange(Action<TInterface, string> listener)
        {
            var disposable = new ChangeTrackerDisposable(this, listener);
            
            _onChange += disposable.OnChange;
            
            return disposable;
        }

        public TInterface CurrentValue => _options.CurrentValue;

        internal sealed class ChangeTrackerDisposable : IDisposable
        {
            private readonly Action<TInterface, string> _listener;
            private readonly MonitorRedirect<TConcrete, TInterface> _monitor;

            public ChangeTrackerDisposable(MonitorRedirect<TConcrete, TInterface> monitor,
                Action<TInterface, string> listener)
            {
                _listener = listener;
                _monitor = monitor;
            }

            public void Dispose() => _monitor._onChange -= OnChange;

            public void OnChange(TInterface options, string name) => _listener.Invoke(options, name);
        }
    }

    internal class SnapshotRedirect<TConcrete, TInterface> : IOptionsSnapshot<TInterface>
        where TConcrete : class, TInterface where TInterface : class
    {
        private readonly IOptionsSnapshot<TConcrete> _options;

        public SnapshotRedirect(IOptionsSnapshot<TConcrete> options)
        {
            _options = options;
        }

        public TInterface Get(string name)
        {
            return _options.Get(name);
        }

        public TInterface Value => _options.Value;
    }
}