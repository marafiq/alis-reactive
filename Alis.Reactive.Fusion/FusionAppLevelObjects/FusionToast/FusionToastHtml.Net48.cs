#if NET48
using System;
using System.Web;
using System.Web.Mvc;

namespace Alis.Reactive.Fusion.FusionAppLevelObjects;

/// <summary>
/// The door of a layout to the page's FusionToast.
/// </summary>
public static class FusionToastHtml
{
    /// <summary>Renders the element the page's FusionToast is made on, once in the layout.</summary>
    /// <remarks>A view reaches the toast with <c>pipeline.Component&lt;FusionToast&gt;()</c>, sets its members and calls <c>Show()</c>.</remarks>
    /// <returns>The toast's host element.</returns>
    public static IHtmlString FusionToast(this HtmlHelper html) => throw new NotImplementedException();
}
#endif
