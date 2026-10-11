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
    /// Render it with <c>Html.FusionSplitButton(plan, "visit-actions", b =&gt; ...)</c> and target it with
    /// <c>p.Component&lt;FusionSplitButton&gt;("visit-actions")</c>; every declared member works with <c>Set</c>,
    /// <c>Call</c> and <c>Get</c>. Syncfusion applies property writes on its next tick; follow writes with
    /// <c>.Call(c =&gt; c.DataBind)</c>: <c>.Set(c =&gt; c.Content, "Saved").Call(c =&gt; c.DataBind)</c>.
    /// </remarks>
    public sealed class FusionSplitButton : FusionComponent
    {
        /// <summary>The button text.</summary>
        public ComponentProperty<string> Content => Property<string>("content");

        /// <summary>Whether the button is disabled.</summary>
        public ComponentProperty<bool> Disabled => Property<bool>("disabled");

        /// <summary>The CSS classes on the button.</summary>
        public ComponentProperty<string> CssClass => Property<string>("cssClass");

        /// <summary>Applies pending property changes to the rendered component.</summary>
        public ComponentMethod DataBind => Method("dataBind");

        /// <summary>Opens or closes the popup.</summary>
        public ComponentMethod Toggle => Method("toggle");

        /// <summary>Moves focus to the button.</summary>
        public ComponentMethod FocusIn => Method("focusIn");

        /// <summary>Removes items by their text, or by their IDs when the second argument is <see langword="true"/>.</summary>
        public ComponentMethod<string[], bool> RemoveItems => Method<string[], bool>("removeItems");

        /// <summary>Raised when the user clicks the primary button.</summary>
        public TypedEvent<FusionSplitButtonClickArgs> Click =>
            Event<FusionSplitButtonClickArgs>("click");

        /// <summary>Raised when the user selects a popup item.</summary>
        public TypedEvent<FusionSplitButtonSelectArgs> Select =>
            Event<FusionSplitButtonSelectArgs>("select");
    }

    /// <summary>Payload of <see cref="FusionSplitButton.Click"/>; carries no data.</summary>
    public class FusionSplitButtonClickArgs
    {
    }

    /// <summary>Payload of <see cref="FusionSplitButton.Select"/>.</summary>
    public class FusionSplitButtonSelectArgs
    {
        /// <summary>The selected item.</summary>
        public FusionSplitButtonItem Item { get; set; } = new FusionSplitButtonItem();
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
