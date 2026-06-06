using Alis.Reactive.Builders;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Event payload delivered before a Stepper step changes.
    /// </summary>
    public sealed class FusionStepperChangingArgs
    {
        /// <summary>Zero-based index of the step that is active before the transition.</summary>
        public int PreviousStep { get; set; }

        /// <summary>Zero-based index of the step targeted by the pending transition.</summary>
        public int ActiveStep { get; set; }

        /// <summary>Whether user interaction triggered the pending step transition.</summary>
        public bool IsInteracted { get; set; }

        /// <summary>Whether Syncfusion should cancel the step transition.</summary>
        public bool Cancel { get; set; }
    }

    /// <summary>
    /// Typed event-payload operations for the stepChanging event args of <see cref="FusionStepper"/>.
    /// </summary>
    public static class FusionStepperChangingArgsExtensions
    {
        /// <summary>Cancels the pending step transition.</summary>
        public static void PreventDefault(
            this FusionStepperChangingArgs args,
            IReactionEmitter pipeline)
        {
            pipeline.AddStep(ReactionGraph.Set(PayloadSource.Event(), "cancel", ValueExpression.Literal(true)));
        }
    }
}
