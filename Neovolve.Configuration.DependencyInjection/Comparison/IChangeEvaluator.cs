namespace Neovolve.Configuration.DependencyInjection.Comparison
{
    using System.Collections.Generic;

    /// <summary>
    ///     The <see cref="IChangeEvaluator" />
    ///     interface defines the methods for evaluating values and converting them to strings for logging.
    /// </summary>
    public interface IChangeEvaluator
    {
        /// <summary>
        ///     Determines all the changes for the specified values and how the differences should be reported.
        /// </summary>
        /// <param name="propertyPath">The property that is being evaluated.</param>
        /// <param name="originalValue">The original value to evaluate.</param>
        /// <param name="newValue">The new value to evaluate.</param>
        /// <param name="next">The next evaluator to use if the current evaluator cannot determine equality.</param>
        /// <returns>The comparison results.</returns>
        /// <remarks>
        ///     The <paramref name="propertyPath" /> value may be just the name of a property, or it may identify a sub value
        ///     such a MyProperty[Key] or MyProperty[0].
        /// </remarks>
        IEnumerable<IdentifiedChange> FindChanges(string propertyPath, object? originalValue, object? newValue,
            NextFindChanges next);
    }
}