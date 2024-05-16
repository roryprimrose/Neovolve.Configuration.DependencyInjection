namespace Neovolve.Configuration.DependencyInjection.Comparison;

using System;

/// <summary>
///     The <see cref="IdentifiedChange" />
///     record defines the result of a comparison between two values.
/// </summary>
public class IdentifiedChange
{
    /// <summary>
    ///     Defines the default message format that is used to log the change in value.
    /// </summary>
    public const string DefaultMessageFormat = $"from '{OldValueMask}' to '{NewValueMask}'";

    /// <summary>
    ///     Defines the default message prefix applied to the message format.
    /// </summary>
    public const string DefaultMessageFormatPrefix = "Configuration updated on property {TargetType}.{PropertyPath} ";

    /// <summary>
    ///     Defines the mask used to identify the new value in the message format.
    /// </summary>
    public const string NewValueMask = "{NewValue}";

    /// <summary>
    ///     Defines the mask used to identify the old value in the message format.
    /// </summary>
    public const string OldValueMask = "{OldValue}";

    /// <summary>
    ///     Initializes a new instance of the <see cref="IdentifiedChange" /> class.
    /// </summary>
    /// <param name="propertyPath">The property path that identifies the property being changed.</param>
    /// <param name="firstLogValue">The log value that represents the first value.</param>
    /// <param name="secondLogValue">The log value that represents the second value.</param>
    /// <param name="messageFormat">The format of the log message that records the change in value.</param>
    /// <returns>The comparison result.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="messageFormat" /> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException">
    ///     The <paramref name="messageFormat" /> value must contain {OldValue} and {NewValue}
    ///     placeholders.
    /// </exception>
    /// <remarks>
    ///     The <paramref name="propertyPath" /> value may be just the name of a property, or it may identify a sub value
    ///     such a MyProperty[Key] or MyProperty[0].
    /// </remarks>
    public IdentifiedChange(string propertyPath, string firstLogValue, string secondLogValue,
        string messageFormat = DefaultMessageFormat)
    {
        MessageFormat = messageFormat ?? throw new ArgumentNullException(nameof(messageFormat));

        if (MessageFormat.Contains(OldValueMask) == false
            || MessageFormat.Contains(NewValueMask) == false)
        {
            throw new ArgumentException(
                $"The log message format must contain {OldValueMask} and {NewValueMask} placeholders",
                nameof(messageFormat));
        }

        PropertyPath = propertyPath;
        FirstLogValue = firstLogValue;
        SecondLogValue = secondLogValue;
        MessageFormat = DefaultMessageFormatPrefix + messageFormat;
    }

    /// <summary>
    ///     Gets the log value that represents the first value.
    /// </summary>
    public string FirstLogValue { get; }

    /// <summary>
    ///     Gets the message format used for logging the change to the property value.
    /// </summary>
    public string MessageFormat { get; }

    /// <summary>
    ///     Gets the property path that identifies the property being changed.
    /// </summary>
    public string PropertyPath { get; }

    /// <summary>
    ///     Gets the log value that represents the second value.
    /// </summary>
    public string SecondLogValue { get; }
}