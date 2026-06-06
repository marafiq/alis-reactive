using System;
using Alis.Reactive.Builders.Conditions;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Builders.Arrays
{
    /// <summary>
    /// Scalar value produced by an array operation, such as <c>Count</c> or <c>Sum</c>.
    /// It is a <see cref="TypedSource{TValue}"/>, so it can feed DSL members that read typed
    /// sources, including <c>SetText</c>, <c>When</c>, and dispatch payloads.
    /// Gather intake remains limited to component and plugin sources.
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
