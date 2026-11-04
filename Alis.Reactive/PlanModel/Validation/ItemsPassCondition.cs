using System;
using System.Collections.Generic;
using System.Linq;
using Alis.Reactive.Grammar;
using Alis.Reactive.Inputs;
using Alis.Reactive.PlanModel.Conditions;
using Alis.Reactive.PlanModel.Writing;

namespace Alis.Reactive.PlanModel.Validation
{
    internal sealed class ItemsPassCondition : Condition
    {
        private readonly FieldReference _collection;
        private readonly IReadOnlyList<BrowserRule> _rules;
        private readonly FormFieldValues? _values;

        internal ItemsPassCondition(FieldReference collection, IReadOnlyList<BrowserRule> rules)
            : this(collection, rules, null)
        {
        }

        private ItemsPassCondition(FieldReference collection, IReadOnlyList<BrowserRule> rules, FormFieldValues? values)
        {
            _collection = collection ?? throw new ArgumentNullException(nameof(collection));
            _rules = rules ?? throw new ArgumentNullException(nameof(rules));
            _values = values;
        }

        internal override IEnumerable<FieldReference> Fields
            => new[] { _collection }.Concat(_rules.SelectMany(rule => rule.Fields.Concat(rule.Condition.Fields)));

        internal override Condition PrefixedBy(string prefix)
            => new ItemsPassCondition(_collection.PrefixedBy(prefix), _rules.Select(rule => rule.PrefixedBy(prefix, Always)).ToList());

        internal override Condition Bound(FieldValues fields)
            => new ItemsPassCondition(_collection, _rules, fields as FormFieldValues
                ?? throw new ArgumentException("The rules a member's rules depend on are bound to a form's inputs.", nameof(fields)));

        internal override Expr Write(Scope scope)
        {
            if (_values == null)
                throw new InvalidOperationException($"The item rules of '{_collection.Path}' are bound to the form's inputs before they are written.");
            var item = FieldReference.ItemOf(_collection).PrefixedBy(_collection.Path + "[*]");
            var value = _values.Value(item).Write(scope);
            var everyCheckPasses = G.And(_rules.Select(rule => Passes(rule, value, item.Shape, scope)).ToList());
            return G.Call(_values.ItemIndexes(item).Write(scope), "every", G.Fn(everyCheckPasses, FormFieldValues.Indexes));
        }

        private Expr Passes(BrowserRule rule, Expr value, Shape itemShape, Scope scope)
        {
            var check = rule.Check(value, itemShape, _values!, scope);
            var when = rule.Condition.Bound(_values!);
            return when.IsAlways ? check : G.Or(G.Not(when.Write(scope)), check);
        }
    }
}
