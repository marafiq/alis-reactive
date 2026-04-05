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
        public GuardBuilder<TModel> Eq(TProp operand) => Build("eq", operand);
        public GuardBuilder<TModel> NotEq(TProp operand) => Build("neq", operand);
        public GuardBuilder<TModel> Gt(TProp operand) => Build("gt", operand);
        public GuardBuilder<TModel> Gte(TProp operand) => Build("gte", operand);
        public GuardBuilder<TModel> Lt(TProp operand) => Build("lt", operand);
        public GuardBuilder<TModel> Lte(TProp operand) => Build("lte", operand);

        // Presence operators
        public GuardBuilder<TModel> Truthy() => Build("truthy");
        public GuardBuilder<TModel> Falsy() => Build("falsy");
        public GuardBuilder<TModel> IsNull() => Build("is-null");
        public GuardBuilder<TModel> NotNull() => Build("not-null");
        public GuardBuilder<TModel> IsEmpty() => Build("is-empty");
        public GuardBuilder<TModel> NotEmpty() => Build("not-empty");

        // Membership
        public GuardBuilder<TModel> In(params TProp[] values) => Build("in", values);
        public GuardBuilder<TModel> NotIn(params TProp[] values) => Build("not-in", values);

        // Range
        public GuardBuilder<TModel> Between(TProp low, TProp high) =>
            Build("between", new object[] { low, high });

        // Text operators
        public GuardBuilder<TModel> Contains(string substring) => Build("contains", substring);
        public GuardBuilder<TModel> StartsWith(string prefix) => Build("starts-with", prefix);
        public GuardBuilder<TModel> EndsWith(string suffix) => Build("ends-with", suffix);
        public GuardBuilder<TModel> Matches(string pattern) => Build("matches", pattern);
        public GuardBuilder<TModel> MinLength(int length) => Build("min-length", length);

        // Array
        public GuardBuilder<TModel> ArrayContains(object item)
        {
            var left = _typedSource.ToValueProducer();
            var right = ValueProducer.LiteralRaw(item, _typedSource.ElementShape);
            var condition = Condition.Compare(left, "array-contains", right, _shape, _typedSource.ElementShape);
            return ComposeAndWrap(condition);
        }

        // Source-vs-source comparison
        public GuardBuilder<TModel> Eq(TypedSource<TProp> right) => BuildVsSource("eq", right);
        public GuardBuilder<TModel> NotEq(TypedSource<TProp> right) => BuildVsSource("neq", right);
        public GuardBuilder<TModel> Gt(TypedSource<TProp> right) => BuildVsSource("gt", right);
        public GuardBuilder<TModel> Gte(TypedSource<TProp> right) => BuildVsSource("gte", right);
        public GuardBuilder<TModel> Lt(TypedSource<TProp> right) => BuildVsSource("lt", right);
        public GuardBuilder<TModel> Lte(TypedSource<TProp> right) => BuildVsSource("lte", right);

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
