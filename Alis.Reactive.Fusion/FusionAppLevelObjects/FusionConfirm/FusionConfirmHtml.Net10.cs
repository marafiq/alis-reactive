#if NET10_0_OR_GREATER
using System;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Alis.Reactive.Fusion.FusionAppLevelObjects;

/// <summary>
/// The door of a layout to the page's FusionConfirm.
/// </summary>
public static class FusionConfirmHtml
{
    /// <summary>Renders the element the page's FusionConfirm is made on, once in the layout, with the heading Confirm and the buttons OK and Cancel.</summary>
    /// <remarks>A view reaches the confirm with <c>pipeline.Component&lt;FusionConfirm&gt;()</c> and branches on its <c>Ask</c>.</remarks>
    /// <returns>The confirm's host element.</returns>
    public static IHtmlContent FusionConfirmDialog(this IHtmlHelper html) => throw new NotImplementedException();

    /// <summary>Renders the element the page's FusionConfirm is made on, once in the layout, with the app's heading and button texts.</summary>
    /// <remarks>A view reaches the confirm with <c>pipeline.Component&lt;FusionConfirm&gt;()</c> and branches on its <c>Ask</c>.</remarks>
    /// <param name="html">The layout's HTML helper.</param>
    /// <param name="title">The heading every question shows.</param>
    /// <param name="ok">The text of the button that answers yes.</param>
    /// <param name="cancel">The text of the button that answers no.</param>
    /// <returns>The confirm's host element.</returns>
    public static IHtmlContent FusionConfirmDialog(this IHtmlHelper html, string title, string ok, string cancel) => throw new NotImplementedException();
}
#endif
