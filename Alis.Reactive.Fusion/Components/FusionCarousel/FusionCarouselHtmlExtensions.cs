using System;
using Microsoft.AspNetCore.Mvc.Rendering;
using Syncfusion.EJ2;
using Syncfusion.EJ2.Navigations;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Factory extension for creating a typed FusionCarousel slice.
    /// Non-input component — no input registration wrapper.
    /// </summary>
    public static class FusionCarouselHtmlExtensions
    {
        public static FusionCarouselBuilder<TModel> FusionCarousel<TModel>(
            this IHtmlHelper<TModel> html,
            ReactivePlan<TModel> plan,
            string elementId,
            Action<CarouselBuilder> build)
            where TModel : class
        {
            var builder = html.EJS().Carousel(elementId);
            build(builder);

            return new FusionCarouselBuilder<TModel>(plan, elementId, builder.Render());
        }
    }
}
