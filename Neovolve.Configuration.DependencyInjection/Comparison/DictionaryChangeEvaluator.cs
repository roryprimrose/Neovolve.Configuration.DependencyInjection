namespace Neovolve.Configuration.DependencyInjection.Comparison;

using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;

internal class DictionaryChangeEvaluator : InternalTypedChangeEvaluator<IDictionary>
{
    protected override IEnumerable<IdentifiedChange> AreEqual(string propertyPath, IDictionary originalValue,
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
            var removedMessage = BuildKeyMessage(removedKeys);
            var addedMessage = BuildKeyMessage(addedKeys);
            var result = new IdentifiedChange(propertyPath, removedMessage, addedMessage,
                $"removed {IdentifiedChange.OldValueMask} keys and added {IdentifiedChange.NewValueMask} keys");

            return [result];
        }

        var results = new List<IdentifiedChange>();

        // At this point we have the same number of items in the dictionary and all the keys are the same
        // The next check is to see if the dictionary values are the same
        foreach (DictionaryEntry firstEntry in originalValue)
        {
            var firstValue = firstEntry.Value;
            var secondValue = newValue[firstEntry.Key];

            var entryResults = next(propertyPath, firstValue, secondValue);

            results.AddRange(entryResults);
        }

        return results;
    }

    private static string BuildKeyMessage(List<string> keys)
    {
        if (keys.Count > 0)
        {
            return string.Join(", ", keys);
        }

        return "no";
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
            }
            else if (converter == null)
            {
                converter = TypeDescriptor.GetConverter(key);
            }

            if (converter != null)
            {
                if (converter.CanConvertTo(typeof(string)) == false)
                {
                    // We can't convert this type of key to a string
                    return null;
                }

                var convertedValue = converter.ConvertTo(key, typeof(string));

                if (convertedValue is string convertedKey)
                {
                    convertedKeys.Add(convertedKey);
                }
                else
                {
                    // We can't convert this type of key to a string
                    return null;
                }
            }
            else
            {
                // This key type does not give us a type converter
                return null;
            }
        }

        return convertedKeys;
    }

    public override int Order { get; } = 2;
}