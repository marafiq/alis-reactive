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
    /// The Syncfusion Accordion, declared by the names its JavaScript API uses.
    /// </summary>
    /// <remarks>
    /// Render it with <c>Html.FusionAccordion(plan, "care-accordion", b =&gt; ...)</c>, then reach it in a callback
    /// and use its members directly:
    /// <code>
    /// var accordion = p.Component&lt;FusionAccordion&gt;("care-accordion");
    /// accordion.ExpandItem(true, 1);
    /// </code>
    /// </remarks>
    public sealed class FusionAccordion : FusionComponent
    {
        /// <summary>Expands (<see langword="true"/>) or collapses (<see langword="false"/>) the item at a zero-based index.</summary>
        /// <param name="isExpand"><see langword="true"/> to expand the item, <see langword="false"/> to collapse it.</param>
        /// <param name="index">The item's zero-based index.</param>
        public void ExpandItem(Arg<bool> isExpand, Arg<int> index) => Method(isExpand, index);

        /// <summary>Enables (<see langword="true"/>) or disables (<see langword="false"/>) the item at a zero-based index.</summary>
        /// <param name="index">The item's zero-based index.</param>
        /// <param name="isEnable"><see langword="true"/> to enable the item, <see langword="false"/> to disable it.</param>
        public void EnableItem(Arg<int> index, Arg<bool> isEnable) => Method(index, isEnable);

        /// <summary>Raised after an item expands or collapses.</summary>
        public Event<FusionAccordionExpandedArgs> Expanded =>
            Event<FusionAccordionExpandedArgs>();
    }

    /// <summary>Payload of <see cref="FusionAccordion.Expanded"/>.</summary>
    public class FusionAccordionExpandedArgs : JsObject
    {
        /// <summary>Index of the item that changed.</summary>
        public ReadOnly<int> Index => ReadOnly<int>();

        /// <summary>Whether the item is now expanded.</summary>
        public ReadOnly<bool> IsExpanded => ReadOnly<bool>();
    }

    /// <summary>
    /// Renders the component for a plan, ready for <c>.Reactive(...)</c>.
    /// </summary>
    public static class FusionAccordionHtmlExtensions
    {
        /// <summary>
        /// Creates a Syncfusion Accordion and carries its controlled component ID into the Reactive Plan.
        /// </summary>
        /// <typeparam name="TModel">The view's model type.</typeparam>
        /// <param name="html">The view's HTML helper.</param>
        /// <param name="plan">Reactive Plan receiving accordion event wiring.</param>
        /// <param name="elementId">Controlled component ID shared by markup and Reactive Plan behavior.</param>
        /// <param name="build">Configures the component before rendering.</param>
        public static RenderedComponent<global::Alis.Reactive.Fusion.Components.FusionAccordion, TModel> FusionAccordion<TModel>(
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
            var builder = html.EJS().Accordion(elementId);
            build(builder);

            return plan.Render<global::Alis.Reactive.Fusion.Components.FusionAccordion>(elementId, builder.Render());
        }
    }
}
