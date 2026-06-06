using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Builders.Conditions
{
    /// <summary>
    /// Authors comparison nodes for a typed value source.
    /// </summary>
    /// <remarks>
    /// Obtained via <c>p.When(source)</c> or <c>p.When(args, x =&gt; x.Prop)</c>.
    /// Chain an operator such as <c>.Eq(5)</c> or <c>.Truthy()</c> to produce a
    /// <see cref="GuardBuilder{TModel}"/>. Operators are serialized into the
    /// Reactive Plan and evaluated by the runtime; they do not read values on the server.
    /// Literal and source comparisons use <typeparamref name="TProp"/> and the source
    /// shape for compile-time and runtime type context.
    /// </remarks>
    /// <typeparam name="TModel">The view model that owns the guarded pipeline.</typeparam>
    /// <typeparam name="TProp">The source value type, providing compile-time operator type safety.</typeparam>
    public sealed class ConditionSourceBuilder<TModel, TProp> where TModel : class
    {
        private readonly TypedSource<TProp> _leftSource;
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
            _leftSource = source ?? throw new System.ArgumentNullException(nameof(source));
            _shape = source.Shape;
            _continuation = continuation ?? throw new System.ArgumentNullException(nameof(continuation));
            _composeCondition = composeCondition ?? throw new System.ArgumentNullException(nameof(composeCondition));
        }

        /// <summary>Compares the source with a typed literal using equality.</summary>
        /// <param name="operand">The literal value to compare with the source value.</param>
        /// <returns>A guard for composing or attaching the comparison.</returns>
        public GuardBuilder<TModel> Eq(TProp operand) => BuildLiteral(CompareOperator.Eq, operand);
        /// <summary>Compares the source with a typed literal using inequality.</summary>
        /// <param name="operand">The literal value to compare with the source value.</param>
        /// <returns>A guard for composing or attaching the comparison.</returns>
        public GuardBuilder<TModel> NotEq(TProp operand) => BuildLiteral(CompareOperator.Neq, operand);
        /// <summary>Compares the source with a typed literal using an ordered greater-than check.</summary>
        /// <param name="operand">The literal value to compare with the source value.</param>
        /// <returns>A guard for composing or attaching the comparison.</returns>
        public GuardBuilder<TModel> Gt(TProp operand) => BuildLiteral(CompareOperator.Gt, operand);
        /// <summary>Compares the source with a typed literal using an ordered greater-than-or-equal check.</summary>
        /// <param name="operand">The literal value to compare with the source value.</param>
        /// <returns>A guard for composing or attaching the comparison.</returns>
        public GuardBuilder<TModel> Gte(TProp operand) => BuildLiteral(CompareOperator.Gte, operand);
        /// <summary>Compares the source with a typed literal using an ordered less-than check.</summary>
        /// <param name="operand">The literal value to compare with the source value.</param>
        /// <returns>A guard for composing or attaching the comparison.</returns>
        public GuardBuilder<TModel> Lt(TProp operand) => BuildLiteral(CompareOperator.Lt, operand);
        /// <summary>Compares the source with a typed literal using an ordered less-than-or-equal check.</summary>
        /// <param name="operand">The literal value to compare with the source value.</param>
        /// <returns>A guard for composing or attaching the comparison.</returns>
        public GuardBuilder<TModel> Lte(TProp operand) => BuildLiteral(CompareOperator.Lte, operand);

        /// <summary>Evaluates runtime truthiness: non-null, non-zero, and non-empty.</summary>
        /// <returns>A guard for composing or attaching the comparison.</returns>
        public GuardBuilder<TModel> Truthy() => BuildUnary(CompareOperator.Truthy);
        /// <summary>Evaluates runtime falsiness: null, zero, or empty.</summary>
        /// <returns>A guard for composing or attaching the comparison.</returns>
        public GuardBuilder<TModel> Falsy() => BuildUnary(CompareOperator.Falsy);
        /// <summary>Matches null source values.</summary>
        /// <returns>A guard for composing or attaching the comparison.</returns>
        public GuardBuilder<TModel> IsNull() => BuildUnary(CompareOperator.IsNull);
        /// <summary>Matches non-null source values.</summary>
        /// <returns>A guard for composing or attaching the comparison.</returns>
        public GuardBuilder<TModel> NotNull() => BuildUnary(CompareOperator.NotNull);
        /// <summary>Matches empty source values, including empty strings and empty collections.</summary>
        /// <returns>A guard for composing or attaching the comparison.</returns>
        public GuardBuilder<TModel> IsEmpty() => BuildUnary(CompareOperator.IsEmpty);
        /// <summary>Matches source values that are not empty.</summary>
        /// <returns>A guard for composing or attaching the comparison.</returns>
        public GuardBuilder<TModel> NotEmpty() => BuildUnary(CompareOperator.NotEmpty);

        /// <summary>Compares the source against a typed literal set.</summary>
        /// <param name="values">The literal values accepted by the comparison.</param>
        /// <returns>A guard for composing or attaching the comparison.</returns>
        public GuardBuilder<TModel> In(params TProp[] values) => BuildArray(CompareOperator.In, values);
        /// <summary>Compares the source against values outside a typed literal set.</summary>
        /// <param name="values">The literal values rejected by the comparison.</param>
        /// <returns>A guard for composing or attaching the comparison.</returns>
        public GuardBuilder<TModel> NotIn(params TProp[] values) => BuildArray(CompareOperator.NotIn, values);

        /// <summary>Compares the source against an inclusive typed range.</summary>
        /// <param name="low">The inclusive lower endpoint.</param>
        /// <param name="high">The inclusive upper endpoint.</param>
        /// <returns>A guard for composing or attaching the comparison.</returns>
        public GuardBuilder<TModel> Between(TProp low, TProp high) =>
            Build(CompareOperator.Between, RangeOperands(low, high));

        /// <summary>Compares the source as text and matches when it contains <paramref name="substring"/>.</summary>
        /// <param name="substring">The text that must appear in the source value.</param>
        /// <returns>A guard for composing or attaching the comparison.</returns>
        public GuardBuilder<TModel> Contains(string substring) =>
            BuildTextLiteral(CompareOperator.Contains, substring);
        /// <summary>Compares the source as text and matches when it starts with <paramref name="prefix"/>.</summary>
        /// <param name="prefix">The text that must appear at the start of the source value.</param>
        /// <returns>A guard for composing or attaching the comparison.</returns>
        public GuardBuilder<TModel> StartsWith(string prefix) =>
            BuildTextLiteral(CompareOperator.StartsWith, prefix);
        /// <summary>Compares the source as text and matches when it ends with <paramref name="suffix"/>.</summary>
        /// <param name="suffix">The text that must appear at the end of the source value.</param>
        /// <returns>A guard for composing or attaching the comparison.</returns>
        public GuardBuilder<TModel> EndsWith(string suffix) =>
            BuildTextLiteral(CompareOperator.EndsWith, suffix);
        /// <summary>Compares the source as text and matches it with the regular expression <paramref name="pattern"/>.</summary>
        /// <param name="pattern">The regular expression pattern matched against the source value.</param>
        /// <returns>A guard for composing or attaching the comparison.</returns>
        public GuardBuilder<TModel> Matches(string pattern) =>
            BuildTextLiteral(CompareOperator.Matches, pattern);
        /// <summary>Compares the source text length against the minimum <paramref name="length"/>.</summary>
        /// <param name="length">The minimum accepted text length.</param>
        /// <returns>A guard for composing or attaching the comparison.</returns>
        public GuardBuilder<TModel> MinLength(int length) =>
            Build(CompareOperator.MinLength, MinimumLengthOperands(length));

        /// <summary>Tests array membership using the source element shape for <paramref name="item"/>.</summary>
        /// <param name="item">The item to find in the array source.</param>
        /// <returns>A guard for composing or attaching the comparison.</returns>
        public GuardBuilder<TModel> ArrayContains(object item)
        {
            return Build(CompareOperator.ArrayContains, CollectionItemOperands(item));
        }

        /// <summary>Compares this source with another typed source using equality.</summary>
        /// <param name="right">The right-side source read at runtime.</param>
        /// <returns>A guard for composing or attaching the comparison.</returns>
        public GuardBuilder<TModel> Eq(TypedSource<TProp> right) => BuildVsSource(CompareOperator.Eq, right);
        /// <summary>Compares this source with another typed source using inequality.</summary>
        /// <param name="right">The right-side source read at runtime.</param>
        /// <returns>A guard for composing or attaching the comparison.</returns>
        public GuardBuilder<TModel> NotEq(TypedSource<TProp> right) => BuildVsSource(CompareOperator.Neq, right);
        /// <summary>Compares this source with another typed source using an ordered greater-than check.</summary>
        /// <param name="right">The right-side source read at runtime.</param>
        /// <returns>A guard for composing or attaching the comparison.</returns>
        public GuardBuilder<TModel> Gt(TypedSource<TProp> right) => BuildVsSource(CompareOperator.Gt, right);
        /// <summary>Compares this source with another typed source using an ordered greater-than-or-equal check.</summary>
        /// <param name="right">The right-side source read at runtime.</param>
        /// <returns>A guard for composing or attaching the comparison.</returns>
        public GuardBuilder<TModel> Gte(TypedSource<TProp> right) => BuildVsSource(CompareOperator.Gte, right);
        /// <summary>Compares this source with another typed source using an ordered less-than check.</summary>
        /// <param name="right">The right-side source read at runtime.</param>
        /// <returns>A guard for composing or attaching the comparison.</returns>
        public GuardBuilder<TModel> Lt(TypedSource<TProp> right) => BuildVsSource(CompareOperator.Lt, right);
        /// <summary>Compares this source with another typed source using an ordered less-than-or-equal check.</summary>
        /// <param name="right">The right-side source read at runtime.</param>
        /// <returns>A guard for composing or attaching the comparison.</returns>
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

        private ValueExpression LeftValue() => _leftSource.ToValueExpression();

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
                ValueExpression.LiteralRaw(item, _leftSource.ElementShape),
                _shape,
                _leftSource.ElementShape);

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
