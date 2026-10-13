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
            ComponentVendor vendor,
            TypedEvent<TArgs> typedEvent,
            Action<TArgs, PipelineBuilder<TModel>> pipeline)
            where TModel : class
            where TArgs : JsObject
        {
            if (plan == null) throw new ArgumentNullException(nameof(plan));
            if (componentId == null) throw new ArgumentNullException(nameof(componentId));
            if (vendor == null) throw new ArgumentNullException(nameof(vendor));
            if (typedEvent == null) throw new ArgumentNullException(nameof(typedEvent));
            if (pipeline == null) throw new ArgumentNullException(nameof(pipeline));

            var target = ComponentObjectTarget.For(componentId, vendor);
            var args = typedEvent.Args;
            args.BindTo(new EventPayloadBinding(plan.Context));
            var reaction = PipelineBuilder<TModel>.Build(plan.Context, p => pipeline(args, p));
            plan.Context.WireComponentEvent(
                target.ComponentId.Value,
                target.Vendor,
                typedEvent.ObjectEvent,
                reaction);
        }
    }
}
