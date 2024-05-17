namespace Neovolve.Configuration.DependencyInjection.Comparison;

internal interface IInternalChangeEvaluator : IChangeEvaluator
{
    bool IsFinalEvaluator { get; }
    int Order { get; }
}