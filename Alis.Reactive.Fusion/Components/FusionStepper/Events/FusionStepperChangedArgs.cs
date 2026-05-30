namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Event payload delivered after a Stepper step changes.
    /// </summary>
    public sealed class FusionStepperChangedArgs
    {
        /// <summary>The index of the previous step.</summary>
        public int PreviousStep { get; set; }

        /// <summary>The index of the current step.</summary>
        public int ActiveStep { get; set; }

        /// <summary>Whether the change was triggered by user interaction.</summary>
        public bool IsInteracted { get; set; }
    }
}
