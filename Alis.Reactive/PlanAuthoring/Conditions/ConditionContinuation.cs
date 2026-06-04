using System;
using System.Collections.Generic;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Builders.Conditions
{
    internal static class ConditionComposition
    {
        internal static ConditionGraph None(ConditionGraph incoming) => incoming;

        internal static Func<ConditionGraph, ConditionGraph> All(ConditionGraph existing) =>
            incoming => ComposeAll(existing, incoming);

        internal static Func<ConditionGraph, ConditionGraph> Any(ConditionGraph existing) =>
            incoming => ComposeAny(existing, incoming);

        internal static void FlattenAll(ConditionGraph condition, List<ConditionGraph> target)
        {
            var conditionAlreadyRepresentsAllTerms = condition is AllCondition;
            if (conditionAlreadyRepresentsAllTerms)
            {
                var allTermsCondition = (AllCondition)condition;
                target.AddRange(allTermsCondition.Terms);
                return;
            }

            target.Add(condition);
        }

        internal static void FlattenAny(ConditionGraph condition, List<ConditionGraph> target)
        {
            var conditionAlreadyRepresentsAnyTerms = condition is AnyCondition;
            if (conditionAlreadyRepresentsAnyTerms)
            {
                var anyTermsCondition = (AnyCondition)condition;
                target.AddRange(anyTermsCondition.Terms);
                return;
            }

            target.Add(condition);
        }

        private static ConditionGraph ComposeAll(ConditionGraph existing, ConditionGraph incoming)
        {
            var terms = new List<ConditionGraph>();
            FlattenAll(existing, terms);
            terms.Add(incoming);
            return ConditionGraph.All(terms.ToArray());
        }

        private static ConditionGraph ComposeAny(ConditionGraph existing, ConditionGraph incoming)
        {
            var terms = new List<ConditionGraph>();
            FlattenAny(existing, terms);
            terms.Add(incoming);
            return ConditionGraph.Any(terms.ToArray());
        }
    }

    internal abstract class ConditionContinuation<TModel> where TModel : class
    {
        internal static ConditionContinuation<TModel> Standalone { get; } =
            new StandaloneConditionContinuation<TModel>();

        internal static ConditionContinuation<TModel> ForPipeline(PipelineBuilder<TModel> pipeline) =>
            new PipelineConditionContinuation<TModel>(pipeline);

        internal static ConditionContinuation<TModel> ForBranch(BranchBuilder<TModel> branch) =>
            new BranchConditionContinuation<TModel>(branch);

        internal abstract GuardBuilder<TModel> Wrap(ConditionGraph condition);

        internal abstract BranchBuilder<TModel> Then(
            ConditionGraph condition,
            Action<PipelineBuilder<TModel>> pipeline);
    }

    internal sealed class PipelineConditionContinuation<TModel> : ConditionContinuation<TModel>
        where TModel : class
    {
        private readonly PipelineBuilder<TModel> _pipeline;

        internal PipelineConditionContinuation(PipelineBuilder<TModel> pipeline)
        {
            _pipeline = pipeline ?? throw new ArgumentNullException(nameof(pipeline));
        }

        internal override GuardBuilder<TModel> Wrap(ConditionGraph condition) =>
            new GuardBuilder<TModel>(condition, this);

        internal override BranchBuilder<TModel> Then(
            ConditionGraph condition,
            Action<PipelineBuilder<TModel>> pipeline)
        {
            var reactionPipeline = new PipelineBuilder<TModel>(_pipeline.Context);
            pipeline(reactionPipeline);
            var branchCase = BranchCase.Of(condition, reactionPipeline.BuildReaction());
            var cases = new List<BranchCase> { branchCase };
            _pipeline.SetConditionalBranches(cases);
            return new BranchBuilder<TModel>(_pipeline, cases);
        }
    }

    internal sealed class BranchConditionContinuation<TModel> : ConditionContinuation<TModel>
        where TModel : class
    {
        private readonly BranchBuilder<TModel> _branch;

        internal BranchConditionContinuation(BranchBuilder<TModel> branch)
        {
            _branch = branch ?? throw new ArgumentNullException(nameof(branch));
        }

        internal override GuardBuilder<TModel> Wrap(ConditionGraph condition) =>
            new GuardBuilder<TModel>(condition, this);

        internal override BranchBuilder<TModel> Then(
            ConditionGraph condition,
            Action<PipelineBuilder<TModel>> pipeline)
        {
            var reactionPipeline = new PipelineBuilder<TModel>(_branch.Pipeline.Context);
            pipeline(reactionPipeline);
            _branch.AddBranch(BranchCase.Of(condition, reactionPipeline.BuildReaction()));
            return _branch;
        }
    }

    internal sealed class StandaloneConditionContinuation<TModel> : ConditionContinuation<TModel>
        where TModel : class
    {
        internal override GuardBuilder<TModel> Wrap(ConditionGraph condition) =>
            new GuardBuilder<TModel>(condition, this);

        internal override BranchBuilder<TModel> Then(
            ConditionGraph condition,
            Action<PipelineBuilder<TModel>> pipeline)
        {
            throw new InvalidOperationException(
                "Then() requires a pipeline context. Use When() from a PipelineBuilder, not from a standalone ConditionStart.");
        }
    }
}
