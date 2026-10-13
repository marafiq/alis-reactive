using System;
using System.Collections.Generic;
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
        public TNext Truthy() => Unary(CompareOperator.Truthy);

        /// <summary>Matches a falsy value: null, zero or empty.</summary>
        public TNext Falsy() => Unary(CompareOperator.Falsy);

        /// <summary>Matches null.</summary>
        public TNext IsNull() => Unary(CompareOperator.IsNull);

        /// <summary>Matches any value but null.</summary>
        public TNext NotNull() => Unary(CompareOperator.NotNull);

        /// <summary>Matches an empty value: null, empty text or an empty collection.</summary>
        public TNext IsEmpty() => Unary(CompareOperator.IsEmpty);

        /// <summary>Matches a value that is not empty.</summary>
        public TNext NotEmpty() => Unary(CompareOperator.NotEmpty);

        /// <summary>Matches when the source is one of <paramref name="values"/>.</summary>
        /// <param name="values">The accepted values.</param>
        public TNext In(params TProp[] values) => Among(CompareOperator.In, values);

        /// <summary>Matches when the source is none of <paramref name="values"/>.</summary>
        /// <param name="values">The rejected values.</param>
        public TNext NotIn(params TProp[] values) => Among(CompareOperator.NotIn, values);

        internal Shape Shape => _left.Shape;

        internal TNext CompareWith(CompareOperator op, object? literal) =>
            _next(ConditionGraph.Binary(op, Left, ValueExpression.LiteralRaw(literal, Shape), Shape));

        internal TNext CompareWith<TRight>(CompareOperator op, TypedSource<TRight> right)
        {
            if (right == null) throw new ArgumentNullException(nameof(right));
            return _next(ConditionGraph.Binary(op, Left, right.ToValueExpression(), Shape));
        }

        internal TNext CompareWithText(CompareOperator op, string operand) =>
            _next(ConditionGraph.Binary(op, Left, ValueExpression.LiteralRaw(operand, PlanModel.Shape.String), Shape));

        internal TNext CompareWithLength(int length)
        {
            var minimum = MinimumTextLength.From(length, nameof(length));
            return _next(ConditionGraph.Binary(
                CompareOperator.MinLength, Left, ValueExpression.LiteralRaw(minimum.Value, PlanModel.Shape.Number), Shape));
        }

        internal TNext CompareWithRange(object low, object high)
        {
            var endpoints = new List<ValueExpression> { ValueExpression.LiteralRaw(low, Shape), ValueExpression.LiteralRaw(high, Shape) };
            return _next(ConditionGraph.Binary(
                CompareOperator.Between, Left, ValueExpression.Array(endpoints, PlanModel.Shape.ArrayOf(ItemShape(Shape))), Shape));
        }

        internal TNext ContainsItem(object? item) =>
            _next(ConditionGraph.CollectionItem(
                CompareOperator.ArrayContains, Left, ValueExpression.LiteralRaw(item, _left.ElementShape), Shape, _left.ElementShape));

        private ValueExpression Left => _left.ToValueExpression();

        private TNext Unary(CompareOperator op) => _next(ConditionGraph.Unary(op, Left, Shape));

        private TNext Among(CompareOperator op, TProp[] values)
        {
            if (values == null) throw new ArgumentNullException(nameof(values));
            var left = Left;
            var items = new List<ValueExpression>();
            foreach (var value in values)
                items.Add(ValueExpression.LiteralRaw(value, Shape));

            return _next(ConditionGraph.Binary(op, left, ValueExpression.Array(items, PlanModel.Shape.ArrayOf(ItemShape(Shape))), Shape));
        }

        private static Shape ItemShape(Shape shape) => shape.IsNone ? PlanModel.Shape.Any : shape;
    }
}
