#if NET48
using System;
using System.Web.Mvc;
using Alis.Reactive.RenderingAndDelivery;
using Syncfusion.EJ2.Popups;

namespace Alis.Reactive.Fusion.FusionComponents;

/// <summary>
/// The door of a view to a dialog.
/// </summary>
public static class FusionDialogHtml
{
    /// <summary>Renders a dialog under the id the view gives it, with what the view sets on EJ2's builder.</summary>
    /// <param name="html">The view's HTML helper.</param>
    /// <param name="plan">The view's plan.</param>
    /// <param name="elementId">The id the view gives the dialog's element.</param>
    /// <param name="build">What the view sets on the dialog.</param>
    /// <returns>The rendered dialog, which writes its markup where the view puts it.</returns>
    public static Rendered<FusionDialog, TModel> FusionDialog<TModel>(this HtmlHelper<TModel> html, ReactivePlan<TModel> plan, string elementId, Action<DialogBuilder> build)
        => throw new NotImplementedException();
}
#endif
