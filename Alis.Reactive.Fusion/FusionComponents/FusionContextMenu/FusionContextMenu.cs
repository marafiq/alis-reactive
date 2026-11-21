using System.Collections.Generic;
using System.Text.Json.Serialization;
using Alis.Reactive.Declarations;
using Alis.Reactive.ValuesAndSources;

namespace Alis.Reactive.Fusion.FusionComponents;

/// <summary>
/// A context menu: EJ2's ContextMenu, a menu of items that opens where a person right-clicks or where a view opens it.
/// </summary>
/// <remarks>
/// An event's payload is read with <c>Read</c>. A handler of <see cref="BeforeOpen"/> or <see cref="BeforeClose"/>
/// writes true to the payload's <c>Cancel</c> to keep the menu as it is.
/// </remarks>
[Mirrors("@syncfusion/ej2-navigations", "ContextMenu")]
public sealed class FusionContextMenu : FusionComponent
{
    /// <summary>Opens the menu at a position on the page.</summary>
    /// <param name="top">The menu's distance from the top of the page, in pixels.</param>
    /// <param name="left">The menu's distance from the left of the page, in pixels.</param>
    public void Open(Arg<double> top, Arg<double> left) => Method([top, left]);

    /// <summary>Closes the menu when it is open.</summary>
    public void Close() => Method();

    /// <summary>Gets the event raised as the menu renders each of its items.</summary>
    public Event<MenuEventArgs> BeforeItemRender => Event<MenuEventArgs>();

    /// <summary>Gets the event raised before the menu or one of its submenus opens.</summary>
    public Event<BeforeOpenCloseMenuEventArgs> BeforeOpen => Event<BeforeOpenCloseMenuEventArgs>();

    /// <summary>Gets the event raised as the menu or one of its submenus opens.</summary>
    public Event<OpenCloseMenuEventArgs> OnOpen => Event<OpenCloseMenuEventArgs>();

    /// <summary>Gets the event raised before the menu or one of its submenus closes.</summary>
    public Event<BeforeOpenCloseMenuEventArgs> BeforeClose => Event<BeforeOpenCloseMenuEventArgs>();

    /// <summary>Gets the event raised as the menu or one of its submenus closes.</summary>
    public Event<OpenCloseMenuEventArgs> OnClose => Event<OpenCloseMenuEventArgs>();

    /// <summary>Gets the event raised when a person chooses an item of the menu.</summary>
    public Event<MenuEventArgs> Select => Event<MenuEventArgs>();

    /// <summary>
    /// What a listener of <see cref="BeforeItemRender"/> or <see cref="Select"/> receives.
    /// </summary>
    [Mirrors("@syncfusion/ej2-navigations", "MenuEventArgs")]
    public sealed class MenuEventArgs : JsObject
    {
        /// <summary>Gets the item that renders or that the person chose.</summary>
        public ReadOnly<MenuItemModel> Item => ReadOnly<MenuItemModel>();
    }

    /// <summary>
    /// What a listener of <see cref="BeforeOpen"/> or <see cref="BeforeClose"/> receives.
    /// </summary>
    [Mirrors("@syncfusion/ej2-navigations", "BeforeOpenCloseMenuEventArgs")]
    public sealed class BeforeOpenCloseMenuEventArgs : JsObject
    {
        /// <summary>Gets the items of the menu that opens or closes.</summary>
        public ReadOnly<IReadOnlyList<MenuItemModel>> Items => ReadOnly<IReadOnlyList<MenuItemModel>>();

        /// <summary>Gets the item whose submenu opens or closes.</summary>
        public ReadOnly<MenuItemModel> ParentItem => ReadOnly<MenuItemModel>();

        /// <summary>Gets the menu's distance from the top of the page, in pixels, which a handler of <see cref="BeforeOpen"/> writes to move it.</summary>
        public Property<double> Top => Property<double>(optional: true);

        /// <summary>Gets the menu's distance from the left of the page, in pixels, which a handler of <see cref="BeforeOpen"/> writes to move it.</summary>
        public Property<double> Left => Property<double>(optional: true);

        /// <summary>Gets whether the submenu's parent item takes the focus as the submenu closes; a close starts it true, an open never holds it.</summary>
        public Property<bool> IsFocused => Property<bool>(optional: true);

        /// <summary>Gets whether the menu stays as it is; write true to keep it.</summary>
        public Property<bool> Cancel => Property<bool>();
    }

    /// <summary>
    /// What a listener of <see cref="OnOpen"/> or <see cref="OnClose"/> receives.
    /// </summary>
    [Mirrors("@syncfusion/ej2-navigations", "OpenCloseMenuEventArgs")]
    public sealed class OpenCloseMenuEventArgs : JsObject
    {
        /// <summary>Gets the items of the menu that opened or closed.</summary>
        public ReadOnly<IReadOnlyList<MenuItemModel>> Items => ReadOnly<IReadOnlyList<MenuItemModel>>();

        /// <summary>Gets the item whose submenu opened or closed.</summary>
        public ReadOnly<MenuItemModel> ParentItem => ReadOnly<MenuItemModel>();
    }

    /// <summary>
    /// An item of the menu, as EJ2 holds it, each member named as EJ2 names it.
    /// </summary>
    [Mirrors("@syncfusion/ej2-navigations", "MenuItemModel")]
    public sealed class MenuItemModel
    {
        /// <summary>Gets or sets the item's id.</summary>
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        /// <summary>Gets or sets the item's text.</summary>
        [JsonPropertyName("text")]
        public string? Text { get; set; }

        /// <summary>Gets or sets the CSS classes of the item's icon.</summary>
        [JsonPropertyName("iconCss")]
        public string? IconCss { get; set; }

        /// <summary>Gets or sets the address the item links to.</summary>
        [JsonPropertyName("url")]
        public string? Url { get; set; }

        /// <summary>Gets or sets whether the item is a separator between items.</summary>
        [JsonPropertyName("separator")]
        public bool? Separator { get; set; }

        /// <summary>Gets or sets the items of the item's submenu.</summary>
        [JsonPropertyName("items")]
        public IReadOnlyList<MenuItemModel>? Items { get; set; }
    }
}
