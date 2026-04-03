using System;
using Alis.Reactive.Validation;

namespace Alis.Reactive
{
    /// <summary>
    /// One-time startup configuration for the reactive framework.
    /// </summary>
    /// <remarks>
    /// Call <see cref="UseFormValidationExtractor"/> in <c>Program.cs</c> or <c>Startup.cs</c>
    /// to enable client-side validation extraction from FluentValidation validators.
    /// Without this call, views that use <c>Validate&lt;TValidator&gt;()</c> will throw at render time.
    /// </remarks>
    public static class ReactivePlanConfig
    {
        /// <summary>Gets the registered form-validation extractor, or <see langword="null"/> if none is registered.</summary>
        internal static IFormValidationExtractor? FormValidationExtractor { get; private set; }

        /// <summary>
        /// Registers the extractor that converts validator rules into client-side form validation.
        /// </summary>
        /// <remarks>
        /// Must be called exactly once at app startup. Calling it a second time throws
        /// to prevent accidental double-registration that would silently replace the extractor.
        /// </remarks>
        /// <param name="extractor">The extractor implementation (typically from <c>Alis.Reactive.FluentValidator</c>).</param>
        /// <exception cref="InvalidOperationException">Thrown if an extractor is already registered.</exception>
        public static void UseFormValidationExtractor(IFormValidationExtractor extractor)
        {
            if (FormValidationExtractor != null)
                throw new InvalidOperationException(
                    "Form validation extractor is already registered. " +
                    "UseFormValidationExtractor must be called exactly once at app startup.");
            FormValidationExtractor = extractor;
        }

        /// <summary>Test-only: resets static state so UseFormValidationExtractor can be called again.</summary>
        internal static void Reset() => FormValidationExtractor = null;
    }
}
