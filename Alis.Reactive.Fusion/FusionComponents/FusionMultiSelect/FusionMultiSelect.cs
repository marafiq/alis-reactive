using System.Collections.Generic;
using Alis.Reactive.Declarations;
using Alis.Reactive.InputsAndIds;
using Alis.Reactive.ValuesAndSources;

namespace Alis.Reactive.Fusion.FusionComponents;

/// <summary>
/// A multi-select box: EJ2's MultiSelect, which holds the list of values a person chose from its items.
/// </summary>
/// <remarks>
/// A box bound to a member is an input: a form sends its <see cref="Value"/> and a check reads it. EJ2 raises
/// <see cref="Change"/> when the box loses focus after a choice, so a handler of it reads the committed values.
/// </remarks>
/// <typeparam name="TValue">The type of the member the box binds, a list that admits null, since the box holds null until a choice.</typeparam>
[Mirrors("@syncfusion/ej2-dropdowns", "MultiSelect")]
public sealed class FusionMultiSelect<TValue> : FusionComponent, IInputComponent
{
    /// <summary>Gets the values the box holds, null until a person chooses one.</summary>
    public Property<TValue> Value => Property<TValue>();

    /// <summary>Gets the items the box offers.</summary>
    /// <remarks>Call <see cref="DataBind"/> after a write outside a <see cref="Filtering"/> handler, so the list shows it.</remarks>
    /// <typeparam name="TItem">The class of the items.</typeparam>
    /// <returns>The items.</returns>
    public Property<IReadOnlyList<TItem>> DataSource<TItem>() => Property<IReadOnlyList<TItem>>();

    /// <summary>Gets the text the box shows while it holds no value.</summary>
    public Property<string?> Placeholder => Property<string?>();

    /// <summary>Gets which members of an item the box shows as its text and groups it by.</summary>
    public FieldSettings Fields => LiveObject<FieldSettings>();

    /// <summary>Applies the changes written to the box's properties now.</summary>
    public void DataBind() => Method();

    /// <summary>Opens the box's list.</summary>
    public void ShowPopup() => Method();

    /// <summary>Closes the box's list.</summary>
    public void HidePopup() => Method();

    /// <summary>Chooses every item of the box, or clears every choice.</summary>
    /// <param name="state">True to choose every item, false to clear every choice.</param>
    public void SelectAll(Arg<bool> state) => Method([state]);

    /// <summary>Removes the box from its element, with its listeners.</summary>
    public void Destroy() => Method();

    /// <summary>Gets the event raised when the box loses focus after its values changed, or when a write changes them.</summary>
    public Event<MultiSelectChangeEventArgs> Change => Event<MultiSelectChangeEventArgs>();

    /// <summary>Gets the event raised as a person types in the box, before the list filters its items.</summary>
    public Event<FilteringEventArgs> Filtering => Event<FilteringEventArgs>();

    /// <summary>Gets the event raised when a person chooses an item, before the box shows it as a chip.</summary>
    public Event<TaggingEventArgs> Tagging => Event<TaggingEventArgs>();

    /// <summary>Gets the event raised when the box's list opens.</summary>
    public Event<PopupEventArgs> Open => Event<PopupEventArgs>();

    ComponentInput IInputComponent.Input => ComponentInput.Of(Value, committedBy: Change);

    /// <summary>
    /// Which members of an item the box shows as its text and groups it by, each named as the item's data names it.
    /// </summary>
    [Mirrors("@syncfusion/ej2-dropdowns", "FieldSettingsModel")]
    public sealed class FieldSettings : JsObject
    {
        /// <summary>Gets the name of the member the box shows as an item's text.</summary>
        public Property<string?> Text => Property<string?>(optional: true);

        /// <summary>Gets the name of the member the box groups its items by.</summary>
        public Property<string?> GroupBy => Property<string?>(optional: true);
    }

    /// <summary>
    /// What a listener of <see cref="Change"/> receives.
    /// </summary>
    [Mirrors("@syncfusion/ej2-dropdowns", "MultiSelectChangeEventArgs")]
    public sealed class MultiSelectChangeEventArgs : JsObject
    {
        /// <summary>Gets the values the box holds after the change.</summary>
        public ReadOnly<TValue> Value => ReadOnly<TValue>();

        /// <summary>Gets whether a person made the change; a write reads false.</summary>
        public ReadOnly<bool> IsInteracted => ReadOnly<bool>();
    }

    /// <summary>
    /// What a listener of <see cref="Filtering"/> receives.
    /// </summary>
    /// <remarks>
    /// To filter on the server, write true to <see cref="PreventDefaultAction"/>, request the matches and hand them
    /// to <see cref="UpdateData{TItem}"/> when the reply comes.
    /// </remarks>
    [Mirrors("@syncfusion/ej2-dropdowns", "FilteringEventArgs")]
    public sealed class FilteringEventArgs : JsObject
    {
        /// <summary>Gets the text the person typed.</summary>
        public ReadOnly<string> Text => ReadOnly<string>();

        /// <summary>Gets whether the box skips its own filter; write true to filter on the server.</summary>
        public Property<bool> PreventDefaultAction => Property<bool>();

        /// <summary>Shows the items in the box's list in place of its own.</summary>
        /// <typeparam name="TItem">The class of the items.</typeparam>
        /// <param name="dataSource">The items to show.</param>
        public void UpdateData<TItem>(Arg<IReadOnlyList<TItem>> dataSource) => Method([dataSource]);
    }

    /// <summary>
    /// What a listener of <see cref="Tagging"/> receives.
    /// </summary>
    [Mirrors("@syncfusion/ej2-dropdowns", "TaggingEventArgs")]
    public sealed class TaggingEventArgs : JsObject
    {
        /// <summary>Gets the item the person chose, read with <c>Read</c>.</summary>
        /// <typeparam name="TItem">The class of the box's items.</typeparam>
        /// <returns>The item.</returns>
        public ReadOnly<TItem> ItemData<TItem>() => ReadOnly<TItem>();
    }

    /// <summary>
    /// What a listener of <see cref="Open"/> receives.
    /// </summary>
    [Mirrors("@syncfusion/ej2-dropdowns", "PopupEventArgs")]
    public sealed class PopupEventArgs : JsObject
    {
        /// <summary>Gets the list that opened.</summary>
        public MultiSelectPopup Popup => LiveObject<MultiSelectPopup>();
    }

    /// <summary>
    /// The box's list: EJ2's Popup component, which <see cref="Open"/> hands its listener.
    /// </summary>
    [Mirrors("@syncfusion/ej2-popups", "Popup")]
    public sealed class MultiSelectPopup : FusionComponent
    {
        /// <summary>Gets the list's vertical offset from the box, in pixels.</summary>
        public Property<double> OffsetY => Property<double>();
    }
}
