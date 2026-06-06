namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Event payload delivered when a Stepper step is clicked.
    /// </summary>
    public sealed class FusionStepperClickArgs
    {
        /// <summary>Zero-based index of the step that was active before the click.</summary>
        public int PreviousStep { get; set; }

        /// <summary>Zero-based index of the step associated with the click.</summary>
        public int ActiveStep { get; set; }
    }
}
