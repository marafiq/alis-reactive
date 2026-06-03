using System;
using System.Linq.Expressions;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Builders.Conditions
{
    /// <summary>
    /// A typed source that reads from any payload scope (event, success, error, dispatch).
    /// Generalizes event, response, and error payload reads into a single TypedSource.
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
                PayloadSource.Event(PayloadContract.ForPayload(typeof(TPayload))),
                expression);

        internal override ValueExpression ToValueExpression()
        {
            var payloadPath = ExpressionPathHelper.ToEventPath(_expression);
            return ValueExpression.ReadPayload(_source, payloadPath, Shape);
        }
    }
}
