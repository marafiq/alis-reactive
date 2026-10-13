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
#nullable disable annotations
    /// <summary>
    /// The Syncfusion Accordion, declared by the names its JavaScript API uses.
    /// </summary>
    /// <remarks>
    /// Render it with <c>Html.FusionAccordion(plan, "care-accordion", b =&gt; ...)</c> and target it with
    /// <c>p.Component&lt;FusionAccordion&gt;("care-accordion")</c>; every declared member works with <c>Set</c>,
    /// <c>Call</c> and <c>Get</c>, for example <c>.Call(c =&gt; c.ExpandItem, true, 1)</c>.
    /// </remarks>
    public sealed class FusionAccordion : FusionComponent
    {
        /// <summary>Expands (<see langword="true"/>) or collapses (<see langword="false"/>) the item at a zero-based index.</summary>
        public void ExpandItem(Arg<bool> first, Arg<int> second) => Call(first, second);

        /// <summary>Enables (<see langword="true"/>) or disables (<see langword="false"/>) the item at a zero-based index.</summary>
        public void EnableItem(Arg<int> first, Arg<bool> second) => Call(first, second);

        /// <summary>Raised after an item expands or collapses.</summary>
        public TypedEvent<FusionAccordionExpandedArgs> Expanded =>
            Event<FusionAccordionExpandedArgs>("expanded");
    }

    /// <summary>Payload of <see cref="FusionAccordion.Expanded"/>.</summary>
    public class FusionAccordionExpandedArgs : JsObject
    {
        /// <summary>Index of the item that changed.</summary>
        public ReadOnly<int> Index => ReadOnly<int>();

        /// <summary>Whether the item is now expanded.</summary>
        public ReadOnly<bool> IsExpanded => ReadOnly<bool>();
    }
#nullable restore annotations

    /// <summary>
    /// Renders the component for a plan, ready for <c>.Reactive(...)</c>.
    /// </summary>
    public static class FusionAccordionHtmlExtensions
    {
        /// <summary>
        /// Creates a Syncfusion Accordion and carries its controlled component ID into the Reactive Plan.
        /// </summary>
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
