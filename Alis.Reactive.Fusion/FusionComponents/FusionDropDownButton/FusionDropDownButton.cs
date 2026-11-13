using System.Collections.Generic;
using System.Text.Json.Serialization;
using Alis.Reactive.Declarations;
using Alis.Reactive.ValuesAndSources;

namespace Alis.Reactive.Fusion.FusionComponents;

/// <summary>
/// A drop-down button: EJ2's DropDownButton, a button that opens a list of items.
/// </summary>
/// <remarks>
/// EJ2 raises <see cref="Select"/> when a person chooses an item; a handler of it reads the item's members with
/// <c>Read</c>. Call <see cref="DataBind"/> after writing the button's properties, so it shows them now.
/// </remarks>
[Mirrors("@syncfusion/ej2-splitbuttons", "DropDownButton")]
public sealed class FusionDropDownButton : FusionComponent
{
    /// <summary>Gets the text the button shows.</summary>
    public Property<string> Content => Property<string>();

    /// <summary>Gets the CSS classes of the button's element.</summary>
    public Property<string> CssClass => Property<string>();

    /// <summary>Gets whether the button is disabled.</summary>
    public Property<bool> Disabled => Property<bool>();

    /// <summary>Applies the changes written to the button's properties now.</summary>
    public void DataBind() => Method();

    /// <summary>Opens the button's list when it is closed, and closes it when it is open.</summary>
    public void Toggle() => Method();

    /// <summary>Moves the keyboard's focus to the button.</summary>
    public void FocusIn() => Method();

    /// <summary>Removes items from the button's list, found by their text or by their ids.</summary>
    /// <param name="items">The texts of the items, or their ids when <paramref name="isUniqueId"/> holds.</param>
    /// <param name="isUniqueId">True to find the items by their ids, false to find them by their text.</param>
    public void RemoveItems(Arg<IReadOnlyList<string>> items, Arg<bool> isUniqueId) => Method([items, isUniqueId]);

    /// <summary>Gets the event raised when a person chooses an item of the button's list.</summary>
    public Event<MenuEventArgs> Select => Event<MenuEventArgs>();

    /// <summary>
    /// What a listener of <see cref="Select"/> receives.
    /// </summary>
    [Mirrors("@syncfusion/ej2-splitbuttons", "MenuEventArgs")]
    public sealed class MenuEventArgs : JsObject
    {
        /// <summary>Gets the item the person chose, read with <c>Read</c>.</summary>
        public ReadOnly<ItemModel> Item => ReadOnly<ItemModel>();
    }

    /// <summary>
    /// An item of the button's list, each member named as EJ2 names it.
    /// </summary>
    [Mirrors("@syncfusion/ej2-splitbuttons", "ItemModel")]
    public sealed class ItemModel
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

        /// <summary>Gets or sets whether the item is disabled.</summary>
        [JsonPropertyName("disabled")]
        public bool? Disabled { get; set; }
    }
}
