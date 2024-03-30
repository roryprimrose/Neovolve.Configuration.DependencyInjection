namespace Neovolve.Configuration.DependencyInjection
{
    /// <summary>
    ///     The <see cref="ConfigureWithOptions" />
    ///     class is used to configure how options binding behaves.
    /// </summary>
    public class ConfigureWithOptions : IConfigureWithOptions
    {
        /// <inheritdoc />
        public string CustomLogCategory { get; set; } = string.Empty;
        
        /// <inheritdoc />
        public LogCategory LogCategory { get; set; } = LogCategory.TargetType;
        
        /// <inheritdoc />
        public ReadOnlyPropertyWarning LogReadOnlyPropertyWarning { get; set; } =
            ReadOnlyPropertyWarning.None;
        
        /// <inheritdoc />
        public bool ReloadInjectedRawTypes { get; set; } = true;
    }
}