namespace Neovolve.Configuration.DependencyInjection.Comparison;

using System.Collections;
using System.ComponentModel;
using System.Globalization;

internal class DictionaryChangeEvaluator : InternalTypedChangeEvaluator<IDictionary>
{
    protected override IEnumerable<IdentifiedChange> FindChanges(string propertyPath, IDictionary originalValue,
        IDictionary newValue,
        NextFindChanges next)
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

        // Convert keys to List<string> if possible
        var firstKeys = KeysAsStrings(originalValue);

        if (firstKeys == null)
        {
            // We can't convert the keys to strings, so we cannot determine whether the dictionaries are the same or different
            return next(propertyPath, originalValue, newValue);
        }

        var secondKeys = KeysAsStrings(newValue);

        if (secondKeys == null)
        {
            // We can't convert the keys to strings, so we cannot determine whether the dictionaries are the same or different
            return next(propertyPath, originalValue, newValue);
        }

        // Find keys removed from first and keys added to second
        var removedKeys = firstKeys.Except(secondKeys).ToList();
        var addedKeys = secondKeys.Except(firstKeys).ToList();

        if (removedKeys.Count > 0
            || addedKeys.Count > 0)
        {
            // Because of the initial check on the counts, if we have keys removed and added then those must both be greater than 0
            var removedMessage = string.Join(", ", removedKeys);
            var addedMessage = string.Join(", ", addedKeys);

            var result = new IdentifiedChange(propertyPath, removedMessage, addedMessage,
                $"removed keys {IdentifiedChange.OldValueMask} and added keys {IdentifiedChange.NewValueMask}");

            return [result];
        }

        var results = new List<IdentifiedChange>();

        // At this point we have the same number of items in the dictionary and all the keys are the same
        // The next check is to see if the dictionary values are the same
        foreach (DictionaryEntry firstEntry in originalValue)
        {
            var firstValue = firstEntry.Value;
            var secondValue = newValue[firstEntry.Key];

            var entryResults = next($"{propertyPath}[{firstEntry.Key}]", firstValue, secondValue);

            results.AddRange(entryResults);
        }

        return results;
    }

    private static List<string>? KeysAsStrings(IDictionary dictionary)
    {
        var convertedKeys = new List<string>();
        var keys = dictionary.Keys;

        if (keys.Count == 0)
        {
            return convertedKeys;
        }

        TypeConverter? converter = null;

        foreach (var key in keys)
        {
            if (key is string keyAsString)
            {
                convertedKeys.Add(keyAsString);

                continue;
            }

            if (converter == null)
            {
                converter = TypeDescriptor.GetConverter(key);
            }

            if (converter.CanConvertTo(typeof(string)) == false)
            {
                // We can't convert this type of key to a string
                return null;
            }

            var convertedValue = (string)converter.ConvertTo(key, typeof(string));

            convertedKeys.Add(convertedValue);
        }

        return convertedKeys;
    }

    public override int Order { get; } = 2;
}