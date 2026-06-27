namespace Neovolve.Configuration.DependencyInjection.Comparison
{
    using System.Collections.Concurrent;
    using System.Diagnostics;

    internal class EquatableChangeEvaluator : InternalChangeEvaluator
    {
        private readonly ConcurrentDictionary<Type, bool> _equatableCache = new();

        public override IEnumerable<IdentifiedChange> FindChanges(string propertyPath, object? originalValue,
            object? newValue, NextFindChanges next)
        {
            Debug.Assert(originalValue != null);
            Debug.Assert(newValue != null);

            // This evaluator only claims values that expose value equality via IEquatable<T> for the same
            // runtime type. Anything else defers to the next evaluator (for example IComparable handling)
            // so the pipeline ordering is preserved.
            if (IsEquatableOfSameType(originalValue!, newValue!) == false)
            {
                return next(propertyPath, originalValue, newValue);
            }

            // A correct IEquatable<T> implementation overrides object.Equals to delegate to its strongly
            // typed Equals(T), so the value comparison can be performed without resolving and invoking the
            // generic method through reflection.
            if (originalValue!.Equals(newValue))
            {
                return [];
            }

            var change = new IdentifiedChange(propertyPath, originalValue.ToString() ?? string.Empty,
                newValue!.ToString() ?? string.Empty);

            return [change];
        }

        private bool IsEquatableOfSameType(object originalValue, object newValue)
        {
            var targetType = originalValue.GetType();

            if (newValue.GetType() != targetType)
            {
                // The two values are different runtime types, so they cannot share a single IEquatable<T>
                // implementation for that type.
                return false;
            }

            return _equatableCache.GetOrAdd(targetType, static type =>
            {
                var equatableType = typeof(IEquatable<>);

                foreach (var implemented in type.GetInterfaces())
                {
                    if (implemented.IsGenericType
                        && implemented.GetGenericTypeDefinition() == equatableType
                        && implemented.GenericTypeArguments.Length == 1
                        && implemented.GenericTypeArguments[0] == type)
                    {
                        return true;
                    }
                }

                return false;
            });
        }

        public override int Order => int.MaxValue - 2;
    }
}