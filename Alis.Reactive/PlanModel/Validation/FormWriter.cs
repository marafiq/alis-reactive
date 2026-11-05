using System.Collections.Generic;
using System.Linq;
using Alis.Reactive.Grammar;
using Alis.Reactive.PlanModel.Writing;

namespace Alis.Reactive.PlanModel.Validation
{
    internal sealed class FormWriter
    {
        private const string Value = "v";

        private readonly Scope _scope;

        internal FormWriter(Scope scope)
        {
            _scope = scope;
        }

        internal FormPluginData Form(string formId, IReadOnlyList<CheckedField> fields)
            => new FormPluginData(FormSummaries.IdOf(formId), fields.Select(Field).ToList());

        private FormField Field(CheckedField field)
        {
            var tests = field.Rules
                .Select(rule => new FormTest(Check(rule, field), When(rule, field.Values), rule.Message))
                .ToList();
            return new FormField(field.Name, tests);
        }

        private FnExpr When(BrowserRule rule, FormFieldValues values)
            => G.Fn(rule.Condition.Bound(values).Write(_scope), FormFieldValues.Indexes);

        private FnExpr Check(BrowserRule rule, CheckedField field)
            => G.Fn(rule.Check(field.Values.Checked(field.Reference, G.Ref(Value), _scope), field.Shape, field.Values, _scope), Value, FormFieldValues.Indexes);
    }
}
