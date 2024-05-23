namespace Neovolve.Configuration.DependencyInjection.Comparison
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;

    internal class EquatableChangeEvaluator : InternalChangeEvaluator
    {
        public override IEnumerable<IdentifiedChange> FindChanges(string propertyPath, object? originalValue,
            object? newValue, NextFindChanges next)
        {
            Debug.Assert(originalValue != null);
            Debug.Assert(newValue != null);

            var equatableBase = typeof(IEquatable<>);
            var equatableType = equatableBase.MakeGenericType(originalValue!.GetType());

            // Check if originalValue and newValue implement equatableType
            if (equatableType.IsInstanceOfType(originalValue) == false)
            {
                // Default behavior if originalValue does not implement equatableType
                return next(propertyPath, originalValue, newValue);
            }

            if (equatableType.IsInstanceOfType(newValue) == false)
            {
                // Default behavior if newValue does not implement equatableType
                return next(propertyPath, originalValue, newValue);
            }

            // Get the typed FindChanges method
            var typedFindChangesMethod = GetType().GetMethod(nameof(FindChanges), [
                typeof(string), equatableType, equatableType, typeof(NextFindChanges)
            ]);

            // Invoke the typed FindChanges method
            var result =
                (IEnumerable<IdentifiedChange>)typedFindChangesMethod?.Invoke(this,
                    [propertyPath, originalValue, newValue!, next])!;

            return result;

        }

        public IEnumerable<IdentifiedChange> FindChanges<T>(string propertyPath, IEquatable<T> originalValue,
            IEquatable<T> newValue, NextFindChanges next)
        {
            if (originalValue.Equals(newValue))
            {
                return Array.Empty<IdentifiedChange>();
            }
            
            return next(propertyPath, originalValue, newValue);
        }
        
        public override int Order => int.MaxValue - 2;
    }
}