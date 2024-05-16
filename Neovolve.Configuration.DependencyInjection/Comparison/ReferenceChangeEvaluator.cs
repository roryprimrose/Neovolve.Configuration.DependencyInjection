namespace Neovolve.Configuration.DependencyInjection.Comparison;

using System;
using System.Collections.Generic;

internal class ReferenceChangeEvaluator : InternalChangeEvaluator
{
    public override IEnumerable<IdentifiedChange> FindChanges(string propertyPath, object? originalValue,
        object? newValue,
        Func<string, object?, object?, IEnumerable<IdentifiedChange>> next)
    {
        if (ReferenceEquals(originalValue, newValue))
        {
            return Array.Empty<IdentifiedChange>();
        }

        return next(propertyPath, originalValue, newValue);
    }

    public override int Order => 1;
}