using System.Collections.Generic;

namespace Alis.Reactive.Validation
{
    /// <summary>
    /// Describes a single field's validation rules within a form.
    /// Runtime enriches fieldId/vendor/readExpr from plan.components at boot time.
    /// </summary>
    public sealed class ValidationField
    {
        /// <summary>Model property name using dot notation (e.g. "Address.Street").</summary>
        public string FieldName { get; }

        public List<ValidationRule> Rules { get; }

        public ValidationField(string fieldName, List<ValidationRule> rules)
        {
            FieldName = fieldName;
            Rules = rules;
        }
    }
}
