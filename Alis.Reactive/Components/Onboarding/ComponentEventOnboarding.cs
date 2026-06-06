using System;
using Alis.Reactive.Builders;

namespace Alis.Reactive
{
    /// <summary>
    /// Shared component event onboarding path for vertical slices.
    /// Vertical slice selects a typed event and a rendered component id; this helper builds the
    /// reaction pipeline and wires the event against the plan-registered component.
    /// </summary>
    internal static class ComponentEventOnboarding
    {
        internal static void Wire<TModel, TArgs>(
            ReactivePlan<TModel> plan,
            string componentId,
            string vendor,
            TypedEvent<TArgs> typedEvent,
            Action<TArgs, PipelineBuilder<TModel>> pipeline)
            where TModel : class
        {
            if (plan == null) throw new ArgumentNullException(nameof(plan));
            if (componentId == null) throw new ArgumentNullException(nameof(componentId));
            if (vendor == null) throw new ArgumentNullException(nameof(vendor));
            if (typedEvent == null) throw new ArgumentNullException(nameof(typedEvent));
            if (pipeline == null) throw new ArgumentNullException(nameof(pipeline));

            var target = ComponentObjectTarget.For(componentId, vendor);
            var pipelineBuilder = new PipelineBuilder<TModel>(plan.Context);
            pipeline(typedEvent.Args, pipelineBuilder);
            plan.Context.WireComponentEvent(
                target.IdForJson,
                target.Vendor.Value,
                typedEvent.ObjectEvent,
                pipelineBuilder.BuildReaction());
        }
    }
}
