using Alis.Reactive.Declarations;

namespace Alis.Reactive.Fusion.FusionComponents;

/// <summary>
/// A stepper: EJ2's Stepper, which shows the steps of a task and the step a person is on.
/// </summary>
[Mirrors("@syncfusion/ej2-navigations", "Stepper")]
public sealed class FusionStepper : FusionComponent
{
    /// <summary>Gets the index of the step the stepper is on, from 0.</summary>
    public Property<int> ActiveStep => Property<int>();

    /// <summary>Moves the stepper to its next step.</summary>
    public void NextStep() => Method();

    /// <summary>Moves the stepper to its previous step.</summary>
    public void PreviousStep() => Method();

    /// <summary>Moves the stepper back to its first step, as it started.</summary>
    public void Reset() => Method();

    /// <summary>Places the stepper's progress bar again, after the size of the element that holds it changed.</summary>
    public void RefreshProgressbar() => Method();

    /// <summary>Gets the event raised after the stepper's step changed.</summary>
    public Event<StepperChangedEventArgs> StepChanged => Event<StepperChangedEventArgs>();

    /// <summary>Gets the event raised before the stepper's step changes.</summary>
    public Event<StepperChangingEventArgs> StepChanging => Event<StepperChangingEventArgs>();

    /// <summary>Gets the event raised when a person clicks a step.</summary>
    public Event<StepperClickEventArgs> StepClick => Event<StepperClickEventArgs>();

    /// <summary>
    /// What a listener of <see cref="StepChanged"/> receives.
    /// </summary>
    [Mirrors("@syncfusion/ej2-navigations", "StepperChangedEventArgs")]
    public sealed class StepperChangedEventArgs : JsObject
    {
        /// <summary>Gets the index of the step the stepper was on.</summary>
        public ReadOnly<int> PreviousStep => ReadOnly<int>();

        /// <summary>Gets the index of the step the stepper is on.</summary>
        public ReadOnly<int> ActiveStep => ReadOnly<int>();

        /// <summary>Gets whether a person made the change; a write reads false.</summary>
        public ReadOnly<bool> IsInteracted => ReadOnly<bool>();
    }

    /// <summary>
    /// What a listener of <see cref="StepChanging"/> receives.
    /// </summary>
    [Mirrors("@syncfusion/ej2-navigations", "StepperChangingEventArgs")]
    public sealed class StepperChangingEventArgs : JsObject
    {
        /// <summary>Gets the index of the step the stepper is on.</summary>
        public ReadOnly<int> PreviousStep => ReadOnly<int>();

        /// <summary>Gets the index of the step the stepper moves to.</summary>
        public ReadOnly<int> ActiveStep => ReadOnly<int>();

        /// <summary>Gets whether a person made the change; a write reads false.</summary>
        public ReadOnly<bool> IsInteracted => ReadOnly<bool>();

        /// <summary>Gets whether the stepper stays on its step; write true to keep it there.</summary>
        public Property<bool> Cancel => Property<bool>();
    }

    /// <summary>
    /// What a listener of <see cref="StepClick"/> receives.
    /// </summary>
    [Mirrors("@syncfusion/ej2-navigations", "StepperClickEventArgs")]
    public sealed class StepperClickEventArgs : JsObject
    {
        /// <summary>Gets the index of the step the stepper was on.</summary>
        public ReadOnly<int> PreviousStep => ReadOnly<int>();

        /// <summary>Gets the index of the step the person clicked.</summary>
        public ReadOnly<int> ActiveStep => ReadOnly<int>();
    }
}
