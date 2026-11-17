#if NET10_0_OR_GREATER
using System;
using System.Globalization;
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
    public static IHtmlContent NativeToast(this IHtmlHelper html, TimeSpan shownFor, string dismiss)
    {
        if (shownFor <= TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(shownFor), shownFor, "A toast is shown for a time longer than zero.");
        if (dismiss is null) throw new ArgumentNullException(nameof(dismiss));
        var status = new TagBuilder("div");
        status.AddCssClass("alis-toasts__status");
        status.MergeAttribute("role", "status");
        var alert = new TagBuilder("div");
        alert.AddCssClass("alis-toasts__alert");
        alert.MergeAttribute("role", "alert");
        var region = new TagBuilder("section");
        region.MergeAttribute("id", "alis-toasts");
        region.AddCssClass("alis-toasts");
        region.MergeAttribute("popover", "manual");
        region.MergeAttribute("data-native-toast", "");
        region.MergeAttribute("data-native-toast-shown-for", ((long)shownFor.TotalMilliseconds).ToString(CultureInfo.InvariantCulture));
        region.MergeAttribute("data-native-toast-dismiss", dismiss);
        region.InnerHtml.AppendHtml(status);
        region.InnerHtml.AppendHtml(alert);
        return region;
    }
}
#endif
