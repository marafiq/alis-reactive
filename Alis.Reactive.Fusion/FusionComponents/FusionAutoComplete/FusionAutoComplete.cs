using System.Collections.Generic;
using Alis.Reactive.Declarations;
using Alis.Reactive.InputsAndIds;
using Alis.Reactive.ValuesAndSources;

namespace Alis.Reactive.Fusion.FusionComponents;

/// <summary>
/// An autocomplete box: EJ2's AutoComplete, which suggests items as a person types and holds the value of the one
/// chosen.
/// </summary>
/// <remarks>
/// A box bound to a member is an input: a form sends its <see cref="Value"/> and a check reads it. EJ2 raises
/// <see cref="Change"/> when a person chooses an item of the list or the value changes.
/// </remarks>
/// <typeparam name="TValue">The type of the member the box binds, which admits null, since the box holds null until a choice.</typeparam>
[Mirrors("@syncfusion/ej2-dropdowns", "AutoComplete")]
public sealed class FusionAutoComplete<TValue> : FusionComponent, IInputComponent
{
    /// <summary>Gets the value the box holds, null until a person chooses an item.</summary>
    public Property<TValue> Value => Property<TValue>();

    /// <summary>Gets the items the box suggests.</summary>
    /// <remarks>Call <see cref="DataBind"/> after a write outside a <see cref="Filtering"/> handler, so the list shows it.</remarks>
    /// <typeparam name="TItem">The class of the items.</typeparam>
    /// <returns>The items.</returns>
    public Property<IReadOnlyList<TItem>> DataSource<TItem>() => Property<IReadOnlyList<TItem>>();

    /// <summary>Gets whether the box is enabled.</summary>
    public Property<bool> Enabled => Property<bool>();

    /// <summary>Applies the changes written to the box's properties now.</summary>
    public void DataBind() => Method();

    /// <summary>Moves the keyboard's focus to the box.</summary>
    public void FocusIn() => Method();

    /// <summary>Moves the keyboard's focus out of the box, if the box holds it.</summary>
    public void FocusOut() => Method();

    /// <summary>Opens the box's list.</summary>
    public void ShowPopup() => Method();

    /// <summary>Closes the box's list.</summary>
    public void HidePopup() => Method();

    /// <summary>Clears the box's value.</summary>
    public void Clear() => Method();

    /// <summary>Gets the event raised when a person chooses an item of the list, or when the box's value changes.</summary>
    public Event<ChangeEventArgs> Change => Event<ChangeEventArgs>();

    /// <summary>Gets the event raised as a person types in the box, before the list filters its items.</summary>
    public Event<FilteringEventArgs> Filtering => Event<FilteringEventArgs>();

    ComponentInput IInputComponent.Input => ComponentInput.Of(Value, committedBy: Change);

    /// <summary>
    /// What a listener of <see cref="Change"/> receives.
    /// </summary>
    [Mirrors("@syncfusion/ej2-dropdowns", "ChangeEventArgs")]
    public sealed class ChangeEventArgs : JsObject
    {
        /// <summary>Gets the value the box holds after the change.</summary>
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
}
