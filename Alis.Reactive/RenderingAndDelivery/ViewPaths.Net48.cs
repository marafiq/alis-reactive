#if NET48
using System.Web.Mvc;
using System.Web.WebPages;

namespace Alis.Reactive.RenderingAndDelivery;

internal static class ViewPaths
{
    internal static string Of(HtmlHelper html) => ((WebPageExecutingBase)html.ViewDataContainer).VirtualPath.TrimStart('~');
}
#endif
