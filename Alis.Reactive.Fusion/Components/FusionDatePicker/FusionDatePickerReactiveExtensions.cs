using System;
using System.Collections.Generic;
using Alis.Reactive.Builders;
using Alis.Reactive.Descriptors;
using Alis.Reactive.Descriptors.Triggers;
using Syncfusion.EJ2.Calendars;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Wires browser events from a <see cref="FusionDatePicker"/> into the reactive plan.
    /// </summary>
    /// <remarks>
    /// <c>.Reactive()</c> is always the last call inside the build callback passed to
    /// <see cref="FusionDatePickerHtmlExtensions.DatePicker{TModel, TProp}"/>:
    /// <code>
    /// Html.InputField(plan, m =&gt; m.BirthDate).DatePicker(b =&gt;
    /// {
    ///     b.Format("MM/dd/yyyy");
    ///     b.Reactive(plan, evt =&gt; evt.Changed, (args, p) =&gt; { /* commands */ });
    /// });
    /// </code>
    /// </remarks>
    public static class FusionDatePickerReactiveExtensions
    {
        private static readonly FusionDatePicker Component = new FusionDatePicker();

        /// <summary>
        /// Wires a DatePicker event to a reactive pipeline that executes in the browser.
        /// </summary>
        /// <typeparam name="TModel">The view model type.</typeparam>
        /// <typeparam name="TArgs">The event args type, inferred from the event selector.</typeparam>
        /// <param name="builder">The Syncfusion builder.</param>
        /// <param name="plan">The plan to add the reactive behavior to.</param>
        /// <param name="eventSelector">Selects which event to react to (e.g. <c>evt =&gt; evt.Changed</c>).</param>
        /// <param name="pipeline">Configures the commands to run when the event fires.</param>
        /// <returns>The builder for method chaining.</returns>
        public static DatePickerBuilder Reactive<TModel, TArgs>(
            this DatePickerBuilder builder,
            ReactivePlan<TModel> plan,
            Func<FusionDatePickerEvents, TypedEventDescriptor<TArgs>> eventSelector,
            Action<TArgs, PipelineBuilder<TModel>> pipeline)
            where TModel : class
        {
            var descriptor = eventSelector(FusionDatePickerEvents.Instance);
            var pb = new PipelineBuilder<TModel>();
            pipeline(descriptor.Args, pb);

            var attrs = (IDictionary<string, object>)builder.model.HtmlAttributes;
            var componentId = (string)attrs["id"];
            var bindingPath = (string)attrs["name"];

            var trigger = new ComponentEventTrigger(componentId, descriptor.JsEvent, Component.Vendor, bindingPath, Component.ReadExpr);
            foreach (var reaction in pb.BuildReactions())
                plan.AddEntry(new Entry(trigger, reaction));

            return builder;
        }
    }
}
