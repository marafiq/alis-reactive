using System;
using System.Linq.Expressions;
using Alis.Reactive.Builders.Conditions;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Builders.Arrays
{
    /// <summary>
    /// Typed, deferred array transform. Operators capture authoring intent as
    /// Reactive Plan <c>array-op</c> nodes; they do not execute on the server. Deliberately not
    /// <see cref="System.Collections.IEnumerable"/>/<c>IQueryable</c>, so LINQ extension methods
    /// are not candidates (no collision) and lambdas are captured, not invoked. Per-element
    /// predicates and selectors read the element scope; chains compose as plan nodes.
    /// </summary>
    /// <typeparam name="TElement">Element type carried through transforms.</typeparam>
    public sealed class ReactiveArray<TElement>
    {
        private readonly ValueExpression _source;
        private readonly Shape _elementShape;

        internal ReactiveArray(ValueExpression source, Shape elementShape)
        {
            _source = source ?? throw new ArgumentNullException(nameof(source));
            _elementShape = elementShape ?? throw new ArgumentNullException(nameof(elementShape));
        }

        /// <summary>Keeps only the elements that match the per-element predicate.</summary>
        public ReactiveArray<TElement> Where(Expression<Func<TElement, bool>> predicate) =>
            new ReactiveArray<TElement>(
                ValueExpression.ArrayFilter(_source, Predicate(predicate), _elementShape),
                _elementShape);

        /// <summary>Projects each element through a per-element selector.</summary>
        public ReactiveArray<TResult> Select<TResult>(Expression<Func<TElement, TResult>> selector)
        {
            var resultShape = Shape.FromClrType(typeof(TResult));
            return new ReactiveArray<TResult>(
                ValueExpression.ArrayMap(_source, Projection(selector), _elementShape, resultShape),
                resultShape);
        }

        /// <summary>Orders elements ascending by a per-element key.</summary>
        public ReactiveArray<TElement> OrderBy<TKey>(Expression<Func<TElement, TKey>> key) =>
            Order(key, descending: false);

        /// <summary>Orders elements descending by a per-element key.</summary>
        public ReactiveArray<TElement> OrderByDescending<TKey>(Expression<Func<TElement, TKey>> key) =>
            Order(key, descending: true);

        private ReactiveArray<TElement> Order<TKey>(Expression<Func<TElement, TKey>> key, bool descending)
        {
            // A sort key must coerce to a comparable scalar. A non-scalar key (object/collection)
            // serializes as Shape.Any, and the runtime would fall back to lexicographic
            // String(value) order, where every object becomes "[object Object]".
            // Reject it where it is authored rather than emit a silently wrong runtime sort.
            var keyKind = Shape.FromClrType(typeof(TKey)).Kind;
            var keyIsSortableScalar = keyKind is "string" or "number" or "boolean" or "date" or "nullable";
            if (!keyIsSortableScalar)
                throw new InvalidOperationException(
                    "OrderBy key must project to a scalar (string, number, date, bool, enum), not '" +
                    typeof(TKey).Name + "'. Project a scalar field, e.g. .OrderBy(x => x.StartDate), " +
                    "not .OrderBy(x => x.Address).");

            return new ReactiveArray<TElement>(
                ValueExpression.ArrayOrderBy(_source, Projection(key), _elementShape, descending),
                _elementShape);
        }

        /// <summary>Counts all elements.</summary>
        public ReactiveValue<int> Count() =>
            new ReactiveValue<int>(ValueExpression.ArrayCount(_source, _elementShape));

        /// <summary>Counts the elements that match the predicate.</summary>
        public ReactiveValue<int> Count(Expression<Func<TElement, bool>> predicate) =>
            Where(predicate).Count();

        /// <summary>True when the array is non-empty.</summary>
        public ReactiveValue<bool> Any() =>
            new ReactiveValue<bool>(ValueExpression.ArrayAny(_source, predicate: null, _elementShape));

        /// <summary>True when any element matches the predicate.</summary>
        public ReactiveValue<bool> Any(Expression<Func<TElement, bool>> predicate) =>
            new ReactiveValue<bool>(ValueExpression.ArrayAny(_source, Predicate(predicate), _elementShape));

        /// <summary>True when every element matches the predicate.</summary>
        public ReactiveValue<bool> All(Expression<Func<TElement, bool>> predicate) =>
            new ReactiveValue<bool>(ValueExpression.ArrayAll(_source, Predicate(predicate), _elementShape));

        /// <summary>Sums an integer per-element selector.</summary>
        public ReactiveValue<int> Sum(Expression<Func<TElement, int>> selector) =>
            new ReactiveValue<int>(ValueExpression.ArraySum(_source, Projection(selector), _elementShape));

        /// <summary>Sums a decimal per-element selector.</summary>
        public ReactiveValue<decimal> Sum(Expression<Func<TElement, decimal>> selector) =>
            new ReactiveValue<decimal>(ValueExpression.ArraySum(_source, Projection(selector), _elementShape));

        /// <summary>Sums a double per-element selector.</summary>
        public ReactiveValue<double> Sum(Expression<Func<TElement, double>> selector) =>
            new ReactiveValue<double>(ValueExpression.ArraySum(_source, Projection(selector), _elementShape));

        /// <summary>Finds the first element matching the predicate, or null when none match.</summary>
        public ReactiveValue<TElement> Find(Expression<Func<TElement, bool>> predicate) =>
            new ReactiveValue<TElement>(
                ValueExpression.ArrayFind(_source, Predicate(predicate), projection: null, _elementShape, _elementShape));

        /// <summary>Finds a per-element field from the first element matching the predicate.</summary>
        public ReactiveValue<TField> Find<TField>(
            Expression<Func<TElement, bool>> predicate, Expression<Func<TElement, TField>> selector)
        {
            var fieldShape = Shape.FromClrType(typeof(TField));
            return new ReactiveValue<TField>(
                ValueExpression.ArrayFind(_source, Predicate(predicate), Projection(selector), _elementShape, fieldShape));
        }

        /// <summary>
        /// Exposes the composed array as a typed source so the transformed array can bind to a
        /// component data source wherever a <see cref="TypedSource{T}"/> is accepted, such as a
        /// <c>SetDataSource(TypedSource&lt;T[]&gt;)</c> overload, without an HTTP round-trip. The
        /// underlying value is the same array-op expression the runtime already evaluates.
        /// </summary>
        public TypedSource<TElement[]> AsSource() => new ReactiveArraySource<TElement>(_source);

        private static ConditionGraph Predicate(Expression<Func<TElement, bool>> predicate)
        {
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));
            return ElementExpressionCompiler.CompilePredicate(predicate);
        }

        private static ValueExpression Projection<TValue>(Expression<Func<TElement, TValue>> selector)
        {
            if (selector == null) throw new ArgumentNullException(nameof(selector));
            return ElementExpressionCompiler.CompileProjection(selector);
        }
    }

    /// <summary>Array-op result exposed as a typed source for component data-source binding.</summary>
    internal sealed class ReactiveArraySource<TElement> : TypedSource<TElement[]>
    {
        private readonly ValueExpression _value;

        internal ReactiveArraySource(ValueExpression value)
        {
            _value = value ?? throw new ArgumentNullException(nameof(value));
        }

        internal override ValueExpression ToValueExpression() => _value;
    }
}
