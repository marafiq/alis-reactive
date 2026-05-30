namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Event payload delivered when a Stepper step is clicked.
    /// </summary>
    public sealed class FusionStepperClickArgs
    {
        /// <summary>The index of the previous step.</summary>
        public int PreviousStep { get; set; }

        /// <summary>The index of the current step.</summary>
        public int ActiveStep { get; set; }
    }
}
