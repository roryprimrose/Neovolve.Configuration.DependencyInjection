namespace Neovolve.Configuration.DependencyInjection.Comparison;

internal abstract class InternalTypedChangeEvaluator<T> : TypedChangeEvaluator<T>, IInternalChangeEvaluator
{
    public virtual bool IsFinalEvaluator { get; } = false;
    public abstract int Order { get; }
}