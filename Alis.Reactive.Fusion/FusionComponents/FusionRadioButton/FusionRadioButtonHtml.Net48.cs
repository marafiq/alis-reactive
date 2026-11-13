#if NET48
using System;
using System.Web.Mvc;
using Alis.Reactive.RenderingAndDelivery;
using Syncfusion.EJ2.Buttons;

namespace Alis.Reactive.Fusion.FusionComponents;

/// <summary>
/// The door of a view to a radio button.
/// </summary>
public static class FusionRadioButtonHtml
{
    /// <summary>Renders a radio button under the id the view gives it, with what the view sets on EJ2's builder.</summary>
    /// <remarks>A radio button is no input: no field renders it and it binds no member.</remarks>
    /// <param name="html">The view's HTML helper.</param>
    /// <param name="plan">The view's plan.</param>
    /// <param name="elementId">The id the view gives the button's element.</param>
    /// <param name="build">What the view sets on the button.</param>
    /// <returns>The rendered button, which writes its markup where the view puts it.</returns>
    public static Rendered<FusionRadioButton, TModel> FusionRadioButton<TModel>(this HtmlHelper<TModel> html, ReactivePlan<TModel> plan, string elementId, Action<RadioButtonBuilder> build)
        => throw new NotImplementedException();
}
#endif
