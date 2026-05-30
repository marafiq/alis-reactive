using System;
using Alis.Reactive.Builders.Conditions;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Builders.Arrays
{
    /// <summary>
    /// A scalar value produced by an array operation (count, sum, any, find, ...). It is a
    /// <see cref="TypedSource{TValue}"/>, so it plugs into every place a source is accepted —
    /// <c>SetText</c>, <c>When</c>, dispatch payloads, and gather — with no new overloads.
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
