using System;
using System.Linq.Expressions;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Builders.Conditions
{
    /// <summary>
    /// Reads a typed value from a Reactive Plan payload scope such as event,
    /// success, error, or dispatch.
    /// </summary>
    public sealed class PayloadTypedSource<TPayload, TProp> : TypedSource<TProp>
    {
        private readonly PayloadSource _source;
        private readonly Expression<Func<TPayload, TProp>> _expression;

        internal PayloadTypedSource(PayloadSource source, Expression<Func<TPayload, TProp>> expression)
        {
            _source = source;
            _expression = expression;
        }

        internal static PayloadTypedSource<TPayload, TProp> FromEvent(
            Expression<Func<TPayload, TProp>> expression) =>
            new PayloadTypedSource<TPayload, TProp>(
                PayloadSource.Event(),
                expression);

        internal override ValueExpression ToValueExpression()
        {
            var payloadPath = ExpressionPathHelper.ToEventPath(_expression);
            return ValueExpression.ReadPayload(_source, payloadPath, Shape);
        }
    }
}
