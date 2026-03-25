using System.IO;
using System.Text.Encodings.Web;
#if NET48
using System.Web;
#else
using Microsoft.AspNetCore.Html;
#endif

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
#if NET48
    public class TestWidgetSyncFusionBuilder<TModel> : IHtmlString where TModel : class
#else
    public class TestWidgetSyncFusionBuilder<TModel> : IHtmlContent where TModel : class
#endif
    {
        private readonly string _elementId;
        private string? _cssClass;
        private string? _initialValue;

        internal TestWidgetSyncFusionBuilder(string elementId)
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

#if NET48
        public string ToHtmlString()
        {
            using (var sw = new StringWriter())
            {
                WriteTo(sw, HtmlEncoder.Default);
                return sw.ToString();
            }
        }
#endif

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

}
