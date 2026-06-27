namespace Neovolve.Configuration.DependencyInjection
{
    using System.Collections.Concurrent;
    using Neovolve.Configuration.DependencyInjection.Generated;

    /// <summary>
    ///     The <see cref="ReflectionPropertyAccessorFactory" /> class builds <see cref="ConfigPropertyAccessor" /> instances
    ///     for a configuration type using reflection.
    /// </summary>
    /// <remarks>
    ///     This factory is the transitional fallback used while the source generator is being rolled out. It is only used for
    ///     configuration types that the generator has not registered accessors for through
    ///     <see cref="GeneratedConfigRegistry" />.
    /// </remarks>
    internal static class ReflectionPropertyAccessorFactory
    {
        private static readonly ConcurrentDictionary<Type, ConfigPropertyAccessor[]> _cache = new();

        public static ConfigPropertyAccessor[] GetAccessors(Type targetType)
        {
            return _cache.GetOrAdd(targetType, BuildAccessors);
        }

        private static ConfigPropertyAccessor[] BuildAccessors(Type targetType)
        {
            var properties = from property in targetType.GetProperties()
                where property.CanRead && property.GetMethod!.IsPublic && property.GetIndexParameters().Length == 0
                select property;

            var accessors = new List<ConfigPropertyAccessor>();

            foreach (var property in properties)
            {
                var canWrite = property.CanWrite && property.SetMethod!.IsPublic;
                var isValueType = property.PropertyType.IsValueType || property.PropertyType == typeof(string);

                var captured = property;

                Func<object, object?> getValue = instance => captured.GetValue(instance);

                Action<object, object?>? setValue = canWrite
                    ? (instance, value) => captured.SetValue(instance, value)
                    : null;

                accessors.Add(new ConfigPropertyAccessor(property.Name, canWrite, isValueType, getValue, setValue));
            }

            return accessors.ToArray();
        }
    }
}
