using System.Collections.Generic;
using System.Linq;
using Alis.Reactive.Grammar;
using Alis.Reactive.Validation;

namespace Alis.Reactive.PlanModel
{
    /// <summary>
    /// Writes a validation container's rules as <c>alis.form</c> data: per field the binding name the server
    /// reports errors under (an item's standing for every item, <c>Lines[*].Qty</c>), and one test per rule. A
    /// test's check is a lambda of the field's value and the indexes of the item it checks, true when the value
    /// passes; its condition is a lambda of the indexes. Every input a rule reads is read as MVC binds it, and each
    /// check is the server rule's own verdict on that value (grammar.md 7.4).
    /// </summary>
    internal sealed class FormWriter
    {
        private const string Value = "v";

        // A rule's reads wait for no promise: it is checked on every keystroke, where nothing waits.
        private readonly Scope _scope;

        internal FormWriter(Scope scope)
        {
            _scope = scope;
        }

        /// <summary>The form's data: its summary, the form's own whichever plan renders it, and its fields.</summary>
        internal FormPluginData Form(string formId, IReadOnlyList<CheckedField> fields) =>
            new FormPluginData(FormSummaries.IdOf(formId), fields.Select(Field).ToList());

        private FormField Field(CheckedField field)
        {
            var tests = field.Rules
                .Select(rule => new FormTest(rule.Name, Check(rule, field), When(rule, field.Values), rule.Message))
                .ToList();
            return new FormField(field.Name, tests);
        }

        // Whether the rule applies to the item at the indexes the form hands it.
        private FnExpr When(BrowserRule rule, FormFieldValues values) =>
            G.Fn(rule.Condition.Bound(values).Write(_scope), FormInputRead.Indexes);

        // The rule's verdict on the field's value, as the server's rule decides it.
        private FnExpr Check(BrowserRule rule, CheckedField field) =>
            G.Fn(rule.Check(G.Ref(Value), field.Shape, field.Values, _scope), Value, FormInputRead.Indexes);
    }
}
