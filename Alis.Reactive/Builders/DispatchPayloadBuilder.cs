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
        private readonly Dictionary<string, ValueProducer> _fields = new Dictionary<string, ValueProducer>();

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
            _fields[fieldName] = source.ToValueProducer();
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
            _fields[fieldName] = ValueProducer.Literal(value);
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
            _fields[fieldName] = ValueProducer.Literal(value);
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
            _fields[fieldName] = ValueProducer.Literal(value);
            return this;
        }

        internal ValueProducer Build()
        {
            if (_fields.Count == 0)
                throw new InvalidOperationException(
                    "Dispatch payload must have at least one field. Use Dispatch(eventName) for no-payload dispatch.");

            return ValueProducer.Object(ExpandNestedPaths(_fields));
        }

        /// <summary>
        /// Expands dotted keys like <c>"address.city"</c> into nested ObjectProducers
        /// so the dispatched event carries <c>{ address: { city: value } }</c> instead
        /// of <c>{ "address.city": value }</c>. Matches how typed payload listeners
        /// resolve nested properties.
        /// </summary>
        private static Dictionary<string, ValueProducer> ExpandNestedPaths(
            Dictionary<string, ValueProducer> flat)
        {
            var root = new Dictionary<string, ValueProducer>();

            foreach (var kvp in flat)
            {
                var segments = kvp.Key.Split('.');
                var isTopLevel = segments.Length == 1;
                if (isTopLevel)
                {
                    if (root.ContainsKey(kvp.Key) && root[kvp.Key] is ObjectProducer)
                        throw new InvalidOperationException(
                            $"Dispatch payload conflict: '{kvp.Key}' has nested children " +
                            $"but is also set as a leaf value. " +
                            $"Set either the parent or the children, not both.");
                    root[kvp.Key] = kvp.Value;
                    continue;
                }

                // Walk/create nested dictionaries for each intermediate segment
                var current = root;
                for (var i = 0; i < segments.Length - 1; i++)
                {
                    var needsNesting = !current.ContainsKey(segments[i]);
                    if (needsNesting)
                    {
                        current[segments[i]] = ValueProducer.Object(new Dictionary<string, ValueProducer>());
                    }
                    else if (!(current[segments[i]] is ObjectProducer))
                    {
                        throw new InvalidOperationException(
                            $"Dispatch payload conflict: '{segments[i]}' is set as a leaf value " +
                            $"but also used as a parent for '{kvp.Key}'. " +
                            $"Set either the parent or the children, not both.");
                    }

                    current = ((ObjectProducer)current[segments[i]]).WritableFields;
                }

                var leafKey = segments[segments.Length - 1];
                if (current.ContainsKey(leafKey) && current[leafKey] is ObjectProducer)
                    throw new InvalidOperationException(
                        $"Dispatch payload conflict: '{kvp.Key}' has nested children " +
                        $"but is also set as a leaf value. " +
                        $"Set either the parent or the children, not both.");
                current[leafKey] = kvp.Value;
            }

            return root;
        }
    }
}
