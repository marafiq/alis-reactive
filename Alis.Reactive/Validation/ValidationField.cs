using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Alis.Reactive.Validation
{
    /// <summary>
    /// Holds the validation rules for a single binding within a form.
    /// Validation now flows through bindings in the V2 plan rather than through
    /// per-field component enrichment.
    /// </summary>
    public sealed class ValidationField
    {
        /// <summary>Model property name using dot notation (e.g. "Address.Street").</summary>
        public string FieldName { get; }

        /// <summary>
        /// Gets the validation rules associated with the binding.
        /// </summary>
        public IReadOnlyList<ValidationRule> Rules { get; }

        /// <summary>
        /// NEVER make public. Constructed exclusively by framework builders. Public constructors
        /// on framework-owned contract types allow devs to bypass the builder API and create invalid plan state.
        /// </summary>
        internal ValidationField(string fieldName, List<ValidationRule> rules)
        {
            FieldName = fieldName;
            Rules = rules.ToArray();
        }
    }
}
