using System.Collections.Generic;
using System.Text.Json.Serialization;
using Alis.Reactive.Declarations;
using Alis.Reactive.Elements;
using Alis.Reactive.InputsAndIds;
using Alis.Reactive.ValuesAndSources;

namespace Alis.Reactive.Fusion.FusionComponents;

/// <summary>
/// A text box: EJ2's TextBox, which holds a member's text.
/// </summary>
/// <remarks>
/// A text box bound to a member is an input: a request container sends its <see cref="Value"/> and a check reads it. EJ2 raises
/// <see cref="Input"/> at each keystroke and <see cref="Change"/> when the box loses focus after its text changed,
/// which commits the text.
/// </remarks>
[Mirrors("@syncfusion/ej2-inputs", "TextBox")]
public sealed class FusionTextBox : FusionComponent, IInputComponent
{
    /// <summary>Gets the text the box holds, null until set.</summary>
    public Property<string?> Value => Property<string?>();

    /// <summary>Gets the HTML input type used by a single-line box.</summary>
    public Property<string> Type => Property<string>();

    /// <summary>Gets whether the box prevents typing while allowing focus.</summary>
    public Property<bool> Readonly => Property<bool>();

    /// <summary>Gets when the placeholder appears as a floating label.</summary>
    public Property<LabelMode> FloatLabelType => Property<LabelMode>();

    /// <summary>Gets the CSS classes applied to the text box wrapper.</summary>
    public Property<string> CssClass => Property<string>();

    /// <summary>Gets the hint shown for an empty text box.</summary>
    public Property<string?> Placeholder => Property<string?>();

    /// <summary>Gets the browser completion token, with on selecting the browser default.</summary>
    public Property<string> Autocomplete => Property<string>();

    /// <summary>Gets whether the box uses a textarea when rendered.</summary>
    public ReadOnly<bool> Multiline => ReadOnly<bool>();

    /// <summary>Gets whether the box accepts input and focus.</summary>
    public Property<bool> Enabled => Property<bool>();

    /// <summary>Gets whether the box offers a clear control.</summary>
    public Property<bool> ShowClearButton => Property<bool>();

    /// <summary>Gets whether EJ2 registers this box to persist its value on page exit.</summary>
    /// <remarks>Refresh after enabling persistence on an existing box. Clearing the flag leaves an attached page-exit listener in place.</remarks>
    public Property<bool> EnablePersistence => Property<bool>();

    /// <summary>Gets whether the box is laid out from right to left.</summary>
    public Property<bool> EnableRtl => Property<bool>();

    /// <summary>Gets the language used to resolve the box's localized placeholder.</summary>
    public Property<string> Locale => Property<string>();

    /// <summary>Gets the CSS width of the wrapper.</summary>
    /// <remarks>Use a CSS length or percentage. A null leaves the rendered width as it stands.</remarks>
    public WriteOnly<string?> Width => WriteOnly<string?>();

    /// <summary>Gets the numeric width of the wrapper in pixels.</summary>
    public WriteOnly<double?> WidthInPixels => WriteOnly<double?>(name: "width");

    /// <summary>Gets the HTML template displayed before the field.</summary>
    /// <remarks>Use the text returned by a Fusion template's Render method. A null removes the template.</remarks>
    public WriteOnly<string?> PrependTemplate => WriteOnly<string?>();

    /// <summary>Gets the HTML template displayed after the field.</summary>
    /// <remarks>Use the text returned by a Fusion template's Render method. A null removes the template.</remarks>
    public WriteOnly<string?> AppendTemplate => WriteOnly<string?>();

    /// <summary>Sets the function that supplies the prefix when the text box renders its template.</summary>
    /// <remarks>EJ2 interprets the returned string as HTML. Return app-authored markup, such as a rendered Fusion template.</remarks>
    public Callback<TemplateArgs, string> PrependTemplateCallback => Callback<TemplateArgs, string>(name: "prependTemplate");

    /// <summary>Sets the function that supplies the suffix when the text box renders its template.</summary>
    /// <remarks>EJ2 interprets the returned string as HTML. Return app-authored markup, such as a rendered Fusion template.</remarks>
    public Callback<TemplateArgs, string> AppendTemplateCallback => Callback<TemplateArgs, string>(name: "appendTemplate");

