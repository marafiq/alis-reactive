using Alis.Reactive.Declarations;
using Alis.Reactive.InputsAndIds;

namespace Alis.Reactive.Fusion.FusionComponents;

/// <summary>
/// A rich text editor: EJ2's RichTextEditor, which holds a member's formatted text as HTML.
/// </summary>
/// <remarks>
/// An editor bound to a member is an input: a form sends its <see cref="Value"/> and a check reads it. EJ2 raises
/// <see cref="Change"/> when the editor loses focus after its content changed, which commits the content.
/// </remarks>
[Mirrors("@syncfusion/ej2-richtexteditor", "RichTextEditor")]
public sealed class FusionRichTextEditor : FusionComponent, IInputComponent
{
    /// <summary>Gets the content the editor holds, as HTML, null until set.</summary>
    public Property<string?> Value => Property<string?>();

    /// <summary>Moves the keyboard's focus to the editor.</summary>
    public void FocusIn() => Method();

    /// <summary>Gets the event raised when the editor loses focus after its content changed.</summary>
    public Event<ChangeEventArgs> Change => Event<ChangeEventArgs>();

    ComponentInput IInputComponent.Input => ComponentInput.Of(Value, committedBy: Change);

    /// <summary>
    /// What a listener of <see cref="Change"/> receives.
    /// </summary>
    [Mirrors("@syncfusion/ej2-richtexteditor", "ChangeEventArgs")]
    public sealed class ChangeEventArgs : JsObject
    {
        /// <summary>Gets the content the editor holds after the change, as HTML.</summary>
        public ReadOnly<string> Value => ReadOnly<string>();

        /// <summary>Gets whether a person made the change; a write reads false.</summary>
        public ReadOnly<bool> IsInteracted => ReadOnly<bool>();
    }
}
