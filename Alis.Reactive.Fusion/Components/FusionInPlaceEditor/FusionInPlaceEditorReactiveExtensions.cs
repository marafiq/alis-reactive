using System;
using System.Collections.Generic;
using Alis.Reactive.Builders;
using Alis.Reactive.PlanModel;
using Syncfusion.EJ2.InPlaceEditor;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Wires browser events from a <see cref="FusionInPlaceEditor"/> into the reactive plan.
    /// </summary>
    /// <remarks>
    /// <c>.Reactive()</c> is always the last call inside the build callback passed to
    /// <see cref="FusionInPlaceEditorHtmlExtensions.FusionInPlaceEditor{TModel,TProp}"/>:
    /// <code>
    /// Html.InputField(plan, m =&gt; m.DateOfBirth).FusionInPlaceEditor(b =&gt;
    /// {
    ///     b.Type(InputType.Date).Mode(RenderMode.Inline);
    ///     b.Reactive(plan, evt =&gt; evt.ActionBegin, (args, p) =&gt;
    ///     {
    ///         args.PreventDefault(p);
    ///         p.Post("/Residents/UpdateDateOfBirth")
    ///          .Gather(g =&gt; g.Include&lt;FusionInPlaceEditor, TModel&gt;(m =&gt; m.DateOfBirth))
    ///          .Validate&lt;ResidentEditValidator&gt;("resident-form")
    ///          .Response(r =&gt; r.OnSuccess(s =&gt; { /* … */ }));
    ///     });
    /// });
    /// </code>
    /// </remarks>
    public static class FusionInPlaceEditorReactiveExtensions
    {
        private static readonly FusionInPlaceEditor Component = new FusionInPlaceEditor();

        /// <summary>
        /// Wires a FusionInPlaceEditor event to a reactive pipeline that executes in the browser.
        /// </summary>
        /// <typeparam name="TModel">The view model type.</typeparam>
        /// <typeparam name="TArgs">The event args type, inferred from the event selector.</typeparam>
        /// <param name="builder">The Fusion builder.</param>
        /// <param name="plan">The plan to add the reactive behavior to.</param>
        /// <param name="eventSelector">Selects which event to react to (e.g. <c>evt =&gt; evt.ActionBegin</c>).</param>
        /// <param name="pipeline">Configures the commands to run when the event fires.</param>
        /// <returns>The builder for method chaining.</returns>
        public static InPlaceEditorBuilder Reactive<TModel, TArgs>(
            this InPlaceEditorBuilder builder,
            ReactivePlan<TModel> plan,
            Func<FusionInPlaceEditorEvents, TypedEvent<TArgs>> eventSelector,
            Action<TArgs, PipelineBuilder<TModel>> pipeline)
            where TModel : class
        {
            var descriptor = eventSelector(FusionInPlaceEditorEvents.Instance);

            var attrs = (IDictionary<string, object>)builder.model.HtmlAttributes;
            var componentId = (string)attrs["id"];

            ComponentEventOnboarding.Wire(plan, componentId, Component.Vendor, descriptor, pipeline);

            return builder;
        }
    }
}