    /// <summary>Removes the selected attributes from the field, including the corresponding vendor state for editing restrictions and the placeholder.</summary>
    /// <param name="attributes">The attributes to remove.</param>
    public void RemoveAttributes(Arg<IReadOnlyList<RemovableAttribute>> attributes) => Method([attributes]);

    /// <summary>Gets the original input element that owns this box.</summary>
    public ChildElement Element => LiveObject<ChildElement>();

    /// <summary>Gets the original root element returned by the box.</summary>
    public ChildElement GetRootElement() => ReturnedObject<ChildElement>();

    /// <summary>Rebuilds the editor from its current settings.</summary>
    public void Refresh() => Method();

    /// <summary>Closes the editor and restores its original input.</summary>
    public void Destroy() => Method();

    /// <summary>Gets the vendor's JSON recovery snapshot of the current value.</summary>
    public MethodResult<string> GetPersistData() => Method<string>();

    /// <summary>Gets the terminal event raised when the enhanced editor closes.</summary>
    public Event<DestructionEventArgs> Destroyed => Event<DestructionEventArgs>();

    /// <summary>Applies the changes written to the box's properties now.</summary>
    public void DataBind() => Method();

    /// <summary>Moves the keyboard's focus to the box.</summary>
    public void FocusIn() => Method();

    /// <summary>Moves the keyboard's focus away from the box.</summary>
    public void FocusOut() => Method();

    /// <summary>Adds icons to one side of the box.</summary>
    /// <param name="position">The side the icons go on.</param>
    /// <param name="icons">The CSS classes for each icon, with commas between icons.</param>
    public void AddIcon(Arg<IconPosition> position, Arg<string> icons) => Method([position, icons]);

    /// <summary>Adds an ordered list of icons to one side of the box.</summary>
    /// <param name="position">The side to place the icons on.</param>
    /// <param name="icons">The CSS class names of the individual icons.</param>
    public void AddIcon(Arg<IconPosition> position, Arg<IReadOnlyList<string>> icons) => Method([position, icons]);

    /// <summary>Gets the event raised each time the box's text changes.</summary>
    public Event<InputEventArgs> Input => Event<InputEventArgs>();

    /// <summary>Gets the event raised when the box loses focus after its text changed.</summary>
    public Event<ChangedEventArgs> Change => Event<ChangedEventArgs>();

    /// <summary>Gets the event raised when the box gets focus.</summary>
    public Event<FocusInEventArgs> Focus => Event<FocusInEventArgs>();

    /// <summary>Gets the event raised when the box loses focus.</summary>
    public Event<FocusOutEventArgs> Blur => Event<FocusOutEventArgs>();

    ComponentInput IInputComponent.Input => ComponentInput.Of(Value, committedBy: Change);

    /// <summary>Field attributes that can be removed without changing the registered input identity.</summary>
    [JsonConverter(typeof(FusionTextBoxStringEnumConverter<RemovableAttribute>))]
    public enum RemovableAttribute
    {
        /// <summary>The disabled editing lock.</summary>
        [JsonStringEnumMemberName("disabled")]
        Disabled,

        /// <summary>The read-only editing restriction.</summary>
        [JsonStringEnumMemberName("readonly")]
        Readonly,

        /// <summary>The placeholder text.</summary>
        [JsonStringEnumMemberName("placeholder")]
        Placeholder,

        /// <summary>The explicitly supplied accessible label.</summary>
        [JsonStringEnumMemberName("aria-label")]
        AccessibleName,
    }

    /// <summary>The floating-label modes supported by the box.</summary>
    [JsonConverter(typeof(FusionTextBoxStringEnumConverter<LabelMode>))]
    public enum LabelMode
    {
        /// <summary>Use an inline placeholder.</summary>
        [JsonStringEnumMemberName("Never")]
        Never,

        /// <summary>Keep the caption above the field.</summary>
        [JsonStringEnumMemberName("Always")]
        Always,

        /// <summary>Raise the caption on focus or while the field holds text.</summary>
        [JsonStringEnumMemberName("Auto")]
        Auto,
    }

    /// <summary>The information supplied when a text box asks for a prefix or suffix template.</summary>
    public sealed class TemplateArgs : JsObject
    {
        /// <summary>Gets the template name supplied by EJ2. Both sides of a text box use the name template.</summary>
        public ReadOnly<string> TemplateName => Argument<string>(2);
    }

