using System.Web;
using System.Web.Mvc;

namespace Alis.Reactive.Conformance.App.Components;

public static class ConformanceNoticeHtml
{
    public static IHtmlString ConformanceNotice(this HtmlHelper html)
    {
        var host = new TagBuilder("div");
        host.Attributes["data-conformance-notice"] = string.Empty;
        return new MvcHtmlString(host.ToString(TagRenderMode.Normal));
    }
}
