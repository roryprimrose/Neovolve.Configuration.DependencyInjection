namespace Neovolve.Configuration.DependencyInjection.Comparison;

internal class NullChangeEvaluator : InternalChangeEvaluator
{
    public override IEnumerable<IdentifiedChange> FindChanges(string propertyPath, object? originalValue,
        object? newValue,
        NextFindChanges next)
    {
        if (originalValue == null
            && newValue == null)
        {
            return Array.Empty<IdentifiedChange>();
        }

        if (originalValue != null
            && newValue != null)
        {
            // Both values exist so continue to other evaluators
            return next(propertyPath, originalValue, newValue);
        }

        if (originalValue != null)
        {
            // The second value is null
            return [new(propertyPath, "not null", "null")];
        }

        // The first value is null
        return [new(propertyPath, "null", "not null")];
    }

    // This should always be the first evaluator. No other evaluator should ever see a null value.
    public override int Order => 0;
}