    /// <summary>The original browser event that caused a text box notification.</summary>
    [Mirrors("lib.dom", "Event")]
    public sealed class BrowserEvent : JsObject
    {
        /// <summary>Gets the browser's event type.</summary>
        public ReadOnly<string> Type => ReadOnly<string>();
    }

    /// <summary>The terminal notification from the MVC text box.</summary>
    public sealed class DestructionEventArgs : JsObject
    {
        /// <summary>Gets the notification name.</summary>
        public ReadOnly<string> Name => ReadOnly<string>();

        /// <summary>Gets the false flag supplied on destruction. The notification cannot cancel destruction.</summary>
        public ReadOnly<bool> Cancel => ReadOnly<bool>();
    }

    /// <summary>
    /// The side of the box that receives the icons.
    /// </summary>
    [JsonConverter(typeof(FusionTextBoxStringEnumConverter<IconPosition>))]
    public enum IconPosition
    {
        /// <summary>After the box's text.</summary>
        [JsonStringEnumMemberName("append")]
        Append,

        /// <summary>Before the box's text.</summary>
        [JsonStringEnumMemberName("prepend")]
        Prepend,
    }

    /// <summary>
    /// What a listener of <see cref="Input"/> receives.
    /// </summary>
    [Mirrors("@syncfusion/ej2-inputs", "InputEventArgs")]
    public sealed class InputEventArgs : JsObject
    {
        /// <summary>Gets the wrapper of the field that raised the event.</summary>
        public ChildElement Container => LiveObject<ChildElement>(optional: true);

        /// <summary>Gets the original browser event.</summary>
        public BrowserEvent OriginalEvent => LiveObject<BrowserEvent>(name: "event", optional: true);

        /// <summary>Gets the text the box holds after the keystroke.</summary>
        public ReadOnly<string> Value => ReadOnly<string>(optional: true);

        /// <summary>Gets the text the box held before the keystroke.</summary>
        public ReadOnly<string?> PreviousValue => ReadOnly<string?>(optional: true);
    }

    /// <summary>
    /// What a listener of <see cref="Change"/> receives.
    /// </summary>
    [Mirrors("@syncfusion/ej2-inputs", "ChangedEventArgs")]
    public sealed class ChangedEventArgs : JsObject
    {
        /// <summary>Gets the wrapper of the field that raised the event.</summary>
        public ChildElement Container => LiveObject<ChildElement>(optional: true);

        /// <summary>Gets the original browser event, which a programmatic change does not supply.</summary>
        public BrowserEvent OriginalEvent => LiveObject<BrowserEvent>(name: "event", optional: true, nullable: true);

        /// <summary>Gets the text the box holds after the change.</summary>
        public ReadOnly<string?> Value => ReadOnly<string?>(optional: true);

        /// <summary>Gets the text the box held before the change.</summary>
        public ReadOnly<string?> PreviousValue => ReadOnly<string?>(optional: true);

        /// <summary>Gets whether a person made the change; a write reads false.</summary>
        public ReadOnly<bool> IsInteracted => ReadOnly<bool>(optional: true);
    }

    /// <summary>
    /// What a listener of <see cref="Focus"/> receives.
    /// </summary>
    [Mirrors("@syncfusion/ej2-inputs", "FocusInEventArgs")]
    public sealed class FocusInEventArgs : JsObject
    {
        /// <summary>Gets the wrapper of the field that raised the event.</summary>
        public ChildElement Container => LiveObject<ChildElement>(optional: true);

        /// <summary>Gets the original browser event.</summary>
        public BrowserEvent OriginalEvent => LiveObject<BrowserEvent>(name: "event", optional: true);

        /// <summary>Gets the text the box holds.</summary>
        public ReadOnly<string?> Value => ReadOnly<string?>(optional: true);
    }

    /// <summary>
    /// What a listener of <see cref="Blur"/> receives.
    /// </summary>
    [Mirrors("@syncfusion/ej2-inputs", "FocusOutEventArgs")]
    public sealed class FocusOutEventArgs : JsObject
    {
        /// <summary>Gets the wrapper of the field that raised the event.</summary>
        public ChildElement Container => LiveObject<ChildElement>(optional: true);

        /// <summary>Gets the original browser event.</summary>
        public BrowserEvent OriginalEvent => LiveObject<BrowserEvent>(name: "event", optional: true);

        /// <summary>Gets the text the box holds.</summary>
        public ReadOnly<string?> Value => ReadOnly<string?>(optional: true);
    }
}
