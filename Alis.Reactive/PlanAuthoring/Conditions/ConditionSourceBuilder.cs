using System;
using System.Linq;
using Alis.Reactive.Grammar;
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
    /// (numbers, dates, times of day and their nullable forms; a <see cref="System.TimeSpan"/> is refused where
    /// the page renders, because its text does not order as its length does), text operators (<c>Contains</c>,
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
        private readonly Func<Condition, TNext> _next;

        internal ConditionSourceBuilder(TypedSource<TProp> source, Func<Condition, TNext> next)
        {
            _left = source ?? throw new ArgumentNullException(nameof(source));
            _next = next ?? throw new ArgumentNullException(nameof(next));
        }

        /// <summary>Matches when the source equals <paramref name="operand"/>.</summary>
        /// <param name="operand">The value compared with the source.</param>
        public TNext Eq(TProp operand) => CompareWith(Comparison.Equal, operand);

        /// <summary>Matches when the source differs from <paramref name="operand"/>.</summary>
        /// <param name="operand">The value compared with the source.</param>
        public TNext NotEq(TProp operand) => CompareWith(Comparison.NotEqual, operand);

        /// <summary>Matches when the source equals another value read when the condition runs.</summary>
        /// <param name="right">The value compared with the source.</param>
        public TNext Eq(TypedSource<TProp> right) => CompareWith(Comparison.Equal, right);

        /// <summary>Matches when the source differs from another value read when the condition runs.</summary>
        /// <param name="right">The value compared with the source.</param>
        public TNext NotEq(TypedSource<TProp> right) => CompareWith(Comparison.NotEqual, right);

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

        internal TNext CompareWith(Comparison op, object? literal)
        {
            if (!op.IsEquality) RequireBrowserOrder();
            return _next(Condition.Ordered(op, Left, ValueExpression.LiteralFor(literal, Shape), Shape));
        }

        internal TNext CompareWith<TRight>(Comparison op, TypedSource<TRight> right)
        {
            if (right == null) throw new ArgumentNullException(nameof(right));
            if (!op.IsEquality) RequireBrowserOrder();
            return _next(Condition.Ordered(op, Left, right.Expression, Shape));
        }

        internal TNext CompareWithText(TextTest test, string text) =>
            _next(Condition.Text(test, Left, text ?? throw new ArgumentNullException(nameof(text)), Shape));

        internal TNext CompareWithLength(int length)
        {
            var minimum = MinimumTextLength.From(length, nameof(length));
            return _next(Condition.MinLength(Left, minimum.Value, Shape));
        }

        internal TNext CompareWithRange(object low, object high)
        {
            RequireBrowserOrder();
            return _next(Condition.Range(Left, ValueExpression.LiteralFor(low, Shape), ValueExpression.LiteralFor(high, Shape), Shape));
        }

        // A TimeSpan reaches the browser as its text, and text orders by its characters: "1.02:00:00" (26 hours)
        // sorts before "23:00:00". The browser cannot order it as C# does, so the page is refused where it renders.
        private static void RequireBrowserOrder()
        {
            var type = System.Nullable.GetUnderlyingType(typeof(TProp)) ?? typeof(TProp);
            if (type == typeof(TimeSpan))
                throw new InvalidOperationException(
                    "A TimeSpan has no order the browser can compute as C# does: it reaches the browser as its text, and " +
                    "\"1.02:00:00\" sorts before \"23:00:00\". Order it on the server, or read the length as a number.");
        }

        internal TNext ContainsItem<TItem>(TItem item)
        {
            var itemShape = Grammar.Shape.FromClrType(typeof(TItem));
            return _next(Condition.ArrayContains(Left, ValueExpression.LiteralFor(item, itemShape), Shape, itemShape));
        }

        private ValueExpression Left => _left.Expression;

        private TNext Unary(ValueTest test) => _next(Condition.Unary(test, Left, Shape));

        private TNext Among(TProp[] values, bool negated)
        {
            if (values == null) throw new ArgumentNullException(nameof(values));
            var items = values.Select(value => ValueExpression.LiteralFor(value, Shape)).ToList();
            return _next(Condition.Membership(Left, items, negated, Shape));
        }
    }
}
