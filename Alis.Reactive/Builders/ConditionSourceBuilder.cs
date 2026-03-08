using System;
using Alis.Reactive.Descriptors.Guards;

namespace Alis.Reactive.Builders
{
    /// <summary>
    /// Provides operator methods (Eq, NotEq, Gt, etc.) that produce a ValueGuard.
    /// TProp is the compile-time type of the source property — operators demand TProp operands.
    /// </summary>
    public sealed class ConditionSourceBuilder<TModel, TProp> where TModel : class
    {
        private readonly string _source;
        private readonly string _coerceAs;

        // Back-references — only one of these is set depending on the entry path.
        private readonly PipelineBuilder<TModel>? _pipeline;
        private readonly BranchBuilder<TModel>? _branchBuilder;

        /// <summary>
        /// Constructor for top-level: PipelineBuilder.When()
        /// </summary>
        internal ConditionSourceBuilder(string source, PipelineBuilder<TModel> pipeline)
        {
            _source = source;
            _coerceAs = CoercionTypes.InferFromType(typeof(TProp));
            _pipeline = pipeline;
        }

        /// <summary>
        /// Constructor for inner guards: ConditionStart.When() inside And/Or lambdas.
        /// No pipeline or branch reference needed — just builds a guard.
        /// </summary>
        internal ConditionSourceBuilder(string source)
        {
            _source = source;
            _coerceAs = CoercionTypes.InferFromType(typeof(TProp));
        }

        /// <summary>
        /// Constructor for chained branches: BranchBuilder.ElseIf()
        /// </summary>
        internal ConditionSourceBuilder(string source, BranchBuilder<TModel> branchBuilder)
        {
            _source = source;
            _coerceAs = CoercionTypes.InferFromType(typeof(TProp));
            _branchBuilder = branchBuilder;
        }

        // --- Comparison operators (typed operand) ---

        public GuardBuilder<TModel> Eq(TProp operand) =>
            Build(GuardOp.Eq, operand);

        public GuardBuilder<TModel> NotEq(TProp operand) =>
            Build(GuardOp.Neq, operand);

        public GuardBuilder<TModel> Gt(TProp operand) =>
            Build(GuardOp.Gt, operand);

        public GuardBuilder<TModel> Gte(TProp operand) =>
            Build(GuardOp.Gte, operand);

        public GuardBuilder<TModel> Lt(TProp operand) =>
            Build(GuardOp.Lt, operand);

        public GuardBuilder<TModel> Lte(TProp operand) =>
            Build(GuardOp.Lte, operand);

        // --- Presence operators (no operand) ---

        public GuardBuilder<TModel> Truthy() =>
            Build(GuardOp.Truthy);

        public GuardBuilder<TModel> Falsy() =>
            Build(GuardOp.Falsy);

        public GuardBuilder<TModel> IsNull() =>
            Build(GuardOp.IsNull);

        public GuardBuilder<TModel> NotNull() =>
            Build(GuardOp.NotNull);

        // --- Internal ---

        private GuardBuilder<TModel> Build(string op, object? operand = null)
        {
            var guard = new ValueGuard(_source, _coerceAs, op, operand);

            if (_pipeline != null)
                return new GuardBuilder<TModel>(guard, _pipeline);

            if (_branchBuilder != null)
                return new GuardBuilder<TModel>(guard, _branchBuilder);

            // Inner guard (no pipeline or branch) — used in And/Or lambdas
            return new GuardBuilder<TModel>(guard);
        }
    }
}
