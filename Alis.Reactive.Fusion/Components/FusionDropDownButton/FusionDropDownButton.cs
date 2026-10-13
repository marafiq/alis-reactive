using System;
#if NET48
using System.Web.Mvc;
#else
using Microsoft.AspNetCore.Mvc.Rendering;
#endif
using Syncfusion.EJ2;
using Syncfusion.EJ2.SplitButtons;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// The Syncfusion DropDownButton, declared by the names its JavaScript API uses.
    /// </summary>
    /// <remarks>
    /// Render it with <c>Html.FusionDropDownButton(plan, "care-actions", b =&gt; ...)</c>, then reach it in a callback
    /// and use its members directly. Syncfusion applies property writes on its next tick; follow writes with
    /// <c>DataBind()</c>:
    /// <code>
    /// var actions = p.Component&lt;FusionDropDownButton&gt;("care-actions");
    /// actions.Content.Set("Saved");
    /// actions.DataBind();
    /// </code>
    /// </remarks>
    public sealed class FusionDropDownButton : FusionComponent
    {
        /// <summary>The button text.</summary>
        public Prop<string> Content => Prop<string>();

        /// <summary>Whether the button is disabled.</summary>
        public Prop<bool> Disabled => Prop<bool>();

        /// <summary>The CSS classes on the button.</summary>
        public Prop<string> CssClass => Prop<string>();

        /// <summary>Applies pending property changes to the rendered component.</summary>
        public void DataBind() => Call();

        /// <summary>Opens or closes the popup.</summary>
        public void Toggle() => Call();

        /// <summary>Moves focus to the button.</summary>
        public void FocusIn() => Call();

        /// <summary>Removes items by their text, or by their IDs when the second argument is <see langword="true"/>.</summary>
        /// <param name="items">The texts of the items to remove, or their IDs when <paramref name="isUniqueId"/> is <see langword="true"/>.</param>
        /// <param name="isUniqueId"><see langword="true"/> when <paramref name="items"/> holds item IDs rather than texts.</param>
        public void RemoveItems(Arg<string[]> items, Arg<bool> isUniqueId) => Call(items, isUniqueId);

        /// <summary>Raised when the user selects a popup item.</summary>
        public TypedEvent<FusionDropDownButtonSelectArgs> Select =>
            Event<FusionDropDownButtonSelectArgs>();
    }

    /// <summary>Payload of <see cref="FusionDropDownButton.Select"/>.</summary>
    public class FusionDropDownButtonSelectArgs : JsObject
    {
        /// <summary>The selected item.</summary>
        public ReadOnly<FusionDropDownButtonItem> Item => ReadOnly<FusionDropDownButtonItem>();
    }

    /// <summary>A popup item as the <see cref="FusionDropDownButton.Select"/> payload reports it.</summary>
    public class FusionDropDownButtonItem
    {
        /// <summary>The item ID.</summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>The item text.</summary>
        public string Text { get; set; } = string.Empty;

        /// <summary>Whether the item is disabled.</summary>
        public bool Disabled { get; set; }
    }

    /// <summary>
    /// Creates a FusionDropDownButton with Syncfusion MVC builder-owned initial render.
    /// </summary>
    public static class FusionDropDownButtonHtmlExtensions
    {
        /// <summary>
        /// Renders one Syncfusion DropDownButton with a stable component id.
        /// </summary>
        /// <typeparam name="TModel">The view's model type.</typeparam>
        /// <param name="html">The view's HTML helper.</param>
        /// <param name="plan">Reactive Plan receiving drop-down button event wiring.</param>
        /// <param name="elementId">Controlled component ID shared by markup and Reactive Plan behavior.</param>
        /// <param name="build">Configures the component before rendering.</param>
        public static RenderedComponent<global::Alis.Reactive.Fusion.Components.FusionDropDownButton, TModel> FusionDropDownButton<TModel>(
#if NET48
            this HtmlHelper<TModel> html,
#else
            this IHtmlHelper<TModel> html,
#endif
            ReactivePlan<TModel> plan,
            string elementId,
            Action<DropDownButtonBuilder> build)
            where TModel : class
        {
            var builder = html.EJS().DropDownButton(elementId);
            build(builder);

            return plan.Render<global::Alis.Reactive.Fusion.Components.FusionDropDownButton>(elementId, builder.Render());
        }
    }
}
