﻿namespace Neovolve.Configuration.DependencyInjection.Comparison;

internal abstract class InternalChangeEvaluator : IInternalChangeEvaluator
{
    public abstract IEnumerable<IdentifiedChange> FindChanges(string propertyPath, object? originalValue,
        object? newValue,
        NextFindChanges next);

    public virtual bool IsFinalEvaluator { get; } = false;
    public abstract int Order { get; }
}