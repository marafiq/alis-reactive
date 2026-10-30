#if NET48
using System.Web;
using System.Web.Mvc;
#else
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
#endif

namespace Alis.Reactive.Fusion.AppLevel
{
    /// <summary>Renders the host of the confirm dialog that <c>p.Confirm("...")</c> asks through.</summary>
    public static class FusionConfirmHtmlExtensions
    {
        /// <summary>Well-known layout element ID the confirm dialog renders into.</summary>
        public const string ElementId = "alisConfirmDialog";

        /// <summary>Renders the confirm dialog's host; call it once in the layout.</summary>
#if NET48
        public static IHtmlString FusionConfirmDialog(this HtmlHelper html)
#else
        public static IHtmlContent FusionConfirmDialog(this IHtmlHelper html)
#endif
            => new HtmlString($"<div id=\"{ElementId}\"></div>\n");
    }
}
