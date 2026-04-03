using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Alis.Reactive.Validation
{
    /// <summary>
    /// Holds the client-side validation rules for a single form.
    /// Serialized to JSON and consumed by the TS runtime validation engine.
    /// </summary>
    public sealed class FormValidation
    {
        /// <summary>
        /// Gets the form identifier used to scope client-side validation behavior.
        /// </summary>
        public string FormId { get; }

        /// <summary>Plan identity — used by runtime to scope summary div lookup.</summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? PlanId { get; internal set; }

        /// <summary>
        /// Gets the validation fields contributed to the form.
        /// </summary>
        public IReadOnlyList<ValidationField> Fields { get; }

        /// <summary>
        /// NEVER make public. Constructed exclusively by framework builders. Public constructors
        /// on framework-owned contract types allow devs to bypass the builder API and create invalid plan state.
        /// </summary>
        internal FormValidation(string formId, List<ValidationField> fields)
        {
            FormId = formId;
            Fields = fields.ToArray();
        }
    }
}
