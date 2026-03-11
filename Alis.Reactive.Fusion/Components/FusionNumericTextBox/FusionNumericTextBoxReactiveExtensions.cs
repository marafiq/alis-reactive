using System;
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
        public static NumericTextBoxBuilder Reactive<TModel>(
            this NumericTextBoxBuilder builder,
            IReactivePlan<TModel> plan,
            Func<FusionNumericTextBoxEvents, TypedEventDescriptor<FusionNumericTextBoxChangeArgs>> eventSelector,
            Action<FusionNumericTextBoxChangeArgs, PipelineBuilder<TModel>> pipeline)
            where TModel : class
        {
            var descriptor = eventSelector(FusionNumericTextBoxEvents.Instance);
            var pb = new PipelineBuilder<TModel>();
            pipeline(descriptor.Args, pb);

            var componentId = ExtractProperty(builder, "ID") ?? "unknown";
            var bindingPath = ExtractProperty(builder, "Name");

            var trigger = new ComponentEventTrigger(componentId, descriptor.JsEvent, "fusion", bindingPath, ComponentHelper.GetReadExpr<FusionNumericTextBox>());
            var entry = new Entry(trigger, pb.BuildReaction());
            plan.AddEntry(entry);
            (plan as ReactivePlan<TModel>)?.RegisterBuildContexts(pb.BuildContexts);
            if (bindingPath != null)
            {
                plan.RegisterComponent(componentId, "fusion", bindingPath, ComponentHelper.GetReadExpr<FusionNumericTextBox>());
            }

            return builder;
        }

        private static string? ExtractProperty(object builder, string propertyName) =>
            builder.GetType().GetProperty(propertyName,
                BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase)
                ?.GetValue(builder)?.ToString();
    }
}
