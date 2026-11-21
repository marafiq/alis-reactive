using System.Text.Json.Serialization;
using Alis.Reactive.Declarations;
using Alis.Reactive.InputsAndIds;
using Alis.Reactive.ValuesAndSources;

namespace Alis.Reactive.Fusion.FusionComponents;

/// <summary>
/// An in-place editor: EJ2's InPlaceEditor, a value shown as text that a person clicks to edit and saves.
/// </summary>
/// <remarks>
/// An editor bound to a member is an input: a form sends its <see cref="Value"/> and a check reads it. EJ2 raises
/// <see cref="EndEdit"/> when a person finishes editing, the event that commits the input.
/// </remarks>
/// <typeparam name="TValue">The type of the member the editor binds, which admits null, since the editor holds null until a value is set.</typeparam>
[Mirrors("@syncfusion/ej2-inplace-editor", "InPlaceEditor")]
public sealed class FusionInPlaceEditor<TValue> : FusionComponent, IInputComponent
{
    /// <summary>Gets the value the editor holds.</summary>
    public Property<TValue> Value => Property<TValue>();

    /// <summary>Gets whether the editor is disabled, so a click does not open it.</summary>
    public Property<bool> Disabled => Property<bool>();

    /// <summary>Gets whether the editor shows its edit mode.</summary>
    public Property<bool> EnableEditMode => Property<bool>();

    /// <summary>Gets the editor's element, whose CSS classes a view adds and removes.</summary>
    public HTMLElement Element => LiveObject<HTMLElement>();

    /// <summary>Submits the value the person typed, as the editor's save button does.</summary>
    public void Save() => Method();

    /// <summary>Gets the event raised before the editor opens its edit mode.</summary>
    public Event<BeginEditEventArgs> BeginEdit => Event<BeginEditEventArgs>();

    /// <summary>Gets the event raised when a person finishes editing, before the editor submits or cancels the value.</summary>
    public Event<EndEditEventArgs> EndEdit => Event<EndEditEventArgs>();

    /// <summary>Gets the event raised when the value of the component the editor holds changes.</summary>
    public Event<ChangeEventArgs> Change => Event<ChangeEventArgs>();

    /// <summary>Gets the event raised before the editor submits its value.</summary>
    public Event<ActionBeginEventArgs> ActionBegin => Event<ActionBeginEventArgs>();

    /// <summary>Gets the event raised after the editor submitted its value.</summary>
    public Event<ActionEventArgs> ActionSuccess => Event<ActionEventArgs>();

    /// <summary>Gets the event raised when a person clicks the editor's save button.</summary>
    public Event<MouseEvent> SubmitClick => Event<MouseEvent>();

    /// <summary>Gets the event raised when a person clicks the editor's cancel button.</summary>
    public Event<MouseEvent> CancelClick => Event<MouseEvent>();

    ComponentInput IInputComponent.Input => ComponentInput.Of(Value, committedBy: EndEdit);

    /// <summary>
    /// The editor's element.
    /// </summary>
    [Mirrors("lib.dom", "HTMLElement")]
    public sealed class HTMLElement : JsObject
    {
        /// <summary>Gets the element's CSS classes.</summary>
        public DOMTokenList ClassList => LiveObject<DOMTokenList>();
    }

    /// <summary>
    /// The CSS classes of the editor's element.
    /// </summary>
    [Mirrors("lib.dom", "DOMTokenList")]
    public sealed class DOMTokenList : JsObject
    {
        /// <summary>Adds a CSS class to the element.</summary>
        /// <param name="tokens">The class name.</param>
        public void Add(Arg<string> tokens) => Method([tokens]);

        /// <summary>Removes a CSS class from the element.</summary>
        /// <param name="tokens">The class name.</param>
        public void Remove(Arg<string> tokens) => Method([tokens]);
    }

    /// <summary>
    /// What a listener of <see cref="BeginEdit"/> receives.
    /// </summary>
    [Mirrors("@syncfusion/ej2-inplace-editor", "BeginEditEventArgs")]
    public sealed class BeginEditEventArgs : JsObject
    {
        /// <summary>Gets whether the editor stays closed; write true to keep it from opening.</summary>
        public Property<bool> Cancel => Property<bool>(optional: true);

        /// <summary>Gets whether the editor leaves the focus where it is; write true to keep it from moving into the editor.</summary>
        public Property<bool> CancelFocus => Property<bool>(optional: true);

