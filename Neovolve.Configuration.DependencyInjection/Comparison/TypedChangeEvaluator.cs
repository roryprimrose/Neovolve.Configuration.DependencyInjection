namespace Neovolve.Configuration.DependencyInjection.Comparison;

using System.Collections.Generic;
using System.Diagnostics;

/// <summary>
///     The <see cref="TypedChangeEvaluator{T}" />
///     class is used to simplify the evaluation of values that are of a specific type.
/// </summary>
/// <typeparam name="T">The type of value to match.</typeparam>
public abstract class TypedChangeEvaluator<T> : IChangeEvaluator
{
    /// <inheritdoc />
    public IEnumerable<IdentifiedChange> FindChanges(string propertyPath, object? originalValue, object? newValue,
        NextFindChanges next)
    {
        Debug.Assert(originalValue != null);
        Debug.Assert(newValue != null);

        if (originalValue is T originalTypedValue
            && newValue is T newTypedValue)
        {
            return FindChanges(propertyPath, originalTypedValue, newTypedValue, next);
        }

        return next(propertyPath, originalValue, newValue);
    }

    /// <summary>
    ///     Determines all the changes for the specified values and how the differences should be reported.
    /// </summary>
    /// <param name="propertyPath">The property that is being updated.</param>
    /// <param name="originalValue">The original value to evaluate.</param>
    /// <param name="newValue">The new value to evaluate.</param>
    /// <param name="next">The next evaluator to use if the current evaluator cannot determine equality.</param>
    /// <returns>The comparison results.</returns>
    /// <remarks>
    ///     The <paramref name="propertyPath" /> value may be just the name of a property, or it may identify a sub value
    ///     such a MyProperty[Key] or MyProperty[0].
    /// </remarks>
    protected abstract IEnumerable<IdentifiedChange> FindChanges(string propertyPath, T originalValue, T newValue,
        NextFindChanges next);
}