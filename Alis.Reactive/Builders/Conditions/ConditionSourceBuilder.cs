using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Builders.Conditions
{
    /// <summary>
    /// Provides typed comparison operators for a value source in a condition.
    /// </summary>
    /// <remarks>
    /// Obtained via <c>p.When(source)</c> or <c>p.When(args, x =&gt; x.Prop)</c>.
    /// Chain an operator (e.g. <c>.Eq(5)</c>, <c>.Truthy()</c>) to produce a <see cref="GuardBuilder{TModel}"/>.
    /// </remarks>
    /// <typeparam name="TModel">The view model type.</typeparam>
    /// <typeparam name="TProp">The source value type, providing compile-time operator type safety.</typeparam>
    public sealed class ConditionSourceBuilder<TModel, TProp> where TModel : class
    {
        private readonly TypedSource<TProp> _typedSource;
        private readonly Shape _shape;
        private readonly ConditionComposition _composition;
        private readonly ConditionContinuation<TModel> _continuation;

        internal ConditionSourceBuilder(TypedSource<TProp> source, PipelineBuilder<TModel> pipeline)
            : this(source, ConditionContinuation<TModel>.ForPipeline(pipeline), ConditionComposition.None)
        {
        }

        internal ConditionSourceBuilder(TypedSource<TProp> source)
            : this(source, ConditionContinuation<TModel>.Standalone, ConditionComposition.None)
        {
        }

        internal ConditionSourceBuilder(TypedSource<TProp> source, BranchBuilder<TModel> branchBuilder)
            : this(source, ConditionContinuation<TModel>.ForBranch(branchBuilder), ConditionComposition.None)
        {
        }

        internal ConditionSourceBuilder(
            TypedSource<TProp> source,
            ConditionContinuation<TModel> continuation,
            ConditionComposition composition)
        {
            _typedSource = source ?? throw new System.ArgumentNullException(nameof(source));
            _shape = source.Shape;
            _continuation = continuation ?? throw new System.ArgumentNullException(nameof(continuation));
            _composition = composition ?? throw new System.ArgumentNullException(nameof(composition));
        }

        // Comparison operators (typed operand)
        /// <summary>True when the source value equals <paramref name="operand"/>.</summary>
        public GuardBuilder<TModel> Eq(TProp operand) => BuildLiteral(CompareOperator.Eq, operand);
        /// <summary>True when the source value does not equal <paramref name="operand"/>.</summary>
        public GuardBuilder<TModel> NotEq(TProp operand) => BuildLiteral(CompareOperator.Neq, operand);
        /// <summary>True when the source value is greater than <paramref name="operand"/>.</summary>
        public GuardBuilder<TModel> Gt(TProp operand) => BuildLiteral(CompareOperator.Gt, operand);
        /// <summary>True when the source value is greater than or equal to <paramref name="operand"/>.</summary>
        public GuardBuilder<TModel> Gte(TProp operand) => BuildLiteral(CompareOperator.Gte, operand);
        /// <summary>True when the source value is less than <paramref name="operand"/>.</summary>
        public GuardBuilder<TModel> Lt(TProp operand) => BuildLiteral(CompareOperator.Lt, operand);
        /// <summary>True when the source value is less than or equal to <paramref name="operand"/>.</summary>
        public GuardBuilder<TModel> Lte(TProp operand) => BuildLiteral(CompareOperator.Lte, operand);

        // Presence operators
        /// <summary>True when the source value is truthy (non-null, non-zero, non-empty).</summary>
        public GuardBuilder<TModel> Truthy() => BuildUnary(CompareOperator.Truthy);
        /// <summary>True when the source value is falsy (null, zero, or empty).</summary>
        public GuardBuilder<TModel> Falsy() => BuildUnary(CompareOperator.Falsy);
        /// <summary>True when the source value is null.</summary>
        public GuardBuilder<TModel> IsNull() => BuildUnary(CompareOperator.IsNull);
        /// <summary>True when the source value is not null.</summary>
        public GuardBuilder<TModel> NotNull() => BuildUnary(CompareOperator.NotNull);
        /// <summary>True when the source value is empty (empty string or empty collection).</summary>
        public GuardBuilder<TModel> IsEmpty() => BuildUnary(CompareOperator.IsEmpty);
        /// <summary>True when the source value is not empty.</summary>
        public GuardBuilder<TModel> NotEmpty() => BuildUnary(CompareOperator.NotEmpty);

        // Membership
        /// <summary>True when the source value is in the specified set.</summary>
        public GuardBuilder<TModel> In(params TProp[] values) => BuildArray(CompareOperator.In, values);
        /// <summary>True when the source value is not in the specified set.</summary>
        public GuardBuilder<TModel> NotIn(params TProp[] values) => BuildArray(CompareOperator.NotIn, values);

        // Range
        /// <summary>True when the source value is between <paramref name="low"/> and <paramref name="high"/> inclusive.</summary>
        public GuardBuilder<TModel> Between(TProp low, TProp high) =>
            Build(
                CompareOperator.Between,
                ConditionOperand.InclusiveRange(low, high));

        // Text operators
        /// <summary>True when the source string contains the substring.</summary>
        public GuardBuilder<TModel> Contains(string substring) =>
            BuildTextLiteral(CompareOperator.Contains, substring);
        /// <summary>True when the source string starts with the prefix.</summary>
        public GuardBuilder<TModel> StartsWith(string prefix) =>
            BuildTextLiteral(CompareOperator.StartsWith, prefix);
        /// <summary>True when the source string ends with the suffix.</summary>
        public GuardBuilder<TModel> EndsWith(string suffix) =>
            BuildTextLiteral(CompareOperator.EndsWith, suffix);
        /// <summary>True when the source string matches the regex pattern.</summary>
        public GuardBuilder<TModel> Matches(string pattern) =>
            BuildTextLiteral(CompareOperator.Matches, pattern);
        /// <summary>True when the source string length is at least <paramref name="length"/>.</summary>
        public GuardBuilder<TModel> MinLength(int length) =>
            Build(
                CompareOperator.MinLength,
                ConditionOperand.MinimumTextLength(length));

        // Array
        /// <summary>True when the source array contains the specified item.</summary>
        public GuardBuilder<TModel> ArrayContains(object item)
        {
            return Build(
                CompareOperator.ArrayContains,
                ConditionOperand.CollectionItem(item, _typedSource.ElementShape));
        }

        // Source-vs-source comparison
        /// <summary>True when the source value equals another typed source value.</summary>
        public GuardBuilder<TModel> Eq(TypedSource<TProp> right) => BuildVsSource(CompareOperator.Eq, right);
        /// <summary>True when the source value does not equal another typed source value.</summary>
        public GuardBuilder<TModel> NotEq(TypedSource<TProp> right) => BuildVsSource(CompareOperator.Neq, right);
        /// <summary>True when the source value is greater than another typed source value.</summary>
        public GuardBuilder<TModel> Gt(TypedSource<TProp> right) => BuildVsSource(CompareOperator.Gt, right);
        /// <summary>True when the source value is greater than or equal to another typed source value.</summary>
        public GuardBuilder<TModel> Gte(TypedSource<TProp> right) => BuildVsSource(CompareOperator.Gte, right);
        /// <summary>True when the source value is less than another typed source value.</summary>
        public GuardBuilder<TModel> Lt(TypedSource<TProp> right) => BuildVsSource(CompareOperator.Lt, right);
        /// <summary>True when the source value is less than or equal to another typed source value.</summary>
        public GuardBuilder<TModel> Lte(TypedSource<TProp> right) => BuildVsSource(CompareOperator.Lte, right);

        private GuardBuilder<TModel> BuildVsSource(CompareOperator op, TypedSource<TProp> right)
        {
            if (right == null) throw new System.ArgumentNullException(nameof(right));
            return Build(
                op,
                ConditionOperand.Source(right.ToValueProducer()));
        }

        private GuardBuilder<TModel> BuildLiteral(CompareOperator op, object? operand) =>
            Build(
                op,
                ConditionOperand.Literal(operand));

        private GuardBuilder<TModel> BuildTextLiteral(CompareOperator op, string operand) =>
            Build(
                op,
                ConditionOperand.TextLiteral(operand));

        private GuardBuilder<TModel> BuildUnary(CompareOperator op) =>
            Build(
                op,
                ConditionOperand.Unary);

        private GuardBuilder<TModel> BuildArray(CompareOperator op, System.Array values) =>
            Build(
                op,
                ConditionOperand.Array(values));

        private GuardBuilder<TModel> Build(CompareOperator op, ConditionOperand operand)
        {
            var condition = Condition.Compare(
                op,
                operand.ToComparisonOperands(_typedSource.ToValueProducer(), _shape));
            return ComposeAndWrap(condition);
        }


        private GuardBuilder<TModel> ComposeAndWrap(Condition newCondition)
        {
            return _continuation.Wrap(_composition.Compose(newCondition));
        }
    }

    internal abstract class ConditionOperand
    {
        private ConditionOperand() { }

        internal static ConditionOperand Unary { get; } =
            new UnaryConditionOperand();

        internal static ConditionOperand Literal(object? value) =>
            ShapedLiteral(value, Shape.None);

        internal static ConditionOperand TextLiteral(string value) =>
            ShapedLiteral(value, Shape.String);

        internal static ConditionOperand MinimumTextLength(int length)
        {
            var minimumLength = Alis.Reactive.PlanModel.MinimumTextLength.From(length, nameof(length));
            return ShapedLiteral(minimumLength.Value, Shape.Number);
        }

        private static ConditionOperand ShapedLiteral(object? value, Shape shape) =>
            new LiteralConditionOperand(value, shape);

        internal static ConditionOperand Array(System.Array values) =>
            new ArrayConditionOperand(values);

        internal static ConditionOperand Source(ValueProducer value) =>
            new SourceConditionOperand(value);

        internal static ConditionOperand CollectionItem(object item, Shape itemShape) =>
            new CollectionItemConditionOperand(item, itemShape);

        internal static ConditionOperand InclusiveRange<T>(T low, T high) =>
            new InclusiveRangeConditionOperand<T>(low, high);

        internal abstract ComparisonOperands ToComparisonOperands(ValueProducer left, Shape leftShape);

        private sealed class UnaryConditionOperand : ConditionOperand
        {
            internal override ComparisonOperands ToComparisonOperands(ValueProducer left, Shape leftShape)
            {
                return ComparisonOperands.Unary(left, leftShape);
            }
        }

        private sealed class LiteralConditionOperand : ConditionOperand
        {
            private readonly object? _value;
            private readonly Shape _shape;

            internal LiteralConditionOperand(object? value, Shape shape)
            {
                _value = value;
                _shape = shape ?? throw new System.ArgumentNullException(nameof(shape));
            }

            internal override ComparisonOperands ToComparisonOperands(ValueProducer left, Shape leftShape)
            {
                var literalShape = ShapeOrLeftShape(leftShape);
                return ComparisonOperands.Binary(
                    left,
                    ValueProducer.LiteralRaw(_value, literalShape),
                    leftShape);
            }

            private Shape ShapeOrLeftShape(Shape leftShape) =>
                _shape.IsNone ? leftShape : _shape;
        }

        private sealed class ArrayConditionOperand : ConditionOperand
        {
            private readonly System.Array _values;

            internal ArrayConditionOperand(System.Array values)
            {
                _values = values ?? throw new System.ArgumentNullException(nameof(values));
            }

            internal override ComparisonOperands ToComparisonOperands(ValueProducer left, Shape leftShape)
            {
                var items = new System.Collections.Generic.List<ValueProducer>();
                foreach (var item in _values)
                    items.Add(ValueProducer.LiteralRaw(item, leftShape));

                return ComparisonOperands.Binary(
                    left,
                    ValueProducer.Array(items, ConditionCollectionShape.FromItemShape(leftShape)),
                    leftShape);
            }
        }

        private sealed class SourceConditionOperand : ConditionOperand
        {
            private readonly ValueProducer _value;

            internal SourceConditionOperand(ValueProducer value)
            {
                _value = value ?? throw new System.ArgumentNullException(nameof(value));
            }

            internal override ComparisonOperands ToComparisonOperands(ValueProducer left, Shape leftShape)
            {
                return ComparisonOperands.Binary(left, _value, leftShape);
            }
        }

        private sealed class CollectionItemConditionOperand : ConditionOperand
        {
            private readonly object? _item;
            private readonly Shape _itemShape;

            internal CollectionItemConditionOperand(object? item, Shape itemShape)
            {
                _item = item;
                _itemShape = itemShape ?? throw new System.ArgumentNullException(nameof(itemShape));
            }

            internal override ComparisonOperands ToComparisonOperands(ValueProducer left, Shape leftShape)
            {
                var right = ValueProducer.LiteralRaw(_item, _itemShape);
                return ComparisonOperands.CollectionItem(left, right, leftShape, _itemShape);
            }
        }

        private sealed class InclusiveRangeConditionOperand<T> : ConditionOperand
        {
            private readonly T _low;
            private readonly T _high;

            internal InclusiveRangeConditionOperand(T low, T high)
            {
                _low = low;
                _high = high;
            }

            internal override ComparisonOperands ToComparisonOperands(ValueProducer left, Shape leftShape)
            {
                var endpoints = new System.Collections.Generic.List<ValueProducer>
                {
                    ValueProducer.LiteralRaw(_low, leftShape),
                    ValueProducer.LiteralRaw(_high, leftShape)
                };

                return ComparisonOperands.Binary(
                    left,
                    ValueProducer.Array(endpoints, ConditionCollectionShape.FromItemShape(leftShape)),
                    leftShape);
            }
        }
    }

    internal static class ConditionCollectionShape
    {
        internal static Shape FromItemShape(Shape itemShape)
        {
            if (itemShape == null) throw new System.ArgumentNullException(nameof(itemShape));
            if (itemShape.IsNone) return Shape.ArrayOf(Shape.Any);

            return Shape.ArrayOf(itemShape);
        }
    }

}
