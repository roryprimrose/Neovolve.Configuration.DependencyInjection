namespace Neovolve.Configuration.DependencyInjection.Comparison;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

internal class ValueProcessor : IValueProcessor
{
    private readonly List<IChangeEvaluator> _evaluators;

    public ValueProcessor(IEnumerable<IChangeEvaluator> evaluators)
    {
        _evaluators = SortEvaluators(evaluators)!.ToList();
    }

    /// <inheritdoc />
    public IEnumerable<IdentifiedChange> FindChanges(string propertyPath, object? originalValue, object? newValue)
    {
        if (_evaluators.Count == 0)
        {
            return Array.Empty<IdentifiedChange>();
        }

        // This class uses a pipeline design where evaluators are chained together to determine if values are equal
        // At any point in the pipeline an evaluator can handle the request and return a definitive result which will short circuit the remainder of the pipeline

        // The final evaluator is a dummy evaluator that just returns that the values are equal as there has been nothing up to this point to say otherwise
        NextFindChanges finalEvaluator = (_, _, _) => Array.Empty<IdentifiedChange>();

        // If there is only one evaluator then just use that evaluator to determine if the values are equal
        if (_evaluators.Count == 1)
        {
            // Return the dummy evaluator as the final evaluator as there is no other evaluator to call
            return _evaluators[0].FindChanges(propertyPath, originalValue, newValue, finalEvaluator);
        }

        // Start with the final evaluator and work backwards through the evaluators to build the pipeline
        var next = finalEvaluator;

        // Loop backwards through the set of evaluators to build the pipeline bottom up
        // This way we will end up with next being at the start of the pipeline and execute back down through the pipeline to the final evaluator
        for (var index = _evaluators.Count - 1; index >= 0; index--)
        {
            Debug.WriteLine(next.Target);

            // Capture the current value of next so that it can be used in the lambda
            var nextFunc = next;

            var evaluator = _evaluators[index];

            // Create a new function that will call the current evaluator with the next function in the pipeline
            NextFindChanges currentFunc = (x, y, z) => evaluator.FindChanges(propertyPath, y, z, nextFunc);

            // Set the current function as the next function for the next iteration
            next = currentFunc;
        }

        // At this point, next is the first evaluator in the pipeline which allows for all the evaluators to be invoked until a definitive result is found
        return next(propertyPath, originalValue, newValue);
    }

    private IEnumerable<IChangeEvaluator>? SortEvaluators(IEnumerable<IChangeEvaluator> evaluators)
    {
        var allEvaluators = evaluators.ToList();
        var internalChangeEvaluators = allEvaluators.OfType<IInternalChangeEvaluator>().ToList();
        var fallbackEvaluators = internalChangeEvaluators.Where(x => x.IsFinalEvaluator).OrderBy(x => x.Order).ToList();
        var internalEvaluators = internalChangeEvaluators.Where(x => x.IsFinalEvaluator == false).OrderBy(x => x.Order).ToList();
        var externalEvaluators = allEvaluators.Except(internalChangeEvaluators);

        // Ordering of evaluators will be internal evaluators sorted by Order, external evaluators, then fallback evaluators sorted by Order
        foreach (var evaluator in internalEvaluators)
        {
            yield return evaluator;
        }

        foreach (var evaluator in externalEvaluators)
        {
            yield return evaluator;
        }

        foreach (var evaluator in fallbackEvaluators)
        {
            yield return evaluator;
        }
    }
}