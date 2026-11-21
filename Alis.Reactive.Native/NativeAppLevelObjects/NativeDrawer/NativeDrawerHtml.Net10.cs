#if NET10_0_OR_GREATER
using System;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Drawer = Alis.Reactive.Native.NativeAppLevelObjects.NativeDrawer;

namespace Alis.Reactive.Native.NativeAppLevelObjects;

/// <summary>
/// The door of a layout to the page's drawer.
/// </summary>
public static class NativeDrawerHtml
{
    private const string CloseId = "alis-drawer-close";

    /// <summary>Renders the page's drawer, closed, once in the layout, with the app's text on its close button.</summary>
    /// <remarks>The drawer's heading and content are empty until a view sets the heading's text and loads a partial view into its content.</remarks>
    /// <param name="html">The layout's HTML helper.</param>
    /// <param name="closeText">The text of the drawer's close button.</param>
    /// <returns>The drawer's markup.</returns>
    public static IHtmlContent NativeDrawer(this IHtmlHelper html, string closeText)
    {
        ArgumentNullException.ThrowIfNull(closeText);
        var drawer = new TagBuilder("aside");
        drawer.MergeAttribute("id", Drawer.ElementId);
        drawer.MergeAttribute("class", "alis-drawer alis-drawer--md");
        drawer.MergeAttribute("aria-labelledby", Drawer.TitleId);
        drawer.MergeAttribute("aria-hidden", "true");
        var title = new TagBuilder("h2");
        title.MergeAttribute("id", Drawer.TitleId);
        var close = new TagBuilder("button");
        close.MergeAttribute("type", "button");
        close.MergeAttribute("id", CloseId);
        close.InnerHtml.Append(closeText);
        var content = new TagBuilder("div");
        content.MergeAttribute("id", Drawer.ContentId);
        drawer.InnerHtml.AppendHtml(title).AppendHtml(close).AppendHtml(content);
        return drawer;
    }
}
#endif
