using System;
using System.Linq;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Builders.Conditions
{
    /// <summary>
    /// The left side of a comparison: a value source waiting for its operator.
    /// </summary>
    /// <remarks>
    /// Obtained from <c>p.When(..)</c>, <c>ElseIf(..)</c>, <c>And(..)</c>, <c>Or(..)</c> or a group's
    /// <c>g.When(..)</c>. Equality, membership and emptiness apply to every type. Ordering
    /// (<c>Gt</c>, <c>Gte</c>, <c>Lt</c>, <c>Lte</c>, <c>Between</c>) applies only to comparable values
    /// (numbers, dates, times and their nullable forms), text operators (<c>Contains</c>,
    /// <c>StartsWith</c>, <c>EndsWith</c>, <c>Matches</c>, <c>MinLength</c>) only to text, and
    /// <c>ArrayContains</c> only to collections; they are extension methods in <see cref="Alis.Reactive"/>.
    /// </remarks>
    /// <typeparam name="TModel">View model that owns the guarded pipeline.</typeparam>
    /// <typeparam name="TProp">The source's type.</typeparam>
    /// <typeparam name="TNext">What the comparison continues as: a guard that offers <c>Then</c>, or a group term.</typeparam>
    public sealed class ConditionSourceBuilder<TModel, TProp, TNext>
        where TModel : class
        where TNext : Condition<TModel>
    {
        private readonly TypedSource<TProp> _left;
        private readonly Func<ConditionGraph, TNext> _next;

        internal ConditionSourceBuilder(TypedSource<TProp> source, Func<ConditionGraph, TNext> next)
        {
            _left = source ?? throw new ArgumentNullException(nameof(source));
            _next = next ?? throw new ArgumentNullException(nameof(next));
        }

        /// <summary>Matches when the source equals <paramref name="operand"/>.</summary>
        /// <param name="operand">The value compared with the source.</param>
        public TNext Eq(TProp operand) => CompareWith(CompareOperator.Eq, operand);

        /// <summary>Matches when the source differs from <paramref name="operand"/>.</summary>
        /// <param name="operand">The value compared with the source.</param>
        public TNext NotEq(TProp operand) => CompareWith(CompareOperator.Neq, operand);

        /// <summary>Matches when the source equals another value read when the condition runs.</summary>
        /// <param name="right">The value compared with the source.</param>
        public TNext Eq(TypedSource<TProp> right) => CompareWith(CompareOperator.Eq, right);

        /// <summary>Matches when the source differs from another value read when the condition runs.</summary>
        /// <param name="right">The value compared with the source.</param>
        public TNext NotEq(TypedSource<TProp> right) => CompareWith(CompareOperator.Neq, right);

        /// <summary>Matches a truthy value: not null, not zero, not empty.</summary>
        public TNext Truthy() => Unary(ValueTest.Truthy);

        /// <summary>Matches a falsy value: null, zero or empty.</summary>
        public TNext Falsy() => Unary(ValueTest.Falsy);

        /// <summary>Matches null.</summary>
        public TNext IsNull() => Unary(ValueTest.IsNull);

        /// <summary>Matches any value but null.</summary>
        public TNext NotNull() => Unary(ValueTest.NotNull);

        /// <summary>Matches an empty value: null, empty text or an empty collection.</summary>
        public TNext IsEmpty() => Unary(ValueTest.IsEmpty);

        /// <summary>Matches a value that is not empty.</summary>
        public TNext NotEmpty() => Unary(ValueTest.NotEmpty);

        /// <summary>Matches when the source is one of <paramref name="values"/>.</summary>
        /// <param name="values">The accepted values.</param>
        public TNext In(params TProp[] values) => Among(values, negated: false);

        /// <summary>Matches when the source is none of <paramref name="values"/>.</summary>
        /// <param name="values">The rejected values.</param>
        public TNext NotIn(params TProp[] values) => Among(values, negated: true);

        internal Shape Shape => _left.Shape;

        internal TNext CompareWith(CompareOperator op, object? literal) =>
            _next(ConditionGraph.Ordered(op, Left, ValueExpression.LiteralFor(literal, Shape), Shape));

        internal TNext CompareWith<TRight>(CompareOperator op, TypedSource<TRight> right)
        {
            if (right == null) throw new ArgumentNullException(nameof(right));
            return _next(ConditionGraph.Ordered(op, Left, right.Expression, Shape));
        }

        internal TNext CompareWithText(TextTest test, string text) =>
            _next(ConditionGraph.Text(test, Left, text ?? throw new ArgumentNullException(nameof(text)), Shape));

        internal TNext CompareWithLength(int length)
        {
            var minimum = MinimumTextLength.From(length, nameof(length));
            return _next(ConditionGraph.MinLength(Left, minimum.Value, Shape));
        }

        internal TNext CompareWithRange(object low, object high) =>
            _next(ConditionGraph.Range(Left, ValueExpression.LiteralFor(low, Shape), ValueExpression.LiteralFor(high, Shape), Shape));

        internal TNext ContainsItem(object? item) =>
            _next(ConditionGraph.ArrayContains(Left, ValueExpression.LiteralFor(item, _left.ElementShape), Shape, _left.ElementShape));

        private ValueExpression Left => _left.Expression;

        private TNext Unary(ValueTest test) => _next(ConditionGraph.Unary(test, Left, Shape));

        private TNext Among(TProp[] values, bool negated)
        {
            if (values == null) throw new ArgumentNullException(nameof(values));
            var items = values.Select(value => ValueExpression.LiteralFor(value, Shape)).ToList();
            return _next(ConditionGraph.Membership(Left, items, negated, Shape));
        }
    }
}
