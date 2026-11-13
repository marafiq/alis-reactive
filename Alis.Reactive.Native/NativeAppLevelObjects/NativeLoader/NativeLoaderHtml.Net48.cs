#if NET48
using System;
using System.Web;
using System.Web.Mvc;

namespace Alis.Reactive.Native.NativeAppLevelObjects;

/// <summary>
/// The door of a layout to the page's busy indicator.
/// </summary>
public static class NativeLoaderHtml
{
    /// <summary>Renders the page's busy indicator, hidden, once in the layout, with the app's text.</summary>
    /// <remarks>A request shows busy on what it updates once the app's startup chooses <c>ShowBusy()</c>; each target shows an overlay of its own while its requests run.</remarks>
    /// <param name="html">The layout's HTML helper.</param>
    /// <param name="text">The text the page's indicator shows.</param>
    /// <returns>The indicator's markup.</returns>
    public static IHtmlString NativeLoader(this HtmlHelper html, string text) => throw new NotImplementedException();
}
#endif
