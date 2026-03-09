using System;
using System.Linq.Expressions;
using Alis.Reactive.Descriptors.Guards;

namespace Alis.Reactive.Builders.Conditions
{
    public abstract class TypedSource<TProp>
    {
        public abstract BindSource ToBindSource();
        public string CoercionType => CoercionTypes.InferFromType(typeof(TProp));
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
