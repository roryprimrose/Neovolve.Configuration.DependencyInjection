namespace Neovolve.Configuration.DependencyInjection.Comparison;

using System.Collections.Generic;

/// <summary>
///     The <see cref="IValueProcessor" />
///     interface defines the methods for evaluating whether a value has changed.
/// </summary>
internal interface IValueProcessor
{
    /// <summary>
    ///     Determines all the changes for the specified values and how the differences should be reported.
    /// </summary>
    /// <param name="propertyPath">The property path that identifies the property being changed.</param>
    /// <param name="originalValue">The original value.</param>
    /// <param name="newValue">The updated value.</param>
    /// <returns>A <see cref="IdentifiedChange" /> value.</returns>
    IEnumerable<IdentifiedChange> FindChanges(string propertyPath, object? originalValue, object? newValue);
}