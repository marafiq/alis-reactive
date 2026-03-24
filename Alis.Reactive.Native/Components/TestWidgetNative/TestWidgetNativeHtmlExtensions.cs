using Microsoft.AspNetCore.Mvc.Rendering;

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
            this IHtmlHelper<TModel> html, string elementId)
            where TModel : class
        {
            return new TestWidgetNativeBuilder<TModel>(elementId);
        }
    }
}
