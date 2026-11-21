using System.Collections.Generic;
using System.Text.Json.Serialization;
using Alis.Reactive.Declarations;
using Alis.Reactive.ValuesAndSources;

namespace Alis.Reactive.Fusion.FusionComponents;

/// <summary>
/// A chip list: EJ2's ChipList, a row of chips a person clicks, chooses or deletes.
/// </summary>
/// <remarks>
/// A list that lets a person choose one chip reads it with <see cref="GetSelectedChip{TData}"/>, and one that lets a
/// person choose several reads them with <see cref="GetSelectedChips{TData}"/>. <c>TData</c> is the chips' own data:
/// the app's class for chips with values, text for chips with text alone.
/// </remarks>
[Mirrors("@syncfusion/ej2-buttons", "ChipList")]
public sealed class FusionChipList : FusionComponent
{
    /// <summary>Adds a chip with a text to the end of the list.</summary>
    /// <param name="text">The new chip's text.</param>
    public void Add(Arg<string> text) => Method([text]);

    /// <summary>Chooses the chip at a position.</summary>
    /// <param name="index">The chip's position, from 0.</param>
    public void Select(Arg<int> index) => Method([index]);

    /// <summary>Chooses the chips at positions.</summary>
    /// <param name="indexes">The chips' positions, from 0.</param>
    public void Select(Arg<IReadOnlyList<int>> indexes) => Method([indexes]);

    /// <summary>Chooses the chips with values or texts.</summary>
    /// <param name="values">The chips' values or texts.</param>
    /// <param name="selectionType">Whether <paramref name="values"/> holds the chips' values or their texts.</param>
    public void Select(Arg<IReadOnlyList<string>> values, Arg<SelectionType> selectionType) => Method([values, selectionType]);

    /// <summary>Removes the chip at a position.</summary>
    /// <param name="index">The chip's position, from 0.</param>
    public void Remove(Arg<int> index) => Method([index]);

    /// <summary>Removes the chips at positions.</summary>
    /// <param name="indexes">The chips' positions, from 0.</param>
    public void Remove(Arg<IReadOnlyList<int>> indexes) => Method([indexes]);

    /// <summary>Reads the chip at a position.</summary>
    /// <param name="index">The chip's position, from 0.</param>
    /// <returns>The chip.</returns>
    public MethodResult<ChipDataArgs> Find(Arg<int> index) => Method<ChipDataArgs>([index]);

    /// <summary>Reads the chips chosen in a list that lets a person choose several.</summary>
    /// <typeparam name="TData">The chips' own data: the app's class for chips with values, text for chips with text alone.</typeparam>
    /// <returns>The chosen chips.</returns>
    public MethodResult<SelectedItems<TData>> GetSelectedChips<TData>() => Method<SelectedItems<TData>>();

    /// <summary>Reads the chip chosen in a list that lets a person choose one.</summary>
    /// <typeparam name="TData">The chips' own data: the app's class for chips with values, text for chips with text alone.</typeparam>
    /// <returns>The chosen chip.</returns>
    public MethodResult<SelectedItem<TData>> GetSelectedChip<TData>() => Method<SelectedItem<TData>>(name: "getSelectedChips");

    /// <summary>Applies the changes written to the list's properties now.</summary>
    public void DataBind() => Method();

    /// <summary>Gets the chosen chips, as EJ2 holds them.</summary>
    /// <remarks>
    /// EJ2 holds a chip's position, <c>int</c>, in a list of text chips that lets a person choose one, and a list of
    /// positions, <c>int[]</c>, or of values, <c>string[]</c>, in a list that lets a person choose several. Call
    /// <see cref="DataBind"/> after a write, so the list chooses the chips.
    /// </remarks>
    /// <typeparam name="TValue">The kind EJ2 holds for the list.</typeparam>
    /// <returns>The chosen chips.</returns>
    public Property<TValue> SelectedChips<TValue>() => Property<TValue>();

    /// <summary>Gets the event raised when a person clicks a chip.</summary>
    public Event<ClickEventArgs> Click => Event<ClickEventArgs>();

