using System;
using System.Linq.Expressions;
using Alis.Reactive.Builders.Conditions;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Builders.Arrays
{
    /// <summary>
    /// An array read when the step runs, transformed and tested in the browser:
    /// <c>var residents = p.From(json.Get(x =&gt; x.Residents));</c>. A transformed array is itself a value:
    /// <c>grid.DataSource.Set(residents.Where(r =&gt; r.Active))</c>.
    /// </summary>
    /// <remarks>
    /// The operators run in the browser, not on the server. The type is deliberately not
    /// <see cref="System.Collections.IEnumerable"/> or <c>IQueryable</c>, so LINQ's extension methods
    /// never apply and each lambda describes the per-element work instead of running in C#.
    /// </remarks>
    /// <typeparam name="TElement">The type of each element.</typeparam>
    public sealed class ReactiveArray<TElement> : TypedSource<TElement[]>
    {
        private readonly ValueExpression _source;
        private readonly Shape _elementShape;

        internal ReactiveArray(ValueExpression source, Shape elementShape)
        {
            _source = source ?? throw new ArgumentNullException(nameof(source));
            _elementShape = elementShape ?? throw new ArgumentNullException(nameof(elementShape));
        }

        /// <summary>Keeps only the elements that match the per-element predicate.</summary>
        /// <param name="predicate">The test each kept element passes, for example <c>r =&gt; r.Active</c>.</param>
        /// <returns>The matching elements, in their order.</returns>
        public ReactiveArray<TElement> Where(Expression<Func<TElement, bool>> predicate) =>
            new ReactiveArray<TElement>(
                ValueExpression.ArrayFilter(_source, Predicate(predicate), _elementShape),
                _elementShape);

        /// <summary>Projects each element through a per-element selector.</summary>
        /// <typeparam name="TResult">The type each element projects to.</typeparam>
        /// <param name="selector">The projection, for example <c>r =&gt; r.Name</c>.</param>
        /// <returns>The projected elements, in their order.</returns>
        public ReactiveArray<TResult> Select<TResult>(Expression<Func<TElement, TResult>> selector)
        {
            var resultShape = Shape.FromClrType(typeof(TResult));
            return new ReactiveArray<TResult>(
                ValueExpression.ArrayMap(_source, Projection(selector), resultShape),
                resultShape);
        }

        /// <summary>Orders elements ascending by a per-element key.</summary>
        /// <typeparam name="TKey">The key's type: a string, number, date, bool or enum.</typeparam>
        /// <param name="key">The key, for example <c>r =&gt; r.Name</c>.</param>
        /// <returns>The elements in ascending key order.</returns>
        /// <exception cref="InvalidOperationException">Thrown when the key is not a scalar.</exception>
        public ReactiveArray<TElement> OrderBy<TKey>(Expression<Func<TElement, TKey>> key) =>
            Order(key, descending: false);

        /// <summary>Orders elements descending by a per-element key.</summary>
        /// <typeparam name="TKey">The key's type: a string, number, date, bool or enum.</typeparam>
        /// <param name="key">The key, for example <c>r =&gt; r.StartDate</c>.</param>
        /// <returns>The elements in descending key order.</returns>
        /// <exception cref="InvalidOperationException">Thrown when the key is not a scalar.</exception>
        public ReactiveArray<TElement> OrderByDescending<TKey>(Expression<Func<TElement, TKey>> key) =>
            Order(key, descending: true);

        private ReactiveArray<TElement> Order<TKey>(Expression<Func<TElement, TKey>> key, bool descending)
        {
            // Sort keys must coerce to comparable scalars. Non-scalar keys (object/collection)
            // serializes as Shape.Any, and the runtime would fall back to lexicographic
            // String(value) order, where every object becomes "[object Object]".
            // Reject it where it is authored rather than emit a silently wrong runtime sort.
            var keyKind = Shape.FromClrType(typeof(TKey)).Kind;
            var keyIsSortableScalar = keyKind is "string" or "number" or "boolean" or "date" or "instant" or "nullable";
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
        /// <returns>The count, read when the step runs.</returns>
        public ReactiveValue<int> Count() =>
            new ReactiveValue<int>(ValueExpression.ArrayCount(_source));

        /// <summary>Counts the elements that match the predicate.</summary>
        /// <param name="predicate">The test each counted element passes.</param>
        /// <returns>The count, read when the step runs.</returns>
        public ReactiveValue<int> Count(Expression<Func<TElement, bool>> predicate) =>
            Where(predicate).Count();

        /// <summary>True when the array is non-empty.</summary>
        /// <returns>Whether any element exists, read when the step runs.</returns>
        public ReactiveValue<bool> Any() =>
            new ReactiveValue<bool>(ValueExpression.ArrayAny(_source, predicate: null));

        /// <summary>True when any element matches the predicate.</summary>
        /// <param name="predicate">The test an element passes.</param>
        /// <returns>Whether any element passes, read when the step runs.</returns>
        public ReactiveValue<bool> Any(Expression<Func<TElement, bool>> predicate) =>
            new ReactiveValue<bool>(ValueExpression.ArrayAny(_source, Predicate(predicate)));

        /// <summary>True when every element matches the predicate.</summary>
        /// <param name="predicate">The test every element passes.</param>
        /// <returns>Whether every element passes, read when the step runs.</returns>
        public ReactiveValue<bool> All(Expression<Func<TElement, bool>> predicate) =>
            new ReactiveValue<bool>(ValueExpression.ArrayAll(_source, Predicate(predicate)));

        /// <summary>Sums an integer per-element selector.</summary>
        /// <param name="selector">The number each element adds, for example <c>r =&gt; r.OpenTasks</c>.</param>
        /// <returns>The sum, read when the step runs.</returns>
        public ReactiveValue<int> Sum(Expression<Func<TElement, int>> selector) =>
            new ReactiveValue<int>(ValueExpression.ArraySum(_source, Projection(selector)));

        /// <summary>Sums a decimal per-element selector.</summary>
        /// <param name="selector">The number each element adds.</param>
        /// <returns>The sum, read when the step runs.</returns>
        public ReactiveValue<decimal> Sum(Expression<Func<TElement, decimal>> selector) =>
            new ReactiveValue<decimal>(ValueExpression.ArraySum(_source, Projection(selector)));

        /// <summary>Sums a double per-element selector.</summary>
        /// <param name="selector">The number each element adds.</param>
        /// <returns>The sum, read when the step runs.</returns>
        public ReactiveValue<double> Sum(Expression<Func<TElement, double>> selector) =>
            new ReactiveValue<double>(ValueExpression.ArraySum(_source, Projection(selector)));

        /// <summary>Finds the first element matching the predicate, or null when none match.</summary>
        /// <param name="predicate">The test the element passes.</param>
        /// <returns>The element, read when the step runs.</returns>
        public ReactiveValue<TElement> Find(Expression<Func<TElement, bool>> predicate) =>
            new ReactiveValue<TElement>(
                ValueExpression.ArrayFind(_source, Predicate(predicate), projection: null, _elementShape));

        /// <summary>Finds a per-element field from the first element matching the predicate.</summary>
        /// <typeparam name="TField">The field's type.</typeparam>
        /// <param name="predicate">The test the element passes.</param>
        /// <param name="selector">The field read from that element, for example <c>r =&gt; r.Name</c>.</param>
        /// <returns>The field's value, read when the step runs.</returns>
        public ReactiveValue<TField> Find<TField>(
            Expression<Func<TElement, bool>> predicate, Expression<Func<TElement, TField>> selector)
        {
            var fieldShape = Shape.FromClrType(typeof(TField));
            return new ReactiveValue<TField>(
                ValueExpression.ArrayFind(_source, Predicate(predicate), Projection(selector), fieldShape));
        }

        internal override ValueExpression Expression => _source;

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
}
