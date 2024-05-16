namespace Neovolve.Configuration.DependencyInjection.Comparison;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;

internal class CollectionChangeEvaluator : InternalTypedChangeEvaluator<ICollection>
{
    protected override IEnumerable<IdentifiedChange> FindChanges(string propertyPath, ICollection originalValue,
        ICollection newValue,
        Func<string, object?, object?, IEnumerable<IdentifiedChange>> next)
    {
        if (originalValue.Count != newValue.Count)
        {
            var originalCount = originalValue.Count.ToString(CultureInfo.CurrentCulture);
            var newCount = newValue.Count.ToString(CultureInfo.CurrentCulture);
            var result = new IdentifiedChange(propertyPath, originalCount,
                newCount,
                $"from {IdentifiedChange.OldValueMask} entries to {IdentifiedChange.NewValueMask} entries");

            return [result];
        }

        var results = new List<IdentifiedChange>();

        // Get a reference to the same values by index for each iteration through the loop
        var originalEnumerator = originalValue.GetEnumerator();
        var newEnumerator = newValue.GetEnumerator();

        // At this point we have the same number of items in the dictionary and all the keys are the same
        // The next check is to see if the dictionary values are the same
        while (originalEnumerator.MoveNext()
               && newEnumerator.MoveNext())
        {
            var firstValue = originalEnumerator.Current;
            var secondValue = newEnumerator.Current;

            var entryResults = next(propertyPath, firstValue, secondValue);

            results.AddRange(entryResults);
        }

        return results;
    }

    public override int Order { get; } = 3;
}