using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Alis.Reactive.Builders.Conditions;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Builders
{
    /// <summary>
    /// Composes a custom-event dispatch payload from live sources and literals.
    /// Each field is set via a typed expression on <typeparamref name="TPayload"/>,
    /// matching the shape that <see cref="Builders.TriggerBuilder{TModel}.CustomEvent{TPayload}"/>
    /// listeners consume.
    /// </summary>
    /// <typeparam name="TPayload">The payload type that the listener will consume via <c>CustomEvent&lt;TPayload&gt;</c>.</typeparam>
    /// <typeparam name="TModel">The view model type providing component registrations.</typeparam>
    public class DispatchPayloadBuilder<TPayload, TModel>
        where TPayload : class
        where TModel : class
    {
        private readonly DispatchPayloadDraft _payload = new DispatchPayloadDraft();

        internal DispatchPayloadBuilder() { }

        /// <summary>Sets a payload field from a live source resolved at dispatch time.</summary>
        /// <typeparam name="TProp">The field value type, inferred from the expression.</typeparam>
        /// <param name="field">The payload property to populate, e.g. <c>x =&gt; x.Name</c>.</param>
        /// <param name="source">A component value, URL param, or plugin read that provides the runtime value.</param>
        /// <returns>This builder for chaining.</returns>
        public DispatchPayloadBuilder<TPayload, TModel> Set<TProp>(
            Expression<Func<TPayload, TProp>> field,
            TypedSource<TProp> source)
        {
            var fieldName = ExpressionPathHelper.ToEventPath<TPayload, TProp>(field);
            _payload.Set(fieldName, source.ToValueProducer());
            return this;
        }

        /// <summary>Sets a payload field to a literal string value.</summary>
        /// <param name="field">The payload property to populate, e.g. <c>x =&gt; x.Status</c>.</param>
        /// <param name="value">The compile-time constant embedded in the plan.</param>
        /// <returns>This builder for chaining.</returns>
        public DispatchPayloadBuilder<TPayload, TModel> Set(
            Expression<Func<TPayload, string>> field,
            string value)
        {
            var fieldName = ExpressionPathHelper.ToEventPath<TPayload, string>(field);
            _payload.Set(fieldName, ValueProducer.Literal(value));
            return this;
        }

        /// <summary>Sets a payload field to a literal int value.</summary>
        /// <param name="field">The payload property to populate, e.g. <c>x =&gt; x.Status</c>.</param>
        /// <param name="value">The compile-time constant embedded in the plan.</param>
        /// <returns>This builder for chaining.</returns>
        public DispatchPayloadBuilder<TPayload, TModel> Set(
            Expression<Func<TPayload, int>> field,
            int value)
        {
            var fieldName = ExpressionPathHelper.ToEventPath<TPayload, int>(field);
            _payload.Set(fieldName, ValueProducer.Literal(value));
            return this;
        }

        /// <summary>Sets a payload field to a literal bool value.</summary>
        /// <param name="field">The payload property to populate, e.g. <c>x =&gt; x.Status</c>.</param>
        /// <param name="value">The compile-time constant embedded in the plan.</param>
        /// <returns>This builder for chaining.</returns>
        public DispatchPayloadBuilder<TPayload, TModel> Set(
            Expression<Func<TPayload, bool>> field,
            bool value)
        {
            var fieldName = ExpressionPathHelper.ToEventPath<TPayload, bool>(field);
            _payload.Set(fieldName, ValueProducer.Literal(value));
            return this;
        }

        internal ValueProducer Build()
        {
            if (!_payload.HasFields)
                throw new InvalidOperationException(
                    "Dispatch payload must have at least one field. Use Dispatch(eventName) for no-payload dispatch.");

            return _payload.ToValueProducer();
        }
    }

    internal sealed class DispatchPayloadDraft
    {
        private readonly Dictionary<string, DispatchPayloadSlot> _slots =
            new Dictionary<string, DispatchPayloadSlot>(StringComparer.Ordinal);

        internal bool HasFields => _slots.Count > 0;

        internal void Set(string path, ValueProducer value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));

            var payloadPath = DispatchPayloadPath.Of(path);
            var parent = GetOrCreateParent(payloadPath);
            parent.SetLeaf(payloadPath.Leaf, value, payloadPath.Value);
        }

        internal ValueProducer ToValueProducer() =>
            ValueProducer.Object(ToFields());

        private DispatchPayloadDraft GetOrCreateParent(DispatchPayloadPath path)
        {
            var current = this;
            foreach (var segment in path.ParentSegments)
                current = current.GetOrCreateObject(segment, path.Value);

            return current;
        }

        private DispatchPayloadDraft GetOrCreateObject(string segment, string fullPath)
        {
            if (!_slots.TryGetValue(segment, out var existing))
            {
                var child = new DispatchPayloadDraft();
                _slots[segment] = DispatchPayloadSlot.Object(child);
                return child;
            }

            if (existing is DispatchPayloadObjectSlot objectSlot)
                return objectSlot.Draft;

            throw new InvalidOperationException(
                $"Dispatch payload conflict: '{segment}' is set as a leaf value " +
                $"but also used as a parent for '{fullPath}'. " +
                "Set either the parent or the children, not both.");
        }

        private void SetLeaf(string leaf, ValueProducer value, string fullPath)
        {
            var assignment = DispatchPayloadAssignment.ForLeaf(leaf, _slots);
            assignment.EnsureLeafCanBeSet(fullPath);

            _slots[leaf] = DispatchPayloadSlot.Leaf(value);
        }

        private Dictionary<string, ValueProducer> ToFields()
        {
            var fields = new Dictionary<string, ValueProducer>(StringComparer.Ordinal);
            foreach (var slot in _slots)
                fields[slot.Key] = slot.Value.ToValueProducer();

            return fields;
        }
    }

    internal sealed class DispatchPayloadPath
    {
        private readonly IReadOnlyList<string> _segments;

        private DispatchPayloadPath(string value, IReadOnlyList<string> segments)
        {
            Value = value;
            _segments = segments;
        }

        internal string Value { get; }
        internal string Leaf => _segments[_segments.Count - 1];

        internal IEnumerable<string> ParentSegments
        {
            get
            {
                for (var index = 0; index < _segments.Count - 1; index++)
                    yield return _segments[index];
            }
        }

        internal static DispatchPayloadPath Of(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Dispatch payload path must not be empty.", nameof(value));

            var segments = value.Split('.');
            foreach (var segment in segments)
            {
                if (string.IsNullOrWhiteSpace(segment))
                    throw new ArgumentException(
                        "Dispatch payload path '" + value + "' contains an empty segment.",
                        nameof(value));
            }

            return new DispatchPayloadPath(value, segments);
        }
    }

    internal abstract class DispatchPayloadSlot
    {
        private protected DispatchPayloadSlot() { }

        internal abstract ValueProducer ToValueProducer();

        internal static DispatchPayloadSlot Leaf(ValueProducer value) =>
            new DispatchPayloadLeafSlot(value);

        internal static DispatchPayloadSlot Object(DispatchPayloadDraft draft) =>
            new DispatchPayloadObjectSlot(draft);

    }

    internal sealed class DispatchPayloadLeafSlot : DispatchPayloadSlot
    {
        private readonly ValueProducer _value;

        internal DispatchPayloadLeafSlot(ValueProducer value)
        {
            _value = value ?? throw new ArgumentNullException(nameof(value));
        }

        internal override ValueProducer ToValueProducer() => _value;
    }

    internal sealed class DispatchPayloadObjectSlot : DispatchPayloadSlot
    {
        internal DispatchPayloadObjectSlot(DispatchPayloadDraft draft)
        {
            Draft = draft ?? throw new ArgumentNullException(nameof(draft));
        }

        internal DispatchPayloadDraft Draft { get; }

        internal override ValueProducer ToValueProducer() =>
            Draft.ToValueProducer();
    }

    internal abstract class DispatchPayloadAssignment
    {
        private protected DispatchPayloadAssignment() { }

        internal static DispatchPayloadAssignment ForLeaf(
            string leaf,
            IReadOnlyDictionary<string, DispatchPayloadSlot> existingSlots)
        {
            if (string.IsNullOrWhiteSpace(leaf))
                throw new ArgumentException("Dispatch payload leaf must not be empty.", nameof(leaf));
            if (existingSlots == null) throw new ArgumentNullException(nameof(existingSlots));

            var leafAlreadyOwnsNestedPayload =
                existingSlots.TryGetValue(leaf, out var existing) && existing is DispatchPayloadObjectSlot;
            if (leafAlreadyOwnsNestedPayload)
                return new LeafConflictsWithNestedPayload();

            return LeafCanBeAssigned.Instance;
        }

        internal abstract void EnsureLeafCanBeSet(string fullPath);
    }

    internal sealed class LeafCanBeAssigned : DispatchPayloadAssignment
    {
        internal static LeafCanBeAssigned Instance { get; } = new LeafCanBeAssigned();

        private LeafCanBeAssigned() { }

        internal override void EnsureLeafCanBeSet(string fullPath)
        {
        }
    }

    internal sealed class LeafConflictsWithNestedPayload : DispatchPayloadAssignment
    {
        internal override void EnsureLeafCanBeSet(string fullPath)
        {
            throw new InvalidOperationException(
                $"Dispatch payload conflict: '{fullPath}' has nested children " +
                "but is also set as a leaf value. " +
                "Set either the parent or the children, not both.");

        }
    }
}
