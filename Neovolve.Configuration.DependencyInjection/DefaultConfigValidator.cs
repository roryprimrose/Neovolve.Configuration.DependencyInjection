namespace Neovolve.Configuration.DependencyInjection
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;

    /// <summary>
    ///     The <see cref="DefaultConfigValidator" /> class runs the registered <see cref="IValidateOptions{TOptions}" />
    ///     validators for a configuration type so reloads can be rejected when validation fails.
    /// </summary>
    /// <remarks>
    ///     Validators are resolved from the service provider, so data annotation validation, source generated validators
    ///     and custom <see cref="IValidateOptions{TOptions}" /> implementations all participate. When no validators are
    ///     registered every value is treated as valid.
    /// </remarks>
    internal partial class DefaultConfigValidator : IConfigValidator
    {
        private readonly IServiceProvider _provider;

        /// <summary>
        ///     Initializes a new instance of the <see cref="DefaultConfigValidator" /> class.
        /// </summary>
        /// <param name="provider">The service provider used to resolve the registered validators.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="provider" /> parameter is <c>null</c>.</exception>
        public DefaultConfigValidator(IServiceProvider provider)
        {
            _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        }

        /// <inheritdoc />
        public bool IsValid<T>(T value, string? name, ILogger? logger) where T : class
        {
            var result = Run(value, name);

            if (result == null || result.Succeeded)
            {
                return true;
            }

            if (logger != null)
            {
                LogValidationFailed(logger, typeof(T), string.Join("; ", result.Failures ?? Array.Empty<string>()));
            }

            return false;
        }

        /// <summary>
        ///     Validates a configuration value's data annotation attributes, throwing when validation fails.
        /// </summary>
        /// <typeparam name="T">The configuration type being validated.</typeparam>
        /// <param name="value">The configuration value to validate.</param>
        /// <exception cref="OptionsValidationException">Validation failed for one or more data annotation attributes.</exception>
        internal static void ThrowOnInvalidDataAnnotations<T>(T value)
        {
            if (value == null)
            {
                return;
            }

            var results = new List<ValidationResult>();
            var context = new ValidationContext(value);

            if (Validator.TryValidateObject(value, context, results, true))
            {
                return;
            }

            var failures = new List<string>();

            foreach (var result in results)
            {
                failures.Add(result.ErrorMessage ?? "Validation failed.");
            }

            throw new OptionsValidationException(Options.DefaultName, typeof(T), failures);
        }

        private ValidateOptionsResult? Run<T>(T value, string? name) where T : class
        {
            var validators = _provider.GetServices<IValidateOptions<T>>();
            var ran = false;
            var failures = new List<string>();

            foreach (var validator in validators)
            {
                ran = true;

                var result = validator.Validate(name, value);

                if (result.Failed)
                {
                    failures.AddRange(result.Failures);
                }
            }

            if (ran == false)
            {
                return null;
            }

            if (failures.Count > 0)
            {
                return ValidateOptionsResult.Fail(failures);
            }

            return ValidateOptionsResult.Success;
        }
    }
}
