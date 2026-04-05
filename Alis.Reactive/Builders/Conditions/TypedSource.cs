using System;
using System.Linq.Expressions;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Builders.Conditions
{
    /// <summary>
    /// Preserves the property type through the condition and mutation pipeline
    /// for compile-time type safety.
    /// </summary>
    public abstract class TypedSource<TProp>
    {
        /// <summary>
        /// Produces a ValueProducer that reads this source's value.
        /// </summary>
        public abstract ValueProducer ToValueProducer();

        /// <summary>
        /// Returns the ComponentSource for this typed source (for Set/Call reactions).
        /// Only valid for component sources.
        /// </summary>
        public virtual ComponentSource ToComponentSource() =>
            throw new InvalidOperationException("Not a component source.");

        /// <summary>
        /// The member name to read on the resolved source.
        /// </summary>
        public virtual string ReadMember => throw new InvalidOperationException("Not a component source.");

        /// <summary>
        /// Shape inferred from TProp.
        /// </summary>
        public Shape Shape => Shape.FromClrType(typeof(TProp));

        /// <summary>
        /// Element shape for array types (e.g., Shape.String for string[]).
        /// </summary>
        public Shape ElementShape
        {
            get
            {
                var t = typeof(TProp);
                if (t.IsArray) return Shape.FromClrType(t.GetElementType());
                if (t.IsGenericType) return Shape.FromClrType(t.GetGenericArguments()[0]);
                return Shape.None;
            }
        }
    }

    /// <summary>
    /// A typed source that reads from the event payload.
    /// </summary>
    public sealed class EventArgSource<TPayload, TProp> : TypedSource<TProp>
    {
        private readonly Expression<Func<TPayload, TProp>> _expression;

        public EventArgSource(Expression<Func<TPayload, TProp>> expression)
        {
            _expression = expression;
        }

        public override ValueProducer ToValueProducer()
        {
            var eventPath = ExpressionPathHelper.ToEventPath(_expression);
            return ValueProducer.Read(PayloadSource.Event(), eventPath, shape: Shape);
        }
    }
}
