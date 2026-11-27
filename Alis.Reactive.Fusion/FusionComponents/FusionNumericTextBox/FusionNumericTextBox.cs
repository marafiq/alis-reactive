using Alis.Reactive.Declarations;
using Alis.Reactive.Elements;
using Alis.Reactive.InputsAndIds;
using Alis.Reactive.ValuesAndSources;

namespace Alis.Reactive.Fusion.FusionComponents;

/// <summary>
/// A numeric text box: EJ2's NumericTextBox, which holds a number a person types or steps with its spin buttons.
/// </summary>
/// <remarks>
/// A box bound to a member is an input: a request container sends its <see cref="Value"/> and a check reads it. A handler of
/// <see cref="Change"/> reads the committed number.
/// </remarks>
/// <typeparam name="TValue">The type of the member the box binds, a number that admits null, since the box holds null until set.</typeparam>
[Mirrors("@syncfusion/ej2-inputs", "NumericTextBox")]
public sealed class FusionNumericTextBox<TValue> : FusionComponent, IInputComponent
{
    /// <summary>Gets the number the box holds, null until set.</summary>
    public Property<TValue> Value => Property<TValue>();

    /// <summary>Gets the lower bound. EJ2 uses its most negative Number when no bound is configured.</summary>
    /// <remarks>Use a configured bound when sending this value to an integer or decimal model.</remarks>
    public Property<TValue> Min => Property<TValue>();

    /// <summary>Gets the upper bound. EJ2 uses its largest Number when no bound is configured.</summary>
    /// <remarks>Use a configured bound when sending this value to an integer or decimal model.</remarks>
    public Property<TValue> Max => Property<TValue>();

    /// <summary>Gets the amount used by the spin controls and by a step call with no amount.</summary>
    public Property<double> Step => Property<double>();

    /// <summary>Gets whether a committed value is clamped to the configured bounds.</summary>
    public Property<bool> StrictMode => Property<bool>();

    /// <summary>Applies the current property writes to the editor immediately.</summary>
    public void DataBind() => Method();

    /// <summary>Gets whether the focused editor steps in response to the mouse wheel.</summary>
    public Property<bool> AllowMouseWheel => Property<bool>();

    /// <summary>Gets whether the editor accepts input and focus.</summary>
    public Property<bool> Enabled => Property<bool>();

    /// <summary>Gets whether the editor prevents direct edits while retaining focus.</summary>
    public Property<bool> Readonly => Property<bool>();

    /// <summary>Gets whether the editor offers spin controls.</summary>
    public Property<bool> ShowSpinButton => Property<bool>();

    /// <summary>Gets whether the editor offers a clear button.</summary>
    public Property<bool> ShowClearButton => Property<bool>();

    /// <summary>Gets whether EJ2 registers the current count for page-exit recovery when the editor mounts.</summary>
    public Property<bool> EnablePersistence => Property<bool>();

    /// <summary>Gets whether the editor uses right-to-left layout.</summary>
    public Property<bool> EnableRtl => Property<bool>();

    /// <summary>Gets whether the editor limits fractional digits while a person types.</summary>
    public Property<bool> ValidateDecimalOnType => Property<bool>();

    /// <summary>Gets the currency code used with a currency format.</summary>
    public Property<string?> Currency => Property<string?>();

    /// <summary>Gets the CSS classes applied to the editor wrapper.</summary>
    public Property<string?> CssClass => Property<string?>();

    /// <summary>Gets the hint shown while the editor is empty.</summary>
    public Property<string?> Placeholder => Property<string?>();

    /// <summary>Gets the number format used to display the committed value.</summary>
    public Property<string> Format => Property<string>();

    /// <summary>Gets the locale used for number formatting and localized prompts.</summary>
    public Property<string> Locale => Property<string>();

    /// <summary>Gets the permitted fractional digits, or null for unrestricted precision.</summary>
    public Property<double?> Decimals => Property<double?>();

    /// <summary>Gets when the field caption floats above the editor.</summary>
    public Property<FusionNumericLabelMode> FloatLabelType => Property<FusionNumericLabelMode>();

    /// <summary>Sets the CSS length of the editor wrapper. A null leaves the existing width in place.</summary>
    public WriteOnly<string?> Width => WriteOnly<string?>();

    /// <summary>Sets the editor width in pixels. A null leaves the existing width in place.</summary>
    public WriteOnly<double?> WidthInPixels => WriteOnly<double?>(name: "width");

    /// <summary>Sets the markup or template selector displayed before the field. A null removes the prefix.</summary>
    public WriteOnly<string?> PrependTemplate => WriteOnly<string?>();

    /// <summary>Sets the markup or template selector displayed after the field. A null removes the suffix.</summary>
    public WriteOnly<string?> AppendTemplate => WriteOnly<string?>();

    /// <summary>Sets the function that returns app-authored HTML before the number.</summary>
    public Callback<TemplateArgs, string> PrependTemplateCallback => Callback<TemplateArgs, string>(name: "prependTemplate");

    /// <summary>Sets the function that returns app-authored HTML after the number.</summary>
    public Callback<TemplateArgs, string> AppendTemplateCallback => Callback<TemplateArgs, string>(name: "appendTemplate");

    /// <summary>Gets the original numeric input element.</summary>
    public ChildElement Element => LiveObject<ChildElement>();

    /// <summary>Gets the component's root input element.</summary>
    public ChildElement GetRootElement() => ReturnedObject<ChildElement>();

    /// <summary>Gets the field's current displayed text.</summary>
    public MethodResult<string> GetText() => Method<string>();

    /// <summary>Gets the vendor recovery snapshot of the current number.</summary>
    public MethodResult<string> GetPersistData() => Method<string>();

