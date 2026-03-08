using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Alis.Reactive.Descriptors.Commands;
using Alis.Reactive.Descriptors.Reactions;

namespace Alis.Reactive.Builders
{
    public class PipelineBuilder<TModel> where TModel : class
    {
        internal List<Command> Commands { get; } = new List<Command>();
        internal List<Branch>? ConditionalBranches { get; private set; }

        public PipelineBuilder<TModel> Dispatch(string eventName)
        {
            Commands.Add(new DispatchCommand(eventName));
            return this;
        }

        public PipelineBuilder<TModel> Dispatch<TPayload>(string eventName, TPayload payload)
        {
            Commands.Add(new DispatchCommand(eventName, payload));
            return this;
        }

        public ElementBuilder<TModel> Element(string elementId)
        {
            return new ElementBuilder<TModel>(this, elementId);
        }

        /// <summary>
        /// Starts a conditional branch on an event payload property.
        /// TProp is inferred from the expression — operators on the returned builder
        /// demand TProp operands (e.g. int property → Gte(int), string → Eq(string)).
        /// </summary>
        public ConditionSourceBuilder<TModel, TProp> When<TPayload, TProp>(
            TPayload payload,
            Expression<Func<TPayload, TProp>> path)
        {
            if (Commands.Count > 0)
                throw new InvalidOperationException(
                    "Cannot call When() after adding direct commands (Dispatch, Element). " +
                    "Use When().Then() to wrap all commands inside branches.");

            var source = ExpressionPathHelper.ToEventPath(path);
            return new ConditionSourceBuilder<TModel, TProp>(source, this);
        }

        /// <summary>
        /// Sets the branch list for conditional reactions.
        /// Called by GuardBuilder.Then() when creating the first branch.
        /// BranchBuilder continues to add branches to this same list.
        /// </summary>
        internal void SetConditionalBranches(List<Branch> branches)
        {
            ConditionalBranches = branches;
        }

        /// <summary>
        /// Builds the reaction for this pipeline.
        /// If conditional branches were set via When(), creates a ConditionalReaction
        /// with a defensive copy. Otherwise returns a SequentialReaction.
        /// </summary>
        internal Reaction BuildReaction()
        {
            if (ConditionalBranches != null)
            {
                if (Commands.Count > 0)
                    throw new InvalidOperationException(
                        "A pipeline cannot mix When() conditions with direct commands (Dispatch, Element). " +
                        "Use When().Then() to wrap all commands inside branches.");

                // Defensive copy — the ConditionalReaction is an immutable descriptor
                return new ConditionalReaction(ConditionalBranches.ToArray());
            }

            return new SequentialReaction(Commands);
        }
    }
}
