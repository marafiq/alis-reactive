using System;
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
        /// <summary>Gets the registered validation extractor, or <see langword="null"/> if none is registered.</summary>
        internal static IValidationExtractor? Extractor { get; private set; }

        /// <summary>
        /// Registers the validation extractor that converts FluentValidation rules to
        /// client-side validation descriptors.
        /// </summary>
        /// <remarks>
        /// Must be called exactly once at app startup. Calling it a second time throws
        /// to prevent accidental double-registration that would silently replace the extractor.
        /// </remarks>
        /// <param name="extractor">The extractor implementation (typically from <c>Alis.Reactive.FluentValidator</c>).</param>
        /// <exception cref="InvalidOperationException">Thrown if an extractor is already registered.</exception>
        public static void UseValidationExtractor(IValidationExtractor extractor)
        {
            if (Extractor != null)
                throw new InvalidOperationException(
                    "Validation extractor is already registered. " +
                    "UseValidationExtractor must be called exactly once at app startup.");
            Extractor = extractor;
        }

        /// <summary>Test-only: resets static state so UseValidationExtractor can be called again.</summary>
        internal static void Reset() => Extractor = null;
    }
}
