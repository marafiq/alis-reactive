using System;
using Alis.Reactive.Builders.Conditions;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Builders.Arrays
{
    /// <summary>
    /// A scalar value produced by an array operation (count, sum, any, find, ...). It is a
    /// <see cref="TypedSource{TValue}"/>, so it plugs into the places that accept the base
    /// <c>TypedSource&lt;T&gt;</c> — <c>SetText</c>, <c>When</c>, and dispatch payloads — with no new
    /// overloads. (Gather intake is typed to component/plugin sources, not the base source.)
    /// </summary>
    public sealed class ReactiveValue<TValue> : TypedSource<TValue>
    {
        private readonly ValueExpression _value;

        internal ReactiveValue(ValueExpression value)
        {
            _value = value ?? throw new ArgumentNullException(nameof(value));
        }

        internal override ValueExpression ToValueExpression() => _value;
    }
}
