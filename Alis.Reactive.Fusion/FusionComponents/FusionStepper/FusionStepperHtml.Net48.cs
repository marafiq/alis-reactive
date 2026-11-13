#if NET48
using System;
using System.Web.Mvc;
using Alis.Reactive.RenderingAndDelivery;
using Syncfusion.EJ2.Navigations;

namespace Alis.Reactive.Fusion.FusionComponents;

/// <summary>
/// The door of a view to a stepper.
/// </summary>
public static class FusionStepperHtml
{
    /// <summary>Renders a stepper under the id the view gives it, with what the view sets on EJ2's builder.</summary>
    /// <param name="html">The view's HTML helper.</param>
    /// <param name="plan">The view's plan.</param>
    /// <param name="elementId">The id the view gives the stepper's element.</param>
    /// <param name="build">The stepper's steps, and what the view sets on it.</param>
    /// <returns>The rendered stepper, which writes its markup where the view puts it.</returns>
    public static Rendered<FusionStepper, TModel> FusionStepper<TModel>(this HtmlHelper<TModel> html, ReactivePlan<TModel> plan, string elementId, Action<StepperBuilder> build)
        => throw new NotImplementedException();
}
#endif
