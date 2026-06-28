namespace Neovolve.Configuration.DependencyInjection.Generated
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;

    /// <summary>
    ///     The <see cref="IConfigUpdateContext" /> interface defines the operations a generated
    ///     <see cref="IConfigValueApplier{T}" /> uses to report configuration changes, read only properties and copy
    ///     failures while applying updated configuration values.
    /// </summary>
    /// <remarks>
    ///     A context instance is created for a single <c>UpdateConfig</c> call and carries the target type and logging
    ///     policy. The generated applier classifies each property at compile time and calls the matching report method, so
    ///     no runtime type inspection or boxing of value types is required. This type is infrastructure for generated code
    ///     and is not intended to be used directly from application code.
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
        ///     Generated code checks this before calling any report method so that change detection is skipped entirely when
        ///     nothing will be logged.
        /// </remarks>
        bool IsChangeLoggingEnabled { get; }

        /// <summary>
        ///     Reports a scalar property whose value may have changed and logs the difference if it did.
        /// </summary>
        /// <typeparam name="TValue">The compile time type of the property value.</typeparam>
        /// <param name="propertyPath">The name of the property.</param>
        /// <param name="previousValue">The value before the update.</param>
        /// <param name="updatedValue">The value after the update.</param>
        /// <returns><c>true</c> if a change was logged; otherwise <c>false</c>.</returns>
        /// <remarks>
        ///     The method is generic so value types are compared and formatted without boxing.
        /// </remarks>
        bool ReportValue<TValue>(string propertyPath, TValue previousValue, TValue updatedValue);

        /// <summary>
        ///     Reports a collection of scalar values, logging an entry count change or any per-element value differences.
        /// </summary>
        /// <typeparam name="TItem">The compile time element type.</typeparam>
        /// <param name="propertyPath">The name of the property.</param>
        /// <param name="previousValues">The collection before the update.</param>
        /// <param name="updatedValues">The collection after the update.</param>
        /// <returns><c>true</c> if a change was logged; otherwise <c>false</c>.</returns>
        bool ReportValues<TItem>(string propertyPath, IReadOnlyList<TItem>? previousValues,
            IReadOnlyList<TItem>? updatedValues);

        /// <summary>
        ///     Reports a collection whose elements are not scalars, logging only a change in the entry count.
        /// </summary>
        /// <param name="propertyPath">The name of the property.</param>
        /// <param name="previousValue">The collection before the update.</param>
        /// <param name="updatedValue">The collection after the update.</param>
        /// <returns><c>true</c> if a change was logged; otherwise <c>false</c>.</returns>
        /// <remarks>
        ///     Per-element differences are not logged because the elements are complex types whose default formatting carries
        ///     no useful information.
        /// </remarks>
        bool ReportCount(string propertyPath, ICollection? previousValue, ICollection? updatedValue);

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
