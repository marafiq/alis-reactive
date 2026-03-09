using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Alis.Reactive.Builders.Conditions;
using Alis.Reactive.Descriptors.Commands;
using Alis.Reactive.Descriptors.Guards;
using Alis.Reactive.Descriptors.Reactions;

namespace Alis.Reactive.Builders
{
    public class PipelineBuilder<TModel> where TModel : class
    {
        internal List<Command> Commands { get; } = new List<Command>();
        internal List<Branch>? ConditionalBranches { get; private set; }

        /// <summary>
        /// Adds a command to the pipeline. Used by vendor-specific projects
        /// (Fusion, Native) to emit their own command descriptors.
        /// </summary>
        public void AddCommand(Command command)
        {
            Commands.Add(command);
        }

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

        // ── Component<T>() — 3 overloads ──

        /// <summary>
        /// Resolve component by model expression (input components bound to model).
        /// Target ID uses underscore separator matching Html.IdFor() convention:
        /// m => m.Address.City → target "Address_City".
        /// </summary>
        public ComponentRef<TComponent, TModel> Component<TComponent>(
            Expression<Func<TModel, object?>> expr)
            where TComponent : IComponent
        {
            var elementId = ExpressionPathHelper.ToElementId(expr);
            return new ComponentRef<TComponent, TModel>(elementId, this);
        }

        /// <summary>Resolve component by string ref (non-input components by ID).</summary>
        public ComponentRef<TComponent, TModel> Component<TComponent>(string refId)
            where TComponent : IComponent
        {
            return new ComponentRef<TComponent, TModel>(refId, this);
        }

        /// <summary>Resolve app-level component by its default ID (e.g., FusionConfirm).</summary>
        public ComponentRef<TComponent, TModel> Component<TComponent>()
            where TComponent : IAppLevelComponent, new()
        {
            var comp = new TComponent();
            return new ComponentRef<TComponent, TModel>(comp.DefaultId, this);
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

            var source = new EventArgSource<TPayload, TProp>(path);
            return new ConditionSourceBuilder<TModel, TProp>(source, this);
        }

        /// <summary>
        /// Starts a Confirm guard — an async halting condition that pauses the pipeline
        /// and shows a dialog to the user.
        /// </summary>
        public GuardBuilder<TModel> Confirm(string message)
        {
            if (Commands.Count > 0)
                throw new InvalidOperationException(
                    "Cannot call Confirm() after adding direct commands (Dispatch, Element). " +
                    "Use Confirm().Then() to wrap all commands inside branches.");

            return new GuardBuilder<TModel>(new ConfirmGuard(message), this);
        }

        internal void SetConditionalBranches(List<Branch> branches)
        {
            ConditionalBranches = branches;
        }

        public Reaction BuildReaction()
        {
            if (ConditionalBranches != null)
                return new ConditionalReaction(ConditionalBranches.ToArray());

            return new SequentialReaction(Commands);
        }
    }
}
