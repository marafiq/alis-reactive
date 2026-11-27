using Alis.Reactive.Declarations;
using Alis.Reactive.Elements;
using Alis.Reactive.InputsAndIds;

namespace Alis.Reactive.Fusion.FusionComponents;

/// <summary>
/// A check box: EJ2's CheckBox, which holds whether a member of <see cref="bool"/> is true.
/// </summary>
/// <remarks>
/// A check box bound to a member is an input: a request container sends its <see cref="Checked"/> and a check reads it. EJ2 raises
/// <see cref="Change"/> when a person checks or clears it.
/// </remarks>
[Mirrors("@syncfusion/ej2-buttons", "CheckBox")]
public sealed class FusionCheckBox : FusionComponent, IInputComponent
{
    /// <summary>Gets whether the box is checked.</summary>
    public Property<bool> Checked => Property<bool>();

    /// <summary>Gets whether the box shows neither checked nor cleared.</summary>
    public Property<bool> Indeterminate => Property<bool>();

    /// <summary>Gets whether the box is disabled.</summary>
    public Property<bool> Disabled => Property<bool>();

    /// <summary>Gets the classes applied to the check box wrapper.</summary>
    public Property<string> CssClass => Property<string>();

    /// <summary>Gets the native input owned by the check box.</summary>
    public ChildElement Element => LiveObject<ChildElement>();

    /// <summary>Gets whether EJ2 sanitizes the HTML of a caption when it renders that caption.</summary>
    public Property<bool> EnableHtmlSanitizer => Property<bool>();

    /// <summary>Gets whether the browser remembers the checked and indeterminate states.</summary>
    /// <remarks>Refresh after enabling persistence to attach EJ2's page-exit listener. Clearing the setting leaves an attached listener active.</remarks>
    public Property<bool> EnablePersistence => Property<bool>();

    /// <summary>Gets whether the box uses right-to-left layout.</summary>
    public Property<bool> EnableRtl => Property<bool>();

    /// <summary>Gets the caption displayed next to the mark.</summary>
    public Property<string> Label => Property<string>();

    /// <summary>Gets where the caption appears relative to the mark.</summary>
    public Property<FusionCheckBoxLabelPosition> LabelPosition => Property<FusionCheckBoxLabelPosition>();

    /// <summary>Gets the text a native form sends for the checked input.</summary>
    public Property<string> Value => Property<string>();

    /// <summary>Applies the changes written to the box's properties now.</summary>
    public void DataBind() => Method();

    /// <summary>Clicks the box, as a person does.</summary>
    public void Click() => Method();

    /// <summary>Moves the keyboard's focus to the box.</summary>
    public void FocusIn() => Method();

    /// <summary>Gets the root element returned by the box.</summary>
    public ChildElement GetRootElement() => ReturnedObject<ChildElement>();

    /// <summary>Rebuilds the check box with its current settings.</summary>
    public void Refresh() => Method();

    /// <summary>Removes the enhanced box and clears its original input.</summary>
    public void Destroy() => Method();

    /// <summary>Gets the event raised when a person checks or clears the box.</summary>
    public Event<ChangeEventArgs> Change => Event<ChangeEventArgs>();

    ComponentInput IInputComponent.Input => ComponentInput.Of(Checked, committedBy: Change);

    /// <summary>
    /// What a listener of <see cref="Change"/> receives.
    /// </summary>
    [Mirrors("@syncfusion/ej2-buttons", "ChangeEventArgs")]
    public sealed class ChangeEventArgs : JsObject
    {
        /// <summary>Gets whether the box is checked after the change.</summary>
        public ReadOnly<bool> Checked => ReadOnly<bool>(optional: true);

        /// <summary>Gets the category of the vendor notification.</summary>
        public ReadOnly<string> Name => ReadOnly<string>(optional: true);

        /// <summary>Gets the browser event that activated the box when EJ2 supplies one.</summary>
        public ActivationEvent OriginalEvent => LiveObject<ActivationEvent>(name: "event", optional: true, nullable: true);
    }

    /// <summary>The original browser activation that caused a change.</summary>
    [Mirrors("lib.dom", "Event")]
    public sealed class ActivationEvent : JsObject
    {
        /// <summary>Gets whether the browser dispatched the event as a trusted user activation.</summary>
        public ReadOnly<bool> IsTrusted => ReadOnly<bool>();
    }
}
