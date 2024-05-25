namespace Neovolve.Configuration.DependencyInjection
{
    using System.Collections;
    using System.Collections.ObjectModel;
    using System.Reflection;
    using Microsoft.Extensions.Logging;

    /// <summary>
    ///     The <see cref="ConfigureWithOptions" />
    ///     class is used to configure how options binding behaves.
    /// </summary>
    public class ConfigureWithOptions : IConfigureWithOptions
    {
        /// <inheritdoc />
        public string CustomLogCategory { get; set; } = "Neovolve.Configuration.DependencyInjection.ConfigureWith";

        /// <inheritdoc />
        public LogCategoryType LogCategoryType { get; set; } = LogCategoryType.TargetType;

        /// <inheritdoc />
        public LogLevel LogReadOnlyPropertyLevel { get; set; } = LogLevel.Warning;

        /// <inheritdoc />
        public LogReadOnlyPropertyType LogReadOnlyPropertyType { get; set; } =
            LogReadOnlyPropertyType.ValueTypesOnly;

        /// <inheritdoc />
        public bool ReloadInjectedRawTypes { get; set; } = true;

        /// <inheritdoc />
        public Collection<Type> SkipPropertyTypes { get; } =
            [typeof(IEnumerable), typeof(Type), typeof(Assembly), typeof(Stream)];
    }
}