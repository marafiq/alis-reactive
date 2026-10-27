using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Alis.Reactive.Grammar;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Validation
{
    /// <summary>
    /// One member of a validator's model and the rules the browser checks on it. A collection member
    /// carries the rules of each of its items.
    /// </summary>
    public sealed class BrowserField
    {
        // For a collection item field: the condition declared around its RuleForEach, relative to the
        // collection's owner. Relocating the owner prefixes it; the rendered item prefix never does.
        private readonly Condition _ownerCondition;

        internal BrowserField(
            FieldReference field,
            IEnumerable<BrowserRule> rules,
            IEnumerable<BrowserField> itemFields)
            : this(field, rules, itemFields, Condition.Always)
        {
        }

        private BrowserField(
            FieldReference field,
            IEnumerable<BrowserRule> rules,
            IEnumerable<BrowserField> itemFields,
            Condition ownerCondition)
        {
            Reference = field;
            Rules = rules.ToArray();
            ItemFields = itemFields.ToArray();
            _ownerCondition = ownerCondition;
        }

        /// <summary>The member's path in the validator's model, for example <c>"Address.City"</c>.</summary>
        public string FieldName => Reference.Path;

        /// <summary>The rules the browser checks on the member.</summary>
        public IReadOnlyList<BrowserRule> Rules { get; }

        internal IReadOnlyList<BrowserField> ItemFields { get; }

        internal FieldReference Reference { get; }

        internal bool HasRules => Rules.Count > 0;

        internal BrowserField PrefixedBy(string prefix, Condition outer) =>
            new BrowserField(
                Reference.PrefixedBy(prefix),
                Rules.Select(rule => rule.PrefixedBy(prefix, outer)),
                ItemFields.Select(field => field.OwnedUnder(prefix, outer)));

        // A collection item field rendered at itemPrefix: its own conditions take the item prefix,
        // its owner's condition does not.
        internal BrowserField RenderedAt(string itemPrefix) =>
            PrefixedBy(itemPrefix, _ownerCondition);

        // An item field whose owner declared it under a condition (When around RuleForEach).
        internal BrowserField ScopedBy(Condition condition) =>
            new BrowserField(Reference, Rules, ItemFields, Condition.Combine(condition, _ownerCondition));

        // An item field whose collection's owner moves under prefix: its owner condition moves with it.
        private BrowserField OwnedUnder(string prefix, Condition condition) =>
            new BrowserField(Reference, Rules, ItemFields, Condition.Combine(condition, _ownerCondition.PrefixedBy(prefix)));
    }

    /// <summary>
    /// The browser rules a validator declares, built rule by rule: a validation integration reads its
    /// validator and adds every rule the browser can evaluate. A member is placed where the validator
    /// first names it: as a rule's member, a member a condition reads, or a sibling a rule compares with.
    /// </summary>
    /// <example>
    /// <code>
    /// var rules = new BrowserRuleSet();
    /// rules.Add((Admission x) =&gt; x.ResidentName, BrowserRule.NotEmpty("'Resident Name' must not be empty."));
    /// rules.Add((Admission x) =&gt; x.ServiceNumber, BrowserRule.NotEmpty("'Service Number' must not be empty.")
    ///     .When(Condition.Is((Admission x) =&gt; x.IsVeteran, ValueTest.Truthy)));
    /// return rules.Fields;
    /// </code>
    /// </example>
    public sealed class BrowserRuleSet
    {
        private readonly Dictionary<string, Entry> _fields = new Dictionary<string, Entry>(StringComparer.Ordinal);
        private readonly List<Entry> _order = new List<Entry>();

        /// <summary>Adds a rule on a member.</summary>
        /// <param name="member">The member, as a path from the validator's model: <c>x =&gt; x.Address.City</c>.</param>
        /// <param name="rule">The rule, under the condition it is declared under.</param>
        public void Add(LambdaExpression member, BrowserRule rule)
        {
            if (rule == null) throw new ArgumentNullException(nameof(rule));
            var reference = FieldReference.Of(member);
            if (!rule.TryFor(reference, out var resolved)) return;
            Ensure(resolved.Condition.Fields);
            Ensure(reference);
            AddRule(reference, resolved);
        }

        /// <summary>Adds rules written on each item of a collection member itself: <c>RuleForEach(x =&gt; x.Tags).NotEmpty()</c>.</summary>
        /// <param name="collection">The collection member, as a path from the validator's model.</param>
        /// <param name="rules">The rules each item is checked by.</param>
        /// <param name="when">The condition the collection's rule is declared under.</param>
        public void AddItemRules(LambdaExpression collection, IEnumerable<BrowserRule> rules, Condition when)
        {
            if (rules == null) throw new ArgumentNullException(nameof(rules));
            if (when == null) throw new ArgumentNullException(nameof(when));
            Ensure(when.Fields);
            var member = FieldReference.Of(collection);
            var item = FieldReference.ItemOf(member);
            var itemRules = new List<BrowserRule>();
            foreach (var rule in rules)
                if (rule.TryFor(item, out var resolved)) itemRules.Add(resolved);
            if (itemRules.Count == 0) return;
            Ensure(member).ItemFields.Add(new BrowserField(item, itemRules, Array.Empty<BrowserField>()).ScopedBy(when));
        }

        /// <summary>Adds the rules of each item of a collection member: a validator applied to every item.</summary>
        /// <param name="collection">The collection member, as a path from the validator's model.</param>
        /// <param name="itemFields">The item's members and rules, by their paths in the item.</param>
        /// <param name="when">The condition the collection's rule is declared under.</param>
        public void AddItems(LambdaExpression collection, IEnumerable<BrowserField> itemFields, Condition when)
        {
            if (itemFields == null) throw new ArgumentNullException(nameof(itemFields));
            if (when == null) throw new ArgumentNullException(nameof(when));
            Ensure(when.Fields);
            var entry = Ensure(FieldReference.Of(collection));
            foreach (var itemField in itemFields)
                entry.ItemFields.Add(itemField.ScopedBy(when));
        }

        /// <summary>Adds a validator's members in place: a validator included whole.</summary>
        /// <param name="fields">The included validator's members and rules.</param>
        /// <param name="when">The condition the inclusion is declared under.</param>
        public void Add(IEnumerable<BrowserField> fields, Condition when) => AddUnder("", fields, when);

        /// <summary>Adds a child validator's members under the member that holds them.</summary>
        /// <param name="owner">The member that holds them, as a path from the validator's model.</param>
        /// <param name="fields">The child validator's members and rules.</param>
        /// <param name="when">The condition the child's rule is declared under.</param>
        public void Add(LambdaExpression owner, IEnumerable<BrowserField> fields, Condition when) =>
            AddUnder(FieldReference.Of(owner).Path, fields, when);

        private void AddUnder(string prefix, IEnumerable<BrowserField> fields, Condition when)
        {
            if (fields == null) throw new ArgumentNullException(nameof(fields));
            if (when == null) throw new ArgumentNullException(nameof(when));
            Ensure(when.Fields);
            foreach (var field in fields)
            {
                var target = field.PrefixedBy(prefix, when);
                var entry = Ensure(target.Reference);
                foreach (var rule in target.Rules)
                    AddRule(target.Reference, rule);
                entry.ItemFields.AddRange(target.ItemFields);
            }
        }

        /// <summary>Every member the validator names, with its rules, where it first named it.</summary>
        public IReadOnlyList<BrowserField> Fields =>
            _order.Select(entry => new BrowserField(entry.Field, entry.Rules, entry.ItemFields)).ToArray();

        private void AddRule(FieldReference field, BrowserRule rule)
        {
            Ensure(rule.Fields);
            Ensure(field).Rules.Add(rule);
        }

        private void Ensure(IEnumerable<FieldReference> fields)
        {
            foreach (var field in fields)
                Ensure(field);
        }

        private Entry Ensure(FieldReference field)
        {
            if (_fields.TryGetValue(field.Path, out var existing))
            {
                if (!existing.Field.Shape.Equals(field.Shape))
                    throw new InvalidOperationException(
                        $"Client validation field '{field.Path}' was declared with conflicting shapes: " +
                        $"'{existing.Field.Shape.Kind}' and '{field.Shape.Kind}'.");
                return existing;
            }

            var entry = new Entry(field);
            _fields.Add(field.Path, entry);
            _order.Add(entry);
            return entry;
        }

        private sealed class Entry
        {
            internal Entry(FieldReference field)
            {
                Field = field;
            }

            internal FieldReference Field { get; }

            internal List<BrowserRule> Rules { get; } = new List<BrowserRule>();

            internal List<BrowserField> ItemFields { get; } = new List<BrowserField>();
        }
    }
}
