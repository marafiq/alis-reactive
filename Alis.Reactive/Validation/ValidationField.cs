using System.Collections.Generic;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Validation
{
    /// <summary>
    /// Describes a single field's validation rules within a form.
    /// Enriched at render time from ComponentsMap.
    /// </summary>
    public sealed class ValidationField
    {
        public string FieldName { get; }
        public List<ValidationRule> Rules { get; }
        public string FieldId { get; internal set; }
        public string Vendor { get; internal set; }
        public string ValueMember { get; internal set; }
        public Shape Shape { get; internal set; }

        internal ValidationField(string fieldName, List<ValidationRule> rules)
        {
            FieldName = fieldName;
            Rules = rules;
        }
    }
}
