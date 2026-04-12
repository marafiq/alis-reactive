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
        List<ValidationField> ExtractRules(Type validatorType, string formId);
    }
}
