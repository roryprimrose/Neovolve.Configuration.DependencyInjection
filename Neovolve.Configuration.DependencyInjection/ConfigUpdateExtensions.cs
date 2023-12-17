namespace Neovolve.Configuration.DependencyInjection
{
    using System;
    using System.Collections.Concurrent;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    internal static partial class ConfigUpdateExtensions
    {
        private static readonly ConcurrentDictionary<Type, ILogger?> _loggerCache = new();

        public static void CopyValues<T>(this IServiceProvider serviceProvider, T injectedConfig, T updatedConfig)
        {
            if (injectedConfig == null)
            {
                return;
            }

            if (updatedConfig == null)
            {
                return;
            }

            var targetType = typeof(T);
            var properties = targetType.GetBindableProperties(true);
            var logger = GetLogger<T>(serviceProvider);

            // The IOptionsMonitor<T>.OnChange event gets triggered twice on a change notification for file based configuration
            // To prevent noise in logging and updates to properties, we want to try to detect when an actual change has been made
            // We only want to set a property value if there is a change in the value and we want to only log that changes have
            // occurred the first time a change is detected
            var changesFound = false;

            foreach (var property in properties)
            {
                if (property.CanWrite == false
                    || property.SetMethod.IsPublic == false)
                {
                    if (logger != null)
                    {
                        LogConfigCopyDenied(logger, targetType, property.Name);
                    }

                    continue;
                }

                try
                {
                    var oldValue = property.GetValue(injectedConfig);
                    var updatedValue = property.GetValue(updatedConfig, null);

                    if (oldValue == null
                        && updatedValue == null)
                    {
                        // There is no change
                        continue;
                    }

                    if (oldValue != null
                        && updatedValue != null
                        && oldValue.Equals(updatedValue))
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
    }
}