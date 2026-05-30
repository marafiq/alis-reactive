using System;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Builders.Arrays
{
    /// <summary>
    /// A typed, deferred array transform. Operators capture authoring intent and compile to
    /// plan-JSON <c>array-op</c> nodes — nothing executes on the server. Deliberately NOT
    /// <see cref="System.Collections.IEnumerable"/>/<c>IQueryable</c>, so LINQ extension methods
    /// are not candidates (no collision) and lambdas are captured, not invoked.
    /// </summary>
    /// <typeparam name="TElement">The element type, carried through transforms.</typeparam>
    public sealed class ReactiveArray<TElement>
    {
        private readonly ValueExpression _source;
        private readonly Shape _elementShape;

        internal ReactiveArray(ValueExpression source, Shape elementShape)
        {
            _source = source ?? throw new ArgumentNullException(nameof(source));
            _elementShape = elementShape ?? throw new ArgumentNullException(nameof(elementShape));
        }

        /// <summary>Counts the elements of the source array.</summary>
        public ReactiveValue<int> Count() =>
            new ReactiveValue<int>(ValueExpression.ArrayCount(_source, _elementShape));
    }
}
