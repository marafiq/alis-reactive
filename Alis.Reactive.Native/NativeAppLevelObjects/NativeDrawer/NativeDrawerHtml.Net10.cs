#if NET10_0_OR_GREATER
using System;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Alis.Reactive.Native.NativeAppLevelObjects;

/// <summary>
/// The door of a layout to the page's drawer.
/// </summary>
public static class NativeDrawerHtml
{
    /// <summary>Renders the page's drawer, closed, once in the layout, with the app's text on its close button.</summary>
    /// <remarks>The drawer's heading and content are empty until a view sets the heading's text and loads a partial view into its content.</remarks>
    /// <param name="html">The layout's HTML helper.</param>
    /// <param name="closeText">The text of the drawer's close button.</param>
    /// <returns>The drawer's markup.</returns>
    public static IHtmlContent NativeDrawer(this IHtmlHelper html, string closeText) => throw new NotImplementedException();
}
#endif
