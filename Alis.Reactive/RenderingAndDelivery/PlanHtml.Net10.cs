#if NET10_0_OR_GREATER
using System;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;

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
    public static ReactivePlan<TModel> ReactivePlan<TModel>(this IHtmlHelper<TModel> html) => throw new NotImplementedException();

    /// <summary>Writes the view's plan into the page, as the view's last line.</summary>
    /// <remarks>
    /// The build refuses a plan rendered twice and a door that takes the plan after it rendered, since the page
    /// would run its handlers twice or never run what was written late.
    /// </remarks>
    /// <param name="html">The view's HTML helper.</param>
    /// <param name="plan">The plan the view opened.</param>
    /// <returns>The plan's script.</returns>
    public static IHtmlContent RenderPlan<TModel>(this IHtmlHelper html, ReactivePlan<TModel> plan) => throw new NotImplementedException();
}
#endif
