using Alis.Reactive.Declarations;
using Alis.Reactive.InputsAndIds;

namespace Alis.Reactive.Fusion.FusionComponents;

/// <summary>
/// A drop-down tree: EJ2's DropDownTree, a box whose popup lists nodes in a tree, which holds the values of the
/// chosen nodes.
/// </summary>
/// <remarks>
/// A drop-down tree bound to a member is an input: a form sends its <see cref="Value"/> and a check reads it. EJ2
/// raises <see cref="Change"/> when a person chooses a node or the value changes, which commits it.
/// </remarks>
[Mirrors("@syncfusion/ej2-dropdowns", "DropDownTree")]
public sealed class FusionDropDownTree : FusionComponent, IInputComponent
{
    /// <summary>Gets the values of the chosen nodes, null until a node is chosen.</summary>
    public Property<string[]?> Value => Property<string[]?>();

    /// <summary>Gets the text the box shows for the chosen nodes, null until a node is chosen.</summary>
    public Property<string?> Text => Property<string?>();

    /// <summary>Applies the changes written to the tree's properties now.</summary>
    public void DataBind() => Method();

    /// <summary>Opens the tree's popup.</summary>
    public void ShowPopup() => Method();

    /// <summary>Closes the tree's popup.</summary>
    public void HidePopup() => Method();

    /// <summary>Clears the chosen nodes.</summary>
    public void Clear() => Method();

    /// <summary>Gets the event raised when a person chooses a node of the popup, or the value changes.</summary>
    public Event<DdtChangeEventArgs> Change => Event<DdtChangeEventArgs>();

    ComponentInput IInputComponent.Input => ComponentInput.Of(Value, committedBy: Change);

    /// <summary>
    /// What a listener of <see cref="Change"/> receives.
    /// </summary>
    [Mirrors("@syncfusion/ej2-dropdowns", "DdtChangeEventArgs")]
    public sealed class DdtChangeEventArgs : JsObject
    {
        /// <summary>Gets the values of the chosen nodes after the change.</summary>
        public ReadOnly<string[]> Value => ReadOnly<string[]>();

        /// <summary>Gets the values of the chosen nodes before the change.</summary>
        public ReadOnly<string[]> OldValue => ReadOnly<string[]>();

        /// <summary>Gets whether a person made the change; a write reads false.</summary>
        public ReadOnly<bool> IsInteracted => ReadOnly<bool>();
    }
}
