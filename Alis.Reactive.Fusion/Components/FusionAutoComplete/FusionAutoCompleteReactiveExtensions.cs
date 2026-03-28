using System;
using System.Collections.Generic;
using Alis.Reactive.Builders;
using Alis.Reactive.Descriptors;
using Alis.Reactive.Descriptors.Triggers;
using Syncfusion.EJ2.DropDowns;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Wires browser events from a <see cref="FusionAutoComplete"/> into the reactive plan.
    /// </summary>
    /// <remarks>
    /// <c>.Reactive()</c> is always the last call inside the build callback passed to
    /// <see cref="FusionAutoCompleteHtmlExtensions.FusionAutoComplete{TModel,TProp}"/>:
    /// <code>
    /// Html.InputField(plan, m =&gt; m.Physician).FusionAutoComplete(b =&gt;
    /// {
    ///     b.Fields&lt;Item&gt;(t =&gt; t.Text, v =&gt; v.Value);
    ///     b.Reactive(plan, evt =&gt; evt.Changed, (args, p) =&gt;
    ///     {
    ///         p.Component&lt;FusionAutoComplete&gt;(m =&gt; m.Physician).SetValue("Dr. Smith");
    ///     });
    /// });
    /// </code>
    /// </remarks>
    public static class FusionAutoCompleteReactiveExtensions
    {
        private static readonly FusionAutoComplete Component = new FusionAutoComplete();

        /// <summary>
        /// Wires a FusionAutoComplete event to a reactive pipeline that executes in the browser.
        /// </summary>
        /// <typeparam name="TModel">The view model type.</typeparam>
        /// <typeparam name="TArgs">The event args type, inferred from the event selector.</typeparam>
        /// <param name="builder">The Fusion builder.</param>
        /// <param name="plan">The plan to add the reactive behavior to.</param>
        /// <param name="eventSelector">Selects which event to react to (e.g. <c>evt =&gt; evt.Changed</c>).</param>
        /// <param name="pipeline">Configures the commands to run when the event fires.</param>
        /// <returns>The builder for method chaining.</returns>
        public static AutoCompleteBuilder Reactive<TModel, TArgs>(
            this AutoCompleteBuilder builder,
            ReactivePlan<TModel> plan,
            Func<FusionAutoCompleteEvents, TypedEventDescriptor<TArgs>> eventSelector,
            Action<TArgs, PipelineBuilder<TModel>> pipeline)
            where TModel : class
        {
            var descriptor = eventSelector(FusionAutoCompleteEvents.Instance);
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
