using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Alis.Reactive.Builders.Conditions;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Builders
{
    /// <summary>
    /// Builds a dispatch event payload by setting typed fields from live sources
    /// (component values, URL params, plugin reads) or literals.
    /// </summary>
    /// <typeparam name="TPayload">The payload type that the listener will consume.</typeparam>
    /// <typeparam name="TModel">The view model type.</typeparam>
    public class DispatchPayloadBuilder<TPayload, TModel>
        where TPayload : class
        where TModel : class
    {
        private readonly Dictionary<string, ValueProducer> _fields = new Dictionary<string, ValueProducer>();

        internal DispatchPayloadBuilder() { }

        /// <summary>Sets a payload field from a typed source (component value, URL param, plugin read).</summary>
        /// <typeparam name="TProp">The property type.</typeparam>
        /// <param name="field">Expression selecting the payload property to set.</param>
        /// <param name="source">The typed source providing the runtime value.</param>
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
        /// <param name="field">Expression selecting the payload property to set.</param>
        /// <param name="value">The literal value.</param>
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
        /// <param name="field">Expression selecting the payload property to set.</param>
        /// <param name="value">The literal value.</param>
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
        /// <param name="field">Expression selecting the payload property to set.</param>
        /// <param name="value">The literal value.</param>
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
