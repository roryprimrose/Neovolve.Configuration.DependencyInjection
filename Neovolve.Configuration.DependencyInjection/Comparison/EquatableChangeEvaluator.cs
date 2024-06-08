namespace Neovolve.Configuration.DependencyInjection.Comparison
{
    using System.Collections.Concurrent;
    using System.Diagnostics;
    using System.Reflection;

    internal class EquatableChangeEvaluator : InternalChangeEvaluator
    {
        private readonly ConcurrentDictionary<Type, MethodInfo> _methodCache = new();

        public override IEnumerable<IdentifiedChange> FindChanges(string propertyPath, object? originalValue,
            object? newValue, NextFindChanges next)
        {
            Debug.Assert(originalValue != null);
            Debug.Assert(newValue != null);

            var method = GetFindMethod(originalValue, newValue);

            if (method == null)
            {
                // The type is not IEquatable<T>
                return next(propertyPath, originalValue, newValue);
            }

            // Invoke the typed FindEquatableChanges method
            var result =
                (IEnumerable<IdentifiedChange>)method.Invoke(this,
                    [propertyPath, originalValue, newValue!])!;

            return result;
        }

        private IEnumerable<IdentifiedChange> FindEquatableChanges<T>(string propertyPath, IEquatable<T> originalValue,
            IEquatable<T> newValue)
        {
            if (originalValue.Equals(newValue))
            {
                return [];
            }

            var change = new IdentifiedChange(propertyPath, originalValue.ToString(), newValue.ToString());

            return [change];
        }

        private MethodInfo? GetFindMethod(object? originalValue, object? newValue)
        {
            var targetType = originalValue!.GetType();

            if (_methodCache.TryGetValue(targetType, out var method))
            {
                return method;
            }

            var equatableBase = typeof(IEquatable<>);

            var equatableType = equatableBase.MakeGenericType(targetType);

            // Check if originalValue and newValue implement equatableType
            if (equatableType.IsInstanceOfType(originalValue) == false)
            {
                // Default behavior if originalValue does not implement equatableType
                return null;
            }

            if (equatableType.IsInstanceOfType(newValue) == false)
            {
                // Default behavior if newValue does not implement equatableType
                return null;
            }

            // Get the typed FindChanges method
            var bindingFlags = BindingFlags.NonPublic | BindingFlags.Instance;
            var findChangesMethod =
                GetType().GetMethod(nameof(FindEquatableChanges), bindingFlags);

            Debug.Assert(findChangesMethod != null);

            var typedFindChangesMethod = findChangesMethod!.MakeGenericMethod(targetType);

            return _methodCache.AddOrUpdate(targetType, typedFindChangesMethod, (_, y) => y);
        }

        public override int Order => int.MaxValue - 2;
    }
}