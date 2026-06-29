namespace Neovolve.Configuration.DependencyInjection
{
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
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
            services.AddScoped<IOptionsSnapshot<TInterface>>(x =>
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
            IConfigurationSection section) where T : class
        {
            // Bind the type through the options pipeline and enforce data annotation validation at startup so invalid
            // configuration fails fast instead of silently binding. Validation runs through any registered
            // IValidateOptions<T> (data annotations here, plus source generated or custom validators), keeping the
            // path reflection free for AOT when source generated validators are supplied.
            services.AddOptions<T>().Bind(section).ValidateDataAnnotations().ValidateOnStart();

            // Configure the injection of T as a single instance
            // We want this to be a singleton so that we have a single instance that we can update when configuration changes
            services.AddSingleton(c =>
            {
                var monitor = c.GetRequiredService<IOptionsMonitor<T>>();

                var injectedValue = monitor.CurrentValue;

                var options = c.GetRequiredService<IConfigureWithOptions>();

                // If we are auto-reloading injected types then set up the event so that we can copy across the config values
                if (options.ReloadInjectedRawTypes)
                {
                    // Respond to config changes and copy across config changes to the original injected value
                    // This will work because the classes are reference types
                    monitor.OnChange((config, name) =>
                    {
                        // Figure out the logger to use
                        var factory = c.GetService<ILoggerFactory>();
                        ILogger? logger = null;

                        if (factory != null)
                        {
                            // Calculate the logger category based on the provided options

                            if (options.LogCategoryType == LogCategoryType.Custom)
                            {
                                logger = factory.CreateLogger(options.CustomLogCategory);
                            }
                            else if (options.LogCategoryType == LogCategoryType.TargetType)
                            {
                                logger = factory.CreateLogger(injectedValue.GetType());
                            }
                            else
                            {
                                logger = factory.CreateLogger(typeof(TypeRegistrationExtensions).Namespace
                                                              + ".ConfigureWith");
                            }
                        }

                        // Validate the reloaded configuration before applying it. A failed reload keeps the previously
                        // valid configuration so partial, fragmented updates are never applied (all-or-nothing).
                        var validator = c.GetService<IConfigValidator>();

                        if (validator != null
                            && validator.IsValid(config, name, logger) == false)
                        {
                            return;
                        }

                        var updater = c.GetRequiredService<IConfigUpdater>();

                        updater.UpdateConfig(injectedValue, config, name, logger);
                    });
                }

                return injectedValue;
            });

            return services;
        }
    }
}