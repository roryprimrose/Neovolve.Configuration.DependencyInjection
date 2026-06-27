namespace Neovolve.Configuration.DependencyInjection.Generated
{
    using System.ComponentModel;

    /// <summary>
    ///     The <see cref="ConfigPropertyAccessor" /> class describes a single bindable property on a configuration type
    ///     using strongly typed delegates so the value can be read and written without runtime reflection.
    /// </summary>
    /// <remarks>
    ///     Instances of this type are produced by the source generator for each configuration type reachable from a
    ///     <c>ConfigureWith&lt;T&gt;</c> root and registered through <see cref="GeneratedConfigRegistry" />. This type is
    ///     infrastructure for generated code and is not intended to be used directly from application code.
    /// </remarks>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public sealed class ConfigPropertyAccessor
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ConfigPropertyAccessor" /> class.
        /// </summary>
        /// <param name="name">The name of the property.</param>
        /// <param name="canWrite"><c>true</c> if the property exposes a public setter; otherwise <c>false</c>.</param>
        /// <param name="isValueType"><c>true</c> if the property type is a value type or <see cref="string" />; otherwise <c>false</c>.</param>
        /// <param name="getValue">The delegate that reads the property value from a configuration instance.</param>
        /// <param name="setValue">The delegate that writes the property value to a configuration instance, or <c>null</c> when the property is read only.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="name" /> parameter is <c>null</c>.</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="getValue" /> parameter is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">The <paramref name="canWrite" /> parameter is <c>true</c> but <paramref name="setValue" /> is <c>null</c>.</exception>
        public ConfigPropertyAccessor(string name, bool canWrite, bool isValueType, Func<object, object?> getValue,
            Action<object, object?>? setValue)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            GetValue = getValue ?? throw new ArgumentNullException(nameof(getValue));

            if (canWrite && setValue == null)
            {
                throw new ArgumentException(
                    "A writable property accessor must provide a setter delegate.", nameof(setValue));
            }

            CanWrite = canWrite;
            IsValueType = isValueType;
            SetValue = setValue;
        }

        /// <summary>
        ///     Gets a value indicating whether the property exposes a public setter.
        /// </summary>
        /// <returns><c>true</c> if the property can be written; otherwise <c>false</c>.</returns>
        public bool CanWrite { get; }

        /// <summary>
        ///     Gets the delegate that reads the property value from a configuration instance.
        /// </summary>
        public Func<object, object?> GetValue { get; }

        /// <summary>
        ///     Gets a value indicating whether the property type is a value type or <see cref="string" />.
        /// </summary>
        /// <returns><c>true</c> if the property type is a value type or <see cref="string" />; otherwise <c>false</c>.</returns>
        public bool IsValueType { get; }

        /// <summary>
        ///     Gets the name of the property.
        /// </summary>
        public string Name { get; }

        /// <summary>
        ///     Gets the delegate that writes the property value to a configuration instance, or <c>null</c> when the property
        ///     is read only.
        /// </summary>
        public Action<object, object?>? SetValue { get; }
    }
}
