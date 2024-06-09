namespace Neovolve.Configuration.DependencyInjection.Comparison;

internal class ReferenceChangeEvaluator : InternalChangeEvaluator
{
    public override IEnumerable<IdentifiedChange> FindChanges(string propertyPath, object? originalValue,
        object? newValue,
        NextFindChanges next)
    {
        if (ReferenceEquals(originalValue, newValue))
        {
            return [];
        }

        return next(propertyPath, originalValue, newValue);
    }

    public override int Order => 1;
}