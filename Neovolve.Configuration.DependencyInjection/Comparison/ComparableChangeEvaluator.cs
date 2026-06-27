namespace Neovolve.Configuration.DependencyInjection.Comparison
{
    internal class ComparableChangeEvaluator : InternalTypedChangeEvaluator<IComparable>
    {
        protected override IEnumerable<IdentifiedChange> FindChanges(string propertyPath, IComparable originalValue,
            IComparable newValue, NextFindChanges next)
        {
            if (originalValue.CompareTo(newValue) == 0)
            {
                return [];
            }

            return [new(propertyPath, originalValue.ToString() ?? string.Empty, newValue.ToString() ?? string.Empty)];
        }

        public override int Order => int.MaxValue - 1;
    }
}