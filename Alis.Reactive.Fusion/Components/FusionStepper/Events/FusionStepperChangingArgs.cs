using Alis.Reactive.Builders;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Event payload delivered before a Stepper step changes.
    /// </summary>
    public sealed class FusionStepperChangingArgs
    {
        /// <summary>The index of the previous step.</summary>
        public int PreviousStep { get; set; }

        /// <summary>The index of the target step.</summary>
        public int ActiveStep { get; set; }

        /// <summary>Whether the change was triggered by user interaction.</summary>
        public bool IsInteracted { get; set; }

        /// <summary>Whether Syncfusion should cancel the step transition.</summary>
        public bool Cancel { get; set; }
    }

    /// <summary>
    /// Typed mutations on the stepChanging event args for <see cref="FusionStepper"/>.
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
