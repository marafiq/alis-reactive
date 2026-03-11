using System.IO;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Renders a &lt;div data-test-widget&gt; element that auto-mounts as a TestWidget
    /// with ej2_instances pattern. Not model-bound — for standalone test widgets.
    ///
    /// Usage:
    ///   @Html.TestWidget("my-widget")
    ///       .InitialValue("hello")
    ///       .CssClass("rounded-md border border-border px-3 py-1.5 text-sm w-full")
    /// </summary>
    public class TestWidgetSyncFusionBuilder<TModel> : IHtmlContent where TModel : class
    {
        private readonly string _elementId;
        private string? _cssClass;
        private string? _initialValue;

        public TestWidgetSyncFusionBuilder(string elementId)
        {
            _elementId = elementId;
        }

        /// <summary>The element ID — used by .Reactive() to wire events.</summary>
        internal string ElementId => _elementId;

        /// <summary>Sets CSS classes on the container element.</summary>
        public TestWidgetSyncFusionBuilder<TModel> CssClass(string css)
        {
            _cssClass = css;
            return this;
        }

        /// <summary>Sets the initial value via data-initial-value attribute.</summary>
        public TestWidgetSyncFusionBuilder<TModel> InitialValue(string value)
        {
            _initialValue = value;
            return this;
        }

        public void WriteTo(TextWriter writer, HtmlEncoder encoder)
        {
            writer.Write("<div");
            writer.Write(" id=\"");
            writer.Write(encoder.Encode(_elementId));
            writer.Write("\"");
            writer.Write(" data-test-widget");
            if (_initialValue != null)
            {
                writer.Write(" data-initial-value=\"");
                writer.Write(encoder.Encode(_initialValue));
                writer.Write("\"");
            }
            if (_cssClass != null)
            {
                writer.Write(" class=\"");
                writer.Write(encoder.Encode(_cssClass));
                writer.Write("\"");
            }
            writer.Write("></div>");
        }
    }

    /// <summary>
    /// Factory extension for creating TestWidgetSyncFusionBuilder.
    /// </summary>
    public static class TestWidgetSyncFusionHtmlExtensions
    {
        /// <summary>
        /// Creates a TestWidget builder that renders &lt;div data-test-widget&gt;.
        /// </summary>
        public static TestWidgetSyncFusionBuilder<TModel> TestWidget<TModel>(
            this IHtmlHelper<TModel> html, string elementId)
            where TModel : class
        {
            return new TestWidgetSyncFusionBuilder<TModel>(elementId);
        }
    }
}
