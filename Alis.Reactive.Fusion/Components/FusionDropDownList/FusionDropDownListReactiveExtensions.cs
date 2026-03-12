using System;
using System.Collections.Generic;
using System.Reflection;
using Alis.Reactive;
using Alis.Reactive.Builders;
using Alis.Reactive.Descriptors;
using Alis.Reactive.Descriptors.Triggers;
using Syncfusion.EJ2.DropDowns;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Wires reactive event pipelines onto the Syncfusion DropDownListBuilder.
    ///
    /// Usage (in .cshtml):
    ///   @Html.EJS().DropDownListFor(m => m.Country)
    ///       .Reactive(plan, evt => evt.Changed, (args, p) =>
    ///       {
    ///           p.Component&lt;FusionDropDownList&gt;(m => m.Country).SetValue("US");
    ///       })
    ///       .Render()
    /// </summary>
    public static class FusionDropDownListReactiveExtensions
    {
        private static readonly FusionDropDownList _component = new FusionDropDownList();

        /// <summary>Wires the Changed event (typed payload with Value, IsInteracted).</summary>
        public static DropDownListBuilder Reactive<TModel>(
            this DropDownListBuilder builder,
            IReactivePlan<TModel> plan,
            Func<FusionDropDownListEvents, TypedEventDescriptor<FusionDropDownListChangeArgs>> eventSelector,
            Action<FusionDropDownListChangeArgs, PipelineBuilder<TModel>> pipeline)
            where TModel : class
            => ReactiveCore(builder, plan, eventSelector(FusionDropDownListEvents.Instance), pipeline);

        /// <summary>Wires the Focus event (void payload).</summary>
        public static DropDownListBuilder Reactive<TModel>(
            this DropDownListBuilder builder,
            IReactivePlan<TModel> plan,
            Func<FusionDropDownListEvents, TypedEventDescriptor<FusionDropDownListFocusArgs>> eventSelector,
            Action<FusionDropDownListFocusArgs, PipelineBuilder<TModel>> pipeline)
            where TModel : class
            => ReactiveCore(builder, plan, eventSelector(FusionDropDownListEvents.Instance), pipeline);

        /// <summary>Wires the Blur event (void payload).</summary>
        public static DropDownListBuilder Reactive<TModel>(
            this DropDownListBuilder builder,
            IReactivePlan<TModel> plan,
            Func<FusionDropDownListEvents, TypedEventDescriptor<FusionDropDownListBlurArgs>> eventSelector,
            Action<FusionDropDownListBlurArgs, PipelineBuilder<TModel>> pipeline)
            where TModel : class
            => ReactiveCore(builder, plan, eventSelector(FusionDropDownListEvents.Instance), pipeline);

        private static DropDownListBuilder ReactiveCore<TModel, TArgs>(
            DropDownListBuilder builder,
            IReactivePlan<TModel> plan,
            TypedEventDescriptor<TArgs> descriptor,
            Action<TArgs, PipelineBuilder<TModel>> pipeline)
            where TModel : class
        {
            var pb = new PipelineBuilder<TModel>();
            pipeline(descriptor.Args, pb);

            var componentId = ExtractComponentId(builder);
            var bindingPath = ExtractProperty(builder, "Name");

            var trigger = new ComponentEventTrigger(componentId, descriptor.JsEvent, _component.Vendor, bindingPath, _component.ReadExpr);
            var entry = new Entry(trigger, pb.BuildReaction());
            plan.AddEntry(entry);
            (plan as ReactivePlan<TModel>)?.RegisterBuildContexts(pb.BuildContexts);
            if (bindingPath != null)
            {
                plan.RegisterComponent(componentId, _component.Vendor, bindingPath, _component.ReadExpr);
            }

            return builder;
        }

        /// <summary>
        /// Resolves the actual element ID that will be used in the browser DOM.
        /// SF's HtmlAttributes(["id"] = ...) overrides the rendered element id at client-side init,
        /// so we must check htmlAttributes first, then fall back to the builder's ID property.
        /// </summary>
        private static string ExtractComponentId(DropDownListBuilder builder)
        {
            // SF stores HtmlAttributes on model.HtmlAttributes — check for id override
            var model = builder.GetType()
                .GetField("model", BindingFlags.Public | BindingFlags.Instance)
                ?.GetValue(builder);

            if (model != null)
            {
                var htmlAttrs = model.GetType()
                    .GetProperty("HtmlAttributes", BindingFlags.Public | BindingFlags.Instance)
                    ?.GetValue(model);

                if (htmlAttrs is IDictionary<string, object> dict && dict.TryGetValue("id", out var idVal))
                {
                    return idVal?.ToString() ?? builder.ID ?? "unknown";
                }
            }

            return builder.ID ?? "unknown";
        }

        private static string? ExtractProperty(object builder, string propertyName) =>
            builder.GetType().GetProperty(propertyName,
                BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase)
                ?.GetValue(builder)?.ToString();
    }
}
