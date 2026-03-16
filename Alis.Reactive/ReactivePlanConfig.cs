using System;
using Alis.Reactive.Validation;

namespace Alis.Reactive
{
    /// <summary>
    /// One-time configuration. Call at app startup.
    /// </summary>
    public static class ReactivePlanConfig
    {
        internal static IValidationExtractor? Extractor { get; private set; }

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
