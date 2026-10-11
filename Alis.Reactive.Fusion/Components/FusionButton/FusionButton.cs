using System;
#if NET48
using System.Web.Mvc;
#else
using Microsoft.AspNetCore.Mvc.Rendering;
#endif
using Syncfusion.EJ2;
using Syncfusion.EJ2.Buttons;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// The Syncfusion Button, declared by the names its JavaScript API uses.
    /// </summary>
    /// <remarks>
    /// Render it with <c>Html.FusionButton(plan, "begin-check-in", b =&gt; ...)</c> and target it with
    /// <c>p.Component&lt;FusionButton&gt;("begin-check-in")</c>; every declared member works with <c>Set</c>,
    /// <c>Call</c> and <c>Get</c>. Syncfusion applies property writes on its next tick; follow writes with
    /// <c>.Call(c =&gt; c.DataBind)</c>: <c>.Set(c =&gt; c.Content, "Saved").Call(c =&gt; c.DataBind)</c>.
    /// </remarks>
    public sealed class FusionButton : FusionComponent
    {
        /// <summary>The button text.</summary>
        public ComponentProperty<string> Content => Property<string>("content");

        /// <summary>Whether the button is disabled.</summary>
        public ComponentProperty<bool> Disabled => Property<bool>("disabled");

        /// <summary>The icon CSS classes.</summary>
        public ComponentProperty<string> IconCss => Property<string>("iconCss");

        /// <summary>Where the icon sits relative to the text.</summary>
        public ComponentProperty<FusionButtonIconPosition> IconPosition => Property<FusionButtonIconPosition>("iconPosition");

        /// <summary>The CSS classes on the button.</summary>
        public ComponentProperty<string> CssClass => Property<string>("cssClass");

        /// <summary>Whether the button has the primary style.</summary>
        public ComponentProperty<bool> IsPrimary => Property<bool>("isPrimary");

        /// <summary>Whether the button toggles its active state when clicked.</summary>
        public ComponentProperty<bool> IsToggle => Property<bool>("isToggle");

        /// <summary>Applies pending property changes to the rendered component.</summary>
        public ComponentMethod DataBind => Method("dataBind");

        /// <summary>Clicks the button.</summary>
        public ComponentMethod Click => Method("click");

        /// <summary>Moves focus to the button.</summary>
        public ComponentMethod FocusIn => Method("focusIn");
    }

    /// <summary>
    /// Creates a FusionButton with Syncfusion MVC builder-owned initial render.
    /// </summary>
    public static class FusionButtonHtmlExtensions
    {
        /// <summary>
        /// Renders a non-input FusionButton with a stable component id.
        /// </summary>
        /// <param name="plan">Reactive Plan referencing this component.</param>
        /// <param name="elementId">Controlled DOM element ID used as the runtime join key.</param>
        /// <param name="build">Configures initial Syncfusion Button options.</param>
        /// <returns>Builder that renders the Syncfusion button and carries its Reactive Plan ID.</returns>
        public static RenderedComponent<global::Alis.Reactive.Fusion.Components.FusionButton, TModel> FusionButton<TModel>(
#if NET48
            this HtmlHelper<TModel> html,
#else
            this IHtmlHelper<TModel> html,
#endif
            ReactivePlan<TModel> plan,
            string elementId,
            Action<ButtonBuilder> build)
            where TModel : class
        {
            var builder = html.EJS().Button(elementId);
            build(builder);

            return plan.Render<global::Alis.Reactive.Fusion.Components.FusionButton>(elementId, builder.Render());
        }
    }
}
