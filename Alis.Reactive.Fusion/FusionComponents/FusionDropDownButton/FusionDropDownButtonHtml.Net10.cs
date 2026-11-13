#if NET10_0_OR_GREATER
using System;
using Alis.Reactive.RenderingAndDelivery;
using Microsoft.AspNetCore.Mvc.Rendering;
using Syncfusion.EJ2.SplitButtons;

namespace Alis.Reactive.Fusion.FusionComponents;

/// <summary>
/// The door of a view to a drop-down button.
/// </summary>
public static class FusionDropDownButtonHtml
{
    /// <summary>Renders a drop-down button under the id the view gives it, with what the view sets on EJ2's builder.</summary>
    /// <param name="html">The view's HTML helper.</param>
    /// <param name="plan">The view's plan.</param>
    /// <param name="elementId">The id the view gives the drop-down button's element.</param>
    /// <param name="build">What the view sets on the drop-down button.</param>
    /// <returns>The rendered drop-down button, which writes its markup where the view puts it.</returns>
    public static Rendered<FusionDropDownButton, TModel> FusionDropDownButton<TModel>(this IHtmlHelper<TModel> html, ReactivePlan<TModel> plan, string elementId, Action<DropDownButtonBuilder> build)
        => throw new NotImplementedException();
}
#endif
