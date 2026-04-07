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

        internal override ValueProducer ToValueProducer()
        {
            var member = ExpressionPathHelper.ToEventPath(_expression);
            return ValueProducer.Read(_source, member, shape: Shape);
        }
    }
}
