using System;
using Microsoft.AspNetCore.Mvc.Rendering;
using Syncfusion.EJ2;
using Syncfusion.EJ2.Navigations;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Creates typed <see cref="FusionCarouselBuilder{TModel}"/> instances for Reactive Plan wiring.
    /// </summary>
    public static class FusionCarouselHtmlExtensions
    {
        /// <summary>
        /// Creates a Syncfusion Carousel and carries its controlled component ID into the Reactive Plan.
        /// </summary>
        /// <param name="plan">The Reactive Plan that receives carousel event wiring.</param>
        /// <param name="elementId">The controlled component ID shared by markup and Reactive Plan behavior.</param>
        /// <param name="build">Configures the underlying Syncfusion carousel builder.</param>
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
