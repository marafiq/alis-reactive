using System;
using System.Collections.Generic;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Builders.Conditions
{
    internal static class ConditionComposition
    {
        // a && b && c is one all-of node; a nested any-of stays a node of its own.
        internal static ConditionGraph All(ConditionGraph existing, ConditionGraph incoming)
        {
            var terms = new List<ConditionGraph>();
            FlattenAll(existing, terms);
            FlattenAll(incoming, terms);
            return ConditionGraph.All(terms.ToArray());
        }

        internal static ConditionGraph Any(ConditionGraph existing, ConditionGraph incoming)
        {
            var terms = new List<ConditionGraph>();
            FlattenAny(existing, terms);
            FlattenAny(incoming, terms);
            return ConditionGraph.Any(terms.ToArray());
        }

        internal static ConditionGraph Group<TModel>(Func<ConditionStart<TModel>, Condition<TModel>> group)
            where TModel : class
        {
            if (group == null) throw new ArgumentNullException(nameof(group));
            return group(new ConditionStart<TModel>()).Graph;
        }

        private static void FlattenAll(ConditionGraph condition, List<ConditionGraph> terms)
        {
            if (condition is AllCondition all) terms.AddRange(all.Terms);
            else terms.Add(condition);
        }

        private static void FlattenAny(ConditionGraph condition, List<ConditionGraph> terms)
        {
            if (condition is AnyCondition any) terms.AddRange(any.Terms);
            else terms.Add(condition);
        }
    }

    // Where Then puts its branch: the pipeline that started the condition, or the branch an ElseIf continues.
    internal abstract class ConditionContinuation<TModel> where TModel : class
    {
        internal static ConditionContinuation<TModel> ForPipeline(PipelineBuilder<TModel> pipeline) =>
            new PipelineConditionContinuation<TModel>(pipeline);

        internal static ConditionContinuation<TModel> ForBranch(BranchBuilder<TModel> branch) =>
            new BranchConditionContinuation<TModel>(branch);

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

        internal override BranchBuilder<TModel> Then(
            ConditionGraph condition,
            Action<PipelineBuilder<TModel>> pipeline)
        {
            var branchCase = BranchCase.Of(condition, PipelineBuilder<TModel>.Build(_pipeline.Context, pipeline));
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

        internal override BranchBuilder<TModel> Then(
            ConditionGraph condition,
            Action<PipelineBuilder<TModel>> pipeline)
        {
            _branch.AddBranch(BranchCase.Of(condition, PipelineBuilder<TModel>.Build(_branch.Pipeline.Context, pipeline)));
            return _branch;
        }
    }
}
