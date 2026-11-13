using System.Collections.Generic;
using System.Text.Json.Serialization;
using Alis.Reactive.Declarations;
using Alis.Reactive.ValuesAndSources;

namespace Alis.Reactive.Fusion.FusionComponents;

/// <summary>
/// A menu: EJ2's Menu, a bar of items whose submenus open beneath them.
/// </summary>
/// <remarks>
/// EJ2 raises <see cref="BeforeOpen"/> and <see cref="BeforeClose"/> before a submenu opens or closes, and a handler
/// writes true to the payload's <c>Cancel</c> to keep the submenu as it is.
/// </remarks>
[Mirrors("@syncfusion/ej2-navigations", "Menu")]
public sealed class FusionMenu : FusionComponent
{
    /// <summary>Opens the menu, when it shows in hamburger mode.</summary>
    public void Open() => Method();

    /// <summary>Closes the menu, when it shows in hamburger mode and is open.</summary>
    public void Close() => Method();

    /// <summary>Gets the event raised as each item of the menu renders.</summary>
    public Event<MenuEventArgs> BeforeItemRender => Event<MenuEventArgs>();

    /// <summary>Gets the event raised before a submenu opens.</summary>
    public Event<BeforeOpenCloseMenuEventArgs> BeforeOpen => Event<BeforeOpenCloseMenuEventArgs>();

    /// <summary>Gets the event raised when a submenu opens.</summary>
    public Event<OpenCloseMenuEventArgs> OnOpen => Event<OpenCloseMenuEventArgs>();

    /// <summary>Gets the event raised before a submenu closes.</summary>
    public Event<BeforeOpenCloseMenuEventArgs> BeforeClose => Event<BeforeOpenCloseMenuEventArgs>();

    /// <summary>Gets the event raised when a submenu closes.</summary>
    public Event<OpenCloseMenuEventArgs> OnClose => Event<OpenCloseMenuEventArgs>();

    /// <summary>Gets the event raised when a person chooses an item of the menu.</summary>
    public Event<MenuEventArgs> Select => Event<MenuEventArgs>();

    /// <summary>
    /// What a listener of <see cref="BeforeItemRender"/> or <see cref="Select"/> receives.
    /// </summary>
    [Mirrors("@syncfusion/ej2-navigations", "MenuEventArgs")]
    public sealed class MenuEventArgs : JsObject
    {
        /// <summary>Gets the item that renders or that the person chose, read with <c>Read</c>.</summary>
        public ReadOnly<MenuItemModel> Item => ReadOnly<MenuItemModel>();
    }

    /// <summary>
    /// What a listener of <see cref="BeforeOpen"/> or <see cref="BeforeClose"/> receives.
    /// </summary>
    [Mirrors("@syncfusion/ej2-navigations", "BeforeOpenCloseMenuEventArgs")]
    public sealed class BeforeOpenCloseMenuEventArgs : JsObject
    {
        /// <summary>Gets the items of the submenu.</summary>
        public ReadOnly<IReadOnlyList<MenuItemModel>> Items => ReadOnly<IReadOnlyList<MenuItemModel>>();

        /// <summary>Gets the item whose submenu opens or closes.</summary>
        public ReadOnly<MenuItemModel> ParentItem => ReadOnly<MenuItemModel>();

        /// <summary>Gets the position in pixels of the submenu's top edge, which a handler of <see cref="BeforeOpen"/> writes to move it.</summary>
        public Property<double> Top => Property<double>(optional: true);

        /// <summary>Gets the position in pixels of the submenu's left edge, which a handler of <see cref="BeforeOpen"/> writes to move it.</summary>
        public Property<double> Left => Property<double>(optional: true);

        /// <summary>Gets whether the parent item takes the focus once the submenu closes, true on <see cref="BeforeClose"/>; write false to leave the focus where it is.</summary>
        public Property<bool> IsFocused => Property<bool>(optional: true);

        /// <summary>Gets whether the submenu stays as it is; write true to keep it.</summary>
        public Property<bool> Cancel => Property<bool>();
    }

    /// <summary>
    /// What a listener of <see cref="OnOpen"/> or <see cref="OnClose"/> receives.
    /// </summary>
    [Mirrors("@syncfusion/ej2-navigations", "OpenCloseMenuEventArgs")]
    public sealed class OpenCloseMenuEventArgs : JsObject
    {
        /// <summary>Gets the items of the submenu.</summary>
        public ReadOnly<IReadOnlyList<MenuItemModel>> Items => ReadOnly<IReadOnlyList<MenuItemModel>>();

        /// <summary>Gets the item whose submenu opened or closed.</summary>
        public ReadOnly<MenuItemModel> ParentItem => ReadOnly<MenuItemModel>();
    }

    /// <summary>
    /// An item of the menu, as EJ2 holds it.
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

        /// <summary>Gets or sets the address the item opens.</summary>
        [JsonPropertyName("url")]
        public string? Url { get; set; }

        /// <summary>Gets or sets the CSS classes of the item's icon.</summary>
        [JsonPropertyName("iconCss")]
        public string? IconCss { get; set; }

        /// <summary>Gets or sets whether the item is a line between items.</summary>
        [JsonPropertyName("separator")]
        public bool? Separator { get; set; }

        /// <summary>Gets or sets the items of the item's submenu.</summary>
        [JsonPropertyName("items")]
        public IReadOnlyList<MenuItemModel>? Items { get; set; }
    }
}
