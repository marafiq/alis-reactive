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
            _payload.Set(fieldName, source.ToValueExpression());
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
            _payload.Set(fieldName, ValueExpression.Literal(value));
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
            _payload.Set(fieldName, ValueExpression.Literal(value));
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
            _payload.Set(fieldName, ValueExpression.Literal(value));
            return this;
        }

        internal ValueExpression Build()
        {
            if (!_payload.HasFields)
                throw new InvalidOperationException(
                    "Dispatch payload must have at least one field. Use Dispatch(eventName) for no-payload dispatch.");

            return _payload.ToValueExpression();
        }
    }

    internal sealed class DispatchPayloadDraft
    {
        private readonly Dictionary<string, ValueExpression> _leaves =
            new Dictionary<string, ValueExpression>(StringComparer.Ordinal);
        private readonly Dictionary<string, DispatchPayloadDraft> _objects =
            new Dictionary<string, DispatchPayloadDraft>(StringComparer.Ordinal);

        internal bool HasFields => _leaves.Count > 0 || _objects.Count > 0;

        internal void Set(string path, ValueExpression value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));

            var payloadPath = DispatchPayloadPath.Of(path);
            var parent = GetOrCreateParent(payloadPath);
            parent.SetLeaf(payloadPath.Leaf, value, payloadPath.Value);
        }

        internal ValueExpression ToValueExpression() =>
            ValueExpression.Object(ToFields());

        private DispatchPayloadDraft GetOrCreateParent(DispatchPayloadPath path)
        {
            var current = this;
            foreach (var segment in path.ParentSegments)
                current = current.GetOrCreateObject(segment, path.Value);

            return current;
        }

        private DispatchPayloadDraft GetOrCreateObject(string segment, string fullPath)
        {
            if (_leaves.ContainsKey(segment))
                throw new InvalidOperationException(
                    $"Dispatch payload conflict: '{segment}' is set as a leaf value " +
                    $"but also used as a parent for '{fullPath}'. " +
                    "Set either the parent or the children, not both.");

            if (!_objects.TryGetValue(segment, out var child))
            {
                child = new DispatchPayloadDraft();
                _objects[segment] = child;
            }

            return child;
        }

        private void SetLeaf(string leaf, ValueExpression value, string fullPath)
        {
            EnsureLeafCanBeSet(leaf, fullPath);

            _leaves[leaf] = value;
        }

        private void EnsureLeafCanBeSet(string leaf, string fullPath)
        {
            if (string.IsNullOrWhiteSpace(leaf))
                throw new ArgumentException("Dispatch payload leaf must not be empty.", nameof(leaf));

            if (!_objects.ContainsKey(leaf)) return;

            throw new InvalidOperationException(
                $"Dispatch payload conflict: '{fullPath}' has nested children " +
                "but is also set as a leaf value. " +
                "Set either the parent or the children, not both.");
        }

        private Dictionary<string, ValueExpression> ToFields()
        {
            var fields = new Dictionary<string, ValueExpression>(StringComparer.Ordinal);
            foreach (var leaf in _leaves)
                fields[leaf.Key] = leaf.Value;
            foreach (var child in _objects)
                fields[child.Key] = child.Value.ToValueExpression();

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

}
