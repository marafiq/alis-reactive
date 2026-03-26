#if NET48
using System.Web.Mvc;
#else
using Microsoft.AspNetCore.Mvc.Rendering;
#endif

namespace Alis.Reactive.Native.Components
{
    /// <summary>
    /// Factory extension for creating TestWidgetNativeBuilder.
    /// </summary>
    public static class TestWidgetNativeHtmlExtensions
    {
        /// <summary>
        /// Creates a TestWidgetNative builder that renders &lt;input type="text"&gt;.
        /// </summary>
        public static TestWidgetNativeBuilder<TModel> TestWidgetNative<TModel>(
#if NET48
            this HtmlHelper<TModel> html, string elementId)
#else
            this IHtmlHelper<TModel> html, string elementId)
#endif
            where TModel : class
        {
            return new TestWidgetNativeBuilder<TModel>(elementId);
        }
    }
}
