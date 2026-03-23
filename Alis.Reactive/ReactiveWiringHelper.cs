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
            if (string.IsNullOrEmpty(componentId))
                throw new InvalidOperationException(
                    $"{typeof(TComponent).Name} has no element ID. " +
                    "Ensure the component builder sets an id — for Native builders use " +
                    "the factory method (e.g. Html.NativeTextBoxFor), for Fusion builders " +
                    "ensure HtmlAttributes contains 'id'.");

            var pb = new PipelineBuilder<TModel>();
            pipeline(descriptor.Args, pb);

            var trigger = ComponentEventTrigger.For<TComponent>(
                componentId, descriptor.JsEvent, bindingPath);
            foreach (var reaction in pb.BuildReactions())
                plan.AddEntry(new Entry(trigger, reaction));
        }
    }
}
