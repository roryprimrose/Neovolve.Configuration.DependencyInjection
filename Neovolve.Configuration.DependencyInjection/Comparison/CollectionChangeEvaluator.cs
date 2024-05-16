namespace Neovolve.Configuration.DependencyInjection.Comparison;

using System;
using System.Collections.Generic;

internal class CollectionChangeEvaluator : InternalChangeEvaluator
{
    public override IEnumerable<IdentifiedChange> FindChanges(string propertyPath, object? originalValue, object? newValue,
        NextFindChanges next)
    {
        throw new NotImplementedException();
    }

    public override int Order { get; } = 3;
}