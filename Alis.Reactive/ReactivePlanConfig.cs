using System;
using Alis.Reactive.PlanModel;
using Alis.Reactive.Validation;

namespace Alis.Reactive
{
    /// <summary>
    /// One-time startup configuration for the reactive framework.
    /// </summary>
    /// <remarks>
    /// Call <see cref="UseValidationExtractor"/> in <c>Program.cs</c> or <c>Startup.cs</c>
    /// to enable client-side validation extraction from FluentValidation validators.
    /// Without this call, views that use <c>Validate&lt;TValidator&gt;()</c> will throw at render time.
    /// </remarks>
    public static class ReactivePlanConfig
    {
        private static ValidationExtractorRegistration _validationExtractor =
            ValidationExtractorRegistration.Missing;

        internal static ValidationExtractorRegistration ValidationExtractor => _validationExtractor;

        /// <summary>
        /// Registers the validation extractor that converts FluentValidation rules to
        /// client-side validation rules.
        /// </summary>
        /// <remarks>
        /// Must be called exactly once at app startup. Calling it a second time throws
        /// to prevent accidental double-registration that would silently replace the extractor.
        /// </remarks>
        /// <param name="extractor">The extractor implementation (typically from <c>Alis.Reactive.FluentValidator</c>).</param>
        /// <exception cref="InvalidOperationException">Thrown if an extractor is already registered.</exception>
        public static void UseValidationExtractor(IValidationExtractor extractor)
        {
            _validationExtractor = _validationExtractor.Register(extractor);
        }

        /// <summary>Test-only: resets static state so UseValidationExtractor can be called again.</summary>
        internal static void Reset() => _validationExtractor = ValidationExtractorRegistration.Missing;
    }

    internal abstract class ValidationExtractorRegistration
    {
        internal static ValidationExtractorRegistration Missing { get; } =
            new MissingValidationExtractorRegistration();

        internal abstract ValidationExtractorRegistration Register(IValidationExtractor extractor);

        internal abstract IValidationExtractor RequireFor(ValidationJob job);
    }

    internal sealed class MissingValidationExtractorRegistration : ValidationExtractorRegistration
    {
        internal override ValidationExtractorRegistration Register(IValidationExtractor extractor)
        {
            if (extractor == null) throw new ArgumentNullException(nameof(extractor));
            return new RegisteredValidationExtractor(extractor);
        }

        internal override IValidationExtractor RequireFor(ValidationJob job)
        {
            if (job == null) throw new ArgumentNullException(nameof(job));
            throw new InvalidOperationException(
                $"Request at '{job.RequestUrl}' specifies validator '{job.ValidatorType.Name}' " +
                "but no IValidationExtractor is registered. " +
                "Call ReactivePlanConfig.UseValidationExtractor() at app startup.");
        }
    }

    internal sealed class RegisteredValidationExtractor : ValidationExtractorRegistration
    {
        private readonly IValidationExtractor _extractor;

        internal RegisteredValidationExtractor(IValidationExtractor extractor)
        {
            _extractor = extractor ?? throw new ArgumentNullException(nameof(extractor));
        }

        internal override ValidationExtractorRegistration Register(IValidationExtractor extractor)
        {
            if (extractor == null) throw new ArgumentNullException(nameof(extractor));
            throw new InvalidOperationException(
                "Validation extractor is already registered. " +
                "UseValidationExtractor must be called exactly once at app startup.");
        }

        internal override IValidationExtractor RequireFor(ValidationJob job)
        {
            if (job == null) throw new ArgumentNullException(nameof(job));
            return _extractor;
        }
    }
}
