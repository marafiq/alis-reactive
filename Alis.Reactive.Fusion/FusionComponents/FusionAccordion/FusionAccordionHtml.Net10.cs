#if NET10_0_OR_GREATER
using System;
using Alis.Reactive.RenderingAndDelivery;
using Microsoft.AspNetCore.Mvc.Rendering;
using Syncfusion.EJ2.Navigations;

namespace Alis.Reactive.Fusion.FusionComponents;

/// <summary>
/// The door of a view to an accordion.
/// </summary>
public static class FusionAccordionHtml
{
    /// <summary>Renders an accordion under the id the view gives it, with what the view sets on EJ2's builder.</summary>
    /// <param name="html">The view's HTML helper.</param>
    /// <param name="plan">The view's plan.</param>
    /// <param name="elementId">The id the view gives the accordion's element.</param>
    /// <param name="build">What the view sets on the accordion, its items among them.</param>
    /// <returns>The rendered accordion, which writes its markup where the view puts it.</returns>
    public static Rendered<FusionAccordion, TModel> FusionAccordion<TModel>(this IHtmlHelper<TModel> html, ReactivePlan<TModel> plan, string elementId, Action<AccordionBuilder> build)
        => throw new NotImplementedException();
}
#endif
