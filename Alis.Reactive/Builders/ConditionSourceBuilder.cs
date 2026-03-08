using System;
using Alis.Reactive.Descriptors.Guards;

namespace Alis.Reactive.Builders
{
    /// <summary>
    /// Provides operator methods (Eq, NotEq, Gt, etc.) that produce a ValueGuard.
    /// Each operator method returns a GuardBuilder for composition (And/Or/Then).
    /// </summary>
    public sealed class ConditionSourceBuilder<TModel> where TModel : class
    {
        private readonly string _source;
        private readonly string _coerceAs;

        // Back-references — only one of these is set depending on the entry path.
        private readonly PipelineBuilder<TModel>? _pipeline;
        private readonly BranchBuilder<TModel>? _branchBuilder;

        /// <summary>
        /// Constructor for top-level: PipelineBuilder.When()
        /// </summary>
        internal ConditionSourceBuilder(string source, Type propertyType,
            PipelineBuilder<TModel> pipeline)
        {
            _source = source;
            _coerceAs = CoercionTypes.InferFromType(propertyType);
            _pipeline = pipeline;
        }

        /// <summary>
        /// Constructor for inner guards: ConditionStart.When() inside And/Or lambdas.
        /// No pipeline or branch reference needed — just builds a guard.
        /// </summary>
        internal ConditionSourceBuilder(string source, Type propertyType)
        {
            _source = source;
            _coerceAs = CoercionTypes.InferFromType(propertyType);
        }

        /// <summary>
        /// Constructor for chained branches: BranchBuilder.ElseIf()
        /// </summary>
        internal ConditionSourceBuilder(string source, Type propertyType,
            BranchBuilder<TModel> branchBuilder)
        {
            _source = source;
            _coerceAs = CoercionTypes.InferFromType(propertyType);
            _branchBuilder = branchBuilder;
        }

        // --- Operators ---

        public GuardBuilder<TModel> Eq(object operand) =>
            Build(GuardOp.Eq, operand);

        public GuardBuilder<TModel> NotEq(object operand) =>
            Build(GuardOp.Neq, operand);

        public GuardBuilder<TModel> Gt(object operand) =>
            Build(GuardOp.Gt, operand);

        public GuardBuilder<TModel> Gte(object operand) =>
            Build(GuardOp.Gte, operand);

        public GuardBuilder<TModel> Lt(object operand) =>
            Build(GuardOp.Lt, operand);

        public GuardBuilder<TModel> Lte(object operand) =>
            Build(GuardOp.Lte, operand);

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
