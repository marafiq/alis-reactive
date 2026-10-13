using System;
using Alis.Reactive.Builders;

namespace Alis.Reactive
{
    /// <summary>
    /// Runs a callback each time a rendered component raises one of its declared events; the callback is
    /// handed the event's payload, live.
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

            var context = plan.Context;
            var args = typedEvent.Args;
            args.BindTo(new EventPayloadBinding(context));
            var body = PipelineBuilder<TModel>.Build(context, p => pipeline(args, p));

            // The component joins the plan after the elements its own callback names.
            context.DeclareComponent(componentId, vendor);
            context.On(objects => objects.Element(componentId), typedEvent.ObjectEvent, body);
        }
    }
}
