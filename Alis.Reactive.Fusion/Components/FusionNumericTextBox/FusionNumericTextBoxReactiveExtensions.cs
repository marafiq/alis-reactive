using System;
using System.Collections.Generic;
using System.Reflection;
using Alis.Reactive;
using Alis.Reactive.Builders;
using Alis.Reactive.Descriptors;
using Alis.Reactive.Descriptors.Triggers;
using Syncfusion.EJ2.Inputs;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Wires reactive event pipelines onto the Syncfusion NumericTextBoxBuilder.
    ///
    /// Usage (in .cshtml):
    ///   @Html.EJS().NumericTextBoxFor(m => m.Amount)
    ///       .Reactive(plan, evt => evt.Changed, (args, p) =>
    ///       {
    ///           p.Component&lt;FusionNumericTextBox&gt;(m => m.Amount).SetValue(100);
    ///       })
    ///       .Render()
    /// </summary>
    public static class FusionNumericTextBoxReactiveExtensions
    {
        private static readonly FusionNumericTextBox _component = new FusionNumericTextBox();

        /// <summary>Wires the Changed event (typed payload with Value, PreviousValue, IsInteracted).</summary>
        public static NumericTextBoxBuilder Reactive<TModel>(
            this NumericTextBoxBuilder builder,
            IReactivePlan<TModel> plan,
            Func<FusionNumericTextBoxEvents, TypedEventDescriptor<FusionNumericTextBoxChangeArgs>> eventSelector,
            Action<FusionNumericTextBoxChangeArgs, PipelineBuilder<TModel>> pipeline)
            where TModel : class
            => ReactiveCore(builder, plan, eventSelector(FusionNumericTextBoxEvents.Instance), pipeline);

        /// <summary>Wires the Focus event (void payload).</summary>
        public static NumericTextBoxBuilder Reactive<TModel>(
            this NumericTextBoxBuilder builder,
            IReactivePlan<TModel> plan,
            Func<FusionNumericTextBoxEvents, TypedEventDescriptor<FusionNumericTextBoxFocusArgs>> eventSelector,
            Action<FusionNumericTextBoxFocusArgs, PipelineBuilder<TModel>> pipeline)
            where TModel : class
            => ReactiveCore(builder, plan, eventSelector(FusionNumericTextBoxEvents.Instance), pipeline);

        /// <summary>Wires the Blur event (void payload).</summary>
        public static NumericTextBoxBuilder Reactive<TModel>(
            this NumericTextBoxBuilder builder,
            IReactivePlan<TModel> plan,
            Func<FusionNumericTextBoxEvents, TypedEventDescriptor<FusionNumericTextBoxBlurArgs>> eventSelector,
            Action<FusionNumericTextBoxBlurArgs, PipelineBuilder<TModel>> pipeline)
            where TModel : class
            => ReactiveCore(builder, plan, eventSelector(FusionNumericTextBoxEvents.Instance), pipeline);

        private static NumericTextBoxBuilder ReactiveCore<TModel, TArgs>(
            NumericTextBoxBuilder builder,
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
        private static string ExtractComponentId(NumericTextBoxBuilder builder)
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
