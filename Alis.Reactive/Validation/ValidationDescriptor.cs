using System;
using System.Collections.Generic;
using System.Linq;

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
        /// Creates a new descriptor targeting a different form, with optional field ID prefix
        /// and optional field name filter. Used when the same validator backs multiple form
        /// sections with different DOM ID prefixes (e.g. "cmb_", "live_", "hf_").
        /// </summary>
        public ValidationDescriptor WithPrefix(string formId, string prefix, params string[] fieldNames)
        {
            var filtered = fieldNames.Length > 0
                ? Fields.Where(f => Array.IndexOf(fieldNames, f.FieldName) >= 0)
                : Fields;

            var remapped = new List<ValidationField>();
            foreach (var f in filtered)
            {
                var newFieldId = prefix + f.FieldId;
                remapped.Add(new ValidationField(
                    newFieldId, f.FieldName,
                    f.Vendor, f.ReadExpr, f.Rules));
            }

            return new ValidationDescriptor(formId, remapped);
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
