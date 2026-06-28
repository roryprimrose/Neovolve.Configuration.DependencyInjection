namespace Neovolve.Configuration.DependencyInjection.Generated
{
    using System;
    using System.ComponentModel;

    /// <summary>
    ///     The <see cref="IConfigUpdateContext" /> interface defines the operations a generated
    ///     <see cref="IConfigValueApplier{T}" /> uses to report configuration changes, read only properties and copy
    ///     failures while applying updated configuration values.
    /// </summary>
    /// <remarks>
    ///     A context instance is created for a single <c>UpdateConfig</c> call and carries the target type, logger and
    ///     change detection services. This type is infrastructure for generated code and is not intended to be used directly
    ///     from application code.
    /// </remarks>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public interface IConfigUpdateContext
    {
        /// <summary>
        ///     Gets a value indicating whether change logging is enabled for this update.
        /// </summary>
        /// <returns>
        ///     <c>true</c> if a logger is available and enabled at the configured property change level; otherwise
        ///     <c>false</c>.
        /// </returns>
        /// <remarks>
        ///     Generated code checks this before boxing values into a <see cref="Report" /> call so that the change detection
        ///     path is skipped entirely when nothing will be logged.
        /// </remarks>
        bool IsChangeLoggingEnabled { get; }

        /// <summary>
        ///     Reports that a writable property changed and logs the difference.
        /// </summary>
        /// <param name="propertyPath">The name of the property that changed.</param>
        /// <param name="previousValue">The value before the update.</param>
        /// <param name="updatedValue">The value after the update.</param>
        /// <returns><c>true</c> if a change was logged; otherwise <c>false</c>.</returns>
        bool Report(string propertyPath, object? previousValue, object? updatedValue);

        /// <summary>
        ///     Reports that a read only property cannot be updated, applying the configured read only logging policy.
        /// </summary>
        /// <param name="propertyPath">The name of the read only property.</param>
        /// <param name="isValueType"><c>true</c> if the property type is a value type or <see cref="string" />; otherwise <c>false</c>.</param>
        void ReportReadOnly(string propertyPath, bool isValueType);

        /// <summary>
        ///     Reports that copying a property value failed.
        /// </summary>
        /// <param name="propertyName">The name of the property that failed to copy.</param>
        /// <param name="exception">The exception that occurred while copying the value.</param>
        void ReportCopyFailure(string propertyName, Exception exception);
    }
}
