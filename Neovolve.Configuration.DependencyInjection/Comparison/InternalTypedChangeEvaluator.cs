namespace Neovolve.Configuration.DependencyInjection.Comparison;

internal abstract class InternalTypedChangeEvaluator<T> : TypedChangeEvaluator<T>
{
    public virtual bool IsFinalEvaluator { get; } = false;
    public abstract int Order { get; }
}