namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Event payload delivered after a Stepper step changes.
    /// </summary>
    public sealed class FusionStepperChangedArgs
    {
        /// <summary>Zero-based index of the step that was active before the change.</summary>
        public int PreviousStep { get; set; }

        /// <summary>Zero-based index of the active step after the change.</summary>
        public int ActiveStep { get; set; }

        /// <summary>Whether user interaction triggered the completed step change.</summary>
        public bool IsInteracted { get; set; }
    }
}
