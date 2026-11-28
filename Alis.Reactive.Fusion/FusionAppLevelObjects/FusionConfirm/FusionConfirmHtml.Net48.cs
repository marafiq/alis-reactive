#if NET48
using System;
using System.Web;
using System.Web.Mvc;

namespace Alis.Reactive.Fusion.FusionAppLevelObjects;

/// <summary>
/// The door of a layout to the page's FusionConfirm.
/// </summary>
public static class FusionConfirmHtml
{
    /// <summary>Renders the element the page's FusionConfirm is made on, once in the layout, with the heading Confirm and the buttons OK and Cancel.</summary>
    /// <remarks>A view reaches the confirm with <c>pipeline.Component&lt;FusionConfirm&gt;()</c> and branches on its <c>Ask</c>.</remarks>
    /// <returns>The confirm's host element.</returns>
    public static IHtmlString FusionConfirmDialog(this HtmlHelper html) => html.FusionConfirmDialog("Confirm", "OK", "Cancel");

    /// <summary>Renders the element the page's FusionConfirm is made on, once in the layout, with the app's heading and button texts.</summary>
    /// <remarks>A view reaches the confirm with <c>pipeline.Component&lt;FusionConfirm&gt;()</c> and branches on its <c>Ask</c>.</remarks>
    /// <param name="html">The layout's HTML helper.</param>
    /// <param name="title">The heading every question shows.</param>
    /// <param name="ok">The text of the button that answers yes.</param>
    /// <param name="cancel">The text of the button that answers no.</param>
    /// <returns>The confirm's host element.</returns>
    public static IHtmlString FusionConfirmDialog(this HtmlHelper html, string title, string ok, string cancel)
    {
        var host = new TagBuilder("div");
        host.MergeAttribute("id", FusionConfirm.ElementId);
        host.MergeAttribute("data-alis-title", title ?? throw new ArgumentNullException(nameof(title)));
        host.MergeAttribute("data-alis-ok", ok ?? throw new ArgumentNullException(nameof(ok)));
        host.MergeAttribute("data-alis-cancel", cancel ?? throw new ArgumentNullException(nameof(cancel)));
        return new HtmlString(host.ToString(TagRenderMode.Normal));
    }
}
#endif
