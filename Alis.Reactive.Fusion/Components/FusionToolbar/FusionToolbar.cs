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
    /// The Syncfusion Toolbar, declared by the names its JavaScript API uses.
    /// </summary>
    /// <remarks>
    /// Render it with <c>Html.FusionToolbar(plan, "care-toolbar", b =&gt; ...)</c> and target it with
    /// <c>p.Component&lt;FusionToolbar&gt;("care-toolbar")</c>; every declared member works with <c>Set</c>,
    /// <c>Call</c> and <c>Get</c>, for example <c>.Call(c =&gt; c.Disable, true)</c>.
    /// </remarks>
    public sealed class FusionToolbar : FusionComponent
    {
        /// <summary>Disables (<see langword="true"/>) or enables (<see langword="false"/>) the whole toolbar.</summary>
        public void Disable(Arg<bool> value) => Call(value);

        /// <summary>Raised when the user clicks a toolbar item.</summary>
        public TypedEvent<FusionToolbarClickedArgs> Clicked =>
            Event<FusionToolbarClickedArgs>("clicked");
    }

    /// <summary>Payload of <see cref="FusionToolbar.Clicked"/>.</summary>
    /// <remarks>Read the clicked item: <c>p.When(args, x =&gt; x.Item.Id).Eq("save")</c>.</remarks>
    public class FusionToolbarClickedArgs : JsObject
    {
        /// <summary>The clicked item.</summary>
        public ReadOnly<FusionToolbarItem> Item => ReadOnly<FusionToolbarItem>();
    }
#nullable restore annotations

    /// <summary>A toolbar item as the <see cref="FusionToolbar.Clicked"/> payload reports it.</summary>
    public class FusionToolbarItem
    {
        /// <summary>The item ID.</summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>The item text.</summary>
        public string Text { get; set; } = string.Empty;

        /// <summary>Whether the item is disabled.</summary>
        public bool Disabled { get; set; }
    }

    /// <summary>
    /// Renders the component for a plan, ready for <c>.Reactive(...)</c>.
    /// </summary>
    public static class FusionToolbarHtmlExtensions
    {
        /// <summary>
        /// Creates a Syncfusion Toolbar and carries its controlled component ID into the Reactive Plan.
        /// </summary>
        /// <param name="plan">Reactive Plan receiving toolbar event wiring.</param>
        /// <param name="elementId">Controlled component ID shared by markup and Reactive Plan behavior.</param>
        /// <param name="build">Configures the component before rendering.</param>
        public static RenderedComponent<global::Alis.Reactive.Fusion.Components.FusionToolbar, TModel> FusionToolbar<TModel>(
#if NET48
            this HtmlHelper<TModel> html,
#else
            this IHtmlHelper<TModel> html,
#endif
            ReactivePlan<TModel> plan,
            string elementId,
            Action<ToolbarBuilder> build)
            where TModel : class
        {
            var builder = html.EJS().Toolbar(elementId);
            build(builder);

            return plan.Render<global::Alis.Reactive.Fusion.Components.FusionToolbar>(elementId, builder.Render());
        }
    }
}
