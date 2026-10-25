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
    /// The Syncfusion Menu, declared by the names its JavaScript API uses.
    /// </summary>
    /// <remarks>
    /// Render it with <c>Html.FusionMenu(plan, "care-menu", b =&gt; ...)</c>, reach it with
    /// <c>var menu = p.Component&lt;FusionMenu&gt;("care-menu");</c> and use its members directly:
    /// <c>menu.Open();</c>.
    /// </remarks>
    public sealed class FusionMenu : FusionComponent
    {
        /// <summary>Opens the menu (a hamburger menu).</summary>
        public void Open() => Method();

        /// <summary>Closes the menu (a hamburger menu).</summary>
        public void Close() => Method();

        /// <summary>Raised before each item renders.</summary>
        public Event<FusionMenuBeforeItemRenderArgs> BeforeItemRender =>
            Event<FusionMenuBeforeItemRenderArgs>();

        /// <summary>Raised before a menu level opens.</summary>
        public Event<FusionMenuBeforeOpenArgs> BeforeOpen =>
            Event<FusionMenuBeforeOpenArgs>();

        /// <summary>Raised after a menu level opens.</summary>
        public Event<FusionMenuOpenCloseArgs> OnOpen =>
            Event<FusionMenuOpenCloseArgs>();

        /// <summary>Raised before a menu level closes.</summary>
        public Event<FusionMenuBeforeCloseArgs> BeforeClose =>
            Event<FusionMenuBeforeCloseArgs>();

        /// <summary>Raised after a menu level closes.</summary>
        public Event<FusionMenuOpenCloseArgs> OnClose =>
            Event<FusionMenuOpenCloseArgs>();

        /// <summary>Raised when the user selects an item.</summary>
        public Event<FusionMenuSelectArgs> Select =>
            Event<FusionMenuSelectArgs>();
    }

    /// <summary>Payload of <see cref="FusionMenu.BeforeItemRender"/>.</summary>
    public class FusionMenuBeforeItemRenderArgs : JsObject
    {
        /// <summary>The item about to render.</summary>
        public ReadOnly<FusionMenuItem> Item => ReadOnly<FusionMenuItem>();
    }

    /// <summary>Payload of <see cref="FusionMenu.BeforeOpen"/>.</summary>
    public class FusionMenuBeforeOpenArgs : JsObject
    {
        /// <summary>The items of the menu level involved.</summary>
        public ReadOnly<List<FusionMenuItem>> Items => ReadOnly<List<FusionMenuItem>>();

        /// <summary>The parent item of a submenu; <see langword="null"/> for the top level.</summary>
        public ReadOnly<FusionMenuItem> ParentItem => ReadOnly<FusionMenuItem>();

        /// <summary>Top position of the menu, in pixels.</summary>
        public ReadOnly<double> Top => ReadOnly<double>();

        /// <summary>Left position of the menu, in pixels.</summary>
        public ReadOnly<double> Left => ReadOnly<double>();

        /// <summary>Set to <see langword="true"/> with <c>args.Cancel.Set(true)</c> to stop the action.</summary>
        public Property<bool> Cancel => Property<bool>();
    }

    /// <summary>Payload of <see cref="FusionMenu.OnOpen"/> and <see cref="FusionMenu.OnClose"/>.</summary>
    public class FusionMenuOpenCloseArgs : JsObject
    {
        /// <summary>The items of the menu level involved.</summary>
        public ReadOnly<List<FusionMenuItem>> Items => ReadOnly<List<FusionMenuItem>>();

        /// <summary>The parent item of a submenu; <see langword="null"/> for the top level.</summary>
        public ReadOnly<FusionMenuItem> ParentItem => ReadOnly<FusionMenuItem>();
    }

    /// <summary>Payload of <see cref="FusionMenu.BeforeClose"/>.</summary>
    public class FusionMenuBeforeCloseArgs : JsObject
    {
        /// <summary>The items of the menu level involved.</summary>
        public ReadOnly<List<FusionMenuItem>> Items => ReadOnly<List<FusionMenuItem>>();

        /// <summary>The parent item of a submenu; <see langword="null"/> for the top level.</summary>
        public ReadOnly<FusionMenuItem> ParentItem => ReadOnly<FusionMenuItem>();

        /// <summary>Set to <see langword="true"/> with <c>args.Cancel.Set(true)</c> to stop the action.</summary>
        public Property<bool> Cancel => Property<bool>();
    }

    /// <summary>Payload of <see cref="FusionMenu.Select"/>.</summary>
    public class FusionMenuSelectArgs : JsObject
    {
        /// <summary>The selected item.</summary>
        public ReadOnly<FusionMenuItem> Item => ReadOnly<FusionMenuItem>();
    }

    /// <summary>A menu item as the <see cref="FusionMenu"/> payloads report it.</summary>
    public class FusionMenuItem
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
        public List<FusionMenuItem> Items { get; set; } = [];
    }

    /// <summary>
    /// Renders the component for a plan, ready for <c>.Reactive(...)</c>.
    /// </summary>
    public static class FusionMenuHtmlExtensions
    {
        /// <summary>
        /// Creates a Syncfusion Menu and carries its controlled component ID into the Reactive Plan.
        /// </summary>
        /// <typeparam name="TModel">The view's model type.</typeparam>
        /// <param name="html">The view's HTML helper.</param>
        /// <param name="plan">Reactive Plan receiving menu event wiring.</param>
        /// <param name="elementId">Controlled component ID shared by markup and Reactive Plan behavior.</param>
        /// <param name="build">Configures the component before rendering.</param>
        public static RenderedComponent<global::Alis.Reactive.Fusion.Components.FusionMenu, TModel> FusionMenu<TModel>(
#if NET48
            this HtmlHelper<TModel> html,
#else
            this IHtmlHelper<TModel> html,
#endif
            ReactivePlan<TModel> plan,
            string elementId,
            Action<MenuBuilder> build)
            where TModel : class
        {
            var builder = html.EJS().Menu(elementId);
            build(builder);

            return plan.Render<global::Alis.Reactive.Fusion.Components.FusionMenu>(elementId, builder.Render());
        }
    }
}
