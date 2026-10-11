#if NET48
using System.Web;
using System.Web.Mvc;
#else
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
#endif

namespace Alis.Reactive.Native.AppLevel
{
    /// <summary>
    /// The page's loading overlay, declared by the members its element carries.
    /// </summary>
    /// <remarks>
    /// One loader exists per page, rendered by <c>@Html.NativeLoader()</c> in the layout, and is
    /// referenced without an ID:
    /// <code>p.Component&lt;NativeLoader&gt;().Set(c =&gt; c.Target, "resident-form").Call(c =&gt; c.Show)</code>
    /// Without a target it covers the viewport.
    /// </remarks>
    public sealed class NativeLoader : NativeComponent, IAppLevelComponent
    {
        /// <summary>Well-known layout element ID for the loader.</summary>
        public const string ElementId = "alis-loader";

        /// <inheritdoc />
        public string DefaultId => ElementId;

        /// <summary>The ID of the element the loader covers on its next show; hiding clears it.</summary>
        public ComponentProperty<string> Target => Property<string>("target");

        /// <summary>Milliseconds after which the next show hides itself; hiding clears it.</summary>
        public ComponentProperty<int> Timeout => Property<int>("timeout");

        /// <summary>Shows the loader.</summary>
        public ComponentMethod Show => Method("show");

        /// <summary>Hides the loader.</summary>
        public ComponentMethod Hide => Method("hide");
    }

    /// <summary>Renders the page's <see cref="NativeLoader"/>.</summary>
    public static class NativeLoaderHtmlExtensions
    {
        /// <summary>Renders the loader, hidden; call it once in the layout.</summary>
#if NET48
        public static IHtmlString NativeLoader(this HtmlHelper html)
        {
            return new MvcHtmlString(
#else
        public static IHtmlContent NativeLoader(this IHtmlHelper html)
        {
            return new HtmlString(
#endif
                "<div id=\"" + AppLevel.NativeLoader.ElementId + "\" class=\"alis-loader\" aria-hidden=\"true\">\n" +
                "  <div class=\"alis-loader__spinner\"></div>\n" +
                "  <p id=\"alis-loader-message\" class=\"alis-loader__message\"></p>\n" +
                "</div>\n");
        }
    }
}
