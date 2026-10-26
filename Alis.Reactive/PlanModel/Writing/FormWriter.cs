using System;
using System.Collections.Generic;
using System.Linq;
using Alis.Reactive.Grammar;
using Alis.Reactive.Validation;

namespace Alis.Reactive.PlanModel
{
    /// <summary>
    /// Writes a validation container's rules as <c>alis.form</c> data: per field its value read, the
    /// name the server reports errors under, and one test per rule. A test's check is a lambda of the
    /// field's value that is true when the value passes. Every input a rule reads is read as MVC binds
    /// it, and each check is the server rule's own verdict on that value (grammar.md 7.4).
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

        internal FormPluginData Form(IReadOnlyList<CheckedField> fields) =>
            new FormPluginData(SummaryId, fields.Select(Field).ToList());

        /// <summary>The element id of the plan's validation summary, as <c>Html.RenderPlan</c> renders it.</summary>
        private string SummaryId => PlanElementId.ValidationSummaryFor(_scope.PlanId);

        private FormField Field(CheckedField field)
        {
            var tests = field.Rules
                .Select(rule => new FormTest(rule.Name, Check(rule, field), When(rule, field.Values), rule.Message))
                .ToList();
            return new FormField(field.ComponentId, field.ServerName, field.Value.Write(_scope), tests);
        }

        private Expr When(ClientRule rule, FormFieldValues values) => rule.Condition.ToGraph(values).Write(_scope);

        // The rule's verdict on the field's value, as the server's rule decides it.
        private FnExpr Check(ClientRule rule, CheckedField field) =>
            G.Fn(rule.Check(G.Ref(Value), field.Shape, field.Values, _scope), Value);
    }
}