    /// <summary>Rebuilds the editor with its current settings.</summary>
    public void Refresh() => Method();

    /// <summary>Closes the editor and restores its original input.</summary>
    public void Destroy() => Method();

    /// <summary>Gets the terminal event raised when the enhanced editor closes.</summary>
    public Event<DestructionEventArgs> Destroyed => Event<DestructionEventArgs>();

    /// <summary>Moves the keyboard's focus to the box.</summary>
    public void FocusIn() => Method();

    /// <summary>Takes the keyboard's focus away from the box.</summary>
    public void FocusOut() => Method();

    /// <summary>Raises the box's number by one step.</summary>
    public void Increment() => Method();

    /// <summary>Raises the box's number by the supplied step.</summary>
    /// <param name="step">The amount to add.</param>
    public void Increment(Arg<double> step) => Method([step]);

    /// <summary>Lowers the box's number by one step.</summary>
    public void Decrement() => Method();

    /// <summary>Lowers the box's number by the supplied step.</summary>
    /// <param name="step">The amount to subtract.</param>
    public void Decrement(Arg<double> step) => Method([step]);

    /// <summary>
    /// Gets the event raised when the box's number changes: an edit when the box loses focus, a spin button, a scroll
    /// over the focused box or a write of <see cref="Value"/>.
    /// </summary>
    public Event<ChangeEventArgs> Change => Event<ChangeEventArgs>();

    /// <summary>Gets the event raised when the box gets focus.</summary>
    public Event<NumericFocusEventArgs> Focus => Event<NumericFocusEventArgs>();

    /// <summary>Gets the event raised when the box loses focus.</summary>
    public Event<NumericBlurEventArgs> Blur => Event<NumericBlurEventArgs>();

    ComponentInput IInputComponent.Input => ComponentInput.Of(Value, committedBy: Change);

    /// <summary>The arguments common to a numeric prefix or suffix template callback.</summary>
    public sealed class TemplateArgs : JsObject
    {
        /// <summary>Gets the template name. EJ2 supplies template on both sides of this editor.</summary>
        public ReadOnly<string> TemplateName => Argument<string>(2);
    }

    /// <summary>The browser event that originated a numeric editor notification.</summary>
    [Mirrors("lib.dom", "Event")]
    public sealed class BrowserEvent : JsObject
    {
        /// <summary>Gets the browser event type.</summary>
        public ReadOnly<string> Type => ReadOnly<string>();
    }

    /// <summary>The terminal notification supplied by the MVC numeric editor.</summary>
    public sealed class DestructionEventArgs : JsObject
    {
        /// <summary>Gets the destruction notification's name.</summary>
        public ReadOnly<string> Name => ReadOnly<string>();

        /// <summary>Gets the false flag supplied by EJ2 when closing the editor.</summary>
        public ReadOnly<bool> Cancel => ReadOnly<bool>();
    }

    /// <summary>
    /// What a listener of <see cref="Change"/> receives.
    /// </summary>
    [Mirrors("@syncfusion/ej2-inputs", "ChangeEventArgs")]
    public sealed class ChangeEventArgs : JsObject
    {
        /// <summary>Gets the EJ2 notification name.</summary>
        public ReadOnly<string> Name => ReadOnly<string>(optional: true);

        /// <summary>Gets the original browser event, absent for a programmatic update.</summary>
        public BrowserEvent OriginalEvent => LiveObject<BrowserEvent>(name: "event", optional: true, nullable: true);

        /// <summary>Gets the number the box holds after the change, null when a person cleared it.</summary>
        public ReadOnly<TValue> Value => ReadOnly<TValue>(optional: true);

        /// <summary>Gets the number the box held before the change, null when it was clear.</summary>
        public ReadOnly<TValue> PreviousValue => ReadOnly<TValue>(optional: true);

        /// <summary>Gets whether a person made the change; a write reads false.</summary>
        public ReadOnly<bool> IsInteracted => ReadOnly<bool>(optional: true);
    }

    /// <summary>
    /// What a listener of <see cref="Focus"/> receives.
    /// </summary>
    [Mirrors("@syncfusion/ej2-inputs", "NumericFocusEventArgs")]
    public sealed class NumericFocusEventArgs : JsObject
    {
        /// <summary>Gets the EJ2 notification name.</summary>
        public ReadOnly<string> Name => ReadOnly<string>(optional: true);

        /// <summary>Gets the original browser event.</summary>
        public BrowserEvent OriginalEvent => LiveObject<BrowserEvent>(name: "event", optional: true);

        /// <summary>Gets the wrapper of the field that raised the notification.</summary>
        public ChildElement Container => LiveObject<ChildElement>(optional: true);

        /// <summary>Gets the committed number at the time of the notification, including null for an empty field.</summary>
        public ReadOnly<TValue> Value => ReadOnly<TValue>();
    }

    /// <summary>
    /// What a listener of <see cref="Blur"/> receives.
    /// </summary>
    [Mirrors("@syncfusion/ej2-inputs", "NumericBlurEventArgs")]
    public sealed class NumericBlurEventArgs : JsObject
    {
        /// <summary>Gets the EJ2 notification name.</summary>
        public ReadOnly<string> Name => ReadOnly<string>(optional: true);

        /// <summary>Gets the original browser event.</summary>
        public BrowserEvent OriginalEvent => LiveObject<BrowserEvent>(name: "event", optional: true);

        /// <summary>Gets the wrapper of the field that raised the notification.</summary>
        public ChildElement Container => LiveObject<ChildElement>(optional: true);

        /// <summary>Gets the committed number at the time of the notification, including null for an empty field.</summary>
        public ReadOnly<TValue> Value => ReadOnly<TValue>();
    }
}
