namespace Neovolve.Configuration.DependencyInjection
{
    using System;
    using System.Reflection;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Options;

    internal static class RootConfigExtensions
    {
        private static readonly Type _extensionType = typeof(TypeRegistrationExtensions);

        private static readonly MethodInfo _registerConfigInterfaceTypeMember =
            _extensionType.GetMethod(nameof(TypeRegistrationExtensions.RegisterConfigInterfaceType),
                BindingFlags.Static | BindingFlags.Public,
                null, [typeof(IServiceCollection)], null)!;

        private static readonly MethodInfo _registerConfigTypeMember =
            _extensionType.GetMethod(nameof(TypeRegistrationExtensions.RegisterConfigType),
                BindingFlags.Static | BindingFlags.Public, null,
                [typeof(IServiceCollection), typeof(IConfigurationSection), typeof(IConfigureWithOptions)],
                null)!;

        public static IHostBuilder RegisterConfigurationRoot<T>(this IHostBuilder builder,
            IConfigureWithOptions options)
            where T : class
        {
            // Register the configuration types starting from the root type and recursing through all properties
            // using the path of property names as the mapping to configuration sections

            // This registers static values
            builder.ConfigureServices((context, services) =>
            {
                var value = context.Configuration.Get<T>()!;

                services.AddSingleton(value);

                // The underlying configuration support creates default registrations for IOptions<T> and related interfaces but the service resolution
                // just returns a new instance of T. This can cause great confusing in calling applications because they get a config instance from the services however it is not populated as expected
                // To avoid this for the root object, the following code will explicitly null out those registrations
                // If the application code uses services.GetService<IOptions<T>>() then it will get null back which is an expected scenario when the service is not registered
                // If the application code uses services.GetRequiredService<IOptions<T>>() then it will throw an exception which is an expected scenario when the service is not registered
                // The following code simulates the behaviour of when the service is not registered by registering null values for the services
                services.AddSingleton<IOptions<T>>(_ => null!);
                services.AddScoped<IOptionsSnapshot<T>>(_ => null!);
                services.AddSingleton<IOptionsMonitor<T>>(_ => null!);

                var configType = typeof(T);
                var interfaces = configType.GetInterfaces();

                foreach (var interfaceType in interfaces)
                {
                    services.AddSingleton(interfaceType, value);

                    // Same as the above IOptions<T> registrations for the root config, we need to wipe out the same variants for the interfaces
                    var optionsType = typeof(IOptions<>).MakeGenericType(interfaceType);
                    var snapshotType = typeof(IOptionsSnapshot<>).MakeGenericType(interfaceType);
                    var monitorType = typeof(IOptionsMonitor<>).MakeGenericType(interfaceType);

                    services.AddSingleton(optionsType, _ => null!);
                    services.AddScoped(snapshotType, _ => null!);
                    services.AddSingleton(monitorType, _ => null!);
                }

                RegisterChildTypes(context.Configuration, services, configType, Options.DefaultName,
                    options);
            });

            return builder;
        }

        private static void RegisterChildTypes(IConfiguration configuration, IServiceCollection services,
            Type owningType,
            string sectionPrefix, IConfigureWithOptions options)
        {
            // Get the reference to the RegisterConfigType method

            if (string.IsNullOrWhiteSpace(sectionPrefix) == false)
            {
                // Add the required spacer between the section values
                sectionPrefix += ":";
            }

            var properties = owningType.GetBindableProperties(options.ReloadInjectedRawTypes);

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
                    // Edge case, we can't recursively look through strings which are reference types
                    continue;
                }

                var sectionPath = sectionPrefix + propertyInfo.Name;

                RegisterSection(services, configuration, configType, sectionPath, options);
            }
        }

        private static void RegisterSection(IServiceCollection services, IConfiguration configuration, Type configType,
            string sectionPath, IConfigureWithOptions options)
        {
            var section = configuration.GetSection(sectionPath);

            var registerConfigType = _registerConfigTypeMember.MakeGenericMethod(configType);

            // Call RegisterConfigType<PropertyType>(services, section)
            registerConfigType.Invoke(null, [services, section, options]);

            var interfaces = configType.GetInterfaces();

            foreach (var interfaceType in interfaces)
            {
                var registerConfigInterfaceType =
                    _registerConfigInterfaceTypeMember.MakeGenericMethod(configType, interfaceType);

                // Call RegisterConfigInterfaceType<PropertyType, InterfaceType>(services, section)
                registerConfigInterfaceType.Invoke(null, [services]);
            }

            RegisterChildTypes(configuration, services, configType, sectionPath,
                options);
        }
    }
}