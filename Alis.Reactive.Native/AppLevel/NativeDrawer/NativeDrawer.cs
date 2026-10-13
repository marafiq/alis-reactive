using System.Text.Json.Serialization;
#if NET48
using System.Web;
using System.Web.Mvc;
#else
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
#endif

namespace Alis.Reactive.Native.AppLevel
{
#nullable disable annotations
    /// <summary>
    /// The page's slide-out drawer, declared by the members its element carries.
    /// </summary>
    /// <remarks>
    /// One drawer exists per page, rendered by <c>@Html.NativeDrawer()</c> in the layout, and is
    /// referenced without an ID:
    /// <code>
    /// var drawer = p.Component&lt;NativeDrawer&gt;();
    /// drawer.Size.Set(DrawerSize.Md);
    /// drawer.Open();
    /// </code>
    /// Load its content into <c>alis-drawer-content</c> and its heading into <c>alis-drawer-title</c>.
    /// </remarks>
    public sealed class NativeDrawer : NativeComponent, IAppLevelComponent
    {
        /// <summary>Well-known layout element ID for the drawer.</summary>
        public const string ElementId = "alis-drawer";

        /// <inheritdoc />
        public string DefaultId => ElementId;

        /// <summary>The panel width; closing the drawer resets it.</summary>
        public Prop<DrawerSize> Size => Prop<DrawerSize>();

        /// <summary>Slides the drawer in.</summary>
        public void Open() => Call();

        /// <summary>Slides the drawer out; its content and heading clear when it has closed.</summary>
        public void Close() => Call();
    }
#nullable restore annotations

    /// <summary>The width of the <see cref="NativeDrawer"/> panel.</summary>
    public enum DrawerSize
    {
        /// <summary>Small width.</summary>
        [JsonStringEnumMemberName("sm")]
        Sm,

        /// <summary>Medium width.</summary>
        [JsonStringEnumMemberName("md")]
        Md,

        /// <summary>Large width.</summary>
        [JsonStringEnumMemberName("lg")]
        Lg
    }

    /// <summary>Renders the page's <see cref="NativeDrawer"/>.</summary>
    public static class NativeDrawerHtmlExtensions
    {
        /// <summary>Renders the drawer, hidden; call it once in the layout.</summary>
#if NET48
        public static IHtmlString NativeDrawer(this HtmlHelper html)
        {
            return new MvcHtmlString(
#else
        public static IHtmlContent NativeDrawer(this IHtmlHelper html)
        {
            return new HtmlString(
#endif
                "<aside id=\"" + AppLevel.NativeDrawer.ElementId + "\" class=\"alis-drawer\" aria-hidden=\"true\">\n" +
                "  <div class=\"alis-drawer__panel\">\n" +
                "    <div class=\"alis-drawer__header\">\n" +
                "      <h2 id=\"alis-drawer-title\" class=\"alis-drawer__title\"></h2>\n" +
                "      <button id=\"alis-drawer-close\" type=\"button\" class=\"alis-drawer__close\" aria-label=\"Close\">\n" +
                "        <svg width=\"20\" height=\"20\" viewBox=\"0 0 20 20\" fill=\"currentColor\"><path d=\"M6.28 5.22a.75.75 0 00-1.06 1.06L8.94 10l-3.72 3.72a.75.75 0 101.06 1.06L10 11.06l3.72 3.72a.75.75 0 101.06-1.06L11.06 10l3.72-3.72a.75.75 0 00-1.06-1.06L10 8.94 6.28 5.22z\"/></svg>\n" +
                "      </button>\n" +
                "    </div>\n" +
                "    <div id=\"alis-drawer-content\" class=\"alis-drawer__content\"></div>\n" +
                "  </div>\n" +
                "</aside>\n");
        }
    }
}
