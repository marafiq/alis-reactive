using System;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Syncfusion.EJ2;
using Syncfusion.EJ2.Navigations;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Creates typed <see cref="FusionStepperBuilder{TModel}"/> instances for Reactive Plan wiring.
    /// </summary>
    public static class FusionStepperHtmlExtensions
    {
        /// <summary>
        /// Creates a Syncfusion Stepper and carries its controlled component ID into the Reactive Plan.
        /// </summary>
        /// <typeparam name="TModel">The view model used to author the Reactive Plan.</typeparam>
        /// <param name="html">The Razor HTML helper.</param>
        /// <param name="plan">The Reactive Plan that receives stepper event wiring.</param>
        /// <param name="elementId">The controlled component ID shared by markup and plan entries.</param>
        /// <param name="build">Configures the underlying Syncfusion stepper builder.</param>
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
