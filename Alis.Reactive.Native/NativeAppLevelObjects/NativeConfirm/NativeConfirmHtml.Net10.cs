#if NET10_0_OR_GREATER
using System;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Alis.Reactive.Native.NativeAppLevelObjects;

/// <summary>
/// The door of a layout to the page's confirm.
/// </summary>
public static class NativeConfirmHtml
{
    /// <summary>Renders the page's confirm, hidden, once in the layout, with the app's heading and button texts.</summary>
    /// <remarks>Focus starts on the cancel button when a question shows, and Escape answers no.</remarks>
    /// <param name="html">The layout's HTML helper.</param>
    /// <param name="title">The heading every question shows.</param>
    /// <param name="ok">The text of the button that answers yes.</param>
    /// <param name="cancel">The text of the button that answers no.</param>
    /// <returns>The confirm's markup.</returns>
    public static IHtmlContent NativeConfirm(this IHtmlHelper html, string title, string ok, string cancel) => throw new NotImplementedException();
}
#endif
