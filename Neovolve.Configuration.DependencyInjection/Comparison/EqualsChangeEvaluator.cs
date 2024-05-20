namespace Neovolve.Configuration.DependencyInjection.Comparison;

using System;
using System.Collections.Generic;

internal class EqualsChangeEvaluator : InternalChangeEvaluator
{
    public override IEnumerable<IdentifiedChange> FindChanges(string propertyPath, object? originalValue,
        object? newValue,
        NextFindChanges next)
    {
        if (Equals(originalValue, newValue))
        {
            return Array.Empty<IdentifiedChange>();
        }

        var firstLogValue = GetLogValue(originalValue);
        var secondLogValue = GetLogValue(newValue);

        return [new(propertyPath, firstLogValue, secondLogValue)];
    }

    private static string GetLogValue(object? value)
    {
        return value?.ToString() ?? "null";
    }

    public override bool IsFinalEvaluator => true;
    public override int Order => int.MaxValue;
}