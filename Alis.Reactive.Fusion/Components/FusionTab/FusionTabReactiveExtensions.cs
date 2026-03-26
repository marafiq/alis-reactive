using System;
using Alis.Reactive.Builders;
using Alis.Reactive.Descriptors;
using Alis.Reactive.Descriptors.Triggers;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Wires reactive event pipelines onto the FusionTabBuilder.
    /// Non-input component: no readExpr in the trigger.
    /// </summary>
    public static class FusionTabReactiveExtensions
    {
        private static readonly FusionTab Component = new FusionTab();

        public static FusionTabBuilder<TModel> Reactive<TModel, TArgs>(
            this FusionTabBuilder<TModel> builder,
            Func<FusionTabEvents, TypedEventDescriptor<TArgs>> eventSelector,
            Action<TArgs, PipelineBuilder<TModel>> pipeline)
            where TModel : class
        {
            var descriptor = eventSelector(FusionTabEvents.Instance);
            var pb = new PipelineBuilder<TModel>();
            pipeline(descriptor.Args, pb);

            var trigger = new ComponentEventTrigger(
                builder.ElementId,
                descriptor.JsEvent,
                Component.Vendor);

            foreach (var reaction in pb.BuildReactions())
                builder.Plan.AddEntry(new Entry(trigger, reaction));

            return builder;
        }
    }
}
