namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Typed events exposed by the <see cref="FusionStepper"/> component.
    /// </summary>
    public sealed class FusionStepperEvents
    {
        public static readonly FusionStepperEvents Instance = new FusionStepperEvents();

        private FusionStepperEvents()
        {
        }

        /// <summary>Fires after the active step changes.</summary>
        public TypedEvent<FusionStepperChangedArgs> StepChanged =>
            new TypedEvent<FusionStepperChangedArgs>("stepChanged", new FusionStepperChangedArgs());

        /// <summary>Fires before the active step changes.</summary>
        public TypedEvent<FusionStepperChangingArgs> StepChanging =>
            new TypedEvent<FusionStepperChangingArgs>("stepChanging", new FusionStepperChangingArgs());

        /// <summary>Fires when a step is clicked.</summary>
        public TypedEvent<FusionStepperClickArgs> StepClick =>
            new TypedEvent<FusionStepperClickArgs>("stepClick", new FusionStepperClickArgs());
    }
}
