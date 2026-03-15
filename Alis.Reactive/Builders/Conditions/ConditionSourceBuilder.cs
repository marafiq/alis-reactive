using System;
using Alis.Reactive.Descriptors.Guards;
using Alis.Reactive.Descriptors.Sources;

namespace Alis.Reactive.Builders.Conditions
{
    internal enum CompositionMode { None, All, Any }

    /// <summary>
    /// Provides operator methods (Eq, NotEq, Gt, etc.) that produce a ValueGuard.
    /// TProp is the compile-time type of the source property — operators demand TProp operands.
    /// </summary>
    public sealed class ConditionSourceBuilder<TModel, TProp> where TModel : class
    {
        private readonly TypedSource<TProp> _typedSource;
        private readonly string _coerceAs;

        // Composition state for direct And/Or chaining
        private readonly CompositionMode _mode;
        private readonly Guard? _existingGuard;

        // Back-references — only one of these is set depending on the entry path.
        private readonly PipelineBuilder<TModel>? _pipeline;
        private readonly BranchBuilder<TModel>? _branchBuilder;

        internal ConditionSourceBuilder(TypedSource<TProp> source, PipelineBuilder<TModel> pipeline)
        {
            _typedSource = source;
            _coerceAs = source.CoercionType;
            _pipeline = pipeline;
            _mode = CompositionMode.None;
        }

        internal ConditionSourceBuilder(TypedSource<TProp> source)
        {
            _typedSource = source;
            _coerceAs = source.CoercionType;
            _mode = CompositionMode.None;
        }

        internal ConditionSourceBuilder(TypedSource<TProp> source, BranchBuilder<TModel> branchBuilder)
        {
            _typedSource = source;
            _coerceAs = source.CoercionType;
            _branchBuilder = branchBuilder;
            _mode = CompositionMode.None;
        }

        internal ConditionSourceBuilder(TypedSource<TProp> source, CompositionMode mode,
            Guard existingGuard, PipelineBuilder<TModel>? pipeline, BranchBuilder<TModel>? branchBuilder)
        {
            _typedSource = source;
            _coerceAs = source.CoercionType;
            _mode = mode;
            _existingGuard = existingGuard;
            _pipeline = pipeline;
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

        public GuardBuilder<TModel> IsEmpty() =>
            Build(GuardOp.IsEmpty);

        public GuardBuilder<TModel> NotEmpty() =>
            Build(GuardOp.NotEmpty);

        // --- Membership operators ---

        public GuardBuilder<TModel> In(params TProp[] values) =>
            Build(GuardOp.In, values);

        public GuardBuilder<TModel> NotIn(params TProp[] values) =>
            Build(GuardOp.NotIn, values);

        // --- Range ---

        public GuardBuilder<TModel> Between(TProp low, TProp high) =>
            Build(GuardOp.Between, new object?[] { low, high });

        // --- Text operators ---

        public GuardBuilder<TModel> Contains(string substring) =>
            Build(GuardOp.Contains, substring);

        public GuardBuilder<TModel> StartsWith(string prefix) =>
            Build(GuardOp.StartsWith, prefix);

        public GuardBuilder<TModel> EndsWith(string suffix) =>
            Build(GuardOp.EndsWith, suffix);

        public GuardBuilder<TModel> Matches(string pattern) =>
            Build(GuardOp.Matches, pattern);

        public GuardBuilder<TModel> MinLength(int length) =>
            Build(GuardOp.MinLength, length);

        // --- Internal ---

        private GuardBuilder<TModel> Build(string op, object? operand = null)
        {
            var bindSource = _typedSource.ToBindSource();
            var guard = new ValueGuard(bindSource, _coerceAs, op, operand);

            // If in composition mode, combine with existing guard
            if (_mode != CompositionMode.None && _existingGuard != null)
            {
                Guard combined;
                if (_mode == CompositionMode.All)
                {
                    var guards = new System.Collections.Generic.List<Guard>();
                    GuardBuilder<TModel>.FlattenAllStatic(_existingGuard, guards);
                    guards.Add(guard);
                    combined = new AllGuard(guards);
                }
                else // Any
                {
                    var guards = new System.Collections.Generic.List<Guard>();
                    GuardBuilder<TModel>.FlattenAnyStatic(_existingGuard, guards);
                    guards.Add(guard);
                    combined = new AnyGuard(guards);
                }
                return WrapGuard(combined);
            }

            return WrapGuard(guard);
        }

        private GuardBuilder<TModel> WrapGuard(Guard guard)
        {
            if (_pipeline != null)
                return new GuardBuilder<TModel>(guard, _pipeline);

            if (_branchBuilder != null)
                return new GuardBuilder<TModel>(guard, _branchBuilder);

            // Inner guard (no pipeline or branch) — used in And/Or lambdas
            return new GuardBuilder<TModel>(guard);
        }
    }
}
