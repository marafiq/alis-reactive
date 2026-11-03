using System;
using System.Collections.Generic;
using System.Linq;
using Alis.Reactive.Grammar;
using Alis.Reactive.Inputs;
using Alis.Reactive.PlanModel.Conditions;
using Alis.Reactive.PlanModel.Values;
using Alis.Reactive.PlanModel.Writing;

namespace Alis.Reactive.PlanModel.Validation
{
    /// <summary>
    /// The condition of a rule written under <c>DependentRules</c>: every browser rule of the rule it depends on
    /// passes over that member's value, or does not apply. It is bound to the form's inputs where the form is
    /// written, like a member test.
    /// </summary>
    internal sealed class RulesPassCondition : Condition
    {
        private readonly FieldReference _field;
        private readonly IReadOnlyList<BrowserRule> _rules;
        private readonly ValueExpression? _value;
        private readonly FormFieldValues? _values;

        internal RulesPassCondition(FieldReference field, IReadOnlyList<BrowserRule> rules)
            : this(field, rules, null, null)
        {
        }

        private RulesPassCondition(FieldReference field, IReadOnlyList<BrowserRule> rules, ValueExpression? value, FormFieldValues? values)
        {
            _field = field ?? throw new ArgumentNullException(nameof(field));
            _rules = rules ?? throw new ArgumentNullException(nameof(rules));
            _value = value;
            _values = values;
        }

        internal override IEnumerable<FieldReference> Fields =>
            new[] { _field }.Concat(_rules.SelectMany(rule => rule.Fields.Concat(rule.Condition.Fields)));

        internal override Condition PrefixedBy(string prefix) =>
            new RulesPassCondition(_field.PrefixedBy(prefix), _rules.Select(rule => rule.PrefixedBy(prefix, Always)).ToList());

        internal override Condition Bound(FieldValues fields) =>
            new RulesPassCondition(_field, _rules, fields.Value(_field), fields as FormFieldValues
                ?? throw new ArgumentException("The rules a member's rules depend on are bound to a form's inputs.", nameof(fields)));

        internal override Expr Write(Scope scope)
        {
            if (_value == null || _values == null)
                throw new InvalidOperationException($"The rules of '{_field.Path}' are bound to the form's inputs before they are written.");
            var value = _value.Write(scope);
            return G.And(_rules.Select(rule => Passes(rule, value, scope)).ToList());
        }

        // The rule passes, or does not apply: !when || check(value).
        private Expr Passes(BrowserRule rule, Expr value, Scope scope)
        {
            var check = rule.Check(value, _field.Shape, _values!, scope);
            var when = rule.Condition.Bound(_values!);
            return when.IsAlways ? check : G.Or(G.Not(when.Write(scope)), check);
        }
    }
}
