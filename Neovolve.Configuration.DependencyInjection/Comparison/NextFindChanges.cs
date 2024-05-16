namespace Neovolve.Configuration.DependencyInjection.Comparison;

using System.Collections.Generic;

/// <summary>
///     The next evaluator to use if the current evaluator cannot determine equality.
/// </summary>
/// <param name="propertyPath">The property that is being updated.</param>
/// <param name="originalValue">The original value to evaluate.</param>
/// <param name="newValue">The new value to evaluate.</param>
/// <returns>The comparison results.</returns>
public delegate IEnumerable<IdentifiedChange> NextFindChanges(string propertyPath, object? originalValue,
    object? newValue);