using System;
using System.Collections.Generic;

namespace Alis.Reactive.Validation
{
    /// <summary>
    /// Describes a single field's validation rules within a form.
    /// Enriched at render time from registered input component contracts.
    /// </summary>
    public sealed class ClientValidationField
    {
        private readonly ValidationFieldPath _fieldName;

        public string FieldName => _fieldName.Value;
        public List<ValidationRule> Rules { get; }

        internal ClientValidationField(ValidationFieldPath fieldName, List<ValidationRule> rules)
        {
            _fieldName = fieldName ?? throw new ArgumentNullException(nameof(fieldName));
            Rules = ProjectedValidationRules.From(rules).ToPublicList();
        }

        internal ValidationFieldPath FieldPath => _fieldName;
    }
}
