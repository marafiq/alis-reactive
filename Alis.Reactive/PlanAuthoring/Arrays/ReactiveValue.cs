using System;
using Alis.Reactive.Builders.Conditions;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Builders.Arrays
{
    /// <summary>
    /// Scalar value produced by an array operation, such as <c>Count</c> or <c>Sum</c>.
    /// It is a <see cref="TypedSource{TValue}"/>, so it goes wherever a value read when the step runs
    /// goes: <c>Text.Set</c>, <c>When</c>, a property's <c>Set</c>, dispatch payloads, and a gathered request's <c>Set</c>.
    /// </summary>
    /// <typeparam name="TValue">The value's type.</typeparam>
    /// <example><c>p.Element("critical-count").Text.Set(residents.Count(r =&gt; r.Critical))</c></example>
    public sealed class ReactiveValue<TValue> : TypedSource<TValue>
    {
        private readonly ValueExpression _value;

        internal ReactiveValue(ValueExpression value)
        {
            _value = value ?? throw new ArgumentNullException(nameof(value));
        }

        internal override ValueExpression Expression => _value;
    }
}
