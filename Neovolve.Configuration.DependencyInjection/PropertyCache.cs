namespace Neovolve.Configuration.DependencyInjection
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    internal static class PropertyCache
    {
        private static readonly Dictionary<Type, List<PropertyInfo>> _propertyCache = new();

        public static IEnumerable<PropertyInfo> GetBindableProperties(this Type targetType, bool cacheProperties)
        {
            if (_propertyCache.TryGetValue(targetType, out var cachedProperties))
            {
                return cachedProperties;
            }

            // Either we are not auto-reloading injected types or the properties do not exist in cache yet
            var properties = (from x in targetType.GetProperties()
                where x.CanRead && x.GetMethod.IsPublic
                select x).ToList();

            // We are only going to cache the properties of the target type if auto-reloading injected types is enabled
            // because the property values will continue to be used beyond the initial bootstrapping of the application
            if (cacheProperties)
            {
                _propertyCache[targetType] = properties;
            }

            return properties;
        }
    }
}