#if NET10_0_OR_GREATER
using System;
using Alis.Reactive.RenderingAndDelivery;
using Microsoft.AspNetCore.Mvc.Rendering;
using Syncfusion.EJ2;
using Syncfusion.EJ2.Navigations;

namespace Alis.Reactive.Fusion.FusionComponents;

/// <summary>
/// The door of a view to a carousel.
/// </summary>
public static class FusionCarouselHtml
{
    /// <summary>Renders a carousel under the id the view gives it, with what the view sets on EJ2's builder.</summary>
    /// <param name="html">The view's HTML helper.</param>
    /// <param name="plan">The view's plan.</param>
    /// <param name="elementId">The id the view gives the carousel's element.</param>
    /// <param name="build">What the view sets on the carousel, its slides among them.</param>
    /// <returns>The rendered carousel, which writes its markup where the view puts it.</returns>
    public static Rendered<FusionCarousel, TModel> FusionCarousel<TModel>(this IHtmlHelper<TModel> html, ReactivePlan<TModel> plan, string elementId, Action<CarouselBuilder> build)
    {
        if (plan is null) throw new ArgumentNullException(nameof(plan));
        if (elementId is null) throw new ArgumentNullException(nameof(elementId));
        if (build is null) throw new ArgumentNullException(nameof(build));
        var carousel = html.EJS().Carousel(elementId);
        build(carousel);
        return plan.Render<FusionCarousel>(elementId, carousel.Render());
    }
}
#endif
