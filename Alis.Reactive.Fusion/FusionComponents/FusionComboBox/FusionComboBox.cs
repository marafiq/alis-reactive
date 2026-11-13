using Alis.Reactive.Declarations;
using Alis.Reactive.InputsAndIds;

namespace Alis.Reactive.Fusion.FusionComponents;

/// <summary>
/// A combo box: EJ2's ComboBox, a list a person chooses an item from or types into, which holds the value of the
/// item chosen.
/// </summary>
/// <remarks>
/// A box bound to a member is an input: a form sends its <see cref="Value"/> and a check reads it. EJ2 raises
/// <see cref="Change"/> when a person chooses an item of the list or the value changes.
/// </remarks>
/// <typeparam name="TValue">The type of the member the box binds, which admits null, since the box holds null until a choice.</typeparam>
[Mirrors("@syncfusion/ej2-dropdowns", "ComboBox")]
public sealed class FusionComboBox<TValue> : FusionComponent, IInputComponent
{
    /// <summary>Gets the value the box holds, null until a person chooses an item.</summary>
    public Property<TValue> Value => Property<TValue>();

    /// <summary>Gets the text the box shows for the item chosen, null until a choice.</summary>
    public Property<string?> Text => Property<string?>();

    /// <summary>Gets the position of the item chosen in the list, null until a choice.</summary>
    public Property<int?> Index => Property<int?>();

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

    /// <summary>Gets the event raised when the box takes the keyboard's focus.</summary>
    /// <remarks>Its listener receives no member a view reads.</remarks>
    public Event Focus => Event();

    /// <summary>Gets the event raised when the keyboard's focus leaves the box.</summary>
    /// <remarks>Its listener receives no member a view reads.</remarks>
    public Event Blur => Event();

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
}