    /// <summary>Gets the event raised after a chip is removed from the list.</summary>
    public Event<ChipDeletedEventArgs> Deleted => Event<ChipDeletedEventArgs>();

    /// <summary>
    /// What <see cref="Select(Arg{IReadOnlyList{string}}, Arg{SelectionType})"/> reads its chips by, written by its name.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter<SelectionType>))]
    public enum SelectionType
    {
        /// <summary>The chips' values.</summary>
        [JsonStringEnumMemberName("value")]
        Value,

        /// <summary>The chips' texts.</summary>
        [JsonStringEnumMemberName("text")]
        Text,
    }

    /// <summary>
    /// What a listener of <see cref="Click"/> receives.
    /// </summary>
    [Mirrors("@syncfusion/ej2-buttons", "ClickEventArgs")]
    public sealed class ClickEventArgs : JsObject
    {
        /// <summary>Gets the text of the clicked chip.</summary>
        public ReadOnly<string> Text => ReadOnly<string>();

        /// <summary>Gets the position of the clicked chip, from 0.</summary>
        public ReadOnly<int> Index => ReadOnly<int>(optional: true);

        /// <summary>Gets whether the clicked chip is chosen after the click.</summary>
        public ReadOnly<bool> Selected => ReadOnly<bool>(optional: true);
    }

    /// <summary>
    /// What a listener of <see cref="Deleted"/> receives.
    /// </summary>
    [Mirrors("@syncfusion/ej2-buttons", "ChipDeletedEventArgs")]
    public sealed class ChipDeletedEventArgs : JsObject
    {
        /// <summary>Gets the text of the removed chip.</summary>
        public ReadOnly<string> Text => ReadOnly<string>();

        /// <summary>Gets the position the removed chip held, from 0.</summary>
        public ReadOnly<int> Index => ReadOnly<int>();
    }

    /// <summary>
    /// A chip, as <see cref="Find"/> reads it, each member named as EJ2 names it.
    /// </summary>
    [Mirrors("@syncfusion/ej2-buttons", "ChipDataArgs")]
    public sealed class ChipDataArgs
    {
        /// <summary>Gets or sets the chip's text.</summary>
        [JsonPropertyName("text")]
        public string? Text { get; set; }

        /// <summary>Gets or sets the chip's position, from 0.</summary>
        [JsonPropertyName("index")]
        public int Index { get; set; }
    }

    /// <summary>
    /// The chips chosen in a list that lets a person choose several, as <see cref="GetSelectedChips{TData}"/> reads
    /// them, each member named as EJ2 names it.
    /// </summary>
    /// <typeparam name="TData">The chips' own data.</typeparam>
    [Mirrors("@syncfusion/ej2-buttons", "SelectedItems")]
    public sealed class SelectedItems<TData>
    {
        /// <summary>Gets or sets the texts of the chosen chips.</summary>
        [JsonPropertyName("texts")]
        public IReadOnlyList<string> Texts { get; set; } = [];

        /// <summary>Gets or sets the data of the chosen chips.</summary>
        [JsonPropertyName("data")]
        public IReadOnlyList<TData> Data { get; set; } = [];

        /// <summary>Gets or sets the positions of the chosen chips, from 0.</summary>
        [JsonPropertyName("Indexes")]
        public IReadOnlyList<int> Indexes { get; set; } = [];
    }

    /// <summary>
    /// The chip chosen in a list that lets a person choose one, as <see cref="GetSelectedChip{TData}"/> reads it,
    /// each member named as EJ2 names it.
    /// </summary>
    /// <typeparam name="TData">The chips' own data.</typeparam>
    [Mirrors("@syncfusion/ej2-buttons", "SelectedItem")]
    public sealed class SelectedItem<TData>
    {
        /// <summary>Gets or sets the chosen chip's text.</summary>
        [JsonPropertyName("text")]
        public string Text { get; set; } = "";

        /// <summary>Gets or sets the chosen chip's position, from 0.</summary>
        [JsonPropertyName("index")]
        public int Index { get; set; }

        /// <summary>Gets or sets the chosen chip's data.</summary>
        [JsonPropertyName("data")]
        public required TData Data { get; set; }
    }
}
