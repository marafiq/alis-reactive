using System.Collections.Generic;
using Alis.Reactive.Declarations;
using Alis.Reactive.InputsAndIds;

namespace Alis.Reactive.Fusion.FusionComponents;

/// <summary>
/// A drop-down list: EJ2's DropDownList, a list a person chooses one item from, which holds the value of the item
/// chosen.
/// </summary>
/// <remarks>
/// A list bound to a member is an input: a form sends its <see cref="Value"/> and a check reads it. EJ2 raises
/// <see cref="Change"/> when a person chooses an item or the value changes.
/// </remarks>
/// <typeparam name="TValue">The type of the member the list binds, which admits null, since the list holds null until a choice.</typeparam>
[Mirrors("@syncfusion/ej2-dropdowns", "DropDownList")]
public sealed class FusionDropDownList<TValue> : FusionComponent, IInputComponent
{
    /// <summary>Gets the value the list holds, null until a person chooses an item.</summary>
    public Property<TValue> Value => Property<TValue>();

    /// <summary>Gets the text the list shows for the item chosen, null until a choice.</summary>
    public Property<string?> Text => Property<string?>();

    /// <summary>Gets the items the list offers.</summary>
    /// <remarks>Call <see cref="DataBind"/> after a write, so the list shows it.</remarks>
    /// <typeparam name="TItem">The class of the items.</typeparam>
    /// <returns>The items.</returns>
    public Property<IReadOnlyList<TItem>> DataSource<TItem>() => Property<IReadOnlyList<TItem>>();

    /// <summary>Applies the changes written to the list's properties now.</summary>
    public void DataBind() => Method();

    /// <summary>Moves the keyboard's focus to the list.</summary>
    public void FocusIn() => Method();

    /// <summary>Moves the keyboard's focus out of the list, if the list holds it.</summary>
    public void FocusOut() => Method();

    /// <summary>Opens the list's popup.</summary>
    public void ShowPopup() => Method();

    /// <summary>Closes the list's popup.</summary>
    public void HidePopup() => Method();

    /// <summary>Clears the list's value.</summary>
    public void Clear() => Method();

    /// <summary>Gets the event raised when a person chooses an item, or when the list's value changes.</summary>
    public Event<ChangeEventArgs> Change => Event<ChangeEventArgs>();

    /// <summary>Gets the event raised when the list takes the keyboard's focus.</summary>
    /// <remarks>Its listener receives no member a view reads.</remarks>
    public Event Focus => Event();

    /// <summary>Gets the event raised when the keyboard's focus leaves the list.</summary>
    /// <remarks>Its listener receives no member a view reads.</remarks>
    public Event Blur => Event();

    ComponentInput IInputComponent.Input => ComponentInput.Of(Value, committedBy: Change);

    /// <summary>
    /// What a listener of <see cref="Change"/> receives.
    /// </summary>
    [Mirrors("@syncfusion/ej2-dropdowns", "ChangeEventArgs")]
    public sealed class ChangeEventArgs : JsObject
    {
        /// <summary>Gets the value the list holds after the change.</summary>
        public ReadOnly<TValue> Value => ReadOnly<TValue>();

        /// <summary>Gets whether a person made the change; a write reads false.</summary>
        public ReadOnly<bool> IsInteracted => ReadOnly<bool>();
    }
}
