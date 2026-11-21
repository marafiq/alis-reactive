using System.Collections.Generic;
using Alis.Reactive.Declarations;
using Alis.Reactive.InputsAndIds;

namespace Alis.Reactive.Fusion.FusionComponents;

/// <summary>
/// A multi-column combo box: EJ2's MultiColumnComboBox, a list of rows shown in columns that a person chooses one
/// row from, which holds the text of the row's value.
/// </summary>
/// <remarks>
/// A box bound to a member is an input: a form sends its <see cref="Value"/> and a check reads it. EJ2 raises
/// <see cref="Change"/> when a person chooses a row or the value changes.
/// </remarks>
/// <typeparam name="TValue">The type of the rows' value member, which the change payload's value holds.</typeparam>
[Mirrors("@syncfusion/ej2-multicolumn-combobox", "MultiColumnComboBox")]
public sealed class FusionMultiColumnComboBox<TValue> : FusionComponent, IInputComponent
{
    /// <summary>Gets the value of the row chosen, as text.</summary>
    public Property<string?> Value => Property<string?>();

    /// <summary>Gets the text the box shows for the row chosen.</summary>
    public Property<string?> Text => Property<string?>();

    /// <summary>Gets the rows the box offers.</summary>
    /// <remarks>Call <see cref="DataBind"/> after a write, so the list shows it.</remarks>
    /// <typeparam name="TRow">The class of the rows.</typeparam>
    /// <returns>The rows.</returns>
    public Property<IReadOnlyList<TRow>> DataSource<TRow>() => Property<IReadOnlyList<TRow>>();

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

    /// <summary>Gets the event raised when a person chooses a row of the list, or when the box's value changes.</summary>
    public Event<ChangeEventArgs> Change => Event<ChangeEventArgs>();

    ComponentInput IInputComponent.Input => ComponentInput.Of(Value, committedBy: Change);

    /// <summary>
    /// What a listener of <see cref="Change"/> receives.
    /// </summary>
    [Mirrors("@syncfusion/ej2-multicolumn-combobox", "ChangeEventArgs")]
    public sealed class ChangeEventArgs : JsObject
    {
        /// <summary>Gets the value of the row chosen, typed as the rows' value member.</summary>
        public ReadOnly<TValue> Value => ReadOnly<TValue>();

        /// <summary>Gets whether a person made the change; a write reads false.</summary>
        public ReadOnly<bool> IsInteracted => ReadOnly<bool>();
    }
}
