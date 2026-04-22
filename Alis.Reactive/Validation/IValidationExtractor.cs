using System;
using System.Collections.Generic;

namespace Alis.Reactive.Validation
{
    /// <summary>
    /// Extracts client-side validation rules from a validator type.
    /// Implemented by FluentValidationAdapter.
    /// </summary>
    public interface IValidationExtractor
    {
        /// <summary>
        /// Extracts rule metadata from <paramref name="validatorType"/> so the runtime can
        /// enforce the same constraints in the browser. <paramref name="formId"/> scopes
        /// the extracted fields to a specific form container.
        /// </summary>
        /// <param name="validatorType">A FluentValidation validator type to introspect.</param>
        /// <param name="formId">The DOM form container id the rules will apply to.</param>
        /// <returns>Per-field rule descriptors ready to be serialized into the plan.</returns>
        List<ValidationField> ExtractRules(Type validatorType, string formId);
    }
}
