using System;
using System.Collections.Generic;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Builders.Conditions
{
    internal static class ConditionComposition
    {
        // a && b && c is one all-of node; a nested any-of stays a node of its own.
        internal static Condition All(Condition existing, Condition incoming)
        {
            var terms = new List<Condition>();
            FlattenAll(existing, terms);
            FlattenAll(incoming, terms);
            return Condition.All(terms.ToArray());
        }

        internal static Condition Any(Condition existing, Condition incoming)
        {
            var terms = new List<Condition>();
            FlattenAny(existing, terms);
            FlattenAny(incoming, terms);
            return Condition.Any(terms.ToArray());
        }

        internal static Condition Group<TModel>(Func<ConditionStart<TModel>, Condition<TModel>> group)
            where TModel : class
        {
            if (group == null) throw new ArgumentNullException(nameof(group));
            return group(new ConditionStart<TModel>()).Graph;
        }

        private static void FlattenAll(Condition condition, List<Condition> terms)
        {
            if (condition is AllCondition all) terms.AddRange(all.Terms);
            else terms.Add(condition);
        }

        private static void FlattenAny(Condition condition, List<Condition> terms)
        {
            if (condition is AnyCondition any) terms.AddRange(any.Terms);
            else terms.Add(condition);
        }
    }

    // Where Then puts its branch: in the callback the condition was started in, where Then is written, or
    // as the next arm of the branch an ElseIf continues.
    internal abstract class ConditionContinuation<TModel> where TModel : class
    {
        internal static ConditionContinuation<TModel> InCallback(PlanBuildContext context) =>
            new CallbackConditionContinuation<TModel>(context, context.ActiveBlock);

        internal static ConditionContinuation<TModel> ForBranch(BranchBuilder<TModel> branch) =>
            new BranchConditionContinuation<TModel>(branch);

        internal abstract BranchBuilder<TModel> Then(
            Condition condition,
            Action<PipelineBuilder<TModel>> pipeline);
    }

    internal sealed class CallbackConditionContinuation<TModel> : ConditionContinuation<TModel>
        where TModel : class
    {
        private readonly PlanBuildContext _context;
        private readonly Block _callback;

        internal CallbackConditionContinuation(PlanBuildContext context, Block callback)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _callback = callback ?? throw new ArgumentNullException(nameof(callback));
        }

        internal override BranchBuilder<TModel> Then(
            Condition condition,
            Action<PipelineBuilder<TModel>> pipeline)
        {
            var branch = new BranchStep(condition, PipelineBuilder<TModel>.Build(_context, pipeline));
            _callback.Add(branch);
            return new BranchBuilder<TModel>(_context, branch);
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
            Condition condition,
            Action<PipelineBuilder<TModel>> pipeline)
        {
            _branch.AddArm(condition, pipeline);
            return _branch;
        }
    }
}
