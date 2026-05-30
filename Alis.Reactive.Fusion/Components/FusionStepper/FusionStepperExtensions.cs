using Alis.Reactive.Builders.Conditions;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Typed post-render navigation methods for <see cref="FusionStepper"/>.
    /// </summary>
    public static class FusionStepperExtensions
    {
        private static readonly ComponentMethod NextStepMethod =
            ComponentMethod.Named("nextStep");

        private static readonly ComponentMethod PreviousStepMethod =
            ComponentMethod.Named("previousStep");

        private static readonly ComponentMethod ResetMethod =
            ComponentMethod.Named("reset");

        private static readonly ComponentMethod RefreshProgressbarMethod =
            ComponentMethod.Named("refreshProgressbar");

        private static readonly ComponentProperty<int> ActiveStepProperty =
            ComponentProperty<int>.Named("activeStep");

        /// <summary>
        /// Reads the current active step.
        /// </summary>
        public static TypedComponentSource<int> ActiveStep<TModel>(
            this ComponentRef<FusionStepper, TModel> self)
            where TModel : class
            => self.Read(ActiveStepProperty);

        /// <summary>
        /// Updates the active step.
        /// </summary>
        public static ComponentRef<FusionStepper, TModel> SetActiveStep<TModel>(
            this ComponentRef<FusionStepper, TModel> self,
            int activeStep)
            where TModel : class
            => self.EmitSet(ActiveStepProperty, ValueExpression.Literal(activeStep));

        /// <summary>
        /// Advances to the next step.
        /// </summary>
        public static ComponentRef<FusionStepper, TModel> NextStep<TModel>(
            this ComponentRef<FusionStepper, TModel> self)
            where TModel : class
            => self.EmitCall(NextStepMethod);

        /// <summary>
        /// Moves to the previous step.
        /// </summary>
        public static ComponentRef<FusionStepper, TModel> PreviousStep<TModel>(
            this ComponentRef<FusionStepper, TModel> self)
            where TModel : class
            => self.EmitCall(PreviousStepMethod);

        /// <summary>
        /// Resets the stepper to the first step.
        /// </summary>
        public static ComponentRef<FusionStepper, TModel> Reset<TModel>(
            this ComponentRef<FusionStepper, TModel> self)
            where TModel : class
            => self.EmitCall(ResetMethod);

        /// <summary>
        /// Recomputes the progress bar after dynamic state changes.
        /// </summary>
        public static ComponentRef<FusionStepper, TModel> RefreshProgressbar<TModel>(
            this ComponentRef<FusionStepper, TModel> self)
            where TModel : class
            => self.EmitCall(RefreshProgressbarMethod);
    }
}
