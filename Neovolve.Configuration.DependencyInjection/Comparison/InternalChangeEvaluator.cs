namespace Neovolve.Configuration.DependencyInjection.Comparison;

using System;
using System.Collections.Generic;

internal abstract class InternalChangeEvaluator : IChangeEvaluator
{
    public abstract IEnumerable<IdentifiedChange> FindChanges(string propertyPath, object? originalValue,
        object? newValue,
        Func<string, object?, object?, IEnumerable<IdentifiedChange>> next);

    public virtual bool IsFinalEvaluator { get; } = false;
    public abstract int Order { get; }
}