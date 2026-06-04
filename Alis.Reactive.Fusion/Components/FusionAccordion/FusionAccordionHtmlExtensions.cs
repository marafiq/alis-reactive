using System;
using Microsoft.AspNetCore.Mvc.Rendering;
using Syncfusion.EJ2;
using Syncfusion.EJ2.Navigations;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Creates typed <see cref="FusionAccordionBuilder{TModel}"/> instances for Reactive Plan wiring.
    /// </summary>
    public static class FusionAccordionHtmlExtensions
    {
        /// <summary>
        /// Creates a Syncfusion Accordion and carries its controlled component ID into the Reactive Plan.
        /// </summary>
        /// <typeparam name="TModel">The view model used to author the Reactive Plan.</typeparam>
        /// <param name="html">The Razor HTML helper.</param>
        /// <param name="plan">The Reactive Plan that receives accordion event wiring.</param>
        /// <param name="elementId">The controlled component ID shared by markup and plan entries.</param>
        /// <param name="build">Configures the underlying Syncfusion accordion builder.</param>
        public static FusionAccordionBuilder<TModel> FusionAccordion<TModel>(
            this IHtmlHelper<TModel> html,
            ReactivePlan<TModel> plan,
            string elementId,
            Action<AccordionBuilder> build)
            where TModel : class
        {
            var builder = html.EJS().Accordion(elementId);
            build(builder);

            return new FusionAccordionBuilder<TModel>(plan, elementId, builder.Render());
        }
    }
}
