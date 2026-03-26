#if NET48
using System.Web.Mvc;
#else
using Microsoft.AspNetCore.Mvc.Rendering;
#endif

namespace Alis.Reactive.Native.Components
{
    /// <summary>
    /// Factory extension for creating NativeButtonBuilder.
    /// </summary>
    public static class NativeButtonHtmlExtensions
    {
        /// <summary>
        /// Creates a native &lt;button&gt; builder with an explicit element ID.
        /// </summary>
        public static NativeButtonBuilder<TModel> NativeButton<TModel>(
#if NET48
            this HtmlHelper<TModel> html, string elementId, string text)
#else
            this IHtmlHelper<TModel> html, string elementId, string text)
#endif
            where TModel : class
        {
            return new NativeButtonBuilder<TModel>(elementId, text);
        }
    }
}
