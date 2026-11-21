using Alis.Reactive.Declarations;
using Alis.Reactive.InputsAndIds;

namespace Alis.Reactive.Fusion.FusionComponents;

/// <summary>
/// A slider: EJ2's Slider, which holds a number on a scale, or two for a range.
/// </summary>
/// <remarks>
/// A slider bound to a member is an input: a form sends its <see cref="Value"/> and a check reads it. EJ2 raises
/// <see cref="Change"/> while a person drags a handle and <see cref="Changed"/> once the drag ends, which commits the
/// value.
/// </remarks>
/// <typeparam name="TValue">The type of the member the slider binds: a number for one handle, a list of numbers for a range, admitting null, since the slider holds null until set.</typeparam>
[Mirrors("@syncfusion/ej2-inputs", "Slider")]
public sealed class FusionSlider<TValue> : FusionComponent, IInputComponent
{
    /// <summary>Gets the value the slider holds: a number, or a list of two for a range.</summary>
    public Property<TValue> Value => Property<TValue>();

    /// <summary>Applies the changes written to the slider's properties now.</summary>
    public void DataBind() => Method();

    /// <summary>Gets the event raised each time the value changes while a person drags a handle.</summary>
    public Event<SliderChangeEventArgs> Change => Event<SliderChangeEventArgs>();

    /// <summary>Gets the event raised when the value changed and the drag ended.</summary>
    public Event<SliderChangeEventArgs> Changed => Event<SliderChangeEventArgs>();

    ComponentInput IInputComponent.Input => ComponentInput.Of(Value, committedBy: Changed);

    /// <summary>
    /// What a listener of <see cref="Change"/> and <see cref="Changed"/> receives.
    /// </summary>
    [Mirrors("@syncfusion/ej2-inputs", "SliderChangeEventArgs")]
    public sealed class SliderChangeEventArgs : JsObject
    {
        /// <summary>Gets the value the slider holds after the change.</summary>
        public ReadOnly<TValue> Value => ReadOnly<TValue>();

        /// <summary>Gets the value the slider held before the change.</summary>
        public ReadOnly<TValue> PreviousValue => ReadOnly<TValue>();

        /// <summary>Gets the value's text as the slider's tooltip shows it.</summary>
        public ReadOnly<string> Text => ReadOnly<string>(optional: true);

        /// <summary>Gets the action that changed the value.</summary>
        public ReadOnly<string> Action => ReadOnly<string>();

        /// <summary>Gets whether a person made the change; a write reads false.</summary>
        public ReadOnly<bool> IsInteracted => ReadOnly<bool>();
    }
}
