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
        private readonly System.Func<ConditionGraph, ConditionGraph> _composeCondition;
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
            System.Func<ConditionGraph, ConditionGraph> composeCondition)
        {
            _typedSource = source ?? throw new System.ArgumentNullException(nameof(source));
            _shape = source.Shape;
            _continuation = continuation ?? throw new System.ArgumentNullException(nameof(continuation));
            _composeCondition = composeCondition ?? throw new System.ArgumentNullException(nameof(composeCondition));
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
            Build(CompareOperator.Between, RangeOperands(low, high));

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
            Build(CompareOperator.MinLength, MinimumLengthOperands(length));

        // Array
        /// <summary>True when the source array contains the specified item.</summary>
        public GuardBuilder<TModel> ArrayContains(object item)
        {
            return Build(CompareOperator.ArrayContains, CollectionItemOperands(item));
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
            return Build(op, SourceOperands(right));
        }

        private GuardBuilder<TModel> BuildLiteral(CompareOperator op, object? operand) =>
            Build(op, LiteralOperands(operand));

        private GuardBuilder<TModel> BuildTextLiteral(CompareOperator op, string operand) =>
            Build(op, TextLiteralOperands(operand));

        private GuardBuilder<TModel> BuildUnary(CompareOperator op) =>
            Build(op, UnaryOperands());

        private GuardBuilder<TModel> BuildArray(CompareOperator op, System.Array values) =>
            Build(op, ArrayOperands(values));

        private GuardBuilder<TModel> Build(CompareOperator op, ComparisonOperands operands)
        {
            var condition = ConditionGraph.Compare(op, operands);
            return ComposeAndWrap(condition);
        }

        private ValueExpression LeftValue() => _typedSource.ToValueExpression();

        private ComparisonOperands UnaryOperands() =>
            ComparisonOperands.Unary(LeftValue(), _shape);

        private ComparisonOperands LiteralOperands(object? operand) =>
            ComparisonOperands.Binary(
                LeftValue(),
                ValueExpression.LiteralRaw(operand, _shape),
                _shape);

        private ComparisonOperands TextLiteralOperands(string operand) =>
            ComparisonOperands.Binary(
                LeftValue(),
                ValueExpression.LiteralRaw(operand, Shape.String),
                _shape);

        private ComparisonOperands MinimumLengthOperands(int length)
        {
            var minimumLength = MinimumTextLength.From(length, nameof(length));
            return ComparisonOperands.Binary(
                LeftValue(),
                ValueExpression.LiteralRaw(minimumLength.Value, Shape.Number),
                _shape);
        }

        private ComparisonOperands ArrayOperands(System.Array values)
        {
            if (values == null) throw new System.ArgumentNullException(nameof(values));

            var items = new System.Collections.Generic.List<ValueExpression>();
            foreach (var item in values)
                items.Add(ValueExpression.LiteralRaw(item, _shape));

            return ComparisonOperands.Binary(
                LeftValue(),
                ValueExpression.Array(items, Shape.ArrayOf(_shape.IsNone ? Shape.Any : _shape)),
                _shape);
        }

        private ComparisonOperands SourceOperands(TypedSource<TProp> right) =>
            ComparisonOperands.Binary(LeftValue(), right.ToValueExpression(), _shape);

        private ComparisonOperands CollectionItemOperands(object item) =>
            ComparisonOperands.CollectionItem(
                LeftValue(),
                ValueExpression.LiteralRaw(item, _typedSource.ElementShape),
                _shape,
                _typedSource.ElementShape);

        private ComparisonOperands RangeOperands(TProp low, TProp high)
        {
            var endpoints = new System.Collections.Generic.List<ValueExpression>
            {
                ValueExpression.LiteralRaw(low, _shape),
                ValueExpression.LiteralRaw(high, _shape)
            };

            return ComparisonOperands.Binary(
                LeftValue(),
                ValueExpression.Array(endpoints, Shape.ArrayOf(_shape.IsNone ? Shape.Any : _shape)),
                _shape);
        }

        private GuardBuilder<TModel> ComposeAndWrap(ConditionGraph newCondition)
        {
            return _continuation.Wrap(_composeCondition(newCondition));
        }
    }

}
