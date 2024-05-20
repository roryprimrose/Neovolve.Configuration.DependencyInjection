namespace Neovolve.Configuration.DependencyInjection.Comparison
{
    using System;
    using System.Collections.Generic;

    internal class ComparableChangeEvaluator : InternalTypedChangeEvaluator<IComparable>
    {
        protected override IEnumerable<IdentifiedChange> FindChanges(string propertyPath, IComparable originalValue,
            IComparable newValue, NextFindChanges next)
        {
            if (originalValue.CompareTo(newValue) == 0)
            {
                return Array.Empty<IdentifiedChange>();
            }

            return [new(propertyPath, originalValue.ToString(), newValue.ToString())];
        }

        public override int Order => int.MaxValue - 1;
    }
}