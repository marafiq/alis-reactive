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
    /// Creates typed <see cref="FusionStepperBuilder{TModel}"/> instances for Reactive Plan wiring.
    /// </summary>
    public static class FusionStepperHtmlExtensions
    {
        /// <summary>
        /// Creates a Syncfusion Stepper and carries its controlled component ID into the Reactive Plan.
        /// </summary>
        /// <param name="plan">Reactive Plan receiving stepper event wiring.</param>
        /// <param name="elementId">Controlled component ID shared by markup and Reactive Plan behavior.</param>
        /// <param name="build">Configures the component before rendering.</param>
        public static FusionStepperBuilder<TModel> FusionStepper<TModel>(
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

            return new FusionStepperBuilder<TModel>(plan, elementId, builder.Render());
        }
    }
}
