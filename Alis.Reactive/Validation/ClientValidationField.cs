using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Validation
{
    /// <summary>
    /// One member of a validator's model and the rules the browser checks on it. A collection member
    /// carries the rules of each of its items.
    /// </summary>
    public sealed class ClientValidationField
    {
        // For a collection item field: the condition declared around its RuleForEach, relative to the
        // collection's owner. Relocating the owner prefixes it; the rendered item prefix never does.
        private readonly ClientCondition _ownerCondition;

        internal ClientValidationField(
            FieldReference field,
            IEnumerable<ClientRule> rules,
            IEnumerable<ClientValidationField> itemFields)
            : this(field, rules, itemFields, ClientCondition.Always)
        {
        }

        private ClientValidationField(
            FieldReference field,
            IEnumerable<ClientRule> rules,
            IEnumerable<ClientValidationField> itemFields,
            ClientCondition ownerCondition)
        {
            Reference = field;
            Rules = rules.ToArray();
            ItemFields = itemFields.ToArray();
            _ownerCondition = ownerCondition;
        }

        /// <summary>The member's path in the validator's model, for example <c>"Address.City"</c>.</summary>
        public string FieldName => Reference.Path;

        /// <summary>The rules the browser checks on the member.</summary>
        public IReadOnlyList<ClientRule> Rules { get; }

        internal IReadOnlyList<ClientValidationField> ItemFields { get; }

        internal FieldReference Reference { get; }

        internal bool HasRules => Rules.Count > 0;

        internal ClientValidationField PrefixedBy(string prefix, ClientCondition outer) =>
            new ClientValidationField(
                Reference.PrefixedBy(prefix),
                Rules.Select(rule => rule.PrefixedBy(prefix, outer)),
                ItemFields.Select(field => field.OwnedUnder(prefix, outer)));

        // A collection item field rendered at itemPrefix: its own conditions take the item prefix,
        // its owner's condition does not.
        internal ClientValidationField RenderedAt(string itemPrefix) =>
            PrefixedBy(itemPrefix, _ownerCondition);

        // An item field whose owner declared it under a condition (When around RuleForEach).
        internal ClientValidationField ScopedBy(ClientCondition condition) =>
            new ClientValidationField(Reference, Rules, ItemFields, ClientCondition.Combine(condition, _ownerCondition));

        // An item field whose collection's owner moves under prefix: its owner condition moves with it.
        private ClientValidationField OwnedUnder(string prefix, ClientCondition condition) =>
            new ClientValidationField(Reference, Rules, ItemFields, ClientCondition.Combine(condition, _ownerCondition.PrefixedBy(prefix)));
    }

    /// <summary>
    /// The browser rules a validator declares, built rule by rule: a validation integration reads its
    /// validator and adds every rule the browser can evaluate. A member is placed where the validator
    /// first names it: as a rule's member, a member a condition reads, or a sibling a rule compares with.
    /// </summary>
    /// <example>
    /// <code>
    /// var rules = new ClientRuleSet();
    /// rules.Add((Admission x) =&gt; x.ResidentName, ClientRule.NotEmpty("'Resident Name' must not be empty."));
    /// rules.Add((Admission x) =&gt; x.ServiceNumber, ClientRule.NotEmpty("'Service Number' must not be empty.")
    ///     .When(ClientCondition.Is((Admission x) =&gt; x.IsVeteran, ClientValueTest.Truthy)));
    /// return rules.Fields;
    /// </code>
    /// </example>
    public sealed class ClientRuleSet
    {
        private readonly Dictionary<string, Entry> _fields = new Dictionary<string, Entry>(StringComparer.Ordinal);
        private readonly List<Entry> _order = new List<Entry>();

        /// <summary>Adds a rule on a member.</summary>
        /// <param name="member">The member, as a path from the validator's model: <c>x =&gt; x.Address.City</c>.</param>
        /// <param name="rule">The rule, under the condition it is declared under.</param>
        public void Add(LambdaExpression member, ClientRule rule)
        {
            if (rule == null) throw new ArgumentNullException(nameof(rule));
            var reference = FieldReference.Of(member);
            if (!rule.TryFor(reference.Shape, out var resolved)) return;
            Ensure(resolved.Condition.Fields);
            Ensure(reference);
            AddRule(reference, resolved);
        }

        /// <summary>Adds the rules of each item of a collection member: a validator applied to every item.</summary>
        /// <param name="collection">The collection member, as a path from the validator's model.</param>
        /// <param name="itemFields">The item's members and rules, by their paths in the item.</param>
        /// <param name="when">The condition the collection's rule is declared under.</param>
        public void AddItems(LambdaExpression collection, IEnumerable<ClientValidationField> itemFields, ClientCondition when)
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
        public void Add(IEnumerable<ClientValidationField> fields, ClientCondition when) => AddUnder("", fields, when);

        /// <summary>Adds a child validator's members under the member that holds them.</summary>
        /// <param name="owner">The member that holds them, as a path from the validator's model.</param>
        /// <param name="fields">The child validator's members and rules.</param>
        /// <param name="when">The condition the child's rule is declared under.</param>
        public void Add(LambdaExpression owner, IEnumerable<ClientValidationField> fields, ClientCondition when) =>
            AddUnder(FieldReference.Of(owner).Path, fields, when);

        private void AddUnder(string prefix, IEnumerable<ClientValidationField> fields, ClientCondition when)
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
        public IReadOnlyList<ClientValidationField> Fields =>
            _order.Select(entry => new ClientValidationField(entry.Field, entry.Rules, entry.ItemFields)).ToArray();

        private void AddRule(FieldReference field, ClientRule rule)
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

            internal List<ClientRule> Rules { get; } = new List<ClientRule>();

            internal List<ClientValidationField> ItemFields { get; } = new List<ClientValidationField>();
        }
    }
}
