using Microsoft.AspNetCore.Mvc.Rendering;

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
            this IHtmlHelper<TModel> html, string elementId, string text)
            where TModel : class
        {
            return new NativeButtonBuilder<TModel>(elementId, text);
        }
    }
}
