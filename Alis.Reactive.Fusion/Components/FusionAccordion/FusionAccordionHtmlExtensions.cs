using System;
#if NET48
using System.Web.Mvc;
#else
using Microsoft.AspNetCore.Mvc.Rendering;
#endif
using Syncfusion.EJ2;
using Syncfusion.EJ2.Navigations;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Factory extension for creating FusionAccordionBuilder.
    /// Non-input component — NO InputField wrapper, NO ComponentsMap registration.
    /// </summary>
    public static class FusionAccordionHtmlExtensions
    {
        /// <summary>
        /// Creates a FusionAccordion with the given element ID.
        /// Non-input component: renders directly, no label/validation wrapper.
        /// </summary>
        public static FusionAccordionBuilder<TModel> FusionAccordion<TModel>(
#if NET48
            this HtmlHelper<TModel> html,
#else
            this IHtmlHelper<TModel> html,
#endif
            ReactivePlan<TModel> plan,
            string elementId,
            Action<AccordionBuilder> build)
            where TModel : class
        {
            // NO ComponentsMap registration — this is NOT an input component

            var builder = html.EJS().Accordion(elementId);
            build(builder);

            return new FusionAccordionBuilder<TModel>(plan, elementId, builder.Render());
        }
    }
}
