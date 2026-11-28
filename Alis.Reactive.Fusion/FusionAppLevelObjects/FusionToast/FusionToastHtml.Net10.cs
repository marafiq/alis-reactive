#if NET10_0_OR_GREATER
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Toast = Alis.Reactive.Fusion.FusionAppLevelObjects.FusionToast;

namespace Alis.Reactive.Fusion.FusionAppLevelObjects;

/// <summary>
/// The door of a layout to the page's FusionToast.
/// </summary>
public static class FusionToastHtml
{
    /// <summary>Renders the element the page's FusionToast is made on, once in the layout.</summary>
    /// <remarks>A view reaches the toast with <c>pipeline.Component&lt;FusionToast&gt;()</c>, sets its members and calls <c>Show()</c>.</remarks>
    /// <returns>The toast's host element.</returns>
    public static IHtmlContent FusionToast(this IHtmlHelper html)
    {
        var host = new TagBuilder("div");
        host.MergeAttribute("id", Toast.ElementId);
        host.MergeAttribute("data-fusion-toast", "");
        return host;
    }
}
#endif
