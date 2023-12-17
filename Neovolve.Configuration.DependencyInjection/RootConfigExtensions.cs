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
                null, new[] { typeof(IServiceCollection) }, null) ??
            throw new InvalidOperationException(
                $"Unable to find {_extensionType}.{nameof(TypeRegistrationExtensions.RegisterConfigInterfaceType)}<TConcrete, TInterface>(IServiceCollection services) method");

        private static readonly MethodInfo _registerConfigTypeMember =
            _extensionType.GetMethod(nameof(TypeRegistrationExtensions.RegisterConfigType),
                BindingFlags.Static | BindingFlags.Public, null,
                new[] { typeof(IServiceCollection), typeof(IConfigurationSection), typeof(bool) }, null) ??
            throw new InvalidOperationException(
                $"Unable to find {_extensionType}.{nameof(TypeRegistrationExtensions.RegisterConfigType)}<T>(IServiceCollection services, IConfigurationSection section, bool reloadInjectedTypes) method");

        public static IHostBuilder RegisterConfigurationRoot<T>(this IHostBuilder builder, bool reloadInjectedTypes)
            where T : class
        {
            // Register the configuration types starting from the root type and recursing through all properties
            // using the path of property names as the mapping to configuration sections

            // This registers static values
            builder.ConfigureServices((context, services) =>
            {
                var value = context.Configuration.Get<T>()!;

                services.AddSingleton(value);

                var configType = typeof(T);
                var interfaces = configType.GetInterfaces();

                foreach (var interfaceType in interfaces)
                {
                    services.AddSingleton(interfaceType, value);
                }

                RegisterChildTypes(context.Configuration, services, configType, Options.DefaultName,
                    reloadInjectedTypes);
            });

            return builder;
        }

        private static void RegisterChildTypes(IConfiguration configuration, IServiceCollection services,
            Type owningType,
            string sectionPrefix, bool reloadInjectedTypes)
        {
            // Get the reference to the RegisterConfigType method

            if (string.IsNullOrWhiteSpace(sectionPrefix) == false)
            {
                // Add the required spacer between the section values
                sectionPrefix += ":";
            }

            var properties = owningType.GetBindableProperties(reloadInjectedTypes);

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

                RegisterSection(services, configuration, configType, sectionPath, reloadInjectedTypes);
            }
        }

        private static void RegisterSection(IServiceCollection services, IConfiguration configuration, Type configType,
            string sectionPath, bool reloadInjectedTypes)
        {
            var section = configuration.GetSection(sectionPath);

            var registerConfigType = _registerConfigTypeMember.MakeGenericMethod(configType);

            // Call RegisterConfigType<PropertyType>(services, section)
            registerConfigType.Invoke(null, new object[] { services, section, reloadInjectedTypes });

            var interfaces = configType.GetInterfaces();

            foreach (var interfaceType in interfaces)
            {
                var registerConfigInterfaceType =
                    _registerConfigInterfaceTypeMember.MakeGenericMethod(configType, interfaceType);

                // Call RegisterConfigInterfaceType<PropertyType, InterfaceType>(services, section)
                registerConfigInterfaceType.Invoke(null, new object[] { services });
            }

            RegisterChildTypes(configuration, services, configType, sectionPath,
                reloadInjectedTypes);
        }
    }
}