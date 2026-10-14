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
    /// The Syncfusion SplitButton, declared by the names its JavaScript API uses.
    /// </summary>
    /// <remarks>
    /// Render it with <c>Html.FusionSplitButton(plan, "visit-actions", b =&gt; ...)</c>, reach it with
    /// <c>var visitActions = p.Component&lt;FusionSplitButton&gt;("visit-actions");</c> and use its members directly.
    /// Syncfusion applies property writes on its next tick; follow writes with <see cref="DataBind"/>:
    /// <c>visitActions.Content.Set("Saved"); visitActions.DataBind();</c>.
    /// </remarks>
    public sealed class FusionSplitButton : FusionComponent
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

        /// <summary>Removes items by their text, or by their IDs when <paramref name="isUniqueId"/> is <see langword="true"/>.</summary>
        /// <param name="items">The texts, or the IDs, of the items to remove.</param>
        /// <param name="isUniqueId"><see langword="true"/> when <paramref name="items"/> holds item IDs.</param>
        public void RemoveItems(Arg<string[]> items, Arg<bool> isUniqueId) => Call(items, isUniqueId);

        /// <summary>Raised when the user clicks the primary button.</summary>
        public TypedEvent<FusionSplitButtonClickArgs> Click =>
            Event<FusionSplitButtonClickArgs>();

        /// <summary>Raised when the user selects a popup item.</summary>
        public TypedEvent<FusionSplitButtonSelectArgs> Select =>
            Event<FusionSplitButtonSelectArgs>();
    }

    /// <summary>Payload of <see cref="FusionSplitButton.Click"/>; carries no data.</summary>
    public class FusionSplitButtonClickArgs : JsObject
    {
    }

    /// <summary>Payload of <see cref="FusionSplitButton.Select"/>.</summary>
    public class FusionSplitButtonSelectArgs : JsObject
    {
        /// <summary>The selected item.</summary>
        public ReadOnly<FusionSplitButtonItem> Item => ReadOnly<FusionSplitButtonItem>();
    }

    /// <summary>A popup item as the <see cref="FusionSplitButton.Select"/> payload reports it.</summary>
    public class FusionSplitButtonItem
    {
        /// <summary>The item ID.</summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>The item text.</summary>
        public string Text { get; set; } = string.Empty;

        /// <summary>Whether the item is disabled.</summary>
        public bool Disabled { get; set; }
    }

    /// <summary>
    /// Creates a FusionSplitButton with Syncfusion MVC builder-owned initial render.
    /// </summary>
    public static class FusionSplitButtonHtmlExtensions
    {
        /// <summary>
        /// Renders one Syncfusion SplitButton with a stable component id.
        /// </summary>
        /// <typeparam name="TModel">The view's model type.</typeparam>
        /// <param name="html">The view's HTML helper.</param>
        /// <param name="plan">The plan the component's events are wired into.</param>
        /// <param name="elementId">The component's element ID, chosen by the view.</param>
        /// <param name="build">Configures the Syncfusion builder before rendering.</param>
        public static RenderedComponent<global::Alis.Reactive.Fusion.Components.FusionSplitButton, TModel> FusionSplitButton<TModel>(
#if NET48
            this HtmlHelper<TModel> html,
#else
            this IHtmlHelper<TModel> html,
#endif
            ReactivePlan<TModel> plan,
            string elementId,
            Action<SplitButtonBuilder> build)
            where TModel : class
        {
            var builder = html.EJS().SplitButton(elementId);
            build(builder);

            return plan.Render<global::Alis.Reactive.Fusion.Components.FusionSplitButton>(elementId, builder.Render());
        }
    }
}
