using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Alis.Reactive.Builders.Conditions;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Builders
{
    /// <summary>
    /// Composes the payload of a declared <see cref="AppEvent{TPayload}"/> from value sources and
    /// literals. Each field is set through a typed expression on <typeparamref name="TPayload"/>, the
    /// payload the event's listeners read.
    /// </summary>
    /// <typeparam name="TPayload">The payload the event declares.</typeparam>
    /// <typeparam name="TModel">View model type providing component registrations.</typeparam>
    public class DispatchPayloadBuilder<TPayload, TModel>
        where TPayload : class
        where TModel : class
    {
        private readonly DispatchPayloadDraft _draft = new DispatchPayloadDraft();

        internal DispatchPayloadBuilder() { }

        /// <summary>Sets a member of the payload to a value read when the dispatch runs.</summary>
        /// <remarks>The value's type must be the member's type or derive from it.</remarks>
        /// <typeparam name="TProp">The member's type.</typeparam>
        /// <typeparam name="TSource">The value's type.</typeparam>
        /// <param name="field">The payload member, for example <c>x =&gt; x.UnitId</c>.</param>
        /// <param name="source">The value: a component's property, a payload, a response, the URL or an array.</param>
        public DispatchPayloadBuilder<TPayload, TModel> Set<TProp, TSource>(
            Expression<Func<TPayload, TProp>> field,
            TypedSource<TSource> source)
            where TSource : TProp
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            _draft.Set(ExpressionPathHelper.ToJsonPath(field), source.Expression);
            return this;
        }

        /// <summary>Sets a member of the payload to a value fixed when the view renders.</summary>
        /// <typeparam name="TProp">The member's type.</typeparam>
        /// <param name="field">The payload member.</param>
        /// <param name="value">The value, written into the plan.</param>
        public DispatchPayloadBuilder<TPayload, TModel> Set<TProp>(
            Expression<Func<TPayload, TProp>> field,
            TProp value)
        {
            _draft.Set(ExpressionPathHelper.ToJsonPath(field), ValueExpression.LiteralFor(value, Shape.FromClrType(typeof(TProp))));
            return this;
        }

        internal ValueExpression Build()
        {
            if (!_draft.HasFields)
                throw new InvalidOperationException(
                    "Dispatch payload must have at least one field. An event with no payload is declared with AppEvent.Declare() and raised with Dispatch(appEvent).");

            return _draft.ToValueExpression();
        }
    }

    internal sealed class DispatchPayloadDraft
    {
        private readonly Dictionary<string, ValueExpression> _leaves =
            new Dictionary<string, ValueExpression>(StringComparer.Ordinal);
        private readonly Dictionary<string, DispatchPayloadDraft> _objects =
            new Dictionary<string, DispatchPayloadDraft>(StringComparer.Ordinal);

        internal bool HasFields => _leaves.Count > 0 || _objects.Count > 0;

        // A dispatched payload is an object of named members; a list it carries is set as one member.
        internal void Set(Path path, ValueExpression value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            if (path.IsNone)
                throw new ArgumentException("Select a member of the payload, for example x => x.ResidentId.", nameof(path));

            var names = new List<string>(path.Segments.Count);
            foreach (var segment in path.Segments)
            {
                if (segment.IsIndex || segment.IsEachItem)
                    throw new ArgumentException(
                        $"A dispatched payload is an object of named members; '{path}' names an item of a list. " +
                        "Set the member that holds the list.", nameof(path));
                names.Add(segment.ToPathText());
            }

            var parent = this;
            var fullPath = path.ToString();
            for (var index = 0; index < names.Count - 1; index++)
                parent = parent.GetOrCreateObject(names[index], fullPath);
            parent.SetLeaf(names[names.Count - 1], value, fullPath);
        }

        internal ValueExpression ToValueExpression() =>
            ValueExpression.Record(ToFields());

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

}
