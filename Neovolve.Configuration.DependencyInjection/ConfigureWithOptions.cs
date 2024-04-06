﻿namespace Neovolve.Configuration.DependencyInjection
{
    using Microsoft.Extensions.Logging;

    /// <summary>
    ///     The <see cref="ConfigureWithOptions" />
    ///     class is used to configure how options binding behaves.
    /// </summary>
    public class ConfigureWithOptions : IConfigureWithOptions
    {
        /// <inheritdoc />
        public string CustomLogCategory { get; set; } = string.Empty;

        /// <inheritdoc />
        public LogCategoryType LogCategoryType { get; set; } = LogCategoryType.TargetType;

        /// <inheritdoc />
        public LogLevel LogReadOnlyPropertyLevel { get; set; } = LogLevel.Warning;

        /// <inheritdoc />
        public LogReadOnlyPropertyType LogReadOnlyPropertyType { get; set; } =
            LogReadOnlyPropertyType.ValueTypesOnly;

        /// <inheritdoc />
        public bool ReloadInjectedRawTypes { get; set; } = true;
    }
}