using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Alis.Reactive.Conformance.App.Components;

public static class ConformanceNoticeHtml
{
    public static IHtmlContent ConformanceNotice(this IHtmlHelper html)
    {
        var host = new TagBuilder("div");
        host.Attributes["data-conformance-notice"] = string.Empty;
        return host;
    }
}
