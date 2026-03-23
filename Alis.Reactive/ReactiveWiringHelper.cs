using System;
using Alis.Reactive.Builders;
using Alis.Reactive.Descriptors;
using Alis.Reactive.Descriptors.Triggers;

namespace Alis.Reactive
{
    /// <summary>
    /// Shared plumbing for every component's .Reactive() extension method.
    /// Builds the pipeline, creates the trigger via the factory, and adds entries to the plan.
    /// Each component's .Reactive() becomes a thin wrapper that extracts componentId/bindingPath
    /// from its builder type and delegates here.
    /// </summary>
    internal static class ReactiveWiringHelper
    {
        internal static void Wire<TModel, TComponent, TArgs>(
            IReactivePlan<TModel> plan,
            string componentId,
            string? bindingPath,
            TypedEventDescriptor<TArgs> descriptor,
            Action<TArgs, PipelineBuilder<TModel>> pipeline)
            where TModel : class
            where TComponent : IComponent, new()
        {
            var pb = new PipelineBuilder<TModel>();
            pipeline(descriptor.Args, pb);

            var trigger = ComponentEventTrigger.For<TComponent>(
                componentId, descriptor.JsEvent, bindingPath);
            foreach (var reaction in pb.BuildReactions())
                plan.AddEntry(new Entry(trigger, reaction));
        }
    }
}
