using System;
using System.Collections.Generic;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Builders.Requests
{
    /// <summary>
    /// Builds a parallel HTTP workflow — multiple requests fire concurrently.
    /// Each branch owns its own response chain. OnAllSettled fires after all branches complete.
    /// </summary>
    public class ParallelBuilder<TModel> where TModel : class
    {
        private readonly PlanAuthoringContext _authoring;
        private readonly WorkflowScope _scope;
        private readonly List<HttpRequestBuilder<TModel>> _branches = new List<HttpRequestBuilder<TModel>>();
        private List<PlanAction>? _onAllSettled;

        internal ParallelBuilder(PlanAuthoringContext authoring, WorkflowScope scope)
        {
            _authoring = authoring;
            _scope = scope;
        }

        internal void AddBranch(Action<HttpRequestBuilder<TModel>> request)
        {
            var builder = new HttpRequestBuilder<TModel>(_authoring, _scope);
            request(builder);
            _branches.Add(builder);
        }

        /// <summary>
        /// Commands to execute after all parallel requests complete, regardless of individual success or failure.
        /// </summary>
        /// <param name="pipeline">Builds the commands that run after all branches complete.</param>
        public ParallelBuilder<TModel> OnAllSettled(Action<PipelineBuilder<TModel>> pipeline)
        {
            var pb = new PipelineBuilder<TModel>(_authoring, _scope);
            pipeline(pb);
            var actions = pb.BuildActions();
            if (actions.Count != 1 || ContainsStructuredAction(actions[0]))
                throw new InvalidOperationException(
                    "OnAllSettled only supports plain commands (sequential). " +
                    "Conditions, HTTP, and parallel pipelines are not valid here.");

            _onAllSettled = FlattenSequential(actions[0]);
            return this;
        }

        internal ParallelAction BuildAction()
        {
            var steps = new List<PlanAction>();
            foreach (var branch in _branches)
                steps.Add(new RequestAction(branch.BuildRequestPlan()));

            var action = new ParallelAction(steps);
            if (_onAllSettled != null && _onAllSettled.Count > 0)
                action.OnSettled = PlanAuthoringContext.SequenceOrSingle(_onAllSettled);

            return action;
        }

        private static bool ContainsStructuredAction(PlanAction action)
        {
            if (action is SequenceAction sequence)
            {
                foreach (var step in sequence.Steps)
                    if (ContainsStructuredAction(step))
                        return true;

                return false;
            }

            return action is BranchAction || action is RequestAction || action is ParallelAction;
        }

        private static List<PlanAction> FlattenSequential(PlanAction action)
        {
            if (action is SequenceAction sequence)
                return sequence.Steps;

            return new List<PlanAction> { action };
        }
    }
}
