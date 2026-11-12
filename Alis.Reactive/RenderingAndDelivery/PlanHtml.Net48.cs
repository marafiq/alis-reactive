#if NET48
using System;
using System.Web;
using System.Web.Mvc;
using Alis.Reactive.TriggersAndPipelines;

namespace Alis.Reactive.RenderingAndDelivery;

/// <summary>
/// The two doors of a view's plan: one opens it, and one writes it as the view's last line.
/// </summary>
public static class PlanHtml
{
    /// <summary>Opens the view's plan, which every door of the view that writes into it takes.</summary>
    /// <remarks>
    /// A view renders the plan it opens once, with <c>Html.RenderPlan(plan)</c> as its last line; the build refuses
    /// a plan that is opened and never rendered.
    /// </remarks>
    /// <returns>The plan.</returns>
    public static ReactivePlan<TModel> ReactivePlan<TModel>(this HtmlHelper<TModel> html)
        => ViewPlans.Open<TModel>();

    /// <summary>Handles a trigger in the view's plan.</summary>
    /// <remarks>A view calls it once for each trigger it handles, before it renders the plan.</remarks>
    /// <param name="html">The view's HTML helper.</param>
    /// <param name="plan">The plan the view opened.</param>
    /// <param name="trigger">Names the trigger and the pipeline it runs.</param>
    public static void On<TModel>(this HtmlHelper<TModel> html, ReactivePlan<TModel> plan, Action<Trigger<TModel>> trigger)
        => ViewPlans.On(plan, trigger);

    /// <summary>Writes the view's plan into the page, as the view's last line.</summary>
    /// <remarks>
    /// The build refuses a plan rendered twice and a door that takes the plan after it rendered, since the page
    /// would run its handlers twice or never run what was written late.
    /// </remarks>
    /// <param name="html">The view's HTML helper.</param>
    /// <param name="plan">The plan the view opened.</param>
    /// <returns>The plan's script.</returns>
    public static IHtmlString RenderPlan<TModel>(this HtmlHelper html, ReactivePlan<TModel> plan)
        => new HtmlString(ViewPlans.ScriptOf(plan));
}
#endif
