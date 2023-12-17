namespace Neovolve.Configuration.DependencyInjection
{
    /// <summary>
    ///     The <see cref="ConfigureWithOptions" />
    ///     class is used to configure how options binding behaves.
    /// </summary>
    public class ConfigureWithOptions
    {
        /// <summary>
        ///     Returns the default configuration options for binding configuration types.
        /// </summary>
        public static readonly ConfigureWithOptions Default = new ConfigureWithOptions();

        /// <summary>
        ///     Gets or sets the log category to use when logging messages for configuration binding.
        /// </summary>
        public LogCategory LogCategory = LogCategory.TargetType;

        /// <summary>
        ///     Gets or sets the custom log category used when <see cref="LogCategory" /> is
        ///     <see cref="DependencyInjection.LogCategory.Custom" />.
        /// </summary>
        public string CustomLogCategory { get; set; } = string.Empty;

        /// <summary>
        ///     Gets or sets when log warning messages are written as read only properties are encountered.
        /// </summary>
        public ReadOnlyPropertyWarning LogReadOnlyPropertyWarning { get; set; } =
            ReadOnlyPropertyWarning.PrimitiveTypesOnly;

        /// <summary>
        ///     Gets or sets whether injected raw types are updated with reloaded configuration.
        /// </summary>
        public bool ReloadInjectedRawTypes { get; set; } = true;
    }
}