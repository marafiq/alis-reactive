using System.Collections.Generic;
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
    /// The Syncfusion ContextMenu, declared by the names its JavaScript API uses.
    /// </summary>
    /// <remarks>
    /// Render it with <c>Html.FusionContextMenu(plan, "resident-actions", b =&gt; ...)</c>, then reach it in a callback
    /// and use its members directly:
    /// <code>
    /// var menu = p.Component&lt;FusionContextMenu&gt;("resident-actions");
    /// menu.Open(120d, 240d);
    /// </code>
    /// </remarks>
    public sealed class FusionContextMenu : FusionComponent
    {
        /// <summary>Opens the menu at a top and left position, in pixels.</summary>
        /// <param name="top">The menu's vertical position, in pixels.</param>
        /// <param name="left">The menu's horizontal position, in pixels.</param>
        public void Open(Arg<double> top, Arg<double> left) => Call(top, left);

        /// <summary>Closes the menu.</summary>
        public void Close() => Call();

        /// <summary>Raised before each item renders.</summary>
        public TypedEvent<FusionContextMenuBeforeItemRenderArgs> BeforeItemRender =>
            Event<FusionContextMenuBeforeItemRenderArgs>();

        /// <summary>Raised before a menu level opens.</summary>
        public TypedEvent<FusionContextMenuBeforeOpenArgs> BeforeOpen =>
            Event<FusionContextMenuBeforeOpenArgs>();

        /// <summary>Raised after a menu level opens.</summary>
        public TypedEvent<FusionContextMenuOpenCloseArgs> OnOpen =>
            Event<FusionContextMenuOpenCloseArgs>();

        /// <summary>Raised before a menu level closes.</summary>
        public TypedEvent<FusionContextMenuBeforeCloseArgs> BeforeClose =>
            Event<FusionContextMenuBeforeCloseArgs>();

        /// <summary>Raised after a menu level closes.</summary>
        public TypedEvent<FusionContextMenuOpenCloseArgs> OnClose =>
            Event<FusionContextMenuOpenCloseArgs>();

        /// <summary>Raised when the user selects an item.</summary>
        public TypedEvent<FusionContextMenuSelectArgs> Select =>
            Event<FusionContextMenuSelectArgs>();
    }

    /// <summary>Payload of <see cref="FusionContextMenu.BeforeItemRender"/>.</summary>
    public class FusionContextMenuBeforeItemRenderArgs : JsObject
    {
        /// <summary>The item about to render.</summary>
        public ReadOnly<FusionContextMenuItem> Item => ReadOnly<FusionContextMenuItem>();
    }

    /// <summary>Payload of <see cref="FusionContextMenu.BeforeOpen"/>.</summary>
    public class FusionContextMenuBeforeOpenArgs : JsObject
    {
        /// <summary>The items of the menu level involved.</summary>
        public ReadOnly<List<FusionContextMenuItem>> Items => ReadOnly<List<FusionContextMenuItem>>();

        /// <summary>The parent item of a submenu; <see langword="null"/> for the top level.</summary>
        public ReadOnly<FusionContextMenuItem> ParentItem => ReadOnly<FusionContextMenuItem>();

        /// <summary>Top position of the menu, in pixels.</summary>
        public ReadOnly<double> Top => ReadOnly<double>();

        /// <summary>Left position of the menu, in pixels.</summary>
        public ReadOnly<double> Left => ReadOnly<double>();

        /// <summary>Set to <see langword="true"/> with <c>args.Cancel.Set(true)</c> to stop the action.</summary>
        public Prop<bool> Cancel => Prop<bool>();
    }

    /// <summary>Payload of <see cref="FusionContextMenu.OnOpen"/> and <see cref="FusionContextMenu.OnClose"/>.</summary>
    public class FusionContextMenuOpenCloseArgs : JsObject
    {
        /// <summary>The items of the menu level involved.</summary>
        public ReadOnly<List<FusionContextMenuItem>> Items => ReadOnly<List<FusionContextMenuItem>>();

        /// <summary>The parent item of a submenu; <see langword="null"/> for the top level.</summary>
        public ReadOnly<FusionContextMenuItem> ParentItem => ReadOnly<FusionContextMenuItem>();
    }

    /// <summary>Payload of <see cref="FusionContextMenu.BeforeClose"/>.</summary>
    public class FusionContextMenuBeforeCloseArgs : JsObject
    {
        /// <summary>The items of the menu level involved.</summary>
        public ReadOnly<List<FusionContextMenuItem>> Items => ReadOnly<List<FusionContextMenuItem>>();

        /// <summary>The parent item of a submenu; <see langword="null"/> for the top level.</summary>
        public ReadOnly<FusionContextMenuItem> ParentItem => ReadOnly<FusionContextMenuItem>();

        /// <summary>Set to <see langword="true"/> with <c>args.Cancel.Set(true)</c> to stop the action.</summary>
        public Prop<bool> Cancel => Prop<bool>();
    }

    /// <summary>Payload of <see cref="FusionContextMenu.Select"/>.</summary>
    public class FusionContextMenuSelectArgs : JsObject
    {
        /// <summary>The selected item.</summary>
        public ReadOnly<FusionContextMenuItem> Item => ReadOnly<FusionContextMenuItem>();
    }

    /// <summary>A menu item as the <see cref="FusionContextMenu"/> payloads report it.</summary>
    public class FusionContextMenuItem
    {
        /// <summary>The item ID.</summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>The item text.</summary>
        public string Text { get; set; } = string.Empty;

        /// <summary>The item URL.</summary>
        public string Url { get; set; } = string.Empty;

        /// <summary>The item icon classes.</summary>
        public string IconCss { get; set; } = string.Empty;

        /// <summary>Whether the item is a separator.</summary>
        public bool Separator { get; set; }

        /// <summary>The item's submenu items.</summary>
        public List<FusionContextMenuItem> Items { get; set; } = [];
    }

    /// <summary>
    /// Renders the component for a plan, ready for <c>.Reactive(...)</c>.
    /// </summary>
    public static class FusionContextMenuHtmlExtensions
    {
        /// <summary>
        /// Creates a Syncfusion ContextMenu and carries its controlled component ID into the Reactive Plan.
        /// </summary>
        /// <typeparam name="TModel">The view's model type.</typeparam>
        /// <param name="html">The view's HTML helper.</param>
        /// <param name="plan">Reactive Plan receiving context menu event wiring.</param>
        /// <param name="elementId">Controlled component ID shared by markup and Reactive Plan behavior.</param>
        /// <param name="build">Configures the component before rendering.</param>
        public static RenderedComponent<global::Alis.Reactive.Fusion.Components.FusionContextMenu, TModel> FusionContextMenu<TModel>(
#if NET48
            this HtmlHelper<TModel> html,
#else
            this IHtmlHelper<TModel> html,
#endif
            ReactivePlan<TModel> plan,
            string elementId,
            Action<ContextMenuBuilder> build)
            where TModel : class
        {
            var builder = html.EJS().ContextMenu(elementId);
            build(builder);

            return plan.Render<global::Alis.Reactive.Fusion.Components.FusionContextMenu>(elementId, builder.Render());
        }
    }
}
