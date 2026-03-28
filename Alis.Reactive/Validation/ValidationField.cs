using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Alis.Reactive.Validation
{
    /// <summary>
    /// Describes a single field's validation rules within a form.
    /// Enriched at C# render time from ComponentsMap, then at TS boot from plan.components.
    /// </summary>
    public sealed class ValidationField
    {
        /// <summary>Model property name using dot notation (e.g. "Address.Street").</summary>
        public string FieldName { get; }

        public List<ValidationRule> Rules { get; }

        /// <summary>Element ID from component registration. Null when unenriched.</summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? FieldId { get; internal set; }

        /// <summary>Vendor string ("native" or "fusion"). Null when unenriched.</summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Vendor { get; internal set; }

        /// <summary>Property path to read value (e.g. "value", "checked"). Null when unenriched.</summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? ReadExpr { get; internal set; }

        /// <summary>Coercion type from component registration (e.g. "date", "number"). Null when unenriched.</summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? CoerceAs { get; internal set; }

        /// <summary>
        /// Framework-internal. Constructed by FluentValidation extractors — not intended for direct use in views.
        /// TODO: Make constructor internal once Architecture page uses FluentValidator.
        /// </summary>
        public ValidationField(string fieldName, List<ValidationRule> rules)
        {
            FieldName = fieldName;
            Rules = rules;
        }
    }
}
