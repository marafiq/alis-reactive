#if NET48
using System.Web;
using System.Web.Mvc;
#else
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
#endif

namespace Alis.Reactive.Native
{
    /// <summary>
    /// The page's loading overlay, declared by the members its element carries.
    /// </summary>
    /// <remarks>
    /// One loader exists per page, rendered by <c>@Html.NativeLoader()</c> in the layout, and is
    /// referenced without an ID:
    /// <code>
    /// var loader = p.Component&lt;NativeLoader&gt;();
    /// loader.Target.Set("resident-form");
    /// loader.Show();
    /// </code>
    /// Without a target it covers the viewport.
    /// </remarks>
    public sealed class NativeLoader : NativeComponent, IAppLevelComponent
    {
        /// <summary>Well-known layout element ID for the loader.</summary>
        public const string ElementId = "alis-loader";

        /// <inheritdoc />
        public string DefaultId => ElementId;

        /// <summary>The ID of the element the loader covers on its next show; hiding clears it.</summary>
        /// <remarks>
        /// When the element it covers leaves the page, for example replaced by <c>Into</c>, the loader returns to
        /// the page hidden at once, so a <c>Show</c> or <c>Hide</c> later in the same pipeline still finds it.
        /// </remarks>
        public Property<string> Target => Property<string>();

        /// <summary>Milliseconds after which the next show hides itself; hiding clears it.</summary>
        public Property<int> Timeout => Property<int>();

        /// <summary>Shows the loader.</summary>
        public void Show() => Method();

        /// <summary>Hides the loader.</summary>
        public void Hide() => Method();
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
                "<alis-loader id=\"" + Alis.Reactive.Native.NativeLoader.ElementId + "\" class=\"alis-loader\" aria-hidden=\"true\">\n" +
                "  <div class=\"alis-loader__spinner\"></div>\n" +
                "  <p id=\"alis-loader-message\" class=\"alis-loader__message\"></p>\n" +
                "</alis-loader>\n");
        }
    }
}
