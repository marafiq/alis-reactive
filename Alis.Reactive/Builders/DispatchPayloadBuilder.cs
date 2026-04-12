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

            return ValueProducer.Object(_fields);
        }
    }
}
