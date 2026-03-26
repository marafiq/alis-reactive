#if NET48
using System.Web.Mvc;
#else
using Microsoft.AspNetCore.Mvc.Rendering;
#endif

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Factory extension for creating TestWidgetSyncFusionBuilder.
    /// </summary>
    public static class TestWidgetSyncFusionHtmlExtensions
    {
        /// <summary>
        /// Creates a TestWidget builder that renders &lt;div data-test-widget&gt;.
        /// </summary>
        public static TestWidgetSyncFusionBuilder<TModel> TestWidget<TModel>(
#if NET48
            this HtmlHelper<TModel> html, string elementId)
#else
            this IHtmlHelper<TModel> html, string elementId)
#endif
            where TModel : class
        {
            return new TestWidgetSyncFusionBuilder<TModel>(elementId);
        }
    }
}
