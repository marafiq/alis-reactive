#if NET10_0_OR_GREATER
using System;
using Alis.Reactive.RenderingAndDelivery;
using Microsoft.AspNetCore.Mvc.Rendering;
using Syncfusion.EJ2;
using Syncfusion.EJ2.SplitButtons;

namespace Alis.Reactive.Fusion.FusionComponents;

/// <summary>
/// The door of a view to a split button.
/// </summary>
public static class FusionSplitButtonHtml
{
    /// <summary>Renders a split button under the id the view gives it, with what the view sets on EJ2's builder.</summary>
    /// <param name="html">The view's HTML helper.</param>
    /// <param name="plan">The view's plan.</param>
    /// <param name="elementId">The id the view gives the button's element.</param>
    /// <param name="build">What the view sets on the button and its list of actions.</param>
    /// <returns>The rendered button, which writes its markup where the view puts it.</returns>
    public static Rendered<FusionSplitButton, TModel> FusionSplitButton<TModel>(this IHtmlHelper<TModel> html, ReactivePlan<TModel> plan, string elementId, Action<SplitButtonBuilder> build)
    {
        if (plan is null) throw new ArgumentNullException(nameof(plan));
        if (elementId is null) throw new ArgumentNullException(nameof(elementId));
        if (build is null) throw new ArgumentNullException(nameof(build));
        var button = html.EJS().SplitButton(elementId);
        build(button);
        if (button.model.Items is not null) button.Items(FusionData.AsWritten(plan.Json(button.model.Items)));
        return plan.Render<FusionSplitButton>(elementId, button.Render());
    }
}
#endif
