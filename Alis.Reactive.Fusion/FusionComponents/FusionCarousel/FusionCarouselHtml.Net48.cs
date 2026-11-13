#if NET48
using System;
using System.Web.Mvc;
using Alis.Reactive.RenderingAndDelivery;
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
    public static Rendered<FusionCarousel, TModel> FusionCarousel<TModel>(this HtmlHelper<TModel> html, ReactivePlan<TModel> plan, string elementId, Action<CarouselBuilder> build)
        => throw new NotImplementedException();
}
#endif
