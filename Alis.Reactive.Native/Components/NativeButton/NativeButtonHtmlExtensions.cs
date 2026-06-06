#if NET48
using System.Web.Mvc;
#else
using Microsoft.AspNetCore.Mvc.Rendering;
#endif

namespace Alis.Reactive.Native.Components
{
    /// <summary>
    /// Factory extension for rendering explicit-ID native buttons directly in a Razor view.
    /// </summary>
    public static class NativeButtonHtmlExtensions
    {
        /// <summary>
        /// Creates a native button builder with the element ID used by Reactive Plan event wiring.
        /// </summary>
        /// <param name="elementId">The element ID rendered on the button and used for component lookup.</param>
        /// <param name="text">The button text content.</param>
        /// <returns>A builder that renders the button directly in the view.</returns>
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
