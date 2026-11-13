using System.Collections.Generic;
using System.Text.Json.Serialization;
using Alis.Reactive.Declarations;
using Alis.Reactive.ValuesAndSources;

namespace Alis.Reactive.Fusion.FusionComponents;

/// <summary>
/// A split button: EJ2's SplitButton, a primary button beside a list of actions it opens.
/// </summary>
/// <remarks>
/// EJ2 raises <see cref="Click"/> when a person clicks the primary button and <see cref="Select"/> when a person
/// chooses an action of the list.
/// </remarks>
[Mirrors("@syncfusion/ej2-splitbuttons", "SplitButton")]
public sealed class FusionSplitButton : FusionComponent
{
    /// <summary>Gets the text of the primary button.</summary>
    public Property<string> Content => Property<string>();

    /// <summary>Gets the CSS classes of the button's element, separated by spaces.</summary>
    public Property<string> CssClass => Property<string>();

    /// <summary>Gets whether the button is disabled.</summary>
    public Property<bool> Disabled => Property<bool>();

    /// <summary>Applies the changes written to the button's properties now.</summary>
    public void DataBind() => Method();

    /// <summary>Opens the button's list when it is closed, and closes it when it is open.</summary>
    public void Toggle() => Method();

    /// <summary>Moves the keyboard's focus to the button.</summary>
    public void FocusIn() => Method();

    /// <summary>Removes actions from the button's list.</summary>
    /// <param name="items">The texts of the actions to remove, or their ids.</param>
    /// <param name="isUniqueId">True when <paramref name="items"/> holds the actions' ids.</param>
    public void RemoveItems(Arg<IReadOnlyList<string>> items, Arg<bool> isUniqueId) => Method([items, isUniqueId]);

    /// <summary>Gets the event raised when a person clicks the primary button.</summary>
    public Event Click => Event();

    /// <summary>Gets the event raised when a person chooses an action of the button's list.</summary>
    public Event<MenuEventArgs> Select => Event<MenuEventArgs>();

    /// <summary>
    /// What a listener of <see cref="Select"/> receives.
    /// </summary>
    [Mirrors("@syncfusion/ej2-splitbuttons", "MenuEventArgs")]
    public sealed class MenuEventArgs : JsObject
    {
        /// <summary>Gets the action the person chose, read with <c>Read</c>.</summary>
        public ReadOnly<ItemModel> Item => ReadOnly<ItemModel>();
    }

    /// <summary>
    /// An action of the button's list, as EJ2 holds it.
    /// </summary>
    [Mirrors("@syncfusion/ej2-splitbuttons", "ItemModel")]
    public sealed class ItemModel
    {
        /// <summary>Gets or sets the action's id.</summary>
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        /// <summary>Gets or sets the action's text.</summary>
        [JsonPropertyName("text")]
        public string? Text { get; set; }

        /// <summary>Gets or sets the CSS classes of the action's icon.</summary>
        [JsonPropertyName("iconCss")]
        public string? IconCss { get; set; }

        /// <summary>Gets or sets the address the action opens.</summary>
        [JsonPropertyName("url")]
        public string? Url { get; set; }

        /// <summary>Gets or sets whether the item is a line between actions.</summary>
        [JsonPropertyName("separator")]
        public bool? Separator { get; set; }

        /// <summary>Gets or sets whether the action is disabled.</summary>
        [JsonPropertyName("disabled")]
        public bool? Disabled { get; set; }
    }
}
