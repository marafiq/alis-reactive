using System;
#if NET48
using System.Web.Mvc;
#else
using Microsoft.AspNetCore.Mvc.Rendering;
#endif
using Syncfusion.EJ2;
using Syncfusion.EJ2.Navigations;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// The Syncfusion Stepper, declared by the names its JavaScript API uses.
    /// </summary>
    /// <remarks>
    /// Render it with <c>Html.FusionStepper(plan, "admission-steps", b =&gt; ...)</c>, reach it with
    /// <c>var admissionSteps = p.Component&lt;FusionStepper&gt;("admission-steps");</c> and use its members directly:
    /// <c>admissionSteps.NextStep();</c> or <c>p.When(admissionSteps.ActiveStep).Eq(2)</c>.
    /// </remarks>
    public sealed class FusionStepper : FusionComponent
    {
        /// <summary>The zero-based index of the active step.</summary>
        public Property<int> ActiveStep => Property<int>();

        /// <summary>Moves to the next step.</summary>
        public void NextStep() => Method();

        /// <summary>Moves to the previous step.</summary>
        public void PreviousStep() => Method();

        /// <summary>Returns to the first step and clears step states.</summary>
        public void Reset() => Method();

        /// <summary>Redraws the progress bar after the steps change.</summary>
        public void RefreshProgressbar() => Method();

        /// <summary>Raised after the active step changes.</summary>
        public Event<FusionStepperChangedArgs> StepChanged =>
            Event<FusionStepperChangedArgs>();

        /// <summary>Raised before the active step changes.</summary>
        public Event<FusionStepperChangingArgs> StepChanging =>
            Event<FusionStepperChangingArgs>();

        /// <summary>Raised when the user clicks a step.</summary>
        public Event<FusionStepperClickArgs> StepClick =>
            Event<FusionStepperClickArgs>();
    }

    /// <summary>Payload of <see cref="FusionStepper.StepChanged"/>.</summary>
    public class FusionStepperChangedArgs : JsObject
    {
        /// <summary>Index of the step active before the change.</summary>
        public ReadOnly<int> PreviousStep => ReadOnly<int>();

        /// <summary>Index of the step active after the change.</summary>
        public ReadOnly<int> ActiveStep => ReadOnly<int>();

        /// <summary>Whether user interaction made the change.</summary>
        public ReadOnly<bool> IsInteracted => ReadOnly<bool>();
    }

    /// <summary>Payload of <see cref="FusionStepper.StepChanging"/>.</summary>
    public class FusionStepperChangingArgs : JsObject
    {
        /// <summary>Index of the step active now.</summary>
        public ReadOnly<int> PreviousStep => ReadOnly<int>();

        /// <summary>Index of the step the change moves to.</summary>
        public ReadOnly<int> ActiveStep => ReadOnly<int>();

        /// <summary>Whether user interaction started the change.</summary>
        public ReadOnly<bool> IsInteracted => ReadOnly<bool>();

        /// <summary>Set to <see langword="true"/> with <c>args.Cancel.Set(true)</c> to stop the action.</summary>
        public Property<bool> Cancel => Property<bool>();
    }

    /// <summary>Payload of <see cref="FusionStepper.StepClick"/>.</summary>
    public class FusionStepperClickArgs : JsObject
    {
        /// <summary>Index of the step active before the click.</summary>
        public ReadOnly<int> PreviousStep => ReadOnly<int>();

        /// <summary>Index of the clicked step.</summary>
        public ReadOnly<int> ActiveStep => ReadOnly<int>();
    }

    /// <summary>
    /// Renders the component for a plan, ready for <c>.Reactive(...)</c>.
    /// </summary>
    public static class FusionStepperHtmlExtensions
    {
        /// <summary>
        /// Creates a Syncfusion Stepper and carries its controlled component ID into the Reactive Plan.
        /// </summary>
        /// <typeparam name="TModel">The view's model type.</typeparam>
        /// <param name="html">The view's HTML helper.</param>
        /// <param name="plan">Reactive Plan receiving stepper event wiring.</param>
        /// <param name="elementId">Controlled component ID shared by markup and Reactive Plan behavior.</param>
        /// <param name="build">Configures the component before rendering.</param>
        public static RenderedComponent<global::Alis.Reactive.Fusion.Components.FusionStepper, TModel> FusionStepper<TModel>(
#if NET48
            this HtmlHelper<TModel> html,
#else
            this IHtmlHelper<TModel> html,
#endif
            ReactivePlan<TModel> plan,
            string elementId,
            Action<StepperBuilder> build)
            where TModel : class
        {
            var builder = html.EJS().Stepper(elementId);
            build(builder);

            return plan.Render<global::Alis.Reactive.Fusion.Components.FusionStepper>(elementId, builder.Render());
        }
    }
}
