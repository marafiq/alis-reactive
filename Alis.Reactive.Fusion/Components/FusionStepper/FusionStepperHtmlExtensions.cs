using System;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Syncfusion.EJ2;
using Syncfusion.EJ2.Navigations;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Factory extension for creating FusionStepperBuilder.
    /// Non-input component - NO InputField wrapper, NO input component registration.
    /// </summary>
    public static class FusionStepperHtmlExtensions
    {
        /// <summary>
        /// Creates a FusionStepper with the given element ID.
        /// </summary>
        public static FusionStepperBuilder<TModel> FusionStepper<TModel>(
            this IHtmlHelper<TModel> html,
            ReactivePlan<TModel> plan,
            string elementId,
            Action<StepperBuilder> build)
            where TModel : class
        {
            var builder = html.EJS().Stepper(elementId);
            build(builder);

            return new FusionStepperBuilder<TModel>(plan, elementId, builder.Render());
        }
    }
}
