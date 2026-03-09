using System.Collections.Generic;

namespace Alis.Reactive.Validation
{
    /// <summary>
    /// Describes a single field's validation rules within a form.
    /// </summary>
    public sealed class ValidationField
    {
        /// <summary>DOM element ID (e.g. "Address_Street").</summary>
        public string FieldId { get; }

        /// <summary>Model property name using dot notation (e.g. "Address.Street").</summary>
        public string FieldName { get; }

        /// <summary>Error span element ID (e.g. "err_Address_Street").</summary>
        public string ErrorId { get; }

        /// <summary>Component vendor: "fusion" or "native".</summary>
        public string Vendor { get; }

        /// <summary>
        /// JS expression to read the field value. Null for native inputs (uses .value).
        /// For fusion components, e.g. "ej2_instances[0].value".
        /// </summary>
        public string? ReadExpr { get; }

        public List<ValidationRule> Rules { get; }

        public ValidationField(
            string fieldId,
            string fieldName,
            string errorId,
            string vendor,
            string? readExpr,
            List<ValidationRule> rules)
        {
            FieldId = fieldId;
            FieldName = fieldName;
            ErrorId = errorId;
            Vendor = vendor;
            ReadExpr = readExpr;
            Rules = rules;
        }
    }
}
