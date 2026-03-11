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

        /// <summary>
        /// Sets readExpr on the named field. Used when a field needs a non-default
        /// readExpr (e.g. checkboxes use "checked" instead of "value").
        /// </summary>
        public ValidationDescriptor WithReadExpr(string fieldName, string readExpr)
        {
            var updated = new List<ValidationField>();
            foreach (var f in Fields)
            {
                if (f.FieldName == fieldName)
                {
                    updated.Add(new ValidationField(
                        f.FieldId, f.FieldName, f.Vendor, readExpr, f.Rules));
                }
                else
                {
                    updated.Add(f);
                }
            }
            return new ValidationDescriptor(FormId, updated);
        }
    }
}
