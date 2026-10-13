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
#nullable disable annotations
    /// <summary>
    /// The Syncfusion ContextMenu, declared by the names its JavaScript API uses.
    /// </summary>
    /// <remarks>
    /// Render it with <c>Html.FusionContextMenu(plan, "resident-actions", b =&gt; ...)</c> and target it with
    /// <c>p.Component&lt;FusionContextMenu&gt;("resident-actions")</c>; every declared member works with <c>Set</c>,
    /// <c>Call</c> and <c>Get</c>, for example <c>.Call(c =&gt; c.Open, 120d, 240d)</c>.
    /// </remarks>
    public sealed class FusionContextMenu : FusionComponent
    {
        /// <summary>Opens the menu at a top and left position, in pixels.</summary>
        public void Open(Arg<double> first, Arg<double> second) => Call(first, second);

        /// <summary>Closes the menu.</summary>
        public void Close() => Call();

        /// <summary>Raised before each item renders.</summary>
        public TypedEvent<FusionContextMenuBeforeItemRenderArgs> BeforeItemRender =>
            Event<FusionContextMenuBeforeItemRenderArgs>("beforeItemRender");

        /// <summary>Raised before a menu level opens.</summary>
        public TypedEvent<FusionContextMenuBeforeOpenArgs> BeforeOpen =>
            Event<FusionContextMenuBeforeOpenArgs>("beforeOpen");

        /// <summary>Raised after a menu level opens.</summary>
        public TypedEvent<FusionContextMenuOpenCloseArgs> OnOpen =>
            Event<FusionContextMenuOpenCloseArgs>("onOpen");

        /// <summary>Raised before a menu level closes.</summary>
        public TypedEvent<FusionContextMenuBeforeCloseArgs> BeforeClose =>
            Event<FusionContextMenuBeforeCloseArgs>("beforeClose");

        /// <summary>Raised after a menu level closes.</summary>
        public TypedEvent<FusionContextMenuOpenCloseArgs> OnClose =>
            Event<FusionContextMenuOpenCloseArgs>("onClose");

        /// <summary>Raised when the user selects an item.</summary>
        public TypedEvent<FusionContextMenuSelectArgs> Select =>
            Event<FusionContextMenuSelectArgs>("select");
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

        /// <summary>Whether the menu opens with focus.</summary>
        public ReadOnly<bool?> IsFocused => ReadOnly<bool?>();

        /// <summary>Set to <see langword="true"/> with <c>p.Payload(args).Set(a =&gt; a.Cancel, true)</c> to stop the action.</summary>
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

        /// <summary>Set to <see langword="true"/> with <c>p.Payload(args).Set(a =&gt; a.Cancel, true)</c> to stop the action.</summary>
        public Prop<bool> Cancel => Prop<bool>();
    }

    /// <summary>Payload of <see cref="FusionContextMenu.Select"/>.</summary>
    public class FusionContextMenuSelectArgs : JsObject
    {
        /// <summary>The selected item.</summary>
        public ReadOnly<FusionContextMenuItem> Item => ReadOnly<FusionContextMenuItem>();
    }
#nullable restore annotations

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
