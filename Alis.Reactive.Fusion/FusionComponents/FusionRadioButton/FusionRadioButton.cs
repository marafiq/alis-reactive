using Alis.Reactive.Declarations;
using Alis.Reactive.Elements;

namespace Alis.Reactive.Fusion.FusionComponents;

/// <summary>
/// A radio button: EJ2's RadioButton, one choice of a group whose buttons share a name.
/// </summary>
/// <remarks>EJ2 raises <see cref="Change"/> when a person chooses the button.</remarks>
[Mirrors("@syncfusion/ej2-buttons", "RadioButton")]
public sealed class FusionRadioButton : FusionComponent
{
    /// <summary>Gets whether the button is chosen.</summary>
    public Property<bool> Checked => Property<bool>();

    /// <summary>Gets the CSS classes applied to the radio wrapper.</summary>
    public Property<string> CssClass => Property<string>();

    /// <summary>Gets whether the button is disabled.</summary>
    public Property<bool> Disabled => Property<bool>();

    /// <summary>Gets the native radio input.</summary>
    public ChildElement Element => LiveObject<ChildElement>();

    /// <summary>Gets whether EJ2 filters the markup when it creates the caption.</summary>
    /// <remarks>Changing an existing caption writes plain text. Refresh to recreate the caption from its settings.</remarks>
    public Property<bool> EnableHtmlSanitizer => Property<bool>();

    /// <summary>Gets whether the radio remembers its checked state in this browser.</summary>
    /// <remarks>Refresh after enabling persistence to attach its page-exit handler. Clearing the setting leaves an attached handler in place.</remarks>
    public Property<bool> EnablePersistence => Property<bool>();

    /// <summary>Gets whether the label uses a right-to-left layout.</summary>
    public Property<bool> EnableRtl => Property<bool>();

    /// <summary>Gets the caption that describes the choice.</summary>
    public Property<string> Label => Property<string>();

    /// <summary>Gets whether the caption precedes or follows the radio.</summary>
    public Property<FusionRadioButtonLabelPosition> LabelPosition => Property<FusionRadioButtonLabelPosition>();

    /// <summary>Gets the name shared by the radio buttons in this choice group.</summary>
    public Property<string> Name => Property<string>();

    /// <summary>Gets the configured value carried by the radio's change notification.</summary>
    /// <remarks>An unconfigured value is empty. A value supplied in EJ2's HTML attributes can keep the native form value when this property changes.</remarks>
    public Property<string> Value => Property<string>();

    /// <summary>Applies the changes written to the button's properties now.</summary>
    public void DataBind() => Method();

    /// <summary>Reads the native value of the chosen input in this radio group.</summary>
    /// <returns>The checked input's native value, or an empty string when none is chosen. A checked input without a value attribute returns <c>on</c>.</returns>
    public MethodResult<string> GetSelectedValue() => Method<string>();

    /// <summary>Clicks the button, as a person does.</summary>
    public void Click() => Method();

    /// <summary>Moves the keyboard's focus to the button.</summary>
    public void FocusIn() => Method();

    /// <summary>Gets the native radio input returned by EJ2.</summary>
    public ChildElement GetRootElement() => ReturnedObject<ChildElement>();

    /// <summary>Rebuilds the radio control using its current settings.</summary>
    public void Refresh() => Method();

    /// <summary>Removes the enhanced radio and releases its original input, unchecked.</summary>
    public void Destroy() => Method();

    /// <summary>Gets the event raised when a person chooses the button.</summary>
    public Event<ChangeArgs> Change => Event<ChangeArgs>();

    /// <summary>
    /// What a listener of <see cref="Change"/> receives.
    /// </summary>
    [Mirrors("@syncfusion/ej2-buttons", "ChangeArgs")]
    public sealed class ChangeArgs : JsObject
    {
        /// <summary>Gets the name of the EJ2 notification.</summary>
        public ReadOnly<string> Name => ReadOnly<string>(optional: true);

        /// <summary>Gets the original browser event that reported the selection.</summary>
        public SelectionEvent OriginalEvent => LiveObject<SelectionEvent>(name: "event", optional: true);

        /// <summary>Gets the configured value of the chosen button, including an empty string for an uncoded option.</summary>
        public ReadOnly<string> Value => ReadOnly<string>(optional: true);
    }

    /// <summary>The browser event that delivered a radio selection.</summary>
    [Mirrors("lib.dom", "Event")]
    public sealed class SelectionEvent : JsObject
    {
        /// <summary>Gets whether the browser dispatched the selection event.</summary>
        public ReadOnly<bool> IsTrusted => ReadOnly<bool>();

        /// <summary>Gets the type of browser event.</summary>
        public ReadOnly<string> Type => ReadOnly<string>();
    }
}
