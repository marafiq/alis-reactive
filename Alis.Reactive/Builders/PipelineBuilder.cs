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
        internal ConditionalReaction? Conditional { get; private set; }

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
        /// The payload instance is used only for type inference; its value is ignored.
        /// </summary>
        public ConditionSourceBuilder<TModel> When<TPayload>(
            TPayload payload,
            Expression<Func<TPayload, object?>> path)
        {
            var source = ExpressionPathHelper.ToEventPath(path);
            var propertyType = ExpressionPathHelper.GetPropertyType(path);
            return new ConditionSourceBuilder<TModel>(source, propertyType, this);
        }

        /// <summary>
        /// Sets the conditional reaction on this pipeline.
        /// Called by GuardBuilder.Then() when creating the first branch.
        /// </summary>
        internal void SetConditional(ConditionalReaction conditional)
        {
            Conditional = conditional;
        }

        /// <summary>
        /// Builds the reaction for this pipeline.
        /// If a conditional was set via When(), returns the ConditionalReaction.
        /// Otherwise returns a SequentialReaction from the accumulated commands.
        /// </summary>
        internal Reaction BuildReaction()
        {
            if (Conditional != null)
            {
                if (Commands.Count > 0)
                    throw new InvalidOperationException(
                        "A pipeline cannot mix When() conditions with direct commands (Dispatch, Element). " +
                        "Use When().Then() to wrap all commands inside branches.");

                return Conditional;
            }

            return new SequentialReaction(Commands);
        }
    }
}
