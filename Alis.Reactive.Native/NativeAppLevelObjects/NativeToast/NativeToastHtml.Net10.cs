#if NET10_0_OR_GREATER
using System;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Alis.Reactive.Native.NativeAppLevelObjects;

/// <summary>
/// The door of a layout to the page's toasts.
/// </summary>
public static class NativeToastHtml
{
    /// <summary>Renders the page's toast region once in the layout, with the app's time and dismiss text.</summary>
    /// <remarks>A view shows a toast with a request's <c>Notice()</c>; an error toast stays until it is dismissed.</remarks>
    /// <param name="html">The layout's HTML helper.</param>
    /// <param name="shownFor">How long a success, info or warning toast stays while neither hovered nor focused.</param>
    /// <param name="dismiss">The text of each toast's dismiss button.</param>
    /// <returns>The region's markup.</returns>
    public static IHtmlContent NativeToast(this IHtmlHelper html, TimeSpan shownFor, string dismiss) => throw new NotImplementedException();
}
#endif
