using System;
using System.Linq.Expressions;
using Alis.Reactive.Descriptors;
using Alis.Reactive.Descriptors.Sources;

namespace Alis.Reactive.Builders.Conditions
{
    /// <summary>
    /// Base class for typed source bindings that preserve the property type through
    /// the condition and mutation pipeline.
    /// </summary>
    /// <typeparam name="TProp">The property type, used by condition operators for compile-time type safety.</typeparam>
    public abstract class TypedSource<TProp>
    {
        public abstract BindSource ToBindSource();
        public string CoercionType => CoercionTypes.InferFromType(typeof(TProp));

        /// <summary>
        /// For array types, returns the element-level coercion type (e.g. "string" for string[]).
        /// For non-array types, returns null.
        /// </summary>
        public string? ElementCoercionType =>
            CoercionTypes.InferFromType(typeof(TProp)) == CoercionTypes.Array
                ? CoercionTypes.InferElementType(typeof(TProp))
                : null;
    }

    public sealed class EventArgSource<TPayload, TProp> : TypedSource<TProp>
    {
        private readonly Expression<Func<TPayload, TProp>> _expression;

        public EventArgSource(Expression<Func<TPayload, TProp>> expression)
        {
            _expression = expression;
        }

        public override BindSource ToBindSource() =>
            new EventSource(ExpressionPathHelper.ToEventPath(_expression));
    }
}
