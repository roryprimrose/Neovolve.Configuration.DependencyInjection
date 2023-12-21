﻿namespace Neovolve.Configuration.DependencyInjection
{
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Options;

    internal static class TypeRegistrationExtensions
    {
        public static IServiceCollection RegisterConfigInterfaceType<TConcrete, TInterface>(
            this IServiceCollection services) where TConcrete : class, TInterface
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
            services.AddTransient<IOptionsSnapshot<TInterface>>(x =>
            {
                var concreteSnapshot = x.GetRequiredService<IOptionsSnapshot<TConcrete>>();

                return new SnapshotProxy<TConcrete, TInterface>(concreteSnapshot);
            });

            // Add registration to redirect IOptions<TConcrete> to IOptions<TInterface>
            services.AddSingleton<IOptions<TInterface>>(x =>
            {
                var concreteOptions = x.GetRequiredService<IOptions<TConcrete>>();

                return new OptionsProxy<TConcrete, TInterface>(concreteOptions);
            });

            // We want this to be a singleton so that we have a single instance that we can update when configuration changes
            services.AddSingleton<TInterface>(x =>
            {
                var options = x.GetRequiredService<TConcrete>();

                return options;
            });

            return services;
        }

        public static IServiceCollection RegisterConfigType<T>(this IServiceCollection services,
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
                    options.OnChange((config, _) => { x.CopyValues(injectedValue, config); });
                }

                return injectedValue;
            });

            return services;
        }
    }
}