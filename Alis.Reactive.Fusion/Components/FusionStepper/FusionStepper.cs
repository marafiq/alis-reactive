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
    /// Render it with <c>Html.FusionStepper(plan, "admission-steps", b =&gt; ...)</c> and target it with
    /// <c>p.Component&lt;FusionStepper&gt;("admission-steps")</c>; every declared member works with <c>Set</c>,
    /// <c>Call</c> and <c>Get</c>, for example <c>.Call(c =&gt; c.NextStep)</c> or <c>.Get(c =&gt; c.ActiveStep)</c>.
    /// </remarks>
    public sealed class FusionStepper : FusionComponent
    {
        /// <summary>The zero-based index of the active step.</summary>
        public ComponentProperty<int> ActiveStep => Property<int>("activeStep");

        /// <summary>Moves to the next step.</summary>
        public ComponentMethod NextStep => Method("nextStep");

        /// <summary>Moves to the previous step.</summary>
        public ComponentMethod PreviousStep => Method("previousStep");

        /// <summary>Returns to the first step and clears step states.</summary>
        public ComponentMethod Reset => Method("reset");

        /// <summary>Redraws the progress bar after the steps change.</summary>
        public ComponentMethod RefreshProgressbar => Method("refreshProgressbar");

        /// <summary>Raised after the active step changes.</summary>
        public TypedEvent<FusionStepperChangedArgs> StepChanged =>
            Event<FusionStepperChangedArgs>("stepChanged");

        /// <summary>Raised before the active step changes.</summary>
        public TypedEvent<FusionStepperChangingArgs> StepChanging =>
            Event<FusionStepperChangingArgs>("stepChanging");

        /// <summary>Raised when the user clicks a step.</summary>
        public TypedEvent<FusionStepperClickArgs> StepClick =>
            Event<FusionStepperClickArgs>("stepClick");
    }

    /// <summary>Payload of <see cref="FusionStepper.StepChanged"/>.</summary>
    public class FusionStepperChangedArgs
    {
        /// <summary>Index of the step active before the change.</summary>
        public int PreviousStep { get; set; }

        /// <summary>Index of the step active after the change.</summary>
        public int ActiveStep { get; set; }

        /// <summary>Whether user interaction made the change.</summary>
        public bool IsInteracted { get; set; }
    }

    /// <summary>Payload of <see cref="FusionStepper.StepChanging"/>.</summary>
    public class FusionStepperChangingArgs
    {
        /// <summary>Index of the step active now.</summary>
        public int PreviousStep { get; set; }

        /// <summary>Index of the step the change moves to.</summary>
        public int ActiveStep { get; set; }

        /// <summary>Whether user interaction started the change.</summary>
        public bool IsInteracted { get; set; }

        /// <summary>Set to <see langword="true"/> with <c>p.Payload(args).Set(a =&gt; a.Cancel, true)</c> to stop the action.</summary>
        public bool Cancel { get; set; }
    }

    /// <summary>Payload of <see cref="FusionStepper.StepClick"/>.</summary>
    public class FusionStepperClickArgs
    {
        /// <summary>Index of the step active before the click.</summary>
        public int PreviousStep { get; set; }

        /// <summary>Index of the clicked step.</summary>
        public int ActiveStep { get; set; }
    }

    /// <summary>
    /// Renders the component for a plan, ready for <c>.Reactive(...)</c>.
    /// </summary>
    public static class FusionStepperHtmlExtensions
    {
        /// <summary>
        /// Creates a Syncfusion Stepper and carries its controlled component ID into the Reactive Plan.
        /// </summary>
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
