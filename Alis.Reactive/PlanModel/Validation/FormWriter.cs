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
            => new FormPluginData(FormSummaries.IdOf(formId), FormBody.Live(_scope, formId), fields.Select(Field).ToList());

        private FormField Field(CheckedField field)
        {
            var tests = field.Rules
                .Select(rule => new FormTest(Check(rule, field), When(rule, field.Values), rule.Message))
                .ToList();
            var value = G.Fn(field.Values.Value(field.Reference).Write(_scope), FormFieldValues.Indexes, FormFieldValues.Sent);
            var items = G.Fn(field.Values.Items(field.Reference, _scope), FormFieldValues.Sent);
            return new FormField(field.Name, items, value, tests);
        }

        private FnExpr When(BrowserRule rule, FormFieldValues values)
            => G.Fn(rule.Condition.Bound(values).Write(_scope), FormFieldValues.Indexes, FormFieldValues.Sent);

        private FnExpr Check(BrowserRule rule, CheckedField field)
            => G.Fn(rule.Check(G.Ref(Value), field.Shape, field.Values, _scope), Value, FormFieldValues.Indexes, FormFieldValues.Sent);
    }
}
