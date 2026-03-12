using System.Collections.Generic;

namespace Alis.Reactive.Validation
{
    /// <summary>
    /// Describes all validation rules for a single form.
    /// Serialized to JSON and consumed by the TS runtime validation engine.
    /// </summary>
    public sealed class ValidationDescriptor
    {
        public string FormId { get; }
        public List<ValidationField> Fields { get; }

        public ValidationDescriptor(string formId, List<ValidationField> fields)
        {
            FormId = formId;
            Fields = fields;
        }
    }
}
