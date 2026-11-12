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

    /// <summary>Marks an element so that a click at or inside it raises an app event that carries no payload.</summary>
    /// <remarks>Write it inside the element's tag: <c>&lt;button type="button" @Html.Raises(plan, WardEvents.Opened)&gt;</c>.</remarks>
    /// <param name="html">The view's HTML helper.</param>
    /// <param name="plan">The plan the view opened.</param>
    /// <param name="appEvent">The event, as the app declares it.</param>
    /// <returns>The attributes that mark the element.</returns>
    public static IHtmlString Raises<TModel>(this HtmlHelper<TModel> html, ReactivePlan<TModel> plan, AppEvent appEvent) => throw new NotImplementedException();

    /// <summary>Marks an element so that a click at or inside it raises an app event with a payload.</summary>
    /// <remarks>Write it inside the element's tag. The payload is written as the app's JSON writes it.</remarks>
    /// <typeparam name="TModel">The model of the view.</typeparam>
    /// <typeparam name="TPayload">The class of the payload.</typeparam>
    /// <param name="html">The view's HTML helper.</param>
    /// <param name="plan">The plan the view opened.</param>
    /// <param name="appEvent">The event, as the app declares it.</param>
    /// <param name="payload">The payload each click raises the event with.</param>
    /// <returns>The attributes that mark the element.</returns>
    public static IHtmlString Raises<TModel, TPayload>(this HtmlHelper<TModel> html, ReactivePlan<TModel> plan, AppEvent<TPayload> appEvent, TPayload payload) => throw new NotImplementedException();

    /// <summary>Writes the view's plan into the page, as the view's last line.</summary>
    /// <remarks>
    /// The build refuses a plan rendered twice and a door that takes the plan after it rendered, since the page
    /// would run its handlers twice or never run what was written late. Where the build cannot follow the view's
    /// code, as inside a section, rendering the view fails with the same message.
    /// </remarks>
    /// <param name="html">The view's HTML helper.</param>
    /// <param name="plan">The plan the view opened.</param>
    /// <returns>The plan's script.</returns>
    public static IHtmlString RenderPlan<TModel>(this HtmlHelper html, ReactivePlan<TModel> plan)
        => new HtmlString(ViewPlans.ScriptOf(plan));
}
#endif
