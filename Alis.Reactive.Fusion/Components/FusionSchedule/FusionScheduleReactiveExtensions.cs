using System;
using Alis.Reactive.Builders;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Wires browser events from a <see cref="FusionSchedule"/> into the reactive plan.
    /// </summary>
    /// <remarks>
    /// <code>
    /// @(Html.FusionSchedule(plan, "shift-schedule", b =&gt; { /* views, resources */ })
    ///     .Reactive(evt =&gt; evt.CellClicked, (args, p) =&gt; {
    ///         p.Component&lt;FusionDialog&gt;("edit-dialog").Show();
    ///     })
    ///     .Reactive(evt =&gt; evt.Navigating, (args, p) =&gt; {
    ///         p.Get("/api/schedule/assignments")
    ///          .Response(r =&gt; r.OnSuccess(s =&gt;
    ///             s.Component&lt;FusionSchedule&gt;("shift-schedule").SetDataSource(s, j =&gt; j.Assignments)));
    ///     }))
    /// </code>
    /// </remarks>
    public static class FusionScheduleReactiveExtensions
    {
        private static readonly FusionSchedule Component = new FusionSchedule();

        public static FusionScheduleBuilder<TModel> Reactive<TModel, TArgs>(
            this FusionScheduleBuilder<TModel> builder,
            Func<FusionScheduleEvents, TypedEvent<TArgs>> eventSelector,
            Action<TArgs, PipelineBuilder<TModel>> pipeline)
            where TModel : class
        {
            var descriptor = eventSelector(FusionScheduleEvents.Instance);

            ComponentEventOnboarding.Wire(builder.Plan, builder.ElementId, Component.Vendor, descriptor, pipeline);

            return builder;
        }
    }
}
