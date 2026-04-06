using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Builders.Conditions
{
    public sealed class GuardBuilder<TModel> where TModel : class
    {
        internal Condition Condition { get; }

        private readonly PipelineBuilder<TModel> _pipeline;
        private readonly BranchBuilder<TModel> _branchBuilder;

        internal GuardBuilder(Condition condition, PipelineBuilder<TModel> pipeline)
        {
            Condition = condition;
            _pipeline = pipeline;
        }

        internal GuardBuilder(Condition condition, BranchBuilder<TModel> branchBuilder)
        {
            Condition = condition;
            _branchBuilder = branchBuilder;
        }

        internal GuardBuilder(Condition condition)
        {
            Condition = condition;
        }

        public ConditionSourceBuilder<TModel, TProp> And<TPayload, TProp>(
            TPayload payload, Expression<Func<TPayload, TProp>> path)
        {
            var source = new EventArgSource<TPayload, TProp>(path);
            return new ConditionSourceBuilder<TModel, TProp>(
                source, CompositionMode.All, Condition, _pipeline, _branchBuilder);
        }

        public ConditionSourceBuilder<TModel, TProp> Or<TPayload, TProp>(
            TPayload payload, Expression<Func<TPayload, TProp>> path)
        {
            var source = new EventArgSource<TPayload, TProp>(path);
            return new ConditionSourceBuilder<TModel, TProp>(
                source, CompositionMode.Any, Condition, _pipeline, _branchBuilder);
        }

        public ConditionSourceBuilder<TModel, TProp> And<TProp>(TypedSource<TProp> source)
        {
            return new ConditionSourceBuilder<TModel, TProp>(
                source, CompositionMode.All, Condition, _pipeline, _branchBuilder);
        }

        public ConditionSourceBuilder<TModel, TProp> Or<TProp>(TypedSource<TProp> source)
        {
            return new ConditionSourceBuilder<TModel, TProp>(
                source, CompositionMode.Any, Condition, _pipeline, _branchBuilder);
        }

        public GuardBuilder<TModel> And(
            Func<ConditionStart<TModel>, GuardBuilder<TModel>> inner)
        {
            var innerResult = inner(new ConditionStart<TModel>());
            var terms = new List<Condition>();
            FlattenAll(Condition, terms);
            FlattenAll(innerResult.Condition, terms);
            return WrapCondition(PlanModel.Condition.All(terms.ToArray()));
        }

        public GuardBuilder<TModel> Or(
            Func<ConditionStart<TModel>, GuardBuilder<TModel>> inner)
        {
            var innerResult = inner(new ConditionStart<TModel>());
            var terms = new List<Condition>();
            FlattenAny(Condition, terms);
            FlattenAny(innerResult.Condition, terms);
            return WrapCondition(PlanModel.Condition.Any(terms.ToArray()));
        }

        public GuardBuilder<TModel> Not()
        {
            return WrapCondition(PlanModel.Condition.Not(Condition));
        }

        public BranchBuilder<TModel> Then(Action<PipelineBuilder<TModel>> pipeline)
        {
            var context = _pipeline?.Context ?? _branchBuilder?.Pipeline.Context;
            if (context == null)
                throw new InvalidOperationException(
                    "Then() requires a pipeline context. Use When() from a PipelineBuilder, not from a standalone ConditionStart.");

            var pb = new PipelineBuilder<TModel>(context);
            pipeline(pb);
            var reaction = pb.BuildReaction();
            var branchCase = BranchCase.Of(Condition, reaction);

            if (_branchBuilder != null)
            {
                _branchBuilder.AddBranch(branchCase);
                return _branchBuilder;
            }

            if (_pipeline == null)
                throw new InvalidOperationException(
                    "Then() requires a pipeline context.");

            var cases = new List<BranchCase> { branchCase };
            _pipeline.SetConditionalBranches(cases);
            _pipeline.SetConditionalMode();
            return new BranchBuilder<TModel>(_pipeline, cases);
        }

        internal GuardBuilder<TModel> WrapCondition(Condition combined)
        {
            if (_pipeline != null)
                return new GuardBuilder<TModel>(combined, _pipeline);
            if (_branchBuilder != null)
                return new GuardBuilder<TModel>(combined, _branchBuilder);
            return new GuardBuilder<TModel>(combined);
        }

        internal static void FlattenAll(Condition condition, List<Condition> target)
        {
            if (condition is AllCondition all) target.AddRange(all.Terms);
            else target.Add(condition);
        }

        internal static void FlattenAny(Condition condition, List<Condition> target)
        {
            if (condition is AnyCondition any) target.AddRange(any.Terms);
            else target.Add(condition);
        }
    }
}
