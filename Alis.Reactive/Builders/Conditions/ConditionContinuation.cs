using System;
using System.Collections.Generic;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Builders.Conditions
{
    internal abstract class ConditionComposition
    {
        private protected ConditionComposition() { }

        internal static ConditionComposition None { get; } =
            new EmptyConditionComposition();

        internal static ConditionComposition All(Condition existing) =>
            new AllConditionComposition(existing);

        internal static ConditionComposition Any(Condition existing) =>
            new AnyConditionComposition(existing);

        internal abstract Condition Compose(Condition incoming);

        internal static void FlattenAll(Condition condition, List<Condition> target)
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

        internal static void FlattenAny(Condition condition, List<Condition> target)
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
    }

    internal sealed class EmptyConditionComposition : ConditionComposition
    {
        internal override Condition Compose(Condition incoming)
        {
            if (incoming == null) throw new ArgumentNullException(nameof(incoming));
            return incoming;
        }
    }

    internal abstract class ExistingConditionComposition : ConditionComposition
    {
        private readonly Condition _existing;

        private protected ExistingConditionComposition(Condition existing)
        {
            _existing = existing ?? throw new ArgumentNullException(nameof(existing));
        }

        protected Condition Existing => _existing;
    }

    internal sealed class AllConditionComposition : ExistingConditionComposition
    {
        internal AllConditionComposition(Condition existing)
            : base(existing)
        {
        }

        internal override Condition Compose(Condition incoming)
        {
            if (incoming == null) throw new ArgumentNullException(nameof(incoming));

            var terms = new List<Condition>();
            FlattenAll(Existing, terms);
            terms.Add(incoming);
            return Condition.All(terms.ToArray());
        }
    }

    internal sealed class AnyConditionComposition : ExistingConditionComposition
    {
        internal AnyConditionComposition(Condition existing)
            : base(existing)
        {
        }

        internal override Condition Compose(Condition incoming)
        {
            if (incoming == null) throw new ArgumentNullException(nameof(incoming));

            var terms = new List<Condition>();
            FlattenAny(Existing, terms);
            terms.Add(incoming);
            return Condition.Any(terms.ToArray());
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

        internal abstract GuardBuilder<TModel> Wrap(Condition condition);

        internal abstract BranchBuilder<TModel> Then(
            Condition condition,
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

        internal override GuardBuilder<TModel> Wrap(Condition condition) =>
            new GuardBuilder<TModel>(condition, this);

        internal override BranchBuilder<TModel> Then(
            Condition condition,
            Action<PipelineBuilder<TModel>> pipeline)
        {
            var pb = new PipelineBuilder<TModel>(_pipeline.Context);
            pipeline(pb);
            var branchCase = BranchCase.Of(condition, pb.BuildReaction());
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

        internal override GuardBuilder<TModel> Wrap(Condition condition) =>
            new GuardBuilder<TModel>(condition, this);

        internal override BranchBuilder<TModel> Then(
            Condition condition,
            Action<PipelineBuilder<TModel>> pipeline)
        {
            var pb = new PipelineBuilder<TModel>(_branch.Pipeline.Context);
            pipeline(pb);
            _branch.AddBranch(BranchCase.Of(condition, pb.BuildReaction()));
            return _branch;
        }
    }

    internal sealed class StandaloneConditionContinuation<TModel> : ConditionContinuation<TModel>
        where TModel : class
    {
        internal override GuardBuilder<TModel> Wrap(Condition condition) =>
            new GuardBuilder<TModel>(condition, this);

        internal override BranchBuilder<TModel> Then(
            Condition condition,
            Action<PipelineBuilder<TModel>> pipeline)
        {
            throw new InvalidOperationException(
                "Then() requires a pipeline context. Use When() from a PipelineBuilder, not from a standalone ConditionStart.");
        }
    }
}
