using System;

namespace Alis.Reactive.Validation
{
    /// <summary>
    /// Extracts client-side form validation rules from a validator type.
    /// </summary>
    public interface IFormValidationExtractor
    {
        /// <summary>
        /// Extracts client-side validation rules for a validator and form identifier.
        /// </summary>
        /// <param name="validatorType">The validator type to inspect.</param>
        /// <param name="formId">The form identifier the rules should target.</param>
        /// <returns>The extracted form validation, or <see langword="null"/> when no client rules are available.</returns>
        FormValidation? ExtractRules(Type validatorType, string formId);
    }
}
