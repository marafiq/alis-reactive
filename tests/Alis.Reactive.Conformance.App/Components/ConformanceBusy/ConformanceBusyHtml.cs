using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Alis.Reactive.Conformance.App.Components;

public static class ConformanceBusyHtml
{
    public static IHtmlContent ConformanceBusy(this IHtmlHelper html)
    {
        var host = new TagBuilder("div");
        host.Attributes["data-conformance-busy"] = string.Empty;
        return host;
    }
}
