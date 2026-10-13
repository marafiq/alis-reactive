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
    /// Render it with <c>Html.FusionButton(plan, "begin-check-in", b =&gt; ...)</c>, then reach it in a callback
    /// and use its members directly. Syncfusion applies property writes on its next tick; follow writes with
    /// <c>DataBind()</c>:
    /// <code>
    /// var checkIn = p.Component&lt;FusionButton&gt;("begin-check-in");
    /// checkIn.Content.Set("Saved");
    /// checkIn.DataBind();
    /// </code>
    /// </remarks>
    public sealed class FusionButton : FusionComponent
    {
        /// <summary>The button text.</summary>
        public Prop<string> Content => Prop<string>();

        /// <summary>Whether the button is disabled.</summary>
        public Prop<bool> Disabled => Prop<bool>();

        /// <summary>The icon CSS classes.</summary>
        public Prop<string> IconCss => Prop<string>();

        /// <summary>Where the icon sits relative to the text.</summary>
        public Prop<FusionButtonIconPosition> IconPosition => Prop<FusionButtonIconPosition>();

        /// <summary>The CSS classes on the button.</summary>
        public Prop<string> CssClass => Prop<string>();

        /// <summary>Whether the button has the primary style.</summary>
        public Prop<bool> IsPrimary => Prop<bool>();

        /// <summary>Whether the button toggles its active state when clicked.</summary>
        public Prop<bool> IsToggle => Prop<bool>();

        /// <summary>Applies pending property changes to the rendered component.</summary>
        public void DataBind() => Call();

        /// <summary>Clicks the button.</summary>
        public void Click() => Call();

        /// <summary>Moves focus to the button.</summary>
        public void FocusIn() => Call();
    }

    /// <summary>
    /// Creates a FusionButton with Syncfusion MVC builder-owned initial render.
    /// </summary>
    public static class FusionButtonHtmlExtensions
    {
        /// <summary>
        /// Renders a non-input FusionButton with a stable component id.
        /// </summary>
        /// <typeparam name="TModel">The view's model type.</typeparam>
        /// <param name="html">The view's HTML helper.</param>
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
