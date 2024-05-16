namespace Neovolve.Configuration.DependencyInjection.Comparison;

using System;
using System.Collections.Generic;
using System.Diagnostics;

internal class EqualsChangeEvaluator : InternalChangeEvaluator
{
    public override IEnumerable<IdentifiedChange> FindChanges(string propertyPath, object? originalValue,
        object? newValue,
        NextFindChanges next)
    {
        Debug.Assert(originalValue != null,
            "first should never be null because other internal evaluators should have returned a result before this is executed");

        if (originalValue!.Equals(newValue))
        {
            return Array.Empty<IdentifiedChange>();
        }

        var firstLogValue = GetLogValue(originalValue);
        var secondLogValue = GetLogValue(newValue);

        return [new IdentifiedChange(propertyPath, firstLogValue, secondLogValue)];
    }

    private string GetLogValue(object? value)
    {
        return value?.ToString() ?? "null";
    }

    public override bool IsFinalEvaluator => true;
    public override int Order => int.MaxValue;
}