        /// <summary>Gets how the editor shows its edit mode, <c>Inline</c> or <c>Popup</c>.</summary>
        public ReadOnly<string> Mode => ReadOnly<string>(optional: true);

        /// <summary>Gets the event's name, <c>beginEdit</c>.</summary>
        public ReadOnly<string> Name => ReadOnly<string>();
    }

    /// <summary>
    /// What a listener of <see cref="EndEdit"/> receives.
    /// </summary>
    [Mirrors("@syncfusion/ej2-inplace-editor", "EndEditEventArgs")]
    public sealed class EndEditEventArgs : JsObject
    {
        /// <summary>Gets what closed the edit mode, <c>submit</c> or <c>cancel</c>.</summary>
        public ReadOnly<string> Action => ReadOnly<string>(optional: true);

        /// <summary>Gets whether the edit is held; write true to keep the editor in its edit mode.</summary>
        public Property<bool> Cancel => Property<bool>(optional: true);

        /// <summary>Gets how the editor shows its edit mode, <c>Inline</c> or <c>Popup</c>.</summary>
        public ReadOnly<string> Mode => ReadOnly<string>(optional: true);

        /// <summary>Gets the event's name, <c>endEdit</c>.</summary>
        public ReadOnly<string> Name => ReadOnly<string>();
    }

    /// <summary>
    /// What a listener of <see cref="Change"/> receives.
    /// </summary>
    [Mirrors("@syncfusion/ej2-inplace-editor", "ChangeEventArgs")]
    public sealed class ChangeEventArgs : JsObject
    {
        /// <summary>Gets the value the editor holds after the change.</summary>
        public ReadOnly<TValue> Value => ReadOnly<TValue>();

        /// <summary>Gets the value the editor held before the change.</summary>
        public ReadOnly<TValue> PreviousValue => ReadOnly<TValue>();

        /// <summary>Gets the event's name, <c>change</c>.</summary>
        public ReadOnly<string> Name => ReadOnly<string>();
    }

    /// <summary>
    /// What a listener of <see cref="ActionBegin"/> receives.
    /// </summary>
    [Mirrors("@syncfusion/ej2-inplace-editor", "ActionBeginEventArgs")]
    public sealed class ActionBeginEventArgs : JsObject
    {
        /// <summary>Gets what the editor sends: the member's name, the record's key and the value.</summary>
        public ReadOnly<ActionBeginData> Data => ReadOnly<ActionBeginData>();

        /// <summary>Gets whether the submit is held; write true to keep the editor from submitting.</summary>
        public Property<bool> Cancel => Property<bool>(optional: true);
    }

    /// <summary>
    /// What the editor sends when it saves, read with <c>Read</c>.
    /// </summary>
    public sealed class ActionBeginData
    {
        /// <summary>Gets the name of the member the editor saves.</summary>
        [JsonPropertyName("name")]
        public string Name { get; init; } = "";

        /// <summary>Gets the key of the record the editor saves.</summary>
        [JsonPropertyName("primaryKey")]
        public string PrimaryKey { get; init; } = "";

        /// <summary>Gets the value the editor saves, as text.</summary>
        [JsonPropertyName("value")]
        public string Value { get; init; } = "";
    }

    /// <summary>
    /// What a listener of <see cref="ActionSuccess"/> receives.
    /// </summary>
    [Mirrors("@syncfusion/ej2-inplace-editor", "ActionEventArgs")]
    public sealed class ActionEventArgs : JsObject
    {
        /// <summary>Gets what the server answered the save with, read with <c>Read</c>.</summary>
        /// <typeparam name="TResponse">The class of the server's answer.</typeparam>
        /// <returns>The answer.</returns>
        public ReadOnly<TResponse> Data<TResponse>() => ReadOnly<TResponse>();

        /// <summary>Gets the value the editor saved, as text.</summary>
        public ReadOnly<string> Value => ReadOnly<string>();

        /// <summary>Gets the event's name, <c>actionSuccess</c>.</summary>
        public ReadOnly<string> Name => ReadOnly<string>();
    }

    /// <summary>
    /// What a listener of <see cref="SubmitClick"/> or <see cref="CancelClick"/> receives: the browser's click.
    /// </summary>
    [Mirrors("lib.dom", "MouseEvent")]
    public sealed class MouseEvent : JsObject
    {
        /// <summary>Gets the event's name EJ2 sets on the click, <c>submitClick</c> or <c>cancelClick</c>.</summary>
        public ReadOnly<string> Name => ReadOnly<string>();
    }
}
