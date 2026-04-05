using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Builders.Conditions
{
    internal enum CompositionMode { None, All, Any }

    public sealed class ConditionSourceBuilder<TModel, TProp> where TModel : class
    {
        private readonly TypedSource<TProp> _typedSource;
        private readonly Shape _shape;

        private readonly CompositionMode _mode;
        private readonly Condition _existingCondition;

        private readonly PipelineBuilder<TModel> _pipeline;
        private readonly BranchBuilder<TModel> _branchBuilder;

        internal ConditionSourceBuilder(TypedSource<TProp> source, PipelineBuilder<TModel> pipeline)
        {
            _typedSource = source;
            _shape = source.Shape;
            _pipeline = pipeline;
            _mode = CompositionMode.None;
        }

        internal ConditionSourceBuilder(TypedSource<TProp> source)
        {
            _typedSource = source;
            _shape = source.Shape;
            _mode = CompositionMode.None;
        }

        internal ConditionSourceBuilder(TypedSource<TProp> source, BranchBuilder<TModel> branchBuilder)
        {
            _typedSource = source;
            _shape = source.Shape;
            _branchBuilder = branchBuilder;
            _mode = CompositionMode.None;
        }

        internal ConditionSourceBuilder(TypedSource<TProp> source, CompositionMode mode,
            Condition existingCondition, PipelineBuilder<TModel> pipeline, BranchBuilder<TModel> branchBuilder)
        {
            _typedSource = source;
            _shape = source.Shape;
            _mode = mode;
            _existingCondition = existingCondition;
            _pipeline = pipeline;
            _branchBuilder = branchBuilder;
        }

        // Comparison operators (typed operand)
        public GuardBuilder<TModel> Eq(TProp operand) => Build(CompareOp.Eq, operand);
        public GuardBuilder<TModel> NotEq(TProp operand) => Build(CompareOp.Neq, operand);
        public GuardBuilder<TModel> Gt(TProp operand) => Build(CompareOp.Gt, operand);
        public GuardBuilder<TModel> Gte(TProp operand) => Build(CompareOp.Gte, operand);
        public GuardBuilder<TModel> Lt(TProp operand) => Build(CompareOp.Lt, operand);
        public GuardBuilder<TModel> Lte(TProp operand) => Build(CompareOp.Lte, operand);

        // Presence operators
        public GuardBuilder<TModel> Truthy() => Build(CompareOp.Truthy);
        public GuardBuilder<TModel> Falsy() => Build(CompareOp.Falsy);
        public GuardBuilder<TModel> IsNull() => Build(CompareOp.IsNull);
        public GuardBuilder<TModel> NotNull() => Build(CompareOp.NotNull);
        public GuardBuilder<TModel> IsEmpty() => Build(CompareOp.IsEmpty);
        public GuardBuilder<TModel> NotEmpty() => Build(CompareOp.NotEmpty);

        // Membership
        public GuardBuilder<TModel> In(params TProp[] values) => BuildArray(CompareOp.In, values);
        public GuardBuilder<TModel> NotIn(params TProp[] values) => BuildArray(CompareOp.NotIn, values);

        // Range
        public GuardBuilder<TModel> Between(TProp low, TProp high) =>
            BuildArray(CompareOp.Between, new object[] { low, high });

        // Text operators
        public GuardBuilder<TModel> Contains(string substring) => Build(CompareOp.Contains, substring);
        public GuardBuilder<TModel> StartsWith(string prefix) => Build(CompareOp.StartsWith, prefix);
        public GuardBuilder<TModel> EndsWith(string suffix) => Build(CompareOp.EndsWith, suffix);
        public GuardBuilder<TModel> Matches(string pattern) => Build(CompareOp.Matches, pattern);
        public GuardBuilder<TModel> MinLength(int length) => Build(CompareOp.MinLength, length);

        // Array
        public GuardBuilder<TModel> ArrayContains(object item)
        {
            var left = _typedSource.ToValueProducer();
            var right = ValueProducer.LiteralRaw(item, _typedSource.ElementShape);
            var condition = Condition.Compare(left, CompareOp.ArrayContains, right, _shape, _typedSource.ElementShape);
            return ComposeAndWrap(condition);
        }

        // Source-vs-source comparison
        public GuardBuilder<TModel> Eq(TypedSource<TProp> right) => BuildVsSource(CompareOp.Eq, right);
        public GuardBuilder<TModel> NotEq(TypedSource<TProp> right) => BuildVsSource(CompareOp.Neq, right);
        public GuardBuilder<TModel> Gt(TypedSource<TProp> right) => BuildVsSource(CompareOp.Gt, right);
        public GuardBuilder<TModel> Gte(TypedSource<TProp> right) => BuildVsSource(CompareOp.Gte, right);
        public GuardBuilder<TModel> Lt(TypedSource<TProp> right) => BuildVsSource(CompareOp.Lt, right);
        public GuardBuilder<TModel> Lte(TypedSource<TProp> right) => BuildVsSource(CompareOp.Lte, right);

        private GuardBuilder<TModel> BuildVsSource(string op, TypedSource<TProp> right)
        {
            var leftProducer = _typedSource.ToValueProducer();
            var rightProducer = right.ToValueProducer();
            var condition = Condition.Compare(leftProducer, op, rightProducer, _shape);
            return ComposeAndWrap(condition);
        }

        private GuardBuilder<TModel> Build(string op, object operand = null)
        {
            var leftProducer = _typedSource.ToValueProducer();
            var rightProducer = operand != null ? ValueProducer.LiteralRaw(operand, _shape) : null;
            var condition = Condition.Compare(leftProducer, op, rightProducer, _shape);
            return ComposeAndWrap(condition);
        }

        /// <summary>
        /// Builds a condition where the right-hand side is an array of values.
        /// Used by In, NotIn, Between — these operators require array operands,
        /// not scalar literals.
        /// </summary>
        private GuardBuilder<TModel> BuildArray(string op, System.Array values)
        {
            var leftProducer = _typedSource.ToValueProducer();
            var items = new System.Collections.Generic.List<ValueProducer>();
            foreach (var item in values)
                items.Add(ValueProducer.LiteralRaw(item, _shape));
            var rightProducer = ValueProducer.Array(items, _shape);
            var condition = Condition.Compare(leftProducer, op, rightProducer, _shape);
            return ComposeAndWrap(condition);
        }


        private GuardBuilder<TModel> ComposeAndWrap(Condition newCondition)
        {
            if (_mode == CompositionMode.None || _existingCondition == null)
                return WrapCondition(newCondition);

            var terms = new System.Collections.Generic.List<Condition>();
            if (_mode == CompositionMode.All)
                GuardBuilder<TModel>.FlattenAll(_existingCondition, terms);
            else
                GuardBuilder<TModel>.FlattenAny(_existingCondition, terms);

            terms.Add(newCondition);
            Condition combined = _mode == CompositionMode.All
                ? Condition.All(terms.ToArray())
                : Condition.Any(terms.ToArray());

            return WrapCondition(combined);
        }

        private GuardBuilder<TModel> WrapCondition(Condition condition)
        {
            if (_pipeline != null)
                return new GuardBuilder<TModel>(condition, _pipeline);
            if (_branchBuilder != null)
                return new GuardBuilder<TModel>(condition, _branchBuilder);
            return new GuardBuilder<TModel>(condition);
        }
    }
}
