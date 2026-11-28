using System.Text.Json.Serialization;
using Alis.Reactive.Declarations;
using Alis.Reactive.ValuesAndSources;

namespace Alis.Reactive.Fusion.FusionComponents;

/// <summary>
/// A toolbar: EJ2's Toolbar, a row of command items.
/// </summary>
/// <remarks>EJ2 raises <see cref="Clicked"/> when a person clicks an item of the toolbar.</remarks>
[Mirrors("@syncfusion/ej2-navigations", "Toolbar")]
public sealed class FusionToolbar : FusionComponent
{
    /// <summary>Disables every item of the toolbar, or enables them.</summary>
    /// <param name="value">True to disable the items, false to enable them.</param>
    public void Disable(Arg<bool> value) => Method([value]);

    /// <summary>Gets the event raised when a person clicks an item of the toolbar.</summary>
    public Event<ClickEventArgs> Clicked => Event<ClickEventArgs>();

    /// <summary>
    /// What a listener of <see cref="Clicked"/> receives.
    /// </summary>
    [Mirrors("@syncfusion/ej2-navigations", "ClickEventArgs")]
    public sealed class ClickEventArgs : JsObject
    {
        /// <summary>Gets the item the person clicked, read with <c>Read</c>.</summary>
        public ReadOnly<ItemModel> Item => ReadOnly<ItemModel>();
    }

    /// <summary>
    /// An item of the toolbar, as EJ2 holds it.
    /// </summary>
    [Mirrors("@syncfusion/ej2-navigations", "ItemModel")]
    public sealed class ItemModel
    {
        /// <summary>Gets or sets the item's id.</summary>
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        /// <summary>Gets or sets the item's text.</summary>
        [JsonPropertyName("text")]
        public string? Text { get; set; }

        /// <summary>Gets or sets whether the item is disabled.</summary>
        [JsonPropertyName("disabled")]
        public bool? Disabled { get; set; }
    }
}
