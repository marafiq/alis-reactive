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
        internal abstract ValueProducer ToValueProducer();

        /// <summary>
        /// Returns the ComponentSource for this typed source (for Set/Call reactions).
        /// Only valid for component sources.
        /// </summary>
        internal virtual ComponentSource ToComponentSource() =>
            throw new InvalidOperationException("Not a component source.");

        /// <summary>
        /// The member name to read on the resolved source.
        /// </summary>
        internal virtual string ReadMember => throw new InvalidOperationException("Not a component source.");

        /// <summary>
        /// Shape inferred from TProp.
        /// </summary>
        internal Shape Shape => Shape.FromClrType(typeof(TProp));

        /// <summary>
        /// Element shape for array types (e.g., Shape.String for string[]).
        /// </summary>
        internal Shape ElementShape
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
    /// Delegates to <see cref="PayloadTypedSource{TPayload, TProp}"/> with event scope.
    /// </summary>
    public sealed class EventArgSource<TPayload, TProp> : TypedSource<TProp>
    {
        private readonly PayloadTypedSource<TPayload, TProp> _inner;

        internal EventArgSource(Expression<Func<TPayload, TProp>> expression)
        {
            _inner = new PayloadTypedSource<TPayload, TProp>(PayloadSource.Event(), expression);
        }

        internal override ValueProducer ToValueProducer() => _inner.ToValueProducer();
    }
}
