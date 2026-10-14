using System;
using System.Linq.Expressions;
using Alis.Reactive.Grammar;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Builders.Conditions
{
    /// <summary>A member of data a callback received: an event's payload, a response body, a message.</summary>
    internal sealed class PayloadTypedSource<TPayload, TProp> : TypedSource<TProp>
    {
        private readonly Receiver _data;
        private readonly Expression<Func<TPayload, TProp>> _expression;

        internal PayloadTypedSource(Receiver data, Expression<Func<TPayload, TProp>> expression)
        {
            _data = data;
            _expression = expression;
        }

        // The identity expression (x => x) selects no member: it reads the whole payload, for example
        // a response body that is itself the array a grid binds.
        internal override ValueExpression Expression
        {
            get
            {
                var payloadPath = ExpressionPathHelper.ToEventPath(_expression);
                return payloadPath.Length == 0
                    ? ValueExpression.ReadWhole(_data, Shape)
                    : ValueExpression.Read(_data, G.PathOf(Path.Parse(payloadPath)), Shape);
            }
        }
    }
}
