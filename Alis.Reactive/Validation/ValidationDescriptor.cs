using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Alis.Reactive.Validation
{
    /// <summary>
    /// Describes all validation rules for a single form.
    /// Serialized to JSON and consumed by the TS runtime validation engine.
    /// </summary>
    public sealed class ValidationDescriptor
    {
        public string FormId { get; }

        /// <summary>Plan identity — used by runtime to scope summary div lookup.</summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? PlanId { get; internal set; }

        public List<ValidationField> Fields { get; }

        public ValidationDescriptor(string formId, List<ValidationField> fields)
        {
            FormId = formId;
            Fields = fields;
        }
    }
}
