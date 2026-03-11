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

        /// <summary>Component vendor: "fusion" or "native".</summary>
        public string Vendor { get; }

        /// <summary>
        /// Property path from vendor-determined root. Vendor resolves root
        /// (native→el, fusion→ej2_instances[0]), then readExpr is walked from that root.
        /// Examples: "value", "checked".
        /// </summary>
        public string ReadExpr { get; }

        public List<ValidationRule> Rules { get; }

        public ValidationField(
            string fieldId,
            string fieldName,
            string vendor,
            string readExpr,
            List<ValidationRule> rules)
        {
            FieldId = fieldId;
            FieldName = fieldName;
            Vendor = vendor;
            ReadExpr = readExpr;
            Rules = rules;
        }
    }
